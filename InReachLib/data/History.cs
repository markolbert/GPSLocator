namespace J4JSoftware.InReach;

public class History<TLoc>
    where TLoc : ILocation
{
    public List<TLoc> HistoryItems { get; set; } = new();
}
