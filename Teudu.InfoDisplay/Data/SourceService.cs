// -----------------------------------------------------------------------
// <copyright file="SourceService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Teudu.InfoDisplay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;
    using System.Configuration;
    using System.Globalization;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class SourceService : ISourceService
    {
        protected string imageDirectory;

        public virtual void Initialize()
        {
            imageDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\" + ConfigurationManager.AppSettings["CachedImageDirectory"] + @"\";
        }

        public abstract void BeginPoll();

        protected List<Event> ReadEvents(XElement root)
        {
            if (!root.HasElements)
                return new List<Event>();

            CultureInfo culture = CultureInfo.CurrentCulture;
            List<Event> retEvents = new List<Event>();
            foreach (XElement ev in root.Elements("event"))
            {
                int id = -1;
                string name, description, image, location;
                name = description = image = location = "";
                DateTime time = new DateTime();
                DateTime endTime = new DateTime();
                List<Category> categories = new List<Category>();

                XAttribute aId = ev.Attribute("id");
                if (aId == null)
                    continue;

                Int32.TryParse(aId.Value, out id);

                foreach (XElement detail in ev.Elements())
                {
                    if (detail.Name.LocalName.ToLower().Equals("description"))
                        description = detail.Value;

                    if (detail.Name.LocalName.ToLower().Equals("location"))
                        location = detail.Value;

                    if (detail.Name.LocalName.ToLower().Equals("starttime"))
                    {
                        if (!DateTime.TryParseExact(detail.Value.Replace('-', '/').Replace("UTC", "").Trim(), "yyyy/MM/dd HH:mm:ss", culture,
                            DateTimeStyles.AssumeLocal, out time))
                            continue;
                    }

                    if (detail.Name.LocalName.ToLower().Equals("endtime"))
                    {
                        if (!DateTime.TryParseExact(detail.Value.Replace('-', '/').Replace("UTC", "").Trim(), "yyyy/MM/dd HH:mm:ss", culture,
                            DateTimeStyles.AssumeLocal, out endTime))
                            continue;
                    }

                    if (detail.Name.LocalName.ToLower().Equals("image"))
                        image = detail.Value;

                    if (detail.Name.LocalName.ToLower().Equals("categories"))
                        detail.Value.Split(',').Distinct().ToList().ForEach(x => categories.Add(new Category(x.Trim())));
                }
                retEvents.Add(new Event(id, name, description, location, time, endTime, image, categories));
            }

            return retEvents;
        }

        protected virtual void OnEventsUpdated(object sender, SourceEventArgs e)
        {
            if(EventsUpdated != null)
                EventsUpdated(sender, e);
        }

        public abstract void Cleanup();
        public event EventHandler<SourceEventArgs> EventsUpdated;
    }
}
