namespace J4JSoftware.InReach;

public interface IMapLocation
{
    ILocation InReachLocation { get; }
    MapControl.Location DisplayPoint { get; }
    LocationType LocationType { get; set; }
    string Label { get; }
}