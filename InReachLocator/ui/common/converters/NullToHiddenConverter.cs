﻿using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace J4JSoftware.InReach
{
    public class NullToHiddenConverter : IValueConverter
    {
        public object Convert( object? value, Type targetType, object parameter, string language ) =>
            value == null ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack( object value, Type targetType, object parameter, string language ) =>
            throw new NotImplementedException();
    }
}
