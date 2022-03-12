using System;
using System.Collections.Generic;

namespace J4JSoftware.InReach;

public class InReachLocation
{
    public long IMEI { get; set; }
    public DateTime Timestamp { get; set; }
    public InReachLatLong? Coordinate { get; set; }
    public long Altitude { get; set; }
    public long Speed { get; set; }
    public long Course { get; set; }
    public string GpsFixStatus { get; set; } = string.Empty;
}