using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class Helper
    {
        public static string ToSensibleDate(string prefix, double minutes)
        {
            string sensibleDate = "";
            if (minutes < 60)
                sensibleDate = prefix + " " + minutes.ToString() + " minutes";

            double hours = TimeSpan.FromMinutes(minutes).TotalHours;


            if (hours < 24)
                sensibleDate = prefix + " " + Math.Round(hours, 0, MidpointRounding.AwayFromZero).ToString() + " hours";
            else if (hours < 48)
                sensibleDate = " tomorrow";
            else
                sensibleDate = prefix + " " + Math.Round(TimeSpan.FromMinutes(minutes).TotalDays, 0, MidpointRounding.AwayFromZero) + " days";



            return sensibleDate;

        }
    }
}
