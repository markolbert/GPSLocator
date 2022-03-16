using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public class LocationMapViewModel : BaseViewModel
    {
        private readonly List<MapPoint> _locations = new();

        private MapControl.Location? _mapCenter;
        private bool _deferUpdatingMapCenter;
        private int _zoomLevel = 17;
        private bool _refreshEnabled;

        protected LocationMapViewModel(
            IJ4JLogger logger
        )
            : base( logger )
        {
            RefreshCommand = new AsyncRelayCommand(RefreshHandlerAsync);
            IncreaseZoomCommand = new RelayCommand(IncreaseZoomHandler);
            DecreaseZoomCommand = new RelayCommand(DecreaseZoomHandler);
        }

        public AsyncRelayCommand RefreshCommand { get; }

        protected virtual Task RefreshHandlerAsync()
        {
            return Task.CompletedTask;
        }

        public bool RefreshEnabled
        {
            get => _refreshEnabled;
            set => SetProperty(ref _refreshEnabled, value);
        }

        public ObservableCollection<MapPoint> AllPoints { get; } = new();
        public ObservableCollection<MapPoint> MappedPoints { get; } = new();
        public bool MapHasPoints => MappedPoints.Any( x => x.DisplayOnMap == MapPointDisplay.Fixed );

        protected MapPoint AddLocation( ILocation location )
        {
            var mapPoint = new MapPoint( location );
            mapPoint.Display += ChangeMapPointDisplay;

            _locations.Add( mapPoint );
            AllPoints.Add( mapPoint );

            return mapPoint;
        }

        protected void AddLocations( IEnumerable<ILocation> locations, bool clearList = true )
        {
            _deferUpdatingMapCenter = true;

            if( clearList )
            {
                _locations.Clear();
                MappedPoints.Clear();
                AllPoints.Clear();

                OnPropertyChanged( nameof( MapHasPoints ) );
            }

            foreach( var location in locations )
            {
                AddLocation( location );
            }

            UpdateMapCenter();

            _deferUpdatingMapCenter = false;
        }

        private void ChangeMapPointDisplay( object? sender, MapPointDisplay displayPoint )
        {
            if( sender is not MapPoint mapPoint
            || _deferUpdatingMapCenter
            || ( mapPoint.InReachLocation.Coordinate.Latitude == 0
                && mapPoint.InReachLocation.Coordinate.Longitude == 0 ) )
                return;

            var ptIdx = MappedPoints.IndexOf( mapPoint );
            if( ptIdx < 0 )
            {
                if( displayPoint == MapPointDisplay.DoNotDisplay )
                    return;

                MappedPoints.Add( mapPoint );
                OnPropertyChanged(nameof(MapHasPoints));

                UpdateMapCenter();
            }
            else
            {
                if( displayPoint != MapPointDisplay.DoNotDisplay )
                    return;

                MappedPoints.RemoveAt( ptIdx );
                OnPropertyChanged(nameof(MapHasPoints));

                UpdateMapCenter();
            }
        }

        protected virtual void ClearMappedPoints()
        {
            _deferUpdatingMapCenter = true;

            MappedPoints.Clear();
            _locations.ForEach( x => x.DisplayOnMap = MapPointDisplay.DoNotDisplay );

            _deferUpdatingMapCenter = false;
            UpdateMapCenter();
        }

        public MapControl.Location? MapCenter
        {
            get => _mapCenter;
            set => SetProperty( ref _mapCenter, value );
        }

        public int ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public RelayCommand IncreaseZoomCommand { get; }

        private void IncreaseZoomHandler()
        {
            if (_zoomLevel >= 21)
                return;

            ZoomLevel++;
        }

        public RelayCommand DecreaseZoomCommand { get; }

        private void DecreaseZoomHandler()
        {
            if (_zoomLevel <= 2)
                return;

            ZoomLevel--;
        }

        protected void UpdateMapCenter()
        {
            MapCenter = MappedPoints.Count switch
            {
                0 => MapCenter,
                1 => MappedPoints[ 0 ].DisplayPoint,
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

            foreach( var mappedPoint in MappedPoints )
            {
                var latitude = mappedPoint.DisplayPoint.Latitude * Math.PI / 180;
                var longitude = mappedPoint.DisplayPoint.Longitude * Math.PI / 180;

                x += Math.Cos( latitude ) * Math.Cos( longitude );
                y += Math.Cos( latitude ) * Math.Sin( longitude );
                z += Math.Sin( latitude );
            }

            var total = MappedPoints.Count;

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return new MapControl.Location( centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI );
        }
    }
}
