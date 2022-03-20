using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Serilog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.GPSLocator
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;

        private readonly DispatcherQueue _dQueue;
        private readonly IJ4JLogger _buildLogger;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            _dQueue = DispatcherQueue.GetForCurrentThread();

            var hostConfig = new J4JHostConfiguration()
                            .Publisher( "J4JSoftware" )
                            .ApplicationName( "GPSLocator" )
                            .LoggerInitializer( InitializeLogger )
                            .AddNetEventSinkToLogger()
                            .AddDependencyInjectionInitializers( SetupDependencyInjection )
                            .AddServicesInitializers( InitializeServices )
                            .AddUserConfigurationFile( "userConfig.json", optional: true, reloadOnChange: true )
                            .AddApplicationConfigurationFile( "appConfig.json", optional: false )
                            .FilePathTrimmer( FilePathTrimmer );

            _buildLogger = hostConfig.Logger;

            if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
                throw new ApplicationException($"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}");

            Host = hostConfig.Build()
             ?? throw new NullReferenceException($"Failed to build {nameof(IJ4JHost)}");

            var logger = Host.Services.GetRequiredService<IJ4JLogger>();
            logger.OutputCache(hostConfig.Logger);

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

        public void SetWindowSize( int width, int height )
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            var size = new Windows.Graphics.SizeInt32(width, height);
            appWindow.Resize(size);
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

            var launchPage = new LaunchPage();
            launchPage.ViewModel.Initialized += LaunchPageCompleted;

            MainWindow!.Content = launchPage;
        }

        private void LaunchPageCompleted( object? sender, EventArgs e )
        {
            _dQueue.TryEnqueue( () => MainWindow!.Content = new MainPage() );
        }
    }
}
