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
using Microsoft.UI.Dispatching;

namespace J4JSoftware.InReach
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly IInReachConfig _config;
        private readonly IJ4JHost _host;
        private readonly IJ4JLogger _logger;

        private string _website;
        private string _userName;
        private string? _password;
        private string _imei;
        private bool _validated;
        private bool _changed;

        public SettingsViewModel(
            IInReachConfig config,
            IJ4JHost host,
            IJ4JLogger logger
        )
        {
            _config = config;
            _host = host;

            _website = _config.Website;
            _userName = _config.UserName;
            _password = _config.Password.ClearText;
            _imei = _config.IMEI;

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            var dQueue = DispatcherQueue.GetForCurrentThread();

            dQueue.TryEnqueue( async () =>
            {
                Validated = await _config.ValidateConfiguration( _logger );
            } );

            SaveCommand = new AsyncRelayCommand( SaveHandlerAsync );
            ValidateCommand = new AsyncRelayCommand( ValidateHandlerAsync );
            RevertCommand = new RelayCommand( RevertHandler );
        }

        public AsyncRelayCommand SaveCommand { get; }

        private async Task SaveHandlerAsync()
        {
            if( !Validated )
                return;

            _config.Website = Website;
            _config.UserName = UserName;
            _config.Password.ClearText = Password;
            _config.IMEI = IMEI;

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

            var text = JsonSerializer.Serialize( _config, jsonOptions );
            var filePath = _host.UserConfigurationFiles.First();
            var dirPath = Path.GetDirectoryName( filePath );

            Directory.CreateDirectory( dirPath! );
            await File.WriteAllTextAsync( filePath, text );
        }

        public AsyncRelayCommand ValidateCommand { get; }

        private async Task ValidateHandlerAsync()
        {
            // test the proposed configuration
            var testConfig = new InReachConfig
            {
                IMEI = IMEI,
                UserName = UserName,
                Website = Website,
                Password = { ClearText = Password }
            };

            Validated = await testConfig.ValidateConfiguration( _logger );
        }

        public RelayCommand RevertCommand { get; }

        private void RevertHandler()
        {
            Website = _config.Website;
            UserName = _config.UserName;
            Password = _config.Password.ClearText;

            Validated = false;
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

        public string IMEI
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
