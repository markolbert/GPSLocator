using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InReachInboundV1Request("Location.svc","Version", true)]
public class InReachLastKnownLocationRequest : InReachRequest<InReachLastKnownLocation>
{
    private string _imei = string.Empty;

    public InReachLastKnownLocationRequest( 
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
