using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace J4JSoftware.InReach;

public class Location
{
    public long IMEI { get; set; }

    [JsonConverter(typeof(InReachDateTimeConverter))]
    public DateTime Timestamp { get; set; }

    public GeoLocation? Coordinate { get; set; }
    public long Altitude { get; set; }
    public long Speed { get; set; }
    public long Course { get; set; }
    public int GPSFixStatus { get; set; }
}