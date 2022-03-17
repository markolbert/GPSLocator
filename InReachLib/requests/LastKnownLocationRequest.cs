using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Location.svc","LastKnownLocation", true)]
public class LastKnownLocationRequest<TLoc> : LocatorRequest<LastKnownLocation<TLoc>>
    where TLoc : ILocation
{
    private string _imei = string.Empty;

    public LastKnownLocationRequest( 
        LocatorConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
        IMEI = Configuration.IMEI;
    }

    public string IMEI
    {
        get => _imei;
        set => SetQueryProperty( ref _imei, value );
    }
}
