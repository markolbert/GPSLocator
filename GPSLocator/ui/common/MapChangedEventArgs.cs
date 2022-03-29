using System;

namespace J4JSoftware.GPSLocator;

public class MapChangedEventArgs : EventArgs
{
    public MapControl.Location? Center { get; set; }
    public MapControl.BoundingBox? BoundingBox { get; set; }
}
