using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public class LastKnownViewModel : ObservableRecipient
    {
        private readonly IJ4JLogger _logger;

        private LocationViewModel? _locationViewModel;
        private string? _locationUrl;
        private double _mapHeight = 500;
        private double _mapWidth = 500;

        public LastKnownViewModel(
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( GetType() );

            IsActive = true;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<LastKnownViewModel, Location, string>(this, "primary", NewLocationHandler);
            Messenger.Register<LastKnownViewModel, SizeMessage, string>(this, "mainwindow", WindowSizeChangedHandler);
        }

        private void WindowSizeChangedHandler( LastKnownViewModel recipient, SizeMessage message )
        {
            MapHeight = message.Height - 50;
            MapWidth = message.Width - GridWidth - 10;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        private void NewLocationHandler( LastKnownViewModel recipient, Location message )
        {
            LocationViewModel = new LocationViewModel( message, _logger );

            LocationUrl = $"https://maps.google.com?q={LocationViewModel.Latitude},{LocationViewModel.Longitude}";
        }

        public LocationViewModel? LocationViewModel
        {
            get => _locationViewModel;
            private set => SetProperty( ref _locationViewModel, value );
        }

        public string? LocationUrl
        {
            get => _locationUrl;
            set => SetProperty( ref _locationUrl, value );
        }

        public double GridHeight { get; set; }
        public double GridWidth { get; set; }

        public double MapHeight
        {
            get => _mapHeight;
            set=> SetProperty( ref _mapHeight, value );
        }

        public double MapWidth
        {
            get => _mapWidth;
            set => SetProperty(ref _mapWidth, value);
        }

    }
}
