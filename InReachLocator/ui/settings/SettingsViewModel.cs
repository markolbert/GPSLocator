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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;

namespace J4JSoftware.InReach
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly AppConfig _appConfig;
        private readonly string _userConfigPath;
        private readonly IJ4JLogger _logger;

        private string _website = string.Empty;
        private string _userName = string.Empty;
        private string? _password;
        private string _imei = string.Empty;
        private bool _validated;
        private bool _changed;

        public SettingsViewModel(
            IJ4JHost host,
            IJ4JLogger logger
        )
        {
            _appConfig = (App.Current.Resources["AppConfiguration"] as AppConfig)!;
            _userConfigPath = host.UserConfigurationFiles.First();

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            SaveCommand = new AsyncRelayCommand( SaveHandlerAsync );
            ValidateCommand = new AsyncRelayCommand( ValidateHandlerAsync );
            RevertCommand = new RelayCommand( RevertHandler );
        }

        public void OnLoaded()
        {
            Website = _appConfig.InReachConfig.Website;
            UserName = _appConfig.InReachConfig.UserName;
            Password = _appConfig.InReachConfig.Password;
            Imei = _appConfig.InReachConfig.IMEI;

            Validated = _appConfig.InReachConfig.IsValid;
            Changed = false;
        }

        public AsyncRelayCommand SaveCommand { get; }

        private async Task SaveHandlerAsync()
        {
            if( !Validated )
                return;

            _appConfig.InReachConfig.Website = Website;
            _appConfig.InReachConfig.UserName = UserName;
            _appConfig.InReachConfig.Password = Password;
            _appConfig.InReachConfig.IMEI = Imei;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            var text = JsonSerializer.Serialize( _appConfig.InReachConfig, jsonOptions );
            var dirPath = Path.GetDirectoryName( _userConfigPath );

            Directory.CreateDirectory( dirPath! );
            await File.WriteAllTextAsync( _userConfigPath, text );

            StatusMessage.Send("Configuration saved");
        }

        public AsyncRelayCommand ValidateCommand { get; }

        private async Task ValidateHandlerAsync()
        {
            // test the proposed configuration
            var testConfig = new InReachConfig()
            {
                IMEI = Imei, UserName = UserName, Website = Website, Password = Password
            };

            var protector = App.Current.Host.Services.GetRequiredService<IJ4JProtection>();
            testConfig.Initialize( protector, _logger );

            Validated = await testConfig.ValidateAsync();

            if( Validated )
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
            Website = _appConfig.InReachConfig.Website;
            UserName = _appConfig.InReachConfig.UserName;
            Password = _appConfig.InReachConfig.Password;
            Validated = _appConfig.InReachConfig.IsValid;
            Changed = true;
        }

        public string Website
        {
            get => _website;

            set
            {
                Changed = !string.Equals( _website, value, StringComparison.OrdinalIgnoreCase );

                SetProperty( ref _website, value );

                Validated = false;
            }
        }

        public string UserName
        {
            get => _userName;

            set
            {
                Changed = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

                SetProperty( ref _userName, value );

                Validated = false;
            }
        }

        public string? Password
        {
            get => _password;

            set
            {
                Changed = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

                SetProperty( ref _password, value );

                Validated = false;
            }
        }

        public string Imei
        {
            get => _imei;
            
            set
            {
                Changed = !string.Equals(_website, value, StringComparison.OrdinalIgnoreCase);

                SetProperty( ref _imei, value );
                
                Validated = false;
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

        public bool Changed
        {
            get => _changed;

            set
            {
                SetProperty( ref _changed, value );
                OnPropertyChanged(nameof(CanSave));
            }
        }

        public bool CanSave => Validated && Changed;
    }
}
