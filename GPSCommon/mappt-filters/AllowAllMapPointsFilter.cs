using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.GPSCommon;

public class AllowAllMapPointsFilter : ObservableObject, IFilterMapPoints
{
    public virtual bool AllowInDisplay( MapPoint mapPoint ) => true;
}
