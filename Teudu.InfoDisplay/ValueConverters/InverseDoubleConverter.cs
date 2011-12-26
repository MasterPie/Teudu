// -----------------------------------------------------------------------
// <copyright file="InverseDoubleConverter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teudu.InfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Data;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class InverseDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;

            return -val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
