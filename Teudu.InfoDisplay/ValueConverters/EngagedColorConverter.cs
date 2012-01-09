using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace Teudu.InfoDisplay
{
    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    public class EngagedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;

                if (parameter != null)
            {
                bool reverse = false;
                Boolean.TryParse((string)parameter, out reverse);
                if (reverse)
                    val = !val;
            }

            if (val)
                return new SolidColorBrush(Color.FromRgb(16,12,68)); //161268 30 12 69 25 12 68
            else
                return new SolidColorBrush(Color.FromRgb(6, 4, 26));//6 4 26
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
