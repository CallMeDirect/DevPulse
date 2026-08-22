namespace DevPulse.Models;

public sealed record SystemStats(
    double CpuUsagePercent,
    double MemoryUsedBytes,
    double MemoryTotalBytes,
    double DiskFreeBytes,
    double DiskTotalBytes);