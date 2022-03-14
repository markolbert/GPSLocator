namespace J4JSoftware.InReach;

public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SimpleDisplay => $"{Latitude}, {Longitude}";
}
