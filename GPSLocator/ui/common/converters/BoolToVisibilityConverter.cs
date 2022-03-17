using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.GPSLocator
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert( object? value, Type targetType, object parameter, string language )
        {
            if( value is not bool flag )
                return DependencyProperty.UnsetValue;

            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
            throw new NotImplementedException();
    }
}
