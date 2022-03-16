using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Serilog.Events;

namespace J4JSoftware.InReach
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly AppViewModel _appViewModel;
        private readonly string _userConfigPath;
        private readonly IJ4JLogger _logger;
        private readonly DispatcherQueue _dQueue;

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
            _appViewModel = (App.Current.Resources["AppConfiguration"] as AppViewModel)!;
            _userConfigPath = host.UserConfigurationFiles.First();

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            _dQueue = DispatcherQueue.GetForCurrentThread();

            SaveCommand = new AsyncRelayCommand( SaveHandlerAsync );
            ValidateCommand = new AsyncRelayCommand( ValidateHandlerAsync );
            RevertCommand = new RelayCommand( RevertHandler );

            LogLevels = Enum.GetValues<LogEventLevel>().ToList();
        }

        public void OnLoaded()
        {
            RevertHandler();

            Validated = _appViewModel.Configuration.IsValid;
            InReachConfigChanged = false;
            OtherConfigChanged = false;
        }

        public AsyncRelayCommand SaveCommand { get; }

        private async Task SaveHandlerAsync()
        {
            if( !Validated )
                return;

            _appViewModel.SetStatusMessage("Saving configuration");

            _appViewModel.Configuration.Website = Website;
            _appViewModel.Configuration.UserName = UserName;
            _appViewModel.Configuration.Password = Password;
            _appViewModel.Configuration.IMEI = Imei;
            _appViewModel.Configuration.UseCompassHeadings = CompassHeadings;
            _appViewModel.Configuration.UseImperialUnits = ImperialUnits;
            _appViewModel.Configuration.MinimumLogLevel = MinimumLogLevel;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            jsonOptions.Converters.Add( new JsonStringEnumConverter() );

            var text = JsonSerializer.Serialize( _appViewModel.Configuration, jsonOptions );
            var dirPath = Path.GetDirectoryName( _userConfigPath );

            Directory.CreateDirectory( dirPath! );
            await File.WriteAllTextAsync( _userConfigPath, text );

            _appViewModel.SetStatusMessage("Configuration saved");
            InReachConfigChanged = false;
            OtherConfigChanged = false;
        }

        public AsyncRelayCommand ValidateCommand { get; }

        private async Task ValidateHandlerAsync()
        {
            _appViewModel.SetStatusMessage("Validating configuration" );

            // test the proposed configuration
            var testConfig = new InReachConfig()
            {
                IMEI = Imei, UserName = UserName, Website = Website, Password = Password
            };

            var protector = App.Current.Host.Services.GetRequiredService<IJ4JProtection>();
            testConfig.Initialize( protector, _logger );

            Validated = await testConfig.ValidateAsync( RequestStarted, RequestEnded );

            if ( Validated )
                _appViewModel.SetStatusMessage("Ready");
            else
                _appViewModel.SetStatusMessage(( testConfig.ValidationState & ValidationState.CredentialsValid )
                                            == ValidationState.CredentialsValid
                                                   ? "Invalid IMEI"
                                                   : "Invalid user name and/or password",
                                               StatusMessageType.Urgent );
        }

        private void RequestStarted(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue(() =>
            {
                _appViewModel.SetStatusMessage("Validating");
                _appViewModel.IndeterminateVisibility = Visibility.Visible;
            });
        }

        private void RequestEnded(object? sender, EventArgs e)
        {
            _dQueue.TryEnqueue(() => { _appViewModel.IndeterminateVisibility = Visibility.Collapsed; });
        }

        public RelayCommand RevertCommand { get; }

        private void RevertHandler()
        {
            Website = _appViewModel.Configuration.Website;
            UserName = _appViewModel.Configuration.UserName;
            Password = _appViewModel.Configuration.Password;
            Imei = _appViewModel.Configuration.IMEI;
            Validated = _appViewModel.Configuration.IsValid;
            CompassHeadings = _appViewModel.Configuration.UseCompassHeadings;
            ImperialUnits = _appViewModel.Configuration.UseImperialUnits;
            MinimumLogLevel = _appViewModel.Configuration.MinimumLogLevel;

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
