using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Teudu.InfoDisplay
{
    [ValueConversion(typeof(double), typeof(double))]
    public class OffsetNavigatorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double offset = (double)value;
            double actualPosition = Double.Parse((string)parameter);

            return offset + actualPosition;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
