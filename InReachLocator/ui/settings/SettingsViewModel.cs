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

            SaveAndCloseCommand = new AsyncRelayCommand( SaveAndCloseHandler );
            CloseCommand = new RelayCommand( CloseHandler );
            RefreshCommand = new RelayCommand( RefreshHandler );
        }

        public AsyncRelayCommand SaveAndCloseCommand { get; }

        private async Task SaveAndCloseHandler()
        {
            // test the proposed configuration
            var testConfig = new InReachConfig
            {
                IMEI = IMEI,
                UserName = UserName,
                Website = Website,
                Password = { ClearText = Password }
            };

            if ( !await testConfig.ValidateConfiguration( _logger ) )
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

            WeakReferenceMessenger.Default.Send( new ConfigurationChangedMessage( true ), "primary" );

            App.Current.PopContentControl();
        }

        public RelayCommand CloseCommand { get; }

        private void CloseHandler()
        {
            StrongReferenceMessenger.Default.Send<ConfigurationChangedMessage, string>(
                new ConfigurationChangedMessage(false),
                "primary");

            App.Current.PopContentControl();
        }

        public RelayCommand RefreshCommand { get; }

        private void RefreshHandler()
        {
            Website = _config.Website;
            UserName = _config.UserName;
            Password = _config.Password.ClearText;
        }

        public string Website
        {
            get => _website;
            set => SetProperty( ref _website, value );
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty( ref _userName, value );
        }

        public string? Password
        {
            get => _password;
            set=> SetProperty( ref _password, value );
        }

        public string IMEI
        {
            get => _imei;
            set => SetProperty(ref _imei, value);
        }
    }
}
