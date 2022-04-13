using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac.Features.ResolveAnything;
using J4JSoftware.GPSLocator;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.GPSCommon;

public class DisplayedPoints : Collection<MapPoint>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private const double MinimumSeparation = 0.000001d;
    private const string IndexerPropertyName = "Item[]";

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
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
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerPropertyName);
    }

    protected override void ClearItems()
    {
        if( !this.Any() )
            return;

        base.ClearItems();

        OnCollectionReset();
        OnPropertyChanged( nameof( Count ) );
        OnPropertyChanged( IndexerPropertyName );
    }

    #endregion

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

    public MapPoint? GetCenter() =>
        this.Count switch
        {
            0 => null,
            1 => this[ 0 ],
            _ => CalculateMapCenter()
        };

    // https://stackoverflow.com/questions/6671183/calculate-the-center-point-of-multiple-latitude-longitude-coordinate-pairs
    // thanx to Gio and Yodacheese for this!
    private MapPoint CalculateMapCenter()
    {
        double x = 0;
        double y = 0;
        double z = 0;
        double t = 0;

        foreach( var mapPoint in this )
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

        return new MapPoint( centralLatitude * 180 / Math.PI,
                             centralLongitude * 180 / Math.PI,
                             DateTime.UnixEpoch.AddSeconds( t ) );
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

        var center = GetCenter();
        if( center == null )
            return null;

        switch( latSeparation )
        {
            case <= MinimumSeparation when longSeparation <= MinimumSeparation:
                retVal.West = center.MapLocation.Longitude - 0.1 * Math.Abs( center.MapLocation.Longitude );
                retVal.East = center.MapLocation.Longitude + 0.1 * Math.Abs( center.MapLocation.Longitude );
                retVal.South = center.MapLocation.Latitude - 0.1 * Math.Abs( center.MapLocation.Latitude );
                retVal.North = center.MapLocation.Latitude + 0.1 * Math.Abs( center.MapLocation.Latitude );

                return retVal;

            case <= MinimumSeparation:
                retVal.North = center.MapLocation.Latitude - longSeparation / 2;
                retVal.South = center.MapLocation.Latitude + longSeparation / 2;

                return retVal;
        }

        retVal.West = center.MapLocation.Longitude - latSeparation / 2;
        retVal.East = center.MapLocation.Longitude + latSeparation / 2;

        return retVal;
    }

    #endregion
}