using System.Collections.ObjectModel;

namespace J4JSoftware.InReach;

public class HistoryItem : Location
{
    public string TextMessage { get; set; } = string.Empty;
    public ObservableCollection<string> Recipients { get; } = new();
}