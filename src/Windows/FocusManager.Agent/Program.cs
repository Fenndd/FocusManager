using FocusManager.Agent.Composition;
using FocusManager.Agent.Enforcement;
using FocusManager.Agent.Monitoring;
using FocusManager.Agent.Notifications;
using FocusManager.Agent.Services;
using FocusManager.Agent.Tray;
using FocusManager.Core.Abstractions;
using FocusManager.Core.Models;
using FocusManager.Core.Rules;
using FocusManager.Persistence.Sqlite;
using FocusManager.Platform.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // Content files resolve next to the exe; the agent may be started from any working directory.
    ContentRootPath = AppContext.BaseDirectory
});

var whitelistStoreOptions = new SqliteWhitelistStoreOptions
{
    // The location used by the SQLite store before 2026-09-30, so existing data is kept (ADR-0006).
    DatabasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FocusManager",
        "focusmanager.db"),
    // FLD-004: the system drive root is always allowed, without subfolders.
    DefaultFolders = [new AllowedFolder("System Root (C:)", @"C:\", AllowSubfolders: false)]
};

builder.Services.AddSingleton<IWhitelistStore>(_ => new SqliteWhitelistStore(whitelistStoreOptions));
builder.Services.AddSingleton<INotifier, SilentNotifier>();

// Explicit Windows path semantics: backslash separators, drive/UNC roots, case-insensitive (T001), plus the
// current-drive/current-directory resolution of not fully qualified values that the UI may have persisted (T054).
builder.Services.AddWindowsPathRules();

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

namespace FocusManager.Agent.Composition
{
    /// <summary>Service registrations of the agent that are covered by tests (T054).</summary>
    public static class PathRulesRegistration
    {
        /// <summary>
        /// Registers the Windows path semantics of the agent: <see cref="WindowsHostPathRules"/> (the pure Windows rules
        /// plus the host's current-drive/current-directory and 8.3 resolution that the pre-T001 evaluator got from
        /// <c>Path.GetFullPath</c>), and a <see cref="RuleEvaluator"/> built from it. Registering the pure rules
        /// instead would bring back the "D:" versus "D:\" defect, so <c>PathRulesCompositionTests</c> guards this call.
        /// </summary>
        public static IServiceCollection AddWindowsPathRules(this IServiceCollection services)
        {
            services.AddSingleton<IPathRules>(new WindowsHostPathRules());
            services.AddSingleton(provider => new RuleEvaluator(provider.GetRequiredService<IPathRules>()));
            return services;
        }
    }
}
