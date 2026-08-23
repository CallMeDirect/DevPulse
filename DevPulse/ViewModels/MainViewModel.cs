using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevPulse.Services;

namespace DevPulse.ViewModels;

/// <summary>
/// Coordinates resource monitoring and exposes formatted values and history to the main window.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    #region Private constants and fields

    /// <summary>Provides system resource snapshots to the view model.</summary>
    private readonly ISystemMonitorService _systemMonitorService;

    /// <summary>Defines the number of bytes in one gibibyte used for UI conversion.</summary>
    private const double BytesPerGigabyte = 1024d * 1024 * 1024;

    /// <summary>Defines the maximum number of one-second samples retained in each history.</summary>
    private const int MaxHistoryPoints = 60;
    
    #endregion

    #region Public properties

    /// <summary>Gets or sets the heading displayed at the top of the dashboard.</summary>
    [ObservableProperty]
    public partial string HeaderText { get; set; } = "DevPulse";

    /// <summary>Gets or sets the formatted CPU utilization text.</summary>
    [ObservableProperty]
    public partial string CpuUsage { get; set; } = "0%";

    /// <summary>Gets or sets the formatted physical-memory usage text.</summary>
    [ObservableProperty]
    public partial string MemoryUsage { get; set; } = "0 GB";

    /// <summary>Gets or sets the formatted system-drive usage text.</summary>
    [ObservableProperty]
    public partial string DiskUsage { get; set; } = "Loading...";
    
    /// <summary>Gets or sets the current CPU utilization percentage.</summary>
    [ObservableProperty]
    public partial double CpuUsagePercent { get; set; }

    /// <summary>Gets or sets the current physical-memory utilization percentage.</summary>
    [ObservableProperty]
    public partial double MemoryUsagePercent { get; set; }

    /// <summary>Gets or sets the current system-drive utilization percentage.</summary>
    [ObservableProperty]
    public partial double DiskUsagePercent { get; set; }
    
    /// <summary>Gets or sets the latest recoverable monitoring error shown to the user.</summary>
    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    /// <summary>Gets a value indicating whether the monitoring loop is currently active.</summary>
    [ObservableProperty]
    public partial bool IsMonitoring { get; private set; }
    
    /// <summary>Gets the rolling CPU utilization history.</summary>
    public ObservableCollection<double> CpuHistory { get; } = [];

    /// <summary>Gets the rolling physical-memory utilization history.</summary>
    public ObservableCollection<double> MemoryHistory { get; } = [];

    #endregion
    
    #region Public constructors

    /// <summary>Initializes a new dashboard view model.</summary>
    /// <param name="systemMonitorService">The service used to retrieve resource statistics.</param>
    public MainViewModel(ISystemMonitorService systemMonitorService)
    {
        _systemMonitorService = systemMonitorService;
    }
    
    #endregion
    
    #region Private methods
    
    /// <summary>Adds a sample to a rolling history and removes the oldest excess sample.</summary>
    /// <param name="history">The history collection to update.</param>
    /// <param name="value">The percentage sample to append.</param>
    private static void AddHistoryPoint(
        ObservableCollection<double> history,
        double value)
    {
        history.Add(value);

        if (history.Count > MaxHistoryPoints)
        {
            history.RemoveAt(0);
        }
    }
    
    /// <summary>Converts a byte count to gibibytes for display.</summary>
    /// <param name="bytes">The byte count to convert.</param>
    /// <returns>The equivalent value in gibibytes.</returns>
    private static double ConvertBytesToGigabytes(double bytes)
    {
        return bytes / BytesPerGigabyte;
    }
    
    /// <summary>Loads one sample while converting recoverable failures into UI state.</summary>
    /// <param name="cancellationToken">A token used to cancel monitoring.</param>
    private async Task TryLoadStatsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await LoadStatsAsync(cancellationToken);
            ErrorMessage = null;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"Monitoring error: {exception.Message}";
        }
    }
    
    /// <summary>Loads and projects one system snapshot into dashboard properties and histories.</summary>
    /// <param name="cancellationToken">A token used to cancel the pending sample.</param>
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
        
        AddHistoryPoint(CpuHistory, CpuUsagePercent);
        AddHistoryPoint(MemoryHistory, MemoryUsagePercent);

        CpuUsage = $"{stats.CpuUsagePercent:F1}%";
        MemoryUsage = $"{usedMemoryGb:F1} / {totalMemoryGb:F1} GB used";
        DiskUsage = $"{freeDiskGb:F1} / {totalDiskGb:F1} GB free";
    }
    
    #endregion 
    
    #region Public methods
    
    /// <summary>Starts the one-second resource monitoring loop and runs until cancellation.</summary>
    /// <param name="cancellationToken">A token used to stop the monitoring loop.</param>
    /// <returns>A task representing the lifetime of the monitoring loop.</returns>
    public async Task StartMonitoringAsync(
        CancellationToken cancellationToken)
    {
        IsMonitoring = true;
        
        try
        {
            await TryLoadStatsAsync(cancellationToken);

            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(1));

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await TryLoadStatsAsync(cancellationToken);
            }
        }
        finally
        {
            IsMonitoring = false;
        }
    }
    
    #endregion
}
