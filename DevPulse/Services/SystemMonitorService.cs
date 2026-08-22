using System;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Models;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DevPulse.Services;

public sealed class SystemMonitorService : ISystemMonitorService
{
    private readonly object _cpuLock = new();

    private ulong _previousIdleTime;
    private ulong _previousTotalTime;
    private bool _hasPreviousCpuSample;
    
    [StructLayout(LayoutKind.Sequential)]
    private struct CpuFileTime
    {
        public uint LowDateTime;
        public uint HighDateTime;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical;
        public ulong AvailablePhysical;
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
        public ulong AvailableExtendedVirtual;
    }
    
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(
        ref MemoryStatus memoryStatus);
    
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(
        out CpuFileTime idleTime,
        out CpuFileTime kernelTime,
        out CpuFileTime userTime);
    
    private static ulong ConvertToUInt64(CpuFileTime time)
    {
        return ((ulong)time.HighDateTime << 32)
               | time.LowDateTime;
    }
    
    private double GetCpuUsagePercent()
    {
        lock (_cpuLock)
        {
            if (!GetSystemTimes(
                    out var idleTime,
                    out var kernelTime,
                    out var userTime))
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error());
            }

            var idle = ConvertToUInt64(idleTime);
            var kernel = ConvertToUInt64(kernelTime);
            var user = ConvertToUInt64(userTime);

            var total = kernel + user;

            if (!_hasPreviousCpuSample)
            {
                _previousIdleTime = idle;
                _previousTotalTime = total;
                _hasPreviousCpuSample = true;

                return 0;
            }

            var idleDelta = idle - _previousIdleTime;
            var totalDelta = total - _previousTotalTime;

            _previousIdleTime = idle;
            _previousTotalTime = total;

            if (totalDelta == 0)
                return 0;

            var busyDelta =
                totalDelta > idleDelta
                    ? totalDelta - idleDelta
                    : 0;

            var usagePercent =
                busyDelta * 100d / totalDelta;

            return Math.Clamp(usagePercent, 0, 100);
        }
    }
    
    public Task<SystemStats> GetStatsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var memoryStatus = new MemoryStatus
        {
            Length = (uint)Marshal.SizeOf<MemoryStatus>()
        };

        if (!GlobalMemoryStatusEx(ref memoryStatus))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error());
        }
        
        var cpuUsagePercent = GetCpuUsagePercent();
        
        var memoryTotalBytes =
            (double)memoryStatus.TotalPhysical;

        var memoryUsedBytes =
            memoryTotalBytes - memoryStatus.AvailablePhysical;
        
        var systemDriveRoot = 
            Path.GetPathRoot(Environment.SystemDirectory) 
            ?? throw new InvalidOperationException("Failed to locate the system drive root.");

        var systemDrive = new DriveInfo(systemDriveRoot);
        
        var stats = new SystemStats(
            CpuUsagePercent: cpuUsagePercent,
            MemoryUsedBytes: memoryUsedBytes,
            MemoryTotalBytes: memoryTotalBytes,
            DiskFreeBytes: systemDrive.AvailableFreeSpace,
            DiskTotalBytes: systemDrive.TotalSize);
        
        return Task.FromResult(stats);
    }
}