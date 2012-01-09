// -----------------------------------------------------------------------
// <copyright file="OffsetMovementBoundsConverter.cs" company="">
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
    public class OffsetMovementBoundsXConverter : IValueConverter
    {
        private bool inverted = true;
        private double scaleFactor = 4;
        private double inversionFactor = 1;
        private double scaleFactorTwo = 0.2;

        public OffsetMovementBoundsXConverter()
        {
            if (!Boolean.TryParse(ConfigurationManager.AppSettings["Inverted"], out inverted))
                inverted = false;
            if (!Double.TryParse(ConfigurationManager.AppSettings["CorrespondenceScaleX"], out scaleFactor))
                scaleFactor = 4;
            if (inverted)
                inversionFactor = 1;
            else
                inversionFactor = -1;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return 0;

            double factor = 1920 / (Double.Parse((string)parameter));

            return ((((double)(value) / inversionFactor)/scaleFactor))/scaleFactorTwo / factor;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
