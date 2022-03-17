using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Configuration.svc","DeviceConfig", true)]
public class DeviceConfigRequest : LocatorRequest<DeviceConfig>
{
    public DeviceConfigRequest( 
        LocatorConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
