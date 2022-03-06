namespace J4JSoftware.InReach;

public interface ILocationMessage : ILocation
{
    bool HasMessage {get; }
    string Message { get; set; }
    List<string> Recipients { get; set; }
}
