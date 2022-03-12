using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ABI.Windows.Devices.AllJoyn;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Serilog.Events;

namespace J4JSoftware.InReach
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly AppConfigViewModel _appConfigViewModel;
        private readonly string _userConfigPath;
        private readonly IJ4JLogger _logger;

        private string _website = string.Empty;
        private string _userName = string.Empty;
        private string? _password;
        private string _imei = string.Empty;
        private bool _compassHeadings;
        private bool _imperialUnits;
        private LogEventLevel _minEventLevel;
        private bool _validated;
        private bool _inReachConfigChanged;
        private bool _otherConfigChanged;

        public SettingsViewModel(
            IJ4JHost host,
            IJ4JLogger logger
        )
        {
            _appConfigViewModel = (App.Current.Resources["AppConfiguration"] as AppConfigViewModel)!;
            _userConfigPath = host.UserConfigurationFiles.First();

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            SaveCommand = new AsyncRelayCommand( SaveHandlerAsync );
            ValidateCommand = new AsyncRelayCommand( ValidateHandlerAsync );
            RevertCommand = new RelayCommand( RevertHandler );

            LogLevels = Enum.GetValues<LogEventLevel>().ToList();
        }

        public void OnLoaded()
        {
            RevertHandler();

            Validated = _appConfigViewModel.Configuration.IsValid;
            InReachConfigChanged = false;
            OtherConfigChanged = false;
        }

        public AsyncRelayCommand SaveCommand { get; }

        private async Task SaveHandlerAsync()
        {
            if( !Validated )
                return;

            StatusMessage.Send("Saving configuration");

            _appConfigViewModel.Configuration.Website = Website;
            _appConfigViewModel.Configuration.UserName = UserName;
            _appConfigViewModel.Configuration.Password = Password;
            _appConfigViewModel.Configuration.IMEI = Imei;
            _appConfigViewModel.Configuration.UseCompassHeadings = CompassHeadings;
            _appConfigViewModel.Configuration.UseImperialUnits = ImperialUnits;
            _appConfigViewModel.Configuration.MinimumLogLevel = MinimumLogLevel;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            jsonOptions.Converters.Add( new JsonStringEnumConverter() );

            var text = JsonSerializer.Serialize( _appConfigViewModel.Configuration, jsonOptions );
            var dirPath = Path.GetDirectoryName( _userConfigPath );

            Directory.CreateDirectory( dirPath! );
            await File.WriteAllTextAsync( _userConfigPath, text );

            StatusMessage.Send("Configuration saved");
            InReachConfigChanged = false;
            OtherConfigChanged = false;
        }

        public AsyncRelayCommand ValidateCommand { get; }

        private async Task ValidateHandlerAsync()
        {
            var pBar = StatusMessage.SendWithIndeterminateProgressBar( "Validating configuration" );

            // test the proposed configuration
            var testConfig = new InReachConfig()
            {
                IMEI = Imei, UserName = UserName, Website = Website, Password = Password
            };

            var protector = App.Current.Host.Services.GetRequiredService<IJ4JProtection>();
            testConfig.Initialize( protector, _logger );

            Validated = await testConfig.ValidateAsync();

            ProgressBarMessage.EndProgressBar(pBar);

            if ( Validated )
                StatusMessage.Send("Ready");
            else
                StatusMessage.Send( ( testConfig.ValidationState & ValidationState.CredentialsValid )
                                 == ValidationState.CredentialsValid
                                        ? "Invalid IMEI"
                                        : "Invalid user name and/or password",
                                    StatusMessageType.Urgent );
        }

        public RelayCommand RevertCommand { get; }

        private void RevertHandler()
        {
            Website = _appConfigViewModel.Configuration.Website;
            UserName = _appConfigViewModel.Configuration.UserName;
            Password = _appConfigViewModel.Configuration.Password;
            Imei = _appConfigViewModel.Configuration.IMEI;
            Validated = _appConfigViewModel.Configuration.IsValid;
            CompassHeadings = _appConfigViewModel.Configuration.UseCompassHeadings;
            ImperialUnits = _appConfigViewModel.Configuration.UseImperialUnits;
            MinimumLogLevel = _appConfigViewModel.Configuration.MinimumLogLevel;

            InReachConfigChanged = true;
        }

        public string Website
        {
            get => _website;

            set
            {
                InReachConfigChanged = !string.Equals( _website, value, StringComparison.OrdinalIgnoreCase );

                SetProperty( ref _website, value );

                Validated = false;
            }
        }

        public string UserName
        {
            get => _userName;

            set
            {
                InReachConfigChanged = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

                SetProperty( ref _userName, value );

                Validated = false;
            }
        }

        public string? Password
        {
            get => _password;

            set
            {
                InReachConfigChanged = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

                SetProperty( ref _password, value );

                Validated = false;
            }
        }

        public string Imei
        {
            get => _imei;
            
            set
            {
                InReachConfigChanged = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

                SetProperty( ref _imei, value );
                
                Validated = false;
            }
        }

        public bool CompassHeadings
        {
            get => _compassHeadings;

            set
            {
                OtherConfigChanged = _compassHeadings != value;
                SetProperty( ref _compassHeadings, value );
            }
        }

        public bool ImperialUnits
        {
            get => _imperialUnits;

            set
            {
                OtherConfigChanged = _imperialUnits != value;
                SetProperty( ref _imperialUnits, value );
            }
        }

        public List<LogEventLevel> LogLevels { get; }

        public LogEventLevel MinimumLogLevel
        {
            get => _minEventLevel;

            set
            {
                OtherConfigChanged = _minEventLevel != value;
                SetProperty( ref _minEventLevel, value );
            }
        }

        public bool Validated
        {
            get => _validated;
            set
            {
                SetProperty( ref _validated, value );
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public bool InReachConfigChanged
        {
            get => _inReachConfigChanged;

            set
            {
                SetProperty( ref _inReachConfigChanged, value );
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public bool OtherConfigChanged
        {
            get => _otherConfigChanged;

            set
            {
                SetProperty( ref _otherConfigChanged, value );
                OnPropertyChanged( nameof( CanSave ) );
            }
        }

        public bool CanSave => Validated && (InReachConfigChanged || OtherConfigChanged);
    }
}
