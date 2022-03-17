namespace J4JSoftware.GPSLocator;

public class DeviceResponse<T>
    where T : class, new()
{
    public DeviceResponse(
        string requestUri
        )
    {
        RequestUri = requestUri;
    }

    public T? Result { get; set; }
    public string RequestUri { get; }
    public bool Succeeded => Error == null;
    public DeviceError? Error { get; set; }
}
