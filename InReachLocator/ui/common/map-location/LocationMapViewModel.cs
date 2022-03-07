using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace J4JSoftware.InReach
{
    public class LocationMapViewModel : ObservableRecipient
    {
        private MapControl.Location? _mapCenter;
        private double _mapHeight = 500;
        private double _mapWidth = 500;

        protected LocationMapViewModel(
            AnnotatedLocationType.Choices locationTypeChoices,
            IJ4JLogger logger
        )
        {
            LocationTypeChoices = locationTypeChoices;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected override void OnActivated()
        {
            base.OnActivated();

            Messenger.Register<LocationMapViewModel, SizeMessage, string>(this, "mainwindow", WindowSizeChangedHandler);
        }

        private void WindowSizeChangedHandler( LocationMapViewModel recipient, SizeMessage message )
        {
            MapHeight = message.Height - 50;
            MapWidth = message.Width - GridWidth - 10;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            Messenger.UnregisterAll( this );
        }

        public AnnotatedLocationType.Choices LocationTypeChoices { get; }

        public ObservableCollection<MapPoint> MapPoints { get; } = new();

        public bool HasPoints => MapPoints.Any();

        public LocationCollection? Route =>
            new( MapPoints.Where( x => x.SelectedLocationType?.LocationType == LocationType.LinePoint )
                           .Select( x => x.DisplayPoint ) );

        public IEnumerable<MapPoint> Pushpins =>
            MapPoints.Where( x => x.SelectedLocationType?.LocationType == LocationType.Pushpin );

        public virtual void ClearMapLocations()
        {
            MapPoints.Clear();

            OnPropertyChanged(nameof(HasPoints));
            OnPropertyChanged( nameof( Route ) );
            OnPropertyChanged( nameof( Pushpins ) );

            UpdateMapCenter();
        }

        public void AddMapLocation( MapPoint mapPoint )
        {
            MapPoints.Add( mapPoint );

            OnPropertyChanged(nameof(HasPoints));
            OnPropertyChanged( nameof( Pushpins ) );
            OnPropertyChanged( nameof( Route ) );

            UpdateMapCenter();
        }

        public MapControl.Location? MapCenter
        {
            get => _mapCenter;
            set => SetProperty( ref _mapCenter, value );
        }

        private void UpdateMapCenter()
        {
            MapCenter = MapPoints.Count switch
            {
                0 => null,
                1 => MapPoints[ 0 ].DisplayPoint,
                _ => CalculateMapCenter()
            };
        }

        // https://stackoverflow.com/questions/6671183/calculate-the-center-point-of-multiple-latitude-longitude-coordinate-pairs
        // thanx to Gio and Yodacheese for this!
        private MapControl.Location CalculateMapCenter()
        {
            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var mapLocation in MapPoints)
            {
                var latitude = mapLocation.DisplayPoint.Latitude * Math.PI / 180;
                var longitude = mapLocation.DisplayPoint.Longitude * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = MapPoints.Count;

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return new MapControl.Location( centralLatitude, centralLongitude );
        }

        public double GridHeight { get; set; }
        public double GridWidth { get; set; }

        public double MapHeight
        {
            get => _mapHeight;

            set
            {
                if( value < 50 )
                    return;

                SetProperty( ref _mapHeight, value );
            }
        }

        public double MapWidth
        {
            get => _mapWidth;

            set
            {
                if( value < 50 )
                    return;

                SetProperty( ref _mapWidth, value );
            }
        }
    }
}
