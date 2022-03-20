using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace J4JSoftware.GPSLocator;

public partial class App
{
    private void InitializeLogger( IConfiguration config, J4JLoggerConfiguration loggerConfig )
    {
        loggerConfig.SerilogConfiguration
                    .WriteTo.File( System.IO.Path.Combine( Directory.GetCurrentDirectory(), "log.txt" ),
                                   rollingInterval: RollingInterval.Day );
    }

    private void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
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
                        _buildLogger.Error( "Error processing user configuration file, new configuration created" );
                    }

                    retVal ??= new AppConfig();

                    retVal.Initialize( c.Resolve<IJ4JProtection>(), c.Resolve<IJ4JLogger>() );

                    return retVal;
                } )
               .AsSelf()
               .SingleInstance();

        builder.RegisterType<LaunchViewModel>()
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
