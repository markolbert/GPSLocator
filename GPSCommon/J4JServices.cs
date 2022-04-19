using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.WindowsAppUtilities;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS8618

namespace J4JSoftware.GPSCommon
{
    public static class J4JServices
    {
        public static IServiceProvider Default { get; private set; }

        public static IJ4JLogger? BuildLogger;
        public static readonly string CrashFile;
        public static bool IsValid = false;

        private static J4JWinAppHostConfiguration? _hostConfiguration;

        static J4JServices()
        {
            CrashFile = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "crashFile.txt");
        }

        public static bool Initialize( J4JWinAppHostConfiguration? appHostConfig )
        {
            _hostConfiguration = appHostConfig;

            if( _hostConfiguration == null )
            {
                OutputFatalMessage($"{typeof(J4JWinAppHostConfiguration)} is undefined");
                return false;
            }

            BuildLogger = _hostConfiguration.Logger;

            if( _hostConfiguration.MissingRequirements != J4JHostRequirements.AllMet )
            {
                OutputFatalMessage( $"Missing J4JHostConfiguration items: {_hostConfiguration.MissingRequirements}" );
                return false;
            }

            var host = _hostConfiguration.Build();

            if( host != null )
            {
                Default = host.Services;
                IsValid = true;
                BuildLogger = null;

                return true;
            }

            OutputFatalMessage( $"Could not create {typeof( J4JWinAppHostConfiguration )}" );

            return true;
        }

        public static void OutputFatalMessage( string msg )
        {
            // how we log depends on whether we successfully created the service provider
            var logger = IsValid ? Default.GetRequiredService<IJ4JLogger>() : BuildLogger;
            logger?.Fatal( msg );

            File.AppendAllText( CrashFile, $"{msg}\n" );
        }
    }
}
