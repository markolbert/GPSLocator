namespace J4JSoftware.InReach;

public class LastKnownLocation<TLoc>
    where TLoc: ILocation
{
    public List<TLoc> Locations { get; set; } = new();
}
