namespace J4JSoftware.GPSLocator;

public class LastKnownLocation<TLoc>
    where TLoc: ILocation
{
    public List<TLoc> Locations { get; set; } = new();
}
