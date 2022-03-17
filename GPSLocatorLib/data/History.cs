namespace J4JSoftware.GPSLocator;

public class History<TLoc>
    where TLoc : ILocation
{
    public List<TLoc> HistoryItems { get; set; } = new();
}
