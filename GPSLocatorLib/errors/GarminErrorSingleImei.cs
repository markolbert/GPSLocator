namespace J4JSoftware.GPSLocator;

public class GarminErrorSingleImei : GarminErrorBase
{
    public string? IMEI { get; set; }

    protected override string? GetImeiAsText() => IMEI;
}
