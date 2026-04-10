using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Monitoring;
using FocusManager.Agent.Notifications;
using FocusManager.Agent.Services;
using FocusManager.Agent.Tray;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Rules;
using FocusManager.Infrastructure.Persistence;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IWhitelistStore, SqliteWhitelistStore>();
builder.Services.AddSingleton<INotifier, SilentNotifier>();

builder.Services.AddSingleton<RuleEvaluator>();

builder.Services.AddSingleton<WmiProcessWatcher>();
builder.Services.AddSingleton<ExplorerInterop>();
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
