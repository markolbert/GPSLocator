﻿using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Location.svc","Version", false)]
public class VersionRequest : LocatorRequest<InReachVersion>
{
    public VersionRequest( 
        LocatorConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
    }
}
