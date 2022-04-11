using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSCommon;

public class LocationMapViewModel<TAppConfig> : BaseViewModel<TAppConfig>
    where TAppConfig : BaseAppConfig
{
    private int _zoomLevel = 17;
    private bool _refreshEnabled;
    private MapControl.Location? _mapCenter;
    private bool _hideInvalidLoc;
    private bool _deferFilteredPoints;

    protected LocationMapViewModel(
        BaseAppViewModel<TAppConfig> appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( appViewModel, statusMessages, logger )
    {
        RefreshCommand = new RelayCommand(RefreshHandlerInternal);
        IncreaseZoomCommand = new RelayCommand(IncreaseZoomHandler);
        DecreaseZoomCommand = new RelayCommand(DecreaseZoomHandler);

        DisplayedPoints.MapChanged += DisplayedPointsOnMapChanged;

        HideInvalidLocations = AppViewModel.Configuration.HideInvalidLocations;
        AllPoints.CollectionChanged += AllPointsOnCollectionChanged;
    }

    private void AllPointsOnCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
        if( !_deferFilteredPoints )
            OnPropertyChanged( nameof( FilteredPoints ) );
    }

    private void DisplayedPointsOnMapChanged( object? sender, MapChangedEventArgs e )
    {
        OnMapChanged(e.Center, e.BoundingBox);
    }

    protected virtual void OnMapChanged( MapControl.Location? center, MapControl.BoundingBox? boundingBox )
    {
    }

    public RelayCommand RefreshCommand { get; }

    protected virtual void RefreshHandlerInternal()
    {
    }

    protected void RefreshHandler()
    {
        _deferFilteredPoints = true;
        RefreshHandlerInternal();
        _deferFilteredPoints = false;

        OnPropertyChanged(nameof(FilteredPoints));
    }

    public bool RefreshEnabled
    {
        get => _refreshEnabled;
        set => SetProperty(ref _refreshEnabled, value);
    }

    public ObservableCollection<MapPoint> AllPoints { get; } = new();

    public List<MapPoint> FilteredPoints()
    {
        var retVal = AllPoints
            .Where( x => ( AppViewModel.Configuration.HideInvalidLocations && x.DeviceLocation.Coordinate.IsValid )
                         || !AppViewModel.Configuration.HideInvalidLocations )
            .ToList();

        return retVal;
    }

    public DisplayedPoints DisplayedPoints { get; } = new();

    public bool HideInvalidLocations
    {
        get => _hideInvalidLoc;

        set
        {
            var changed = value != _hideInvalidLoc;

            SetProperty( ref _hideInvalidLoc, value );

            if( changed )
                OnPropertyChanged( nameof( FilteredPoints ) );
        }
    }

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