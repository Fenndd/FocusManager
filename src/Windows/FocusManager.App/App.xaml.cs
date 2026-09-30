using Microsoft.UI.Xaml;

namespace FocusManager.App;

public partial class App : Application
{
    public static Window? WindowInstance { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        WindowInstance = new MainWindow();
        WindowInstance.Activate();
    }
}
