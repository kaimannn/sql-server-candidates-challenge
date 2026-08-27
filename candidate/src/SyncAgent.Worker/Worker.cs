namespace SyncAgent.Worker;

/// <summary>
/// Placeholder heartbeat loop, proving the host starts, runs, and shuts down cleanly.
/// Will be replaced with the real polling logic in follow-up commits.
/// </summary>
public class Worker : BackgroundService
{
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(5);

    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sync Agent starting up (skeleton — polling not yet implemented).");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Sync Agent heartbeat at: {Time}", DateTimeOffset.Now);

            try
            {
                await Task.Delay(HeartbeatInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the host is shutting down (stoppingToken fires mid-delay);
                // let the loop exit cleanly instead of surfacing this as an error.
            }
        }

        _logger.LogInformation("Sync Agent shutting down.");
    }
}
