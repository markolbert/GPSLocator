namespace J4JSoftware.InReach;

public class LocationMessage : Location
{
    public bool HasMessage => !string.IsNullOrEmpty( Message );
    public string Message { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}
