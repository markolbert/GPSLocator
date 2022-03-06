using System.Collections.Generic;

namespace J4JSoftware.InReach;

public class History<TLocMesg>
    where TLocMesg : ILocationMessage
{
    public List<TLocMesg> HistoryItems { get; set; } = new();
}
