﻿using System;
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
    public class LocationMapViewModel : BasePassiveViewModel
    {
        private MapControl.Location? _mapCenter;
        private double _mapHeight = 500;
        private double _mapWidth = 500;

        protected LocationMapViewModel(
            IAppConfig configuration,
            IJ4JLogger logger
        )
            : base( configuration, logger )
        {
            LocationTypeChoices.Add(new AnnotatedLocationType(LocationType.RoutePoint, "Include in Route"));
            LocationTypeChoices.Add(new AnnotatedLocationType(LocationType.Pushpin, "Show as Pushpin"));
            LocationTypeChoices.Add(new AnnotatedLocationType(LocationType.Unspecified, "Don't Show"));

            MapPoints = new BindingList<MapPoint>();
            MapPoints.ListChanged += MapPoints_ListChanged;
        }

        private void MapPoints_ListChanged(object? sender, ListChangedEventArgs e)
        {
            OnMapPointsChanged();
        }

        protected virtual void OnMapPointsChanged()
        {
        }

        public List<AnnotatedLocationType> LocationTypeChoices { get; } = new();

        public BindingList<MapPoint> MapPoints { get; }

        public bool HasPoints => MapPoints.Any();

        public LocationCollection? Route =>
            new( MapPoints.Where( x => x.SelectedLocationType?.LocationType == LocationType.RoutePoint )
                           .Select( x => x.DisplayPoint ) );

        public IEnumerable<MapPoint> Pushpins =>
            MapPoints.Where( x => x.SelectedLocationType?.LocationType == LocationType.Pushpin );

        protected virtual void ClearMapLocations()
        {
            MapPoints.Clear();

            OnPropertyChanged(nameof(HasPoints));
            OnPropertyChanged( nameof( Route ) );
            OnPropertyChanged( nameof( Pushpins ) );

            UpdateMapCenter();
        }

        protected void AddMapLocation( ILocation inReachLocation, LocationType locType )
        {
            AddMapLocation(
                new MapPoint( inReachLocation,
                              LocationTypeChoices.First( x => x.LocationType == LocationType.Unspecified ) )
                {
                    SelectedLocationType = LocationTypeChoices
                       .First( x => x.LocationType == LocationType.Pushpin )
                } );
        }

        protected void AddPushpin( ILocation inReachLocation ) => AddMapLocation( inReachLocation, LocationType.Pushpin );

        protected void AddRoutePoint( ILocation inReachLocation ) =>
            AddMapLocation( inReachLocation, LocationType.RoutePoint );

        protected void AddMapLocation( MapPoint mapPoint )
        {
            MapPoints.Add( mapPoint );

            OnPropertyChanged(nameof(HasPoints));

            if( mapPoint.SelectedLocationType is { LocationType: LocationType.Pushpin } )
                OnPropertyChanged( nameof( Pushpins ) );

            if( mapPoint.SelectedLocationType is { LocationType: LocationType.RoutePoint } )
                OnPropertyChanged( nameof( Route ) );

            UpdateMapCenter();
        }

        public MapControl.Location? MapCenter
        {
            get => _mapCenter;
            set => SetProperty( ref _mapCenter, value );
        }

        protected bool DeferUpdatingMapCenter { get; set; }

        protected virtual bool IncludeLocationType( LocationType locationType ) => true;

        protected void UpdateMapCenter()
        {
            if( DeferUpdatingMapCenter )
                return;

            var filteredPoints = MapPoints
                                .Where( x => IncludeLocationType( x.SelectedLocationType.LocationType ) )
                                .ToList();

            MapCenter = filteredPoints.Count switch
            {
                0 => null,
                1 => filteredPoints[ 0 ].DisplayPoint,
                _ => CalculateMapCenter(filteredPoints)
            };
        }

        // https://stackoverflow.com/questions/6671183/calculate-the-center-point-of-multiple-latitude-longitude-coordinate-pairs
        // thanx to Gio and Yodacheese for this!
        private MapControl.Location CalculateMapCenter( List<MapPoint> points )
        {
            double x = 0;
            double y = 0;
            double z = 0;

            foreach( var mapLocation in points )
            {
                var latitude = mapLocation.DisplayPoint.Latitude * Math.PI / 180;
                var longitude = mapLocation.DisplayPoint.Longitude * Math.PI / 180;

                x += Math.Cos( latitude ) * Math.Cos( longitude );
                y += Math.Cos( latitude ) * Math.Sin( longitude );
                z += Math.Sin( latitude );
            }

            var total = MapPoints.Count;

            x = x / total;
            y = y / total;
            z = z / total;

            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);

            return new MapControl.Location( centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI );
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
