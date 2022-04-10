using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[LocatorInboundV1Request("Location.svc","Version", false)]
public class DeviceVersionRequest : DeviceGetRequest<DeviceVersion, ErrorSingleImei>
{
    public DeviceVersionRequest( 
        DeviceConfig config, 
        IJ4JLogger logger,
        IBullshitLogger bsLogger
        )
        : base( config, logger, bsLogger )
    {
    }
}
