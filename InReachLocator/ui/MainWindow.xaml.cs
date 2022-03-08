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
        private record DisplayControl( UserControl Control, Action<ContentControl> ContainerConfigurator );

        private readonly Stack<DisplayControl> _displayControls = new();
        private readonly ContentControl? _placeholder;
        private readonly IJ4JLogger _logger;

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

            Title = "InReach Locator";

            _logger = App.Current.Host.Services.GetRequiredService<IJ4JLogger>();
            _logger.SetLoggedType( GetType() );

            var appConfig = App.Current.Host.Services.GetRequiredService<IAppConfig>();

            DispatcherQueue.TryEnqueue( async () =>
            {
                appConfig.IsValid = await appConfig.ValidateConfiguration( _logger );
            } );

            if (TileImageLoader.Cache is ImageFileCache)
            {
                Task.Run( async () =>
                {
                    await Task.Delay( 2000 );
                    await ( (ImageFileCache) TileImageLoader.Cache ).Clean();
                } );
            }

            NavigationView.DataContext = App.Current.Host.Services.GetRequiredService<MainViewModel>();
        }

        private void NavigationView_OnSelectionChanged( NavigationView sender, NavigationViewSelectionChangedEventArgs args )
        {
            var item = args.SelectedItemContainer as NavigationViewItem;
            if( item?.Tag == null )
                return;

            var (pageType, title) = ( item.Tag as string ) switch
            {
                "Settings"=>(typeof(SettingsPage), "Application Settings"),
                "LastKnown"=>(typeof(LastKnownPage), "Last Known Location"),
                "History" => (typeof(HistoryPage), "Location History"),
                _ => (null, null)
            };

            if( pageType == null )
                return;

            ContentFrame.Navigate( pageType );

            NavigationView.Header = title;
            NavigationView.SelectedItem = item;
        }
    }
}
