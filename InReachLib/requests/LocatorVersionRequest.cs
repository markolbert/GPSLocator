using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[LocatorInboundV1Request("Location.svc","Version", false)]
public class LocatorVersionRequest : LocatorRequest<InReachVersion>
{
    public LocatorVersionRequest( 
        LocatorConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
