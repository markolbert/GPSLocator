using System;

namespace J4JSoftware.InReach;

public class MapLocationMessage : LocationMessage, IMapLocation
{
    private MapControl.Location _mapPoint = new(0, 0);
    private string _label = string.Empty;

    public MapControl.Location MapPoint
    {
        get
        {
            if (Math.Abs(_mapPoint.Latitude - Coordinate.Latitude) + Math.Abs(_mapPoint.Longitude - Coordinate.Longitude) >= MapLocation.MinimumDelta)
            {
                _mapPoint = new MapControl.Location(Coordinate.Latitude, Coordinate.Longitude);
                _label = $"{Coordinate.SimpleDisplay}\n{Timestamp}";
            }

            return _mapPoint;
        }
    }

    public LocationType LocationType { get; set; } = LocationType.Unspecified;
    public string Label => LocationType == InReach.LocationType.Pushpin ? _label : string.Empty;
}
