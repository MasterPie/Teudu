// -----------------------------------------------------------------------
// <copyright file="OffsetMovementBoundsYConverter.cs" company="">
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
    public class OffsetMovementBoundsYConverter : IValueConverter
    {
        private bool inverted = true;
        private double scaleFactor = 4;
        private double inversionFactor = 1;
        private double scaleFactorTwo = 0.2;

        public OffsetMovementBoundsYConverter()
        {
            if (!Boolean.TryParse(ConfigurationManager.AppSettings["Inverted"], out inverted))
                inverted = false;
            if (!Double.TryParse(ConfigurationManager.AppSettings["CorrespondenceScaleY"], out scaleFactor))
                scaleFactor = 6;
            if (inverted)
                inversionFactor = 1;
            else
                inversionFactor = -1;
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return 0;

            double factor = 1080 / (Double.Parse((string)parameter));

            return (((double)(value) / inversionFactor)/scaleFactor)/scaleFactorTwo / factor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
