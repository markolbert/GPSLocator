using System;
using System.Collections;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.GPSLocator;

public class CollectionToBoolConverter : IValueConverter
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