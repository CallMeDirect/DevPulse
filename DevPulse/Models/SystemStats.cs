namespace DevPulse.Models;

/// <summary>
/// Represents a point-in-time snapshot of the monitored system resources.
/// </summary>
/// <param name="CpuUsagePercent">The total CPU utilization as a percentage from 0 to 100.</param>
/// <param name="MemoryUsedBytes">The amount of physical memory currently in use, in bytes.</param>
/// <param name="MemoryTotalBytes">The total amount of physical memory installed, in bytes.</param>
/// <param name="DiskFreeBytes">The available space on the system drive, in bytes.</param>
/// <param name="DiskTotalBytes">The total capacity of the system drive, in bytes.</param>
public sealed record SystemStats(
    double CpuUsagePercent,
    double MemoryUsedBytes,
    double MemoryTotalBytes,
    double DiskFreeBytes,
    double DiskTotalBytes);
