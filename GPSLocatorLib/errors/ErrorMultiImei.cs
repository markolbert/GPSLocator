namespace J4JSoftware.GPSLocator;

public class ErrorMultiImei : ErrorBase
{
    public long[] IMEI { get; set; } = Array.Empty<long>();

    protected override string? GetImeiAsText() => string.Join( ", ", IMEI );
}
