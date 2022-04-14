namespace J4JSoftware.GPSCommon;

public class MessageMapPointsFilter : AllowAllMapPointsFilter
{
    private bool _requireMsg;

    public bool RequireMessage
    {
        get => _requireMsg;
        set => SetProperty( ref _requireMsg, value );
    }

    public override bool AllowInDisplay( MapPoint mapPoint ) => !_requireMsg || mapPoint.HasMessage;
}
