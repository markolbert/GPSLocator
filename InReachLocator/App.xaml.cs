using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Graphics.Printing;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Serilog;
using Path = ABI.Microsoft.UI.Xaml.Shapes.Path;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace J4JSoftware.InReach
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;

        private readonly IJ4JLogger _buildLogger;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            //Register Syncfusion license
            Syncfusion.Licensing
                      .SyncfusionLicenseProvider
                      .RegisterLicense("NTg2NTM0QDMxMzkyZTM0MmUzMGpmNUlrRmd6WXdHenpRd0thSTZDeDA4SW0xV1NJZGJuRUZNWDhWVnR0YkE9");

            var hostConfig = new J4JHostConfiguration()
                            .Publisher( "J4JSoftware" )
                            .ApplicationName( "InReachLocator" )
                            .LoggerInitializer( InitializeLogger )
                            .AddNetEventSinkToLogger()
                            .AddDependencyInjectionInitializers( SetupDependencyInjection )
                            .AddServicesInitializers( InitializeServices )
                            .AddUserConfigurationFile( "userConfig.json", optional: true, reloadOnChange: true )
                            .FilePathTrimmer( FilePathTrimmer );

            _buildLogger = hostConfig.Logger;

            if (hostConfig.MissingRequirements != J4JHostRequirements.AllMet)
                throw new ApplicationException($"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}");

            Host = hostConfig.Build()
             ?? throw new NullReferenceException($"Failed to build {nameof(IJ4JHost)}");

            var logger = Host.Services.GetRequiredService<IJ4JLogger>();
            logger.OutputCache(hostConfig.Logger);
        }

        public MainWindow? MainWindow { get; private set; }
        public IntPtr MainWindowIntPtr { get; private set; }
        public WindowId MainWindowId { get; private set; }

        public IJ4JHost Host { get; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow() { Title = "InReach Locator" };

            MainWindowIntPtr = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            MainWindowId = Win32Interop.GetWindowIdFromWindow(MainWindowIntPtr);

            var appWindow = AppWindow.GetFromWindowId(MainWindowId);

            var winSize = appWindow.Size;
            winSize.Height = winSize.Height > 720 ? 720 : winSize.Height;
            winSize.Width = winSize.Width > 1000 ? 1000 : winSize.Width;

            appWindow.Resize(winSize);

            MainWindow.Activate();
        }

        private void InitializeLogger(IConfiguration config, J4JLoggerConfiguration loggerConfig)
        {
            loggerConfig.SerilogConfiguration
                        .WriteTo.File(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "log.txt"),
                                      rollingInterval: RollingInterval.Day);
        }

        private void SetupDependencyInjection(HostBuilderContext hbc, ContainerBuilder builder)
        {
            builder.Register( ( c ) =>
                    {
                        AppConfig? retVal = null;

                        try
                        {
                            retVal = hbc.Configuration.Get<AppConfig>() ?? new AppConfig();
                        }
                        catch( Exception )
                        {
                            _buildLogger.Error("Error processing user configuration file, new configuration created");
                        }

                        retVal ??= new AppConfig();

                        retVal.Initialize( c.Resolve<IJ4JProtection>(), c.Resolve<IJ4JLogger>() );

                        return retVal;
                    } )
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<MainViewModel>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<SettingsViewModel>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<LastKnownViewModel>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<HistoryViewModel>()
                   .AsSelf()
                   .SingleInstance();

            builder.RegisterType<LogViewerViewModel>()
                   .AsSelf()
                   .SingleInstance();
        }

        private void InitializeServices(HostBuilderContext hbc, IServiceCollection services)
        {
        }

        // these next two methods serve to strip the project path off of source code
        // file paths
        private string FilePathTrimmer(Type? loggedType,
                                              string callerName,
                                              int lineNum,
                                              string srcFilePath)
        {
            return CallingContextEnricher.DefaultFilePathTrimmer(loggedType,
                                                                 callerName,
                                                                 lineNum,
                                                                 CallingContextEnricher.RemoveProjectPath(srcFilePath,
                                                                  GetProjectPath()));
        }

        private static string GetProjectPath([CallerFilePath] string filePath = "")
        {
            var dirInfo = new DirectoryInfo(System.IO.Path.GetDirectoryName(filePath)!);

            while (dirInfo.Parent != null)
            {
                if (dirInfo.EnumerateFiles("*.csproj").Any())
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}
