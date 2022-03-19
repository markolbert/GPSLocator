using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using MapControl.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly IJ4JLogger _logger;
        private readonly DispatcherQueue _dQueue;

        private bool _initialized;

        static MainWindow()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add( "User-Agent", "GPSLocator" );

            TileImageLoader.Cache = new ImageFileCache( TileImageLoader.DefaultCacheFolder );

            //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);

            //BingMapsTileLayer.ApiKey = "Ameqf9cCPjqwawIcun91toGQ-F85jSDu8-XyEFeHEdTDr60dV9ySmZt800aHj6PS";
        }

        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = (App.Current.Resources["AppViewModel"] as AppViewModel)!;

            Title = "GPS Locator";

            _logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            _logger.SetLoggedType( GetType() );

            _dQueue = DispatcherQueue.GetForCurrentThread();

            this.Activated += MainWindow_Activated;
        }

        private AppViewModel ViewModel { get; }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if( _initialized )
                return;

            _initialized = true;

            await ViewModel.Configuration.ValidateAsync( RequestStarted, RequestEnded );

            await ((ImageFileCache)TileImageLoader.Cache).Clean();

            OuterElement.DataContext = App.Current.Host.Services.GetRequiredService<MainViewModel>();

            SetLaunchPage();
        }

        private void RequestStarted(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue(() =>
            {
                ViewModel.SetStatusMessage("Validating configuration");
                ViewModel.IndeterminateVisibility = Visibility.Visible;
            });
        }

        private void RequestEnded(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue(() => { ViewModel.IndeterminateVisibility = Visibility.Collapsed; });
        }

        private void NavigationView_OnSelectionChanged( NavigationView sender, NavigationViewSelectionChangedEventArgs args )
        {
            string? tag;

            if( args.IsSettingsSelected )
                tag = "Settings";
            else
            {
                var item = args.SelectedItemContainer as NavigationViewItem;
                if (item?.Tag is not string temp)
                    return;

                tag = temp;
            }
            
            if ( tag.Equals( AppViewModel.ResourceNames.HelpTag, StringComparison.OrdinalIgnoreCase ) )
            {
                if( string.IsNullOrEmpty( ViewModel.Configuration.HelpLink ) )
                    return;

                try
                {
                    OpenUrl( ViewModel.Configuration.HelpLink );
                }
                catch( Exception ex )
                {
                    _logger.Error<string>( "Could not open help link, message was {0}", ex.Message );
                }

                return;
            }

            var newPage =
                AppViewModel.PageNames.FirstOrDefault( x => x.Value.Equals( tag, StringComparison.OrdinalIgnoreCase ) );

            if( newPage == null )
                return;

            ContentFrame.Navigate( newPage.Item, newPage.Value );
            SetWindowSize( newPage );
        }

        private void SetWindowSize( SingleSelectableItem item )
        {
            ( int width, int height ) = item.Value switch
            {
                "LastKnown" => ( 910, 700 ),
                "History" => ( 1520, 740 ),
                "Messaging" => ( 1150, 700 ),
                "LogViewer" => ( 750, 450 ),
                "Settings" => ( 740, 930 ),
                _ => ( 1250, 730 )
            };

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle( this );
            var windowId = Win32Interop.GetWindowIdFromWindow( hWnd );
            var appWindow = AppWindow.GetFromWindowId( windowId );

            var size = new Windows.Graphics.SizeInt32( width, height );
            appWindow.Resize( size );
        }

        private void SetLaunchPage()
        {
            if( !ViewModel.Configuration.IsValid )
            {
                ContentFrame.Navigate( typeof( SettingsPage ) );
                return;
            }

            var launchPage = AppViewModel.PageNames
                                         .FirstOrDefault( x => x.Value.Equals( ViewModel.Configuration.LaunchPage,
                                                                               StringComparison.OrdinalIgnoreCase ) );

            if( launchPage == null )
                return;

            ContentFrame.Navigate( launchPage.Item, launchPage.Value );
            SetWindowSize( launchPage );
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Process.Start("xdg-open", url);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            Process.Start("open", url);
                        }
                        else
                        {
                            throw;
                        }
            }
        }
    }
}
