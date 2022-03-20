namespace J4JSoftware.GPSLocator;

public class DeviceRequestEventArgs : EventArgs
{
    public DeviceRequestEventArgs(
        RequestEvent reqEvent,
        string? message = null
    )
    {
        RequestEvent = reqEvent;
        Message = message;
    }

    public RequestEvent RequestEvent { get; }
    public string? Message { get; }
}
