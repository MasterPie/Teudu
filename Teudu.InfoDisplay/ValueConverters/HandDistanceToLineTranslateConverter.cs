using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Teudu.InfoDisplay
{
    [ValueConversion(typeof(double), typeof(int))]
    public class HandDistanceToLineTranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;

            //int retValue = (int)(val * 85.7143 - 111.429);
            int retValue = (int)(val * 30*2);
            if (retValue > 30)
                retValue = 30;
            if (retValue < 0)
                retValue = 0;

            return retValue + 40;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
