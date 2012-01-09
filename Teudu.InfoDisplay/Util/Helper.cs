using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class Helper
    {
        /// <summary>
        /// Returns a date/time string in the form of "10 minutes"
        /// </summary>
        /// <param name="time">time when event occurs</param>
        /// <returns>date/time string</returns>
        public static string ToCountableTime(DateTime time)
        {
            return ToCountableTime("", time);
        }

        /// <summary>
        /// Returns a date/time string in the form of "10 minutes"
        /// </summary>
        /// <param name="prefix">a prefix string like "in" to place in front of some of the time text</param>
        /// <param name="time">time when event occurs</param>
        /// <returns>date/time string</returns>
        public static string ToCountableTime(string prefix, DateTime time)
        {
            string retDateString = "";

            TimeSpan difference = (time - DateTime.Now);


            if (time < DateTime.Now)
                retDateString = "happened in the past";
            else if (time == DateTime.Now)
                retDateString = "happening now";
            else if (DateTime.Now.Date.AddDays(1) == time.Date)
                retDateString = "tomorrow";
            else if (difference.TotalMinutes < 60)
                retDateString = prefix + " " + difference.Minutes.ToString() + " minutes";
            else if (difference.TotalHours < 12)
                retDateString = prefix + " " + difference.Hours.ToString() + " hours";
            else if (difference.TotalHours < 24)
                retDateString = "today";
            else
                retDateString = prefix + " " + difference.Days + " days";

            return retDateString;
        }
    }
}
