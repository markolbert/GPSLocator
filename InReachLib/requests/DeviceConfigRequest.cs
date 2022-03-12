using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Configuration.svc","DeviceConfig", true)]
public class DeviceConfigRequest : InReachRequest<DeviceConfig>
{
    public DeviceConfigRequest( 
        InReachConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
