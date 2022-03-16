using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using MapControl.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
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
            ImageLoader.HttpClient.DefaultRequestHeaders.Add( "User-Agent", "InReachLocator" );

            TileImageLoader.Cache = new ImageFileCache( TileImageLoader.DefaultCacheFolder );

            //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);

            //BingMapsTileLayer.ApiKey = "Ameqf9cCPjqwawIcun91toGQ-F85jSDu8-XyEFeHEdTDr60dV9ySmZt800aHj6PS";
        }

        public MainWindow()
        {
            this.InitializeComponent();

            ViewModel = (App.Current.Resources["AppConfiguration"] as AppViewModel)!;

            Title = "InReach Locator";

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
            if( args.IsSettingsSelected )
            {
                ContentFrame.Navigate( typeof( SettingsPage ) );
                return;
            }

            var item = args.SelectedItemContainer as NavigationViewItem;
            if( item?.Tag is not string tag )
                return;

            if( tag.Equals( AppViewModel.ResourceNames.HelpTag, StringComparison.OrdinalIgnoreCase ) )
            {
                OpenUrl( AppViewModel.ResourceNames.HelpLink );
                return;
            }

            var pageType = tag switch
            {
                LastKnownPage.PageName => typeof( LastKnownPage ),
                HistoryPage.PageName => typeof( HistoryPage ),
                LogViewerPage.PageName => typeof( LogViewerPage ),
                _ => null
            };

            if( pageType == null )
                return;

            ContentFrame.Navigate(pageType);
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
