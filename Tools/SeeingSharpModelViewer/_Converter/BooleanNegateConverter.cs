using System;
using System.Globalization;
using System.Windows.Data;

namespace SeeingSharpModelViewer
{
    public class BooleanNegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool)) { throw new InvalidOperationException("Invalid target Type!"); }
            if (value.GetType() != typeof(bool)) { throw new InvalidOperationException("Invalid source Type!"); }

            return !(bool)(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool)) { throw new InvalidOperationException("Invalid target Type!"); }
            if (value.GetType() != typeof(bool)) { throw new InvalidOperationException("Invalid source Type!"); }

            return !(bool)(value);
        }
    }
}