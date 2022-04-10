using Microsoft.UI.Xaml;
using System;
using System.Runtime.ExceptionServices;
using Windows.Graphics;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using UnhandledExceptionEventArgs = System.UnhandledExceptionEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public new static App Current => (App)Application.Current;

    private const string Publisher = "J4JSoftware";
    private const string ApplicationName = "GpsLocator";

    private readonly DispatcherQueue _dQueue;
    private readonly IJ4JLogger _buildLogger;
    private readonly IJ4JLogger _logger;

    private WindowId _windowId;
    private AppWindow? _appWindow;
    private int _appWidth;
    private int _appHeight;
        
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        _dQueue = DispatcherQueue.GetForCurrentThread();

        var hostConfig = new J4JWinAppHostConfiguration()
                        .Publisher( Publisher )
                        .ApplicationName( ApplicationName )
                        .LoggerInitializer( InitializeLogger )
                        .AddNetEventSinkToLogger()
                        .AddDependencyInjectionInitializers( SetupDependencyInjection )
                        .AddServicesInitializers( InitializeServices )
                        .AddUserConfigurationFile( "userConfig.json", optional: true )
                        .FilePathTrimmer( FilePathTrimmer );

        _buildLogger = hostConfig.Logger;

        if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
            throw new ApplicationException($"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}");

        Host = hostConfig.Build()
         ?? throw new NullReferenceException($"Failed to build {nameof(IJ4JHost)}");

        _logger = Host.Services.GetRequiredService<IJ4JLogger>();
        _logger.OutputCache(hostConfig.Logger);

        WeakReferenceMessenger.Default.Register<App, AppConfiguredMessage, string>(
            this,
            "primary",
            AppConfiguredHandler );
    }

    private void AppConfiguredHandler( App recipient, AppConfiguredMessage message )
    {
        _dQueue.TryEnqueue( () => MainWindow!.Content = new MainPage() );
    }

    public MainWindow? MainWindow { get; private set; }

    public IJ4JHost Host { get; }

    private void SetWindowSize( int width, int height )
    {
        //var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
        //var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        //var appWindow = AppWindow.GetFromWindowId(windowId);

        var size = new Windows.Graphics.SizeInt32(width, height);
        _appWindow!.Resize(size);
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow { Title = "GPS Locator" };
        MainWindow.Activate();

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
        _windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        _appWindow = AppWindow.GetFromWindowId(_windowId);

        // set initial window sizes
        var rawBounds = DisplayArea.GetFromWindowId( _windowId!, DisplayAreaFallback.Primary );
        var screenBounds = new RectInt32( 0, 0, 500, 500 );

        if( rawBounds == null )
            _logger.Error( "Failed to retrieve screen bounds, using default 500x500" );
        else screenBounds = rawBounds.WorkArea;

        _appWidth = AllocateScreen( screenBounds.Width, 700, 1200 );
        _appHeight = AllocateScreen(screenBounds.Height, 550,700);

        var launchPage = new LaunchPage();
        launchPage.ViewModel.Initialized += LaunchPageCompleted;

        var launchWidth = AllocateScreen( screenBounds.Width, 500, 700 );
        var launchHeight = AllocateScreen( screenBounds.Height, 500, 700 );

        SetWindowSize( launchWidth, launchHeight );
        MainWindow!.Content = launchPage;
    }

    private int AllocateScreen( int screenPixels, int min, int max )
    {
        var retVal = screenPixels > 2 * min ? screenPixels / 2 : min;
        return retVal > max ? max : retVal;
    }

    private void LaunchPageCompleted( object? sender, EventArgs e )
    {
        _dQueue.TryEnqueue( () =>
        {
            SetWindowSize( _appWidth, _appHeight );
            MainWindow!.Content = new MainPage();
        } );
    }
}