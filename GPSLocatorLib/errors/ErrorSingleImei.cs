namespace J4JSoftware.GPSLocator;

public class ErrorSingleImei : ErrorBase
{
    public string? IMEI { get; set; }

    protected override string? GetImeiAsText() => IMEI;
}
