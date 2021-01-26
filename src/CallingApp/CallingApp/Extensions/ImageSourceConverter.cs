﻿using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;

namespace CallingApp
{
    public class ImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageSource.FromResource("CallingApp.Images." + value?.ToString(), typeof(ImageSourceConverter).GetTypeInfo().Assembly);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
