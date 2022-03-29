﻿using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.GPSLocator
{
    public class NullToVisibleConverter : IValueConverter
    {
        public object Convert( object? value, Type targetType, object parameter, string language ) =>
            value == null ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
            throw new NotImplementedException();
    }
}
