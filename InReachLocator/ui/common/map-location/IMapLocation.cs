namespace J4JSoftware.InReach;

public interface IMapLocation
{
    MapControl.Location MapPoint { get; }
    LocationType LocationType { get; set; }
    string Label { get; }

    bool ImperialUnits { get; set; }
    string AltitudeUnits { get; }
    string SpeedUnits { get; }
}