using DevPulse.Models;
using DevPulse.Services;

namespace DevPulse.Tests;

internal sealed class FakeSystemMonitorService : ISystemMonitorService
{
    private readonly Func<CancellationToken, Task<SystemStats>> _getStats;

    public FakeSystemMonitorService(SystemStats stats)
    {
        _getStats = cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(stats);
        };
    }

    public FakeSystemMonitorService(Exception exception)
    {
        _getStats = _ =>
            Task.FromException<SystemStats>(exception);
    }

    public Task<SystemStats> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        return _getStats(cancellationToken);
    }
}