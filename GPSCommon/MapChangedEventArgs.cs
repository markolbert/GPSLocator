using System;

namespace J4JSoftware.GPSCommon;

public class MapChangedEventArgs : EventArgs
{
    public MapPoint? Center { get; set; }
    public MapControl.BoundingBox? BoundingBox { get; set; }
}
