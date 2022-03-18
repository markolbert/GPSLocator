using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[InboundV1("Configuration.svc","DeviceConfig", true)]
public class DeviceConfigRequest : DeviceGetRequest<DeviceParameters>
{
    public DeviceConfigRequest( 
        DeviceConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
