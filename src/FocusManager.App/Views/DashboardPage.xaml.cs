using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FocusManager.App.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    private void OpenApps_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(AppsWhitelistPage));
    }

    private void OpenFolders_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(FoldersWhitelistPage));
    }

    private void OpenSites_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(SitesWhitelistPage));
    }
}
