namespace J4JSoftware.GPSLocator;

public class RequestEventArgs<TResponse> : EventArgs
    where TResponse : class, new()
{
    public RequestEventArgs(
        RequestEvent reqEvent,
        DeviceResponse<TResponse>? response
    )
    {
        RequestEvent = reqEvent;
        Response = response;
    }

    public RequestEvent RequestEvent { get; }
    public DeviceResponse<TResponse>? Response { get; }
    public string? ErrorMessage => Response?.Error?.Message;
}
