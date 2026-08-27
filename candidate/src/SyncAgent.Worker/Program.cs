using SyncAgent.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Registers the WindowsServiceLifetime so this same executable can be installed and
// managed as a real Windows Service via `sc create ... binPath=...`. When the process
// is *not* launched by the Service Control Manager (e.g. `dotnet run`, or non-Windows),
// this call is a no-op and the app behaves like a normal console app.
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SyncAgent";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
