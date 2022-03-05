using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.InReach
{
    public class BoolToGridBackgroundColor : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, string language )
        {
            if( value is not bool flag )
                return Colors.White;

            return flag ? Colors.LightGoldenrodYellow : Colors.White;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
            throw new NotImplementedException();
    }
}
