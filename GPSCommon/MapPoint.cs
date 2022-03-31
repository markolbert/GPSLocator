using J4JSoftware.GPSLocator;

namespace J4JSoftware.GPSCommon;

public class MapPoint
{
    public MapPoint(
        ILocation deviceLocation
    )
    {
        DeviceLocation = deviceLocation;

        MapLocation = new MapControl.Location( deviceLocation.Coordinate.Latitude,
                                               deviceLocation.Coordinate.Longitude );

        Label =
            $"{deviceLocation.Coordinate.Latitude}, {deviceLocation.Coordinate.Longitude}\n{deviceLocation.Timestamp}";
    }

    public ILocation DeviceLocation { get; }
    public MapControl.Location MapLocation { get; }
    public string Label { get; }
}