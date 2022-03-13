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

        protected LocationMapViewModel(
            IJ4JLogger logger
        )
            : base( logger )
        {
        }

        public ObservableCollection<MapPoint> AllPoints { get; } = new();
        public ObservableCollection<MapPoint> MappedPoints { get; } = new();

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
            }

            foreach( var location in locations )
            {
                AddLocation( location );
            }

            UpdateMapCenter();

            _deferUpdatingMapCenter = false;
        }

        private void ChangeMapPointDisplay( object? sender, bool displayPoint )
        {
            if( sender is not MapPoint mapPoint || _deferUpdatingMapCenter )
                return;

            var ptIdx = MappedPoints.IndexOf( mapPoint );
            if( ptIdx < 0 )
            {
                if( !displayPoint )
                    return;

                MappedPoints.Add( mapPoint );
                UpdateMapCenter();
            }
            else
            {
                if( displayPoint )
                    return;

                MappedPoints.RemoveAt( ptIdx );
                UpdateMapCenter();
            }
        }

        protected virtual void ClearMappedPoints()
        {
            _deferUpdatingMapCenter = true;

            MappedPoints.Clear();
            _locations.ForEach( x => x.DisplayOnMap = false );

            _deferUpdatingMapCenter = false;
            UpdateMapCenter();
        }

        public MapControl.Location? MapCenter
        {
            get => _mapCenter;
            set => SetProperty( ref _mapCenter, value );
        }

        protected void UpdateMapCenter()
        {
            MapCenter = MappedPoints.Count switch
            {
                0 => null,
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
