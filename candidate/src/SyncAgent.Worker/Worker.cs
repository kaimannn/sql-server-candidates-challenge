using Microsoft.Extensions.Options;
using SyncAgent.Worker.Configuration;
using SyncAgent.Worker.Platform;

namespace SyncAgent.Worker;

/// <summary>
/// Polls the SyncPlatform for pending tasks. Executing the query and posting the
/// result back are added in follow-up commits - for now, a received task is only
/// logged, to prove the polling loop itself works end to end.
/// </summary>
public class Worker(
    ISyncPlatformClient platformClient,
    IOptions<SyncPlatformOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly SyncPlatformOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Sync Agent starting up (task execution not yet implemented).");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var task = await platformClient.GetNextTaskAsync(stoppingToken);

                if (task is null)
                {
                    logger.LogInformation("No pending tasks.");
                }
                else
                {
                    logger.LogInformation(
                        "Received task {TaskId} ({TaskType}), created at {CreatedAt}. Execution not yet implemented.",
                        task.TaskId, task.TaskType, task.CreatedAt);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A single failed poll (network blip, platform down, bad response) should
                // not crash the always-on agent - log it and try again next interval.
                logger.LogError(ex, "Polling the SyncPlatform failed.");
            }

            try
            {
                await Task.Delay(_options.PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when the host is shutting down mid-delay.
            }
        }

        logger.LogInformation("Sync Agent shutting down.");
    }
}
