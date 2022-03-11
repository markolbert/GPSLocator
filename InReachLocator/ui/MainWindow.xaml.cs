using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using MapControl.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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
        private readonly AppConfig _appConfig;

        private bool _initialized;

        static MainWindow()
        {
            ImageLoader.HttpClient.DefaultRequestHeaders.Add( "User-Agent", "InReachLocator" );

            TileImageLoader.Cache = new ImageFileCache( TileImageLoader.DefaultCacheFolder );

            //TileImageLoader.Cache = new FileDbCache(TileImageLoader.DefaultCacheFolder);
            //TileImageLoader.Cache = new SQLiteCache(TileImageLoader.DefaultCacheFolder);

            BingMapsTileLayer.ApiKey = "Ameqf9cCPjqwawIcun91toGQ-F85jSDu8-XyEFeHEdTDr60dV9ySmZt800aHj6PS";
        }

        public MainWindow()
        {
            this.InitializeComponent();

            _appConfig = (App.Current.Resources["AppConfiguration"] as AppConfig)!;

            Title = "InReach Locator";

            _logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            _logger.SetLoggedType( GetType() );

            this.Activated += MainWindow_Activated;
        }

        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if( _initialized )
                return;

            _initialized = true;

            var appConfig = App.Current.Resources["AppConfiguration"] as AppConfig;
            appConfig!.IsValid = await appConfig.ValidateConfiguration( _logger );

            await ((ImageFileCache)TileImageLoader.Cache).Clean();

            OuterElement.DataContext = App.Current.Host.Services.GetRequiredService<MainViewModel>();
        }

        private void NavigationView_OnSelectionChanged( NavigationView sender, NavigationViewSelectionChangedEventArgs args )
        {
            if( args.IsSettingsSelected )
            {
                ContentFrame.Navigate( typeof( SettingsPage ) );
                return;
            }

            var item = args.SelectedItemContainer as NavigationViewItem;
            if( item?.Tag == null )
                return;

            var pageType = ( item.Tag as string ) switch
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
    }
}
