using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[LocatorInboundV1Request("Location.svc","Version", true)]
public class LocatorLastKnownLocationRequest : LocatorRequest<LocatorLastKnownLocation>
{
    private string _imei = string.Empty;

    public LocatorLastKnownLocationRequest( 
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
