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
    public class EmptyCollectionToBoolConverter : IValueConverter
    {
        public object Convert( object? value, Type targetType, object parameter, string language )
        {
            if( value is not IList list )
                return true;

            return list.Count != 0;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
            throw new NotImplementedException();
    }
}
