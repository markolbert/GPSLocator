namespace J4JSoftware.InReach;

public class InReachError
{
    public int ErrorCode { get; set; }
    public int HttpResponseCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
