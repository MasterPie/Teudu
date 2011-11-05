using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Teudu.InfoDisplay
{
    [ValueConversion(typeof(double), typeof(double))]
    public class SlowedOffsetNavigationConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double offset = (double)value;
            return offset/10;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double slowedOffset = (double)value;
            return slowedOffset * 10;
        }
    }
}
