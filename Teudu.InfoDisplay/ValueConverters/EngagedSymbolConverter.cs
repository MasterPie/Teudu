using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Teudu.InfoDisplay
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class EngagedSymbolConverter : IValueConverter
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
                return "+";
            else
                return "ⵁ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
