using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Location.svc","Version", false)]
public class VersionRequest : InReachRequest<InReachVersion>
{
    public VersionRequest( 
        IInReachConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
