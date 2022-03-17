namespace J4JSoftware.InReach;

public class LocatorResponse<T>
    where T : class, new()
{
    public LocatorResponse(
        string requestUri
        )
    {
        RequestUri = requestUri;
    }

    public T? Result { get; set; }
    public string RequestUri { get; }
    public bool Succeeded => Error == null;
    public InReachError? Error { get; set; }
}
