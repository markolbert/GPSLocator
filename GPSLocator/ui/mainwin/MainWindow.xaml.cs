using J4JSoftware.DependencyInjection;
using J4JSoftware.DeusEx;
using J4JSoftware.Logging;
using J4JSoftware.WindowsAppUtilities;
using MapControl;
using MapControl.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

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
    }

    public MainWindow()
    {
        this.InitializeComponent();

        ViewModel = J4JDeusEx.ServiceProvider.GetRequiredService<AppViewModel>();

        Title = "GPS Locator";

        _logger = J4JDeusEx.ServiceProvider.GetRequiredService<IJ4JLogger>();
        _logger.SetLoggedType( GetType() );
    }

    private AppViewModel ViewModel { get; }
}