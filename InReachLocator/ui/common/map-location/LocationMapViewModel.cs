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
        private readonly ObservableCollection<IMapLocation>
        private MapControl.Location? _mapCenter;

        private double _mapHeight = 500;
        private double _mapWidth = 500;

        protected LocationMapViewModel(
            IJ4JLogger logger
        )
        {
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

        public ObservableCollection<IMapLocation> MapLocations { get; } = new();

        public LocationCollection? Polyline =>
            new( MapLocations.Where( x => x.LocationType == LocationType.LinePoint )
                           .Select( x => x.MapPoint ) );

        public IEnumerable<MapItem> Pushpins => MapLocations.Where( x => x.LocationType == LocationType.Pushpin )
                                                             .Select(x=>new MapItem()  );

        public virtual void ClearMapLocations()
        {
            MapLocations.Clear();
            OnPropertyChanged( nameof( Polyline ) );
            OnPropertyChanged( nameof( Pushpins ) );

            UpdateMapCenter();
        }

        public void AddMapLocation( IMapLocation mapLocation )
        {
            MapLocations.Add( mapLocation );

            OnPropertyChanged( nameof( Pushpins ) );
            OnPropertyChanged( nameof( Polyline ) );

            UpdateMapCenter();
        }

        public MapControl.Location? MapCenter
        {
            get => _mapCenter;
            set => SetProperty( ref _mapCenter, value );
        }

        private void UpdateMapCenter()
        {
            MapCenter = MapLocations.Count switch
            {
                0 => null,
                1 => MapLocations[ 0 ].MapPoint,
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

            foreach (var mapLocation in MapLocations)
            {
                var latitude = mapLocation.MapPoint.Latitude * Math.PI / 180;
                var longitude = mapLocation.MapPoint.Longitude * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }

            var total = MapLocations.Count;

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
