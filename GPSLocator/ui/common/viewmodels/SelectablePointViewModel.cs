using System.Linq;
using J4JSoftware.Logging;
using MapControl;

namespace J4JSoftware.GPSLocator;

public class SelectablePointViewModel : HistoryViewModelBase
{
    private MapPoint? _pointOnMap;

    public SelectablePointViewModel(
        AppViewModel appViewModel,
        IJ4JLogger logger
    )
        : base(appViewModel, logger)
    {
        Messenger.Send(new MapViewModelMessage(this), "primary");
    }

    public MapPoint? PointOnMap
    {
        get => _pointOnMap;

        set
        {
            if (_pointOnMap?.DeviceLocation.Coordinate.Latitude == 0
             && _pointOnMap?.DeviceLocation.Coordinate.Longitude == 0)
                value = null;

            SetProperty(ref _pointOnMap, value);
            DisplayedPoints.Clear();

            if (value != null)
                DisplayedPoints.Add(value);
        }
    }

    protected override void OnMapChanged(MapControl.Location? center, BoundingBox? boundingBox)
    {
        base.OnMapChanged(center, boundingBox);

        MapCenter = DisplayedPoints.Any() ? center : null;
        MapBoundingBox = DisplayedPoints.Any() ? boundingBox : null;
    }

}