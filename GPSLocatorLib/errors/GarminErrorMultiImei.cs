namespace J4JSoftware.GPSLocator;

public class GarminErrorMultiImei : GarminErrorBase
{
    public long[] IMEI { get; set; } = Array.Empty<long>();

    protected override string? GetImeiAsText() => string.Join( ", ", IMEI );
}
