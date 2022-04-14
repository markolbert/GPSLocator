namespace J4JSoftware.GPSCommon;

public class HistoryMapPointsFilter : MessageMapPointsFilter
{
    private bool _hideInvalid;

    public bool HideInvalid
    {
        get => _hideInvalid;
        set => SetProperty( ref _hideInvalid, value );
    }

    public override bool AllowInDisplay( MapPoint mapPoint )
    {
        var meetsMsg = base.AllowInDisplay( mapPoint );
        var meetsValid = !_hideInvalid || mapPoint.IsValidLocation;

        return meetsMsg && meetsValid;
    }
}
