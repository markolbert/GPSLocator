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

        private bool _imperialUnits;
        private DateTime _timestamp;
        private double _latitude;
        private double _longitude;
        private double _altitude;
        private double _speed;
        private long _course;
        private int _gpsFixStatus;
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
            Timestamp = message.Timestamp;
            Latitude = message.Coordinate!.Latitude;
            Longitude = message.Coordinate!.Longitude;
            Altitude = message.Altitude;
            Speed = message.Speed;
            Course = message.Course;
            GPSFixStatus = message.GPSFixStatus;

            LocationUrl = $"https://maps.google.com?q={Latitude},{Longitude}";
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty( ref _timestamp, value );
        }

        public double Latitude
        {
            get => _latitude;
            set=> SetProperty( ref _latitude, value );
        }

        public double Longitude
        {
            get => _longitude;
            set=> SetProperty( ref _longitude, value );
        }

        public double Altitude
        {
            get => ConvertToImperial( _altitude, App.FeetPerMeter );
            set=> SetProperty( ref _altitude, value );
        }

        public double Speed
        {
            get => ConvertToImperial( _speed, App.MPHperKMH );
            set => SetProperty( ref _speed, value );
        }

        public long Course
        {
            get => _course;
            set => SetProperty( ref _course, value );
        }

        public int GPSFixStatus
        {
            get => _gpsFixStatus;
            set=> SetProperty( ref _gpsFixStatus, value );
        }

        public bool ImperialUnits
        {
            get => _imperialUnits;

            set
            {
                SetProperty( ref _imperialUnits, value );

                OnPropertyChanged( nameof( Speed ) );
                OnPropertyChanged(nameof(SpeedUnits));
                OnPropertyChanged( nameof( Altitude ) );
                OnPropertyChanged( nameof( AltitudeUnits ) );
            }
        }

        private double ConvertToImperial( double metric, double factor ) =>
            Math.Round( metric * ( _imperialUnits ? factor : 1 ), 0 );

        public string? LocationUrl
        {
            get => _locationUrl;
            set => SetProperty( ref _locationUrl, value );
        }

        public string AltitudeUnits => _imperialUnits ? "feet" : "meters";
        public string SpeedUnits => _imperialUnits ? "mph" : "km/h";

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
