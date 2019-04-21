using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SeeingSharpModelViewer
{
    public class NullableToVisibilityConverter : IValueConverter
    {
        public NullableToVisibilityConverter()
        {
            this.ResultOnNull = Visibility.Collapsed;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility)) { throw new InvalidOperationException("Invalid target Type!"); }

            if (value != null) { return Visibility.Visible; }
            else { return ResultOnNull; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Convert back not supported!");
        }

        public Visibility ResultOnNull
        {
            get;
            set;
        }
    }
}