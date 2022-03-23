using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
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
        }

        private AppViewModel ViewModel { get; }
    }
}
