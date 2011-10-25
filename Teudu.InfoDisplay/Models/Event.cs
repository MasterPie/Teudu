using System;
using System.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class Event
    {
        public static string DefaultImage = "default.jpg";
        private int id;
        private string name;
        private string description;
        private DateTime time;
        private DateTime endTime;
        private string image;
        private List<Category> categories;

        public Event(int eventId, string eventName, string eventDescription, DateTime eventTime, DateTime eventEndTime, string eventImage, List<Category> eventCategories)
        {
            ID = eventId;
            Name = eventName;
            Description = eventDescription;
            StartTime = eventTime;
            EndTime = eventEndTime;
            Image = eventImage;
            Categories = eventCategories;
        }

        public int ID
        {
            set { id = value; }
            get { return id; }
        }

        public string Name
        {
            set { name = value; }
            get { return name; }
        }

        public string Description
        {
            set { description = value; }
            get { return description; }
        }

        public DateTime StartTime
        {
            set { time = value; }
            get { return time; }
        }

        public DateTime EndTime
        {
            set { endTime = value; }
            get { return endTime; }
        }

        public string Image
        {
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    image = DefaultImage;
                }
                else
                    image = value; }
            get { return image; }
        }

        public List<Category> Categories
        {
            set { categories = value; }
            get { return categories; }
        }

        public string PrettyTime()
        {
            string start = time.ToString("dddd, MMMM d, h:mm tt");
            string end = endTime.ToString("h:mm tt");
            return start + " - " + end;
        }

        public bool HappeningThisWeek
        {
            get { return time.Subtract(DateTime.Today) <= new TimeSpan(20, 0, 0, 0) && time >= DateTime.Now; }
        }
    }
}
