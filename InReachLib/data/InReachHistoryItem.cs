using System.Collections.ObjectModel;

namespace J4JSoftware.InReach;

public class InReachHistoryItem : InReachLocation
{
    public string TextMessage { get; set; } = string.Empty;
    public ObservableCollection<string> Recipients { get; } = new();
}