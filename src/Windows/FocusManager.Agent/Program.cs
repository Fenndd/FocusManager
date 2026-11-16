using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Monitoring;
using FocusManager.Agent.Notifications;
using FocusManager.Agent.Services;
using FocusManager.Agent.Tray;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Rules;
using FocusManager.Persistence.Postgres;
using FocusManager.Platform.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // appsettings.json lives next to the exe; the agent may be started from any working directory.
    ContentRootPath = AppContext.BaseDirectory
});

var connectionString = builder.Configuration.GetConnectionString("FocusManager");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "PostgreSQL connection string 'ConnectionStrings:FocusManager' is not configured. " +
        "Set it in appsettings.json or the ConnectionStrings__FocusManager environment variable.");
}

builder.Services.AddSingleton<IWhitelistStore>(_ => new PostgresWhitelistStore(connectionString));
builder.Services.AddSingleton<INotifier, SilentNotifier>();

builder.Services.AddSingleton<RuleEvaluator>();

builder.Services.AddSingleton<WmiProcessWatcher>();
builder.Services.AddSingleton<ExplorerInterop>();
builder.Services.AddSingleton<IExplorerNavigator>(services => services.GetRequiredService<ExplorerInterop>());
builder.Services.AddSingleton<ChromePolicyRegistry>();

builder.Services.AddSingleton<ProcessStartMonitor>();
builder.Services.AddSingleton<ExplorerMonitor>();
builder.Services.AddSingleton<ChromeMonitor>();

builder.Services.AddSingleton<AppEnforcer>();
builder.Services.AddSingleton<FolderEnforcer>();
builder.Services.AddSingleton<SiteEnforcer>();

builder.Services.AddSingleton<TrayHost>();
builder.Services.AddSingleton<SessionSnapshotService>();

builder.Services.AddHostedService<AgentHostedService>();

var host = builder.Build();
host.Run();
