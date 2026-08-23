using System;
using System.Threading;
using System.Threading.Tasks;
using DevPulse.Models;
using System.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DevPulse.Services;

/// <summary>
/// Retrieves Windows CPU, physical-memory, and system-drive statistics.
/// </summary>
public sealed class SystemMonitorService : ISystemMonitorService
{
    #region Private fields

    /// <summary>Synchronizes access to the state used for CPU delta calculations.</summary>
    private readonly object _cpuLock = new();

    /// <summary>Stores the idle-time counter from the previous CPU sample.</summary>
    private ulong _previousIdleTime;

    /// <summary>Stores the total-time counter from the previous CPU sample.</summary>
    private ulong _previousTotalTime;

    /// <summary>Indicates whether a baseline CPU sample has already been captured.</summary>
    private bool _hasPreviousCpuSample;

    #endregion

    #region Native interop types

    /// <summary>Represents the two 32-bit components of a Windows FILETIME value.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct CpuFileTime
    {
        /// <summary>Contains the low-order 32 bits of the time value.</summary>
        public uint LowDateTime;

        /// <summary>Contains the high-order 32 bits of the time value.</summary>
        public uint HighDateTime;
    }

    /// <summary>Receives physical and virtual memory information from Windows.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        /// <summary>Specifies the size of this structure in bytes.</summary>
        public uint Length;

        /// <summary>Receives the approximate percentage of physical memory in use.</summary>
        public uint MemoryLoad;

        /// <summary>Receives the total physical memory in bytes.</summary>
        public ulong TotalPhysical;

        /// <summary>Receives the available physical memory in bytes.</summary>
        public ulong AvailablePhysical;

        /// <summary>Receives the current committed-memory limit in bytes.</summary>
        public ulong TotalPageFile;

        /// <summary>Receives the available committed-memory capacity in bytes.</summary>
        public ulong AvailablePageFile;

        /// <summary>Receives the total user-mode virtual address space in bytes.</summary>
        public ulong TotalVirtual;

        /// <summary>Receives the available user-mode virtual address space in bytes.</summary>
        public ulong AvailableVirtual;

        /// <summary>Reserved by Windows and currently reported as zero.</summary>
        public ulong AvailableExtendedVirtual;
    }

    #endregion

    #region Native methods

    /// <summary>Retrieves current physical and virtual memory statistics from Windows.</summary>
    /// <param name="memoryStatus">The structure that receives memory information.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(
        ref MemoryStatus memoryStatus);
    
    /// <summary>Retrieves cumulative idle, kernel, and user CPU time counters from Windows.</summary>
    /// <param name="idleTime">Receives cumulative processor idle time.</param>
    /// <param name="kernelTime">Receives cumulative kernel time, including idle time.</param>
    /// <param name="userTime">Receives cumulative user-mode time.</param>
    /// <returns><see langword="true"/> when the operation succeeds; otherwise, <see langword="false"/>.</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(
        out CpuFileTime idleTime,
        out CpuFileTime kernelTime,
        out CpuFileTime userTime);
    
    #endregion

    #region Private methods

    /// <summary>Combines a native FILETIME value into one unsigned 64-bit counter.</summary>
    /// <param name="time">The native time value to combine.</param>
    /// <returns>The combined counter value.</returns>
    private static ulong ConvertToUInt64(CpuFileTime time)
    {
        return ((ulong)time.HighDateTime << 32)
               | time.LowDateTime;
    }
    
    /// <summary>Calculates CPU utilization from the difference between consecutive system counters.</summary>
    /// <returns>The total CPU utilization percentage from 0 to 100.</returns>
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
    
    #endregion

    #region Public methods

    /// <inheritdoc />
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

    #endregion
}
