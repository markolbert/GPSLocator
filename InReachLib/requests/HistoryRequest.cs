using J4JSoftware.Logging;

namespace J4JSoftware.InReach;

[InboundV1("Location.svc","History", true)]
public class HistoryRequest<TLoc> : InReachRequest<History<TLoc>>
    where TLoc : ILocation
{
    private string _imei = string.Empty;
    private DateTime _startDate = DateTime.Today.AddDays( 7 );
    private DateTime _endDate = DateTime.Today;

    public HistoryRequest( 
        InReachConfig config, 
        IJ4JLogger logger )
        : base( config, logger )
    {
        IMEIs = Configuration.IMEI;
    }

    public string IMEIs
    {
        get => _imei;
        set => SetQueryProperty( ref _imei, value );
    }

    public DateTime Start
    {
        get => _startDate;
        set => SetQueryProperty( ref _startDate, value, x=>x.ToShortDateString() );
    }

    public DateTime End
    {
        get => _endDate;
        set => SetQueryProperty( ref _endDate, value, x=>x.ToShortDateString() );
    }
}
