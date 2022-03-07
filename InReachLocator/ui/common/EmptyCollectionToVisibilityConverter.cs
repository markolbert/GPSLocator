using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.InReach
{
    public class EmptyCollectionToVisibilityConverter : IValueConverter
    {
        public object Convert( object? value, Type targetType, object parameter, string language )
        {
            if( value is not IList list )
                return Visibility.Visible;

            return list.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
            throw new NotImplementedException();
    }
}
