using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InReachInboundV1Request("Location.svc","Version", false)]
public class InReachVersionRequest : InReachRequest<InReachVersion>
{
    public InReachVersionRequest( 
        InReachConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
