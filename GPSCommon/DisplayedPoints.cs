using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.GPSLocator;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.GPSCommon;

public class DisplayedPoints : Collection<MapPoint>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private const double MinimumSeparation = 0.000001d;
    private const string IndexerPropertyName = "Item[]";

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event EventHandler<MapChangedEventArgs>? MapChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    private static readonly NotifyCollectionChangedEventArgs ResetCollectionArgs =
        new( NotifyCollectionChangedAction.Reset );

    private bool _deferNotifications;

    public Collection<MapPoint> Points => this;

    public MapPoint Add( ILocation location )
    {
        var retVal = new MapPoint( location );
        Add( retVal );

        return retVal;
    }

    public void AddRange( IEnumerable<ILocation> locations )
    {
        var newPts = locations.Select( x => new MapPoint( x ) )
                              .ToList();
        var addedAt = Count;

        _deferNotifications = true;

        foreach( var point in newPts )
        {
            Add( point );
        }

        _deferNotifications = false;

        OnCollectionChanged( NotifyCollectionChangedAction.Add, newPts, addedAt );
        OnCenterBoundsChanged();
        OnPropertyChanged( nameof( Count ) );
        OnPropertyChanged( IndexerPropertyName );
    }

    #region internal overrides

    protected override void InsertItem(int index, MapPoint item)
    {
        base.InsertItem(index, item);

        if (_deferNotifications)
            return;

        OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        OnCenterBoundsChanged();
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerPropertyName);
    }

    protected override void RemoveItem(int index)
    {
        var removed = this[index];

        base.RemoveItem(index);

        if (_deferNotifications)
            return;

        OnCollectionChanged(NotifyCollectionChangedAction.Remove, removed, index);
        OnCenterBoundsChanged();
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerPropertyName);
    }

    protected override void ClearItems()
    {
        base.ClearItems();

        OnCollectionReset();
        OnCenterBoundsChanged();
        OnPropertyChanged( nameof( Count ) );
        OnPropertyChanged( IndexerPropertyName );
    }

    #endregion

    private void OnCenterBoundsChanged()
    {
        MapChanged?.Invoke( this,
                            new MapChangedEventArgs { Center = GetCenter(), BoundingBox = GetBoundingBox() } );
    }

    #region INotify... stuff

    private void OnPropertyChanged( [ CallerMemberName ] string propertyName = "" )
    {
        PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    private void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
        CollectionChanged?.Invoke( this, e );
    }

    private void OnCollectionChanged( NotifyCollectionChangedAction action, MapPoint? item, int index )
    {
        OnCollectionChanged( new NotifyCollectionChangedEventArgs( action, item, index ) );
    }

    private void OnCollectionChanged(
        NotifyCollectionChangedAction action,
        List<MapPoint>? addedItems,
        int addedAt
    )
    {
        OnCollectionChanged( new NotifyCollectionChangedEventArgs( action, addedItems, addedAt ) );
    }

    private void OnCollectionReset() => OnCollectionChanged( ResetCollectionArgs );

    #endregion

    #region Map center

    public MapControl.Location? GetCenter() =>
        this.Count switch
        {
            0 => null,
            1 => this[ 0 ].MapLocation,
            _ => CalculateMapCenter()
        };

    // https://stackoverflow.com/questions/6671183/calculate-the-center-point-of-multiple-latitude-longitude-coordinate-pairs
    // thanx to Gio and Yodacheese for this!
    private MapControl.Location CalculateMapCenter()
    {
        double x = 0;
        double y = 0;
        double z = 0;

        foreach( var mappedPoint in this )
        {
            var latitude = mappedPoint.MapLocation.Latitude * Math.PI / 180;
            var longitude = mappedPoint.MapLocation.Longitude * Math.PI / 180;

            x += Math.Cos( latitude ) * Math.Cos( longitude );
            y += Math.Cos( latitude ) * Math.Sin( longitude );
            z += Math.Sin( latitude );
        }

        var total = this.Count;

        x /= total;
        y /= total;
        z /= total;

        var centralLongitude = Math.Atan2( y, x );
        var centralSquareRoot = Math.Sqrt( x * x + y * y );
        var centralLatitude = Math.Atan2( z, centralSquareRoot );

        return new MapControl.Location( centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI );
    }

    #endregion

    #region Map bounding box

    public MapControl.BoundingBox? GetBoundingBox() =>
        this.Count switch
        {
            0 => null,
            1 => null,
            _ => CalculateBoundingBox(),
        };

    private MapControl.BoundingBox? CalculateBoundingBox()
    {
        if( this.Count < 2 )
            return null;

        var retVal = new MapControl.BoundingBox()
        {
            East = double.MinValue, West = double.MaxValue, North = double.MinValue, South = double.MaxValue
        };

        foreach( var point in this )
        {
            if( point.DeviceLocation.Coordinate.Latitude > retVal.East )
                retVal.East = point.DeviceLocation.Coordinate.Latitude;

            if( point.DeviceLocation.Coordinate.Latitude < retVal.West )
                retVal.West = point.DeviceLocation.Coordinate.Latitude;

            if( point.DeviceLocation.Coordinate.Longitude > retVal.North )
                retVal.North = point.DeviceLocation.Coordinate.Longitude;

            if( point.DeviceLocation.Coordinate.Longitude < retVal.South )
                retVal.South = point.DeviceLocation.Coordinate.Longitude;
        }

        // deal with oddball cases where the points all fall along a line
        var longSeparation = Math.Abs( retVal.East - retVal.West );
        var latSeparation = Math.Abs( retVal.North - retVal.South );

        if( longSeparation > MinimumSeparation && latSeparation > MinimumSeparation )
            return retVal;

        var center = GetCenter();
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

    #endregion
}