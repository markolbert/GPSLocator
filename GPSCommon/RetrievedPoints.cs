using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.GPSCommon;

public class RetrievedPoints<TAppConfig> : ObservableObject
    where TAppConfig : BaseAppConfig
{
    private const double MinimumSeparation = 0.000001d;

    private readonly IJ4JLogger _logger;

    private int _zoomLevel = 17;
    private IFilterMapPoints _mapPtsFilter;
    private IMapDisplayLayer _mapLayer;

    public RetrievedPoints(
        TAppConfig config,
        IJ4JLogger logger
        )
    {
        Configuration = config;

        _logger = logger;
        _logger.SetLoggedType( GetType() );

        _mapPtsFilter = new AllowAllMapPointsFilter();
        _mapPtsFilter.PropertyChanged += MapPtsFilterOnPropertyChanged;

        IncreaseZoomCommand = new RelayCommand(IncreaseZoomHandler);
        DecreaseZoomCommand = new RelayCommand(DecreaseZoomHandler);
    }

    private void MapPtsFilterOnPropertyChanged( object? sender, PropertyChangedEventArgs e )
    {
        if( string.IsNullOrEmpty( e.PropertyName ) )
            return;
        
        OnPropertyChanged(nameof(Center));
        OnPropertyChanged(nameof(BoundingBox));

        OnPropertyChanged(nameof(NumDisplayable));
        OnPropertyChanged(nameof(DisplayablePoints));

        OnPropertyChanged(nameof(NumSelected));
        OnPropertyChanged( nameof( SelectedPoints ) );
    }

    public TAppConfig Configuration { get; }

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

    // be careful not to test for whether a MapPoint is selected in this filter
    // that test is handled separately (and must be handled separately for 
    // NumDisplayable to be correct)
    public IFilterMapPoints MapPointsFilter
    {
        get => _mapPtsFilter;

        set
        {
            _mapPtsFilter.PropertyChanged -= MapPtsFilterOnPropertyChanged;

            _mapPtsFilter = value;
            _mapPtsFilter.PropertyChanged += MapPtsFilterOnPropertyChanged;
        }
    }

    public ObservableCollection<MapPoint> AllPoints { get; private set; } = new();

    public IEnumerable<MapPoint> SelectedPoints =>
        AllPoints.Where( x => x.IsSelected && _mapPtsFilter.AllowInDisplay( x ) );

    public IEnumerable<MapPoint> DisplayablePoints => AllPoints.Where( x => _mapPtsFilter.AllowInDisplay( x ) );

    public int NumSelected => AllPoints.Count( x => x.IsSelected && _mapPtsFilter.AllowInDisplay( x ) );
    public int NumDisplayable => AllPoints.Count( x => _mapPtsFilter.AllowInDisplay( x ) );
    public int Count => AllPoints.Count;

    public MapPoint Add( ILocation location )
    {
        var retVal = new MapPoint( location );
        AllPoints.Add( retVal );

        OnPropertyChanged( nameof( Count ) );

        OnPropertyChanged( nameof( NumDisplayable ) );
        OnPropertyChanged( nameof( DisplayablePoints ) );

        return retVal;
    }

    public void AddRange( IEnumerable<MapPoint> mapPoints)
    {
        var origSelected = NumSelected;
        var numSelected = 0;

        foreach( var mapPoint in mapPoints )
        {
            if( mapPoint.IsSelected )
                numSelected++;

            AllPoints.Add( mapPoint );
        }

        OnPropertyChanged(nameof(Count));

        OnPropertyChanged(nameof(NumDisplayable));
        OnPropertyChanged(nameof(DisplayablePoints));

        if( origSelected <= 0 && numSelected <= 0 )
            return;

        OnPropertyChanged(nameof(Center));
        OnPropertyChanged(nameof(BoundingBox));

        OnPropertyChanged(nameof(NumSelected));
        OnPropertyChanged( nameof( SelectedPoints ) );
    }

    public void Clear()
    {
        AllPoints.Clear();

        OnPropertyChanged(nameof(Count));

        OnPropertyChanged(nameof(NumDisplayable));
        OnPropertyChanged(nameof(DisplayablePoints));

        OnPropertyChanged(nameof(Center));
        OnPropertyChanged(nameof(BoundingBox));

        OnPropertyChanged(nameof(NumSelected));
        OnPropertyChanged(nameof(SelectedPoints));
    }

    public void Select(params MapPoint[] selectedPts) => ChangeSelection(selectedPts, true);
    public void Unselect( params MapPoint[] unselectedPts ) => ChangeSelection( unselectedPts, false );

    private void ChangeSelection( MapPoint[] points, bool selected )
    {
        foreach (var curPt in points)
        {
            var idxMatch = -1;

            for( var idx = 0; idx < AllPoints.Count; idx++ )
            {
                if( !ReferenceEquals( AllPoints[ idx ], curPt ) )
                    continue;

                idxMatch = idx;
                break;
            }

            if (idxMatch < 0)
            {
                _logger.Error<string>("Could not find MapPoint '' in collection", curPt.Label);
                continue;
            }

            AllPoints[idxMatch].IsSelected = selected;
        }

        OnPropertyChanged(nameof(Center));
        OnPropertyChanged(nameof(BoundingBox));

        OnPropertyChanged(nameof(NumSelected));
        OnPropertyChanged(nameof(SelectedPoints));
    }

    public void UnselectAll( bool deferNotification = true )
    {
        foreach (var mapPoint in AllPoints.Where(x=>x.IsSelected  ))
        {
            mapPoint.IsSelected = false;
        }

        if( deferNotification )
            return;

        OnPropertyChanged(nameof(Center));
        OnPropertyChanged(nameof(BoundingBox));

        OnPropertyChanged(nameof(NumSelected));
        OnPropertyChanged(nameof(SelectedPoints));
    }

    // https://stackoverflow.com/questions/6671183/calculate-the-center-point-of-multiple-latitude-longitude-coordinate-pairs
    // thanx to Gio and Yodacheese for this!
    public MapControl.Location? Center
    {
        get
        {
            switch( NumSelected )
            {
                case 0:
                    return null;

                case 1:
                    return SelectedPoints.First().MapLocation;
            }

            double x = 0;
            double y = 0;
            double z = 0;
            double t = 0;

            foreach( var mapPoint in SelectedPoints )
            {
                var latitude = mapPoint.MapLocation.Latitude * Math.PI / 180;
                var longitude = mapPoint.MapLocation.Longitude * Math.PI / 180;

                x += Math.Cos( latitude ) * Math.Cos( longitude );
                y += Math.Cos( latitude ) * Math.Sin( longitude );
                z += Math.Sin( latitude );
                t += ( mapPoint.Timestamp - DateTime.UnixEpoch ).TotalSeconds;
            }

            var total = this.Count;

            x /= total;
            y /= total;
            z /= total;
            t /= total;

            var centralLongitude = Math.Atan2( y, x );
            var centralSquareRoot = Math.Sqrt( x * x + y * y );
            var centralLatitude = Math.Atan2( z, centralSquareRoot );

            return new MapControl.Location( centralLatitude * 180 / Math.PI,
                                            centralLongitude * 180 / Math.PI );
        }
    }

    public MapControl.BoundingBox? BoundingBox
    {
        get
        {
            if( NumSelected < 2 )
                return null;

            var retVal = new MapControl.BoundingBox()
            {
                East = double.MinValue, West = double.MaxValue, North = double.MinValue, South = double.MaxValue
            };

            foreach( var point in SelectedPoints )
            {
                if( point.MapLocation.Latitude > retVal.East )
                    retVal.East = point.MapLocation.Latitude;

                if( point.MapLocation.Latitude < retVal.West )
                    retVal.West = point.MapLocation.Latitude;

                if( point.MapLocation.Longitude > retVal.North )
                    retVal.North = point.MapLocation.Longitude;

                if( point.MapLocation.Longitude < retVal.South )
                    retVal.South = point.MapLocation.Longitude;
            }

            // deal with oddball cases where the points all fall along a line
            var longSeparation = Math.Abs( retVal.East - retVal.West );
            var latSeparation = Math.Abs( retVal.North - retVal.South );

            if( longSeparation > MinimumSeparation && latSeparation > MinimumSeparation )
                return retVal;

            var center = Center;
            if( center == null )
                return null;

            switch( latSeparation )
            {
                case <= MinimumSeparation when longSeparation <= MinimumSeparation:
                    retVal.West = center.Longitude - 0.1 * Math.Abs( center.Longitude );
                    retVal.East = center.Longitude + 0.1 * Math.Abs( center.Longitude );
                    retVal.South = center.Latitude - 0.1 * Math.Abs( center.Latitude );
                    retVal.North = center.Latitude + 0.1 * Math.Abs( center.Latitude );

                    return retVal;

                case <= MinimumSeparation:
                    retVal.North = center.Latitude - longSeparation / 2;
                    retVal.South = center.Latitude + longSeparation / 2;

                    return retVal;
            }

            retVal.West = center.Longitude - latSeparation / 2;
            retVal.East = center.Longitude + latSeparation / 2;

            return retVal;
        }
    }

    public IMapDisplayLayer MapLayer
    {
        get => _mapLayer;

        set
        {
            SetProperty( ref _mapLayer, value );

            WeakReferenceMessenger.Default.Send( new MapLayerChangedMessage( _mapLayer ), "primary" );
        }
    }

    public int ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, value);
    }

}