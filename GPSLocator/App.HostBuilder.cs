using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.GPSCommon;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Serilog;

namespace J4JSoftware.GPSLocator;

public partial class App
{
    private void InitializeLogger( IConfiguration config, J4JLoggerConfiguration loggerConfig )
    {
        var appPath = Path.GetDirectoryName( AppContext.BaseDirectory );

        if( string.IsNullOrEmpty( appPath ) )
        {
            _buildLogger.Error("Could not determine application's executable path");
            return;
        }

        loggerConfig.SerilogConfiguration
                    .WriteTo.File( System.IO.Path.Combine( appPath, "log.txt" ),
                                   rollingInterval: RollingInterval.Day );
    }

    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
    {
        builder.Register( ( c ) =>
                {
                    AppConfig? retVal = null;

                    try
                    {
                        retVal = hbc.Configuration.Get<AppConfig>();
                    }
                    catch( Exception )
                    {
                        _buildLogger.Error( "Error processing user configuration file, new configuration created" );
                    }

                    var context = new GpsLocatorContext( c.Resolve<IJ4JProtection>(), c.ResolveOptional<IJ4JLogger>() );

                    if( retVal != null )
                    {
                        retVal.Initialize( context );
                        return retVal;
                    }

                    retVal = new AppConfig();
                    retVal.Initialize( context );

                    return retVal;
                } )
               .AsSelf()
               .SingleInstance();

        builder.Register( c =>
                {
                    var dQueue = DispatcherQueue.GetForCurrentThread();
                    return new StatusMessage.StatusMessages( dQueue );
                } )
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<LaunchViewModel>()
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<AppViewModel>()
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

        builder.RegisterType<MessagingViewModel>()
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<LogViewerViewModel>()
               .AsSelf()
               .SingleInstance();
    }

    private void InitializeServices( HostBuilderContext hbc, IServiceCollection services )
    {
    }


    // these next two methods serve to strip the project path off of source code
    // file paths
    private string FilePathTrimmer(
        Type? loggedType,
        string callerName,
        int lineNum,
        string srcFilePath
    )
    {
        return CallingContextEnricher.DefaultFilePathTrimmer( loggedType,
                                                              callerName,
                                                              lineNum,
                                                              CallingContextEnricher.RemoveProjectPath( srcFilePath,
                                                                  GetProjectPath() ) );
    }

    private static string GetProjectPath( [ CallerFilePath ] string filePath = "" )
    {
        var dirInfo = new DirectoryInfo( System.IO.Path.GetDirectoryName( filePath )! );

        while( dirInfo.Parent != null )
        {
            if( dirInfo.EnumerateFiles( "*.csproj" ).Any() )
                break;

            dirInfo = dirInfo.Parent;
        }

        return dirInfo.FullName;
    }
}
