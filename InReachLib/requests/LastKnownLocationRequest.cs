using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Location.svc","LastKnownLocation", true)]
public class LastKnownLocationRequest<TLoc> : InReachRequest<LastKnownLocation<TLoc>>
    where TLoc : ILocation
{
    private string _imei = string.Empty;

    public LastKnownLocationRequest( 
        InReachConfig config, 
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
