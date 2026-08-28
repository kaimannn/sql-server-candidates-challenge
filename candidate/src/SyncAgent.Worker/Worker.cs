using Microsoft.Extensions.Options;
using SyncAgent.Worker.Configuration;
using SyncAgent.Worker.Platform;
using SyncAgent.Worker.Tasks;
using System.Collections;

namespace SyncAgent.Worker;

/// <summary>
/// Polls the SyncPlatform for pending tasks and executes the matching query via
/// ISyncTaskDispatcher. Posting the result back to the platform lands in step 4 -
/// for now, a completed task only logs how many records it found.
/// </summary>
public class Worker(
    ISyncPlatformClient platformClient,
    ISyncTaskDispatcher taskDispatcher,
    IOptions<SyncPlatformOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly SyncPlatformOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Sync Agent starting up (posting results not yet implemented).");

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
                        "Received task {TaskId} ({TaskType}), created at {CreatedAt}.",
                        task.TaskId, task.TaskType, task.CreatedAt);

                    var result = await taskDispatcher.DispatchAsync(task, stoppingToken);
                    var recordCount = (result as ICollection)?.Count;

                    logger.LogInformation(
                        "Executed task {TaskId}: {RecordCount} record(s) retrieved. Posting result not yet implemented.",
                        task.TaskId, recordCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A single failed poll/execution (network blip, platform down, bad
                // response, unsupported task type, query failure) should not crash the
                // always-on agent - log it and try again next interval.
                logger.LogError(ex, "Polling or executing a sync task failed.");
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
