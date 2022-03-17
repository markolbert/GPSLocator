namespace J4JSoftware.GPSLocator;

public class DeviceLocation
{
    public long IMEI { get; set; }
    public DateTime Timestamp { get; set; }
    public LocatorLatLong? Coordinate { get; set; }
    public long Altitude { get; set; }
    public long Speed { get; set; }
    public long Course { get; set; }
    public string GpsFixStatus { get; set; } = string.Empty;
}