using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator
{
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

            ViewModel = App.Current.Host.Services.GetRequiredService<MainViewModel>();

            Loaded += OnLoaded;
        }

        private MainViewModel ViewModel { get; }

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

            if (tag.Equals(AppViewModel.ResourceNames.HelpTag, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(ViewModel.AppViewModel.Configuration.HelpLink))
                    return;

                try
                {
                    OpenUrl(ViewModel.AppViewModel.Configuration.HelpLink);
                }
                catch (Exception ex)
                {
                    _logger.Error<string>("Could not open help link, message was {0}", ex.Message);
                }

                return;
            }

            var newPage =
                AppViewModel.PageNames.FirstOrDefault(x => x.Value.Equals(tag, StringComparison.OrdinalIgnoreCase));

            if (newPage == null)
                return;

            ContentFrame.Navigate(newPage.Item, newPage.Value);
        }

        private void SetLaunchPage()
        {
            if( !ViewModel.AppViewModel.Configuration.IsValid )
            {
                ContentFrame.Navigate( typeof( SettingsPage ) );
                return;
            }

            var launchPage = AppViewModel.PageNames
                                         .FirstOrDefault( x => x.Value.Equals(
                                                              ViewModel.AppViewModel.Configuration.LaunchPage,
                                                              StringComparison.OrdinalIgnoreCase ) );

            if( launchPage == null )
                return;

            ContentFrame.Navigate( launchPage.Item, launchPage.Value );
        }

        private void OpenUrl( string url )
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
}
