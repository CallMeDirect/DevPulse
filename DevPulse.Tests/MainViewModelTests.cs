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

        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => monitoringTask);

        Assert.False(viewModel.IsMonitoring);
    }
}