using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using J4JSoftware.GPSLocator;
using J4JSoftware.Logging;
using MapControl;
using Microsoft.Toolkit.Mvvm.Input;

namespace J4JSoftware.GPSCommon;

public class LocationMapViewModel<TAppConfig> : BaseViewModel<TAppConfig>
    where TAppConfig : BaseAppConfig
{
    private int _zoomLevel = 17;
    private MapControl.Location? _mapCenter;

    protected LocationMapViewModel(
        DisplayedPoints displayedPoints,
        BaseAppViewModel<TAppConfig> appViewModel,
        StatusMessage.StatusMessages statusMessages,
        IJ4JLogger logger
    )
        : base( appViewModel, statusMessages, logger )
    {
        DisplayedPoints = displayedPoints;
        DisplayedPoints.CollectionChanged += DisplayedPointsOnCollectionChanged;

        IncreaseZoomCommand = new RelayCommand(IncreaseZoomHandler);
        DecreaseZoomCommand = new RelayCommand(DecreaseZoomHandler);
    }

    private void DisplayedPointsOnCollectionChanged( object? sender, NotifyCollectionChangedEventArgs e )
    {
        OnMapChanged( DisplayedPoints.GetCenter(), DisplayedPoints.GetBoundingBox() );
    }

    protected virtual void OnMapChanged( MapPoint? center, BoundingBox? boundingBox )
    {
        MapCenter = center?.MapLocation;
        MapBoundingBox = boundingBox;
    }

    public DisplayedPoints DisplayedPoints { get; }

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