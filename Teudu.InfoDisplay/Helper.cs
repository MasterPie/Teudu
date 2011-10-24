using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class Helper
    {
        public static string ToSensibleDate(double minutes)
        {
            string sensibleDate = "";
            if (minutes < 60)
                sensibleDate = minutes.ToString() + " minutes";

            int hours = TimeSpan.FromMinutes(minutes).Hours;

            if (hours < 24)
                sensibleDate = hours.ToString() + " hours";
            else
                sensibleDate = TimeSpan.FromMinutes(minutes).Days + " days";

            return sensibleDate;

        }
    }
}
