using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public class HomeViewModel : ObservableRecipient
    {
        private readonly IInReachConfig _config;
        private readonly HttpClient _httpClient;
        private readonly IJ4JLogger _logger;

        private bool _validConfig;
        private InReachLastKnownLocation? _lastKnownLocation;

        public HomeViewModel(
            IInReachConfig config,
            IJ4JLogger logger
        )
        {
            _config = config;
            ValidConfiguration = _config.IsValid;

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue( "Basic", _config.Credentials );

            LastKnownLocationCommand = new AsyncRelayCommand( LastKnownLocationHandler );
            ConfigureCommand = new RelayCommand( ConfigureHandler );

            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<HomeViewModel, ConfigurationChangedMessage, string>(this, "primary", ConfigurationChangedHandler);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        private void ConfigurationChangedHandler( HomeViewModel recipient, ConfigurationChangedMessage message )
        {
            if( !message.Changed )
                return;

            ValidConfiguration = _config.IsValid;
        }

        public AsyncRelayCommand LastKnownLocationCommand { get; }

        private async Task LastKnownLocationHandler()
        {
            var request = new InReachVersionRequest( _config, _logger );

            var result = await request.ExecuteAsync();
        }

        //private async Task LastKnownLocationHandler()
        //{
        //    var uri = QueryHelpers.AddQueryString(
        //        $"https://{_config.Website}/IPCInbound/V1/Location.svc/LastKnownLocation",
        //        "IMEI",
        //        _config.IMEI);

        //    var response = await _httpClient.GetAsync(uri);

        //    if (!response.IsSuccessStatusCode)
        //        _logger?.Error<string?>("Last known location request failed ({0})", response.ReasonPhrase);

        //    var jsonText = await response.Content.ReadAsStringAsync();

        //    if (response.IsSuccessStatusCode)
        //        LastKnownLocation = JsonSerializer.Deserialize<InReachLastKnownLocation>(jsonText);
        //}

        public RelayCommand ConfigureCommand { get; }

        private void ConfigureHandler()
        {
            App.Current.SetContentControl( new SettingsControl(),
                                           x =>
                                           {
                                               x.HorizontalAlignment = HorizontalAlignment.Center;
                                               x.VerticalAlignment = VerticalAlignment.Center;
                                           } );
        }

        public bool ValidConfiguration
        {
            get => _validConfig;
            set => SetProperty( ref _validConfig, value );
        }

        public InReachLastKnownLocation? LastKnownLocation
        {
            get => _lastKnownLocation;
            set => SetProperty( ref _lastKnownLocation, value );
        }
    }
}
