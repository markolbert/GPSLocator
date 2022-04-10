using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private readonly IJ4JLogger _logger;

    public MainPage()
    {
        this.InitializeComponent();

        _logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
        _logger.SetLoggedType(GetType());

        ViewModel = App.Current.Host.Services.GetRequiredService<AppViewModel>();

        Loaded += OnLoaded;
    }

    private AppViewModel ViewModel { get; }

    private void OnLoaded( object sender, RoutedEventArgs e )
    {
        SetLaunchPage();
    }

    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        string? tag;

        if (args.IsSettingsSelected)
            tag = "Settings";
        else
        {
            var item = args.SelectedItemContainer as NavigationViewItem;
            if (item?.Tag is not string temp)
                return;

            tag = temp;
        }

        if (tag.Equals(ResourceNames.HelpTag, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(ViewModel.Configuration.HelpLink))
                return;

            try
            {
                OpenUrl(ViewModel.Configuration.HelpLink);
            }
            catch (Exception ex)
            {
                _logger.Error<string>("Could not open help link, message was {0}", ex.Message);
            }

            return;
        }

        var newPage =
            NavigationTargets.Pages.FirstOrDefault(x => x.PageTag.Equals(tag, StringComparison.OrdinalIgnoreCase));

        if (newPage == null)
            return;

        ContentFrame.Navigate(newPage.PageType, newPage.PageTag);
    }

    private void SetLaunchPage()
    {
        if( !ViewModel.Configuration.IsValid )
        {
            ContentFrame.Navigate( typeof( SettingsPage ) );
            return;
        }

        var launchPage = NavigationTargets.Pages
                                     .FirstOrDefault( x => x.PageTag.Equals(
                                                          ViewModel.Configuration.LaunchPage,
                                                          StringComparison.OrdinalIgnoreCase ) );

        if( launchPage == null )
            return;

        ContentFrame.Navigate( launchPage.PageType, launchPage.PageTag );
    }

    private static void OpenUrl( string url )
    {
        try
        {
            Process.Start( url );
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) )
            {
                url = url.Replace( "&", "^&" );
                Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
            }
            else
                if( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) )
                    Process.Start( "xdg-open", url );
                else
                    if( RuntimeInformation.IsOSPlatform( OSPlatform.OSX ) )
                        Process.Start( "open", url );
                    else throw;
        }
    }
}