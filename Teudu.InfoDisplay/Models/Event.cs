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
        private string location;
        private bool cancelled;
        private DateTime time;
        private DateTime endTime;
        private string image;
        private List<Category> categories;

        public Event(int eventId, string eventName, string eventDescription, string eventLocation, DateTime eventTime, DateTime eventEndTime, string eventImage, List<Category> eventCategories) : 
            this(eventId, eventName, eventDescription, eventLocation, eventTime, eventEndTime, eventImage, eventCategories, false)
        {
            
        }

        public Event(int eventId, string eventName, string eventDescription, string eventLocation, DateTime eventTime, DateTime eventEndTime, string eventImage, List<Category> eventCategories, bool eventCancelled)
        {
            ID = eventId;
            Name = eventName;
            Description = eventDescription;
            Location = eventLocation;
            StartTime = eventTime;
            EndTime = eventEndTime;
            Image = eventImage;
            Categories = eventCategories;
            Cancelled = eventCancelled;
        }

        public int ID
        {
            set { id = value; }
            get { return id; }
        }

        public string Name
        {
            set { name = value; }
            get { return name.Trim(); }
        }

        public string Description
        {
            set { description = value; }
            get { return description.Trim(); }
        }

        public string Location
        {
            set { location = value; }
            get { return location.Trim(); }
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

        public bool Cancelled
        {
            set { cancelled = value; }
            get { return cancelled; }
        }

        public string PrettyTime()
        {
            string start = time.ToString("dddd, MMMM d, h:mm tt");
            string end = endTime.ToString("h:mm tt");
            if (EndTime.Equals(StartTime))
                return start;
            else
                return start + " - " + end;
        }

        public bool Happened
        {
            get
            {
                return (this.EndTime < DateTime.Now);
            }
        }

        public bool HappeningNow
        {
            get 
            {
                return (this.StartTime <= DateTime.Now && this.EndTime >= DateTime.Now);
            }
        }

        public bool HappeningToday
        {
            get
            {
                return (this.StartTime.Day == DateTime.Now.Day && this.StartTime.Month == DateTime.Now.Month && this.StartTime.Year == DateTime.Now.Year);
            }
        }

        public bool HappeningThisWeek
        {
            get { return time.Subtract(DateTime.Today) <= new TimeSpan(20, 0, 0, 0) && time >= DateTime.Now; }
        }
    }
}
