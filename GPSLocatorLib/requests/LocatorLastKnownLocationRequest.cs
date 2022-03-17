using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[LocatorInboundV1Request("Location.svc","Version", true)]
public class LocatorLastKnownLocationRequest : LocatorRequest<DeviceLocationHistory>
{
    private string _imei = string.Empty;

    public LocatorLastKnownLocationRequest( 
        DeviceConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
        IMEI = Configuration.IMEI;
    }

    public string IMEI
    {
        get => _imei;
        set => SetQueryProperty( ref _imei, value );
    }
}
