using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;

namespace Teudu.InfoDisplay
{
    public class FileSourceService : SourceService
    {
        private string eventsFile;
        private XElement doc;
        //private XmlDocument doc;
        private BackgroundWorker IOWorker;

        public FileSourceService()
        {
            eventsFile = "events.xml";
        }
        public override void Initialize()
        {
            base.Initialize();
            IOWorker = new BackgroundWorker();
            IOWorker.DoWork += new DoWorkEventHandler(downloadWorker_DoWork);
            IOWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadWorker_RunWorkerCompleted);
        }

        void downloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnEventsUpdated(this, new SourceEventArgs(){ Events = (List<Event>)e.Result });
        }

        void downloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Event> events = ReadEvents(doc);
            e.Result = events;
        }

        public override void BeginPoll()
        {
            //doc.Load(eventsFile);
            doc = XElement.Load(eventsFile);

            IOWorker.RunWorkerAsync();
           

            
        }
        /*
        private List<Event> ReadEvents()
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            List<Event> retEvents = new List<Event>();
            XmlNode root = doc.DocumentElement;
            try
            {
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.ToLower().Equals("event"))
                    {
                        int id = -1;
                        string name, description, image, location;
                        name = description = image = location = "";
                        DateTime time = new DateTime();
                        DateTime endTime = new DateTime();
                        List<Category> categories = new List<Category>();

                        if (node.Attributes.GetNamedItem("id") != null)
                            Int32.TryParse(node.Attributes.GetNamedItem("id").Value, out id);

                        foreach (XmlNode detail in node.ChildNodes)
                        {
                            if (detail.Name.ToLower().Equals("name"))
                                name = detail.InnerText;

                            if (detail.Name.ToLower().Equals("description"))
                                description = detail.InnerText;

                            if (detail.Name.ToLower().Equals("location"))
                                location = detail.InnerText;

                            if (detail.Name.ToLower().Equals("starttime"))
                            {
                                //if (!DateTime.TryParseExact(detail.InnerText.Replace('-', '/').Replace("UTC", "").Trim(), "yyyy/MM/dd HH:mm:ss", culture, DateTimeStyles.AssumeLocal, out time))
                                //    time = DateTime.Now;
                                if (!DateTime.TryParse(detail.InnerText.Replace("UTC", ""), culture, DateTimeStyles.AssumeLocal, out time))
                                    time = DateTime.Now;
                            }

                            if (detail.Name.ToLower().Equals("endtime"))
                            {
                                //if (!DateTime.TryParseExact(detail.InnerText.Replace('-', '/').Replace("UTC", "").Trim(), "yyyy/MM/dd HH:mm:ss", culture, DateTimeStyles.AssumeLocal, out endTime))
                                //    endTime = time;
                                if (!DateTime.TryParse(detail.InnerText.Replace('-', '/').Replace("UTC", ""), culture, DateTimeStyles.AssumeLocal, out endTime))
                                    endTime = time;
                            }

                            if (detail.Name.ToLower().Equals("image"))
                                image = detail.InnerText;

                            if (detail.Name.ToLower().Equals("categories"))
                                detail.InnerText.Split(',').Distinct().ToList().ForEach(x => categories.Add(new Category(x.Trim())));
                        }

                        retEvents.Add(new Event(id, name, description, location,time, endTime, image, categories));
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                //need to do something with it
            }
            return retEvents;
        }
        */

        public override void Cleanup()
        {
            
        }
    }
}
