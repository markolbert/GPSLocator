using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSLocator;

public class LocationMapViewModel : BaseViewModel
{
    private int _zoomLevel = 17;
    private bool _refreshEnabled;
    private MapControl.Location? _mapCenter;

    protected LocationMapViewModel(
        AppViewModel appViewModel,
        IJ4JLogger logger
    )
        : base( appViewModel, logger )
    {
        RefreshCommand = new RelayCommand(RefreshHandler);
        IncreaseZoomCommand = new RelayCommand(IncreaseZoomHandler);
        DecreaseZoomCommand = new RelayCommand(DecreaseZoomHandler);

        DisplayedPoints.MapChanged += DisplayedPointsOnMapChanged;
    }

    private void DisplayedPointsOnMapChanged( object? sender, MapChangedEventArgs e )
    {
        OnMapChanged(e.Center, e.BoundingBox);
    }

    protected virtual void OnMapChanged( MapControl.Location? center, MapControl.BoundingBox? boundingBox )
    {
    }

    public RelayCommand RefreshCommand { get; }

    protected virtual void RefreshHandler()
    {
    }

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set => SetProperty(ref _refreshEnabled, value);
    }

    public ObservableCollection<MapPoint> AllPoints { get; } = new();
    public DisplayedPoints DisplayedPoints { get; } = new();

    protected MapPoint AddLocation( ILocation location )
    {
        var mapPoint = new MapPoint( location );
        AllPoints.Add( mapPoint );

        return mapPoint;
    }

    protected void AddLocations( IEnumerable<ILocation> locations, bool clearList = true )
    {
        if( clearList )
        {
            DisplayedPoints.Clear();
            AllPoints.Clear();
        }

        foreach( var location in locations )
        {
            AddLocation( location );
        }
    }

    protected virtual void ClearDisplayedPoints() => DisplayedPoints.Clear();

    public int ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, value);
    }

    public MapControl.Location? MapCenter
    {
        get => _mapCenter;
        set => SetProperty( ref _mapCenter, value );
    }

    public MapControl.BoundingBox? MapBoundingBox { get; protected set; }

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
}