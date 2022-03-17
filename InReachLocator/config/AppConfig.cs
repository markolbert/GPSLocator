using Serilog.Events;

namespace J4JSoftware.InReach
{
    public class AppConfig : InReachConfig
    {
        private bool _useImperial;
        private bool _useCompass;
        private LogEventLevel _minLevel = LogEventLevel.Verbose;
        private string _helpLink = string.Empty;
        private string? _launchPage;

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

        public string HelpLink
        {
            get => _helpLink;
            set => SetProperty( ref _helpLink, value );
        }

        public string? LaunchPage
        {
            get => _launchPage;
            set => SetProperty( ref _launchPage, value );
        }
    }
}
