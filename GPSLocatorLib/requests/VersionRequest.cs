using J4JSoftware.Logging;

namespace J4JSoftware.GPSLocator;

[InboundV1("Location.svc","Version", false)]
public class VersionRequest : DeviceRequest<DeviceVersion>
{
    public VersionRequest( 
        DeviceConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
