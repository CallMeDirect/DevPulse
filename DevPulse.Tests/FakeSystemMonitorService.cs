using DevPulse.Models;
using DevPulse.Services;

namespace DevPulse.Tests;

internal sealed class FakeSystemMonitorService(SystemStats stats) : ISystemMonitorService
{
    public Task<SystemStats> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(stats);
    }
}