using FocusManager.Agent.Services;
using FocusManager.Agent.Tray;
using FocusManager.Core.Abstractions;
using FocusManager.Infrastructure.Notifications;
using FocusManager.Infrastructure.Persistence;
using FocusManager.Infrastructure.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IWhitelistStore, SqliteWhitelistStore>();
builder.Services.AddSingleton<INotifier, ToastNotifier>();

builder.Services.AddSingleton<WmiProcessWatcher>();
builder.Services.AddSingleton<ExplorerInterop>();
builder.Services.AddSingleton<ChromePolicyRegistry>();

builder.Services.AddSingleton<TrayHost>();
builder.Services.AddSingleton<SessionSnapshotService>();

builder.Services.AddHostedService<AgentHostedService>();

var host = builder.Build();
host.Run();
