using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DevPulse.Services;

namespace DevPulse.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    #region Private fields

    private readonly ISystemMonitorService _systemMonitorService;

    private const double BytesPerGigabyte = 1024d * 1024 * 1024;
    
    #endregion

    #region Public properties

    [ObservableProperty]
    public partial string Greeting { get; set; } = "DevPulse";

    [ObservableProperty]
    public partial string CpuUsage { get; set; } = "0%";

    [ObservableProperty]
    public partial string MemoryUsage { get; set; } = "0 GB";

    [ObservableProperty]
    public partial string DiskUsage { get; set; } = "Loading...";
    
    [ObservableProperty]
    public partial double CpuUsagePercent { get; set; }

    [ObservableProperty]
    public partial double MemoryUsagePercent { get; set; }

    [ObservableProperty]
    public partial double DiskUsagePercent { get; set; }

    #endregion
    
    #region Public constructors

    public MainViewModel(ISystemMonitorService systemMonitorService)
    {
        _systemMonitorService = systemMonitorService;
    }
    
    #endregion
    
    #region Private methods
    
    private static double ConvertBytesToGigabytes(double bytes)
    {
        return bytes / BytesPerGigabyte;
    }
    
    private async Task LoadStatsAsync(
        CancellationToken cancellationToken)
    {
        var stats =
            await _systemMonitorService.GetStatsAsync(cancellationToken);
        
        CpuUsagePercent = stats.CpuUsagePercent;

        MemoryUsagePercent =
            stats.MemoryTotalBytes == 0
                ? 0
                : stats.MemoryUsedBytes
                  / stats.MemoryTotalBytes
                  * 100;

        var diskUsedBytes =
            stats.DiskTotalBytes - stats.DiskFreeBytes;

        DiskUsagePercent =
            stats.DiskTotalBytes == 0
                ? 0
                : diskUsedBytes
                  / stats.DiskTotalBytes
                  * 100;

        var usedMemoryGb = ConvertBytesToGigabytes(stats.MemoryUsedBytes);
        
        var totalMemoryGb = ConvertBytesToGigabytes(stats.MemoryTotalBytes);
        
        var freeDiskGb = ConvertBytesToGigabytes(stats.DiskFreeBytes);

        var totalDiskGb = ConvertBytesToGigabytes(stats.DiskTotalBytes);

        CpuUsage = $"{stats.CpuUsagePercent:F1}%";
        MemoryUsage = $"{usedMemoryGb:F1} / {totalMemoryGb:F1} GB used";
        DiskUsage = $"{freeDiskGb:F1} / {totalDiskGb:F1} GB free";
    }
    
    #endregion 
    
    #region Public methods
    
    public async Task StartMonitoringAsync(
        CancellationToken cancellationToken)
    {
        await LoadStatsAsync(cancellationToken);

        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            await LoadStatsAsync(cancellationToken);
        }
    }
    
    #endregion
}
