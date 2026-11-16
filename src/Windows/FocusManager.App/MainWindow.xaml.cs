using FocusManager.App.Views;
using Microsoft.UI.Xaml;

namespace FocusManager.App;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RootFrame.Navigate(typeof(DashboardPage));
    }
}
