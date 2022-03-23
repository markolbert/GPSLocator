using System;
using System.Reflection;
using Serilog.Events;

namespace J4JSoftware.GPSLocator
{
    public class AppConfig : DeviceConfig
    {
        private bool _useImperial;
        private bool _useCompass;
        private LogEventLevel _minLevel = LogEventLevel.Verbose;
        private string? _launchPage;
        private string? _defaultCallback;
        private int _defaultDaysBack;

        public AppConfig()
        {
            AppVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }

        public Version? AppVersion { get; }

        public bool UseImperialUnits
        {
            get=> _useImperial;
            set => SetProperty( ref _useImperial, value );
        }

        public bool UseCompassHeadings
        {
            get => _useCompass;
            set=> SetProperty( ref _useCompass, value);
        }

        public LogEventLevel MinimumLogLevel
        {
            get => _minLevel;
            set => SetProperty( ref _minLevel, value );
        }

        public string? LaunchPage
        {
            get => _launchPage;
            set => SetProperty( ref _launchPage, value );
        }

        public string? DefaultCallback
        {
            get => _defaultCallback;
            set => SetProperty( ref _defaultCallback, value );
        }

        public int DefaultDaysBack
        {
            get => _defaultDaysBack;
            set => SetProperty( ref _defaultDaysBack, value );
        }

        #region Application-wide settings

        public string HelpLink { get; set; } = string.Empty;

        public int MaxSmsLength { get; set; }

        #endregion
    }
}
