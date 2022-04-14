using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.GPSCommon
{
    public interface IFilterMapPoints : INotifyPropertyChanged
    {
        bool AllowInDisplay( MapPoint mapPoint );
    }
}
