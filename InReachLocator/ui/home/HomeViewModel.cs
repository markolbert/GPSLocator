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
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace J4JSoftware.InReach
{
    public class HomeViewModel : ObservableRecipient
    {
        private readonly IInReachConfig _config;
        private readonly IJ4JLogger _logger;

        private bool _validConfig;

        public HomeViewModel(
            IInReachConfig config,
            IJ4JLogger logger
        )
        {
            _config = config;

            _logger = logger;
            _logger.SetLoggedType( GetType() );

            var dQueue = DispatcherQueue.GetForCurrentThread();

            dQueue.TryEnqueue( async () =>
            {
                ValidConfiguration = await _config.ValidateConfiguration(_logger);
            });

            LastKnownLocationCommand = new AsyncRelayCommand( LastKnownLocationHandler );
            HistoryCommand = new RelayCommand( HistoryHandler );
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

            ValidConfiguration = _config.ValidateConfiguration( _logger ).Result;
        }

        public AsyncRelayCommand LastKnownLocationCommand { get; }

        private async Task LastKnownLocationHandler()
        {
            var request = new LastKnownLocationRequest(_config, _logger);
            var result = await request.ExecuteAsync();

            if (result == null || result.Locations.Count == 0)
            {
                if (request.LastError != null)
                    _logger.Error<string>("Invalid configuration, message was '{0}'", request.LastError.ToString());
                else _logger.Error("Invalid configuration");

                return;
            }

            App.Current.SetContentControl(new LastKnownControl(),
                                          x =>
                                          {
                                              x.HorizontalAlignment = HorizontalAlignment.Left;
                                              x.VerticalAlignment = VerticalAlignment.Top;
                                          });

            WeakReferenceMessenger.Default.Send( result.Locations[ 0 ], "primary" );
        }

        public RelayCommand HistoryCommand { get; }

        private void HistoryHandler()
        {
            App.Current.SetContentControl(new HistoryControl(),
                                          x =>
                                          {
                                              x.HorizontalAlignment = HorizontalAlignment.Left;
                                              x.VerticalAlignment = VerticalAlignment.Top;
                                          });
        }

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
    }
}
