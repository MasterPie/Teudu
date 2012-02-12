// -----------------------------------------------------------------------
// <copyright file="HandDistanceToOpacityConverter.cs" company="">
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
using System.Configuration;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class HandDistanceToOpacityConverter : IValueConverter
    {
        double invisibleScreenLocation = 1.3;
        public HandDistanceToOpacityConverter()
        {
            if (!Double.TryParse(ConfigurationManager.AppSettings["InvisibleScreenLocation"], out invisibleScreenLocation))
                invisibleScreenLocation = 1.3;

            invisibleScreenLocation = invisibleScreenLocation - 0.7;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double val = (double)value;

            if (val < 0)
                val = 0;

            System.Console.WriteLine(val / invisibleScreenLocation);
            return val / invisibleScreenLocation;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
