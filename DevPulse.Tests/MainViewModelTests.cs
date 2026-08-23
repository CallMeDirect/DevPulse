using DevPulse.Models;
using DevPulse.ViewModels;

namespace DevPulse.Tests;

public sealed class MainViewModelTests
{
    [Fact]
    public async Task StartMonitoringAsync_UpdatesResourcePercentages()
    {
        // Arrange
        const double gigabyte =
            1024d * 1024 * 1024;

        var stats = new SystemStats(
            CpuUsagePercent: 25,
            MemoryUsedBytes: 8 * gigabyte,
            MemoryTotalBytes: 16 * gigabyte,
            DiskFreeBytes: 200 * gigabyte,
            DiskTotalBytes: 500 * gigabyte);

        var service =
            new FakeSystemMonitorService(stats);

        var viewModel =
            new MainViewModel(service);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        // Act
        var monitoringTask =
            viewModel.StartMonitoringAsync(
                cancellationTokenSource.Token);

        // Assert
        Assert.True(viewModel.IsMonitoring);
        Assert.Equal(25, viewModel.CpuUsagePercent);
        Assert.Equal(50, viewModel.MemoryUsagePercent);
        Assert.Equal(60, viewModel.DiskUsagePercent);
        
        Assert.Single(viewModel.CpuHistory);
        Assert.Single(viewModel.MemoryHistory);

        Assert.Equal(25, viewModel.CpuHistory[0]);
        Assert.Equal(50, viewModel.MemoryHistory[0]);

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => monitoringTask);

        Assert.False(viewModel.IsMonitoring);
    }
    
    [Fact]
    public async Task StartMonitoringAsync_WhenServiceFails_SetsErrorMessage()
    {
        // Arrange
        var exception =
            new InvalidOperationException("Test failure");

        var service =
            new FakeSystemMonitorService(exception);

        var viewModel =
            new MainViewModel(service);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        // Act
        var monitoringTask =
            viewModel.StartMonitoringAsync(
                cancellationTokenSource.Token);

        // Assert
        Assert.True(viewModel.IsMonitoring);

        Assert.Equal(
            "Monitoring error: Test failure",
            viewModel.ErrorMessage);

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => monitoringTask);

        Assert.False(viewModel.IsMonitoring);
    }
}