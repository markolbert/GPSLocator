using System.ComponentModel;

namespace J4JSoftware.GPSCommon
{
    public interface IFilterMapPoints : INotifyPropertyChanged
    {
        bool AllowInDisplay( MapPoint mapPoint );
    }
}
