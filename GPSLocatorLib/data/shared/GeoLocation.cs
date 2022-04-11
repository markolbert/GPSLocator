namespace J4JSoftware.GPSLocator;

public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SimpleDisplay => $"{Latitude}, {Longitude}";
    public bool IsValid => Latitude != 0.0 && Longitude != 0.0;
}
