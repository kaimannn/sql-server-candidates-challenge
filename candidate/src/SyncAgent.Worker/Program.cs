using SyncAgent.Worker;
using SyncAgent.Worker.Data;

var builder = Host.CreateApplicationBuilder(args);

// Registers the WindowsServiceLifetime so this same executable can be installed and
// managed as a real Windows Service via `sc create ... binPath=...`. When the process
// is *not* launched by the Service Control Manager (e.g. `dotnet run`, or non-Windows),
// this call is a no-op and the app behaves like a normal console app.
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SyncAgent";
});

builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Fail fast if AdventureWorks isn't reachable at startup - a bad connection string or
// stopped SQL Server should surface immediately in the logs, not as a silent failure
// on the first poll once step 2 adds the polling loop.
await using (var scope = host.Services.CreateAsyncScope())
{
    var connectionFactory = scope.ServiceProvider.GetRequiredService<ISqlConnectionFactory>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await using var connection = await connectionFactory.CreateOpenConnectionAsync();
    logger.LogInformation(
        "Connected to AdventureWorks database (server version {ServerVersion}).",
        connection.ServerVersion);
}

host.Run();
