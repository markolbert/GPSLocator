using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[LocatorInboundV1Request("Location.svc","Version", false)]
public class DeviceVersionRequest : DeviceRequest<DeviceVersion>
{
    public DeviceVersionRequest( 
        DeviceConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
