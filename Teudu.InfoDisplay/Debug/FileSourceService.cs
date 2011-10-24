using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.ComponentModel;

namespace Teudu.InfoDisplay.Test
{
    public class FileSourceService : ISourceService
    {
        private string eventsFile;
        private XmlDocument doc;
        private BackgroundWorker downloadWorker;

        public FileSourceService(string file)
        {
            eventsFile = file;
        }
        public void Initialize()
        {
            doc = new XmlDocument();
            downloadWorker = new BackgroundWorker();
            downloadWorker.DoWork += new DoWorkEventHandler(downloadWorker_DoWork);
            downloadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadWorker_RunWorkerCompleted);
        }

        void downloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (EventsUpdated != null)
                EventsUpdated(this, new SourceEventArgs()
                {
                    Events = (List<Event>)e.Result
                });
        }

        void downloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Event> events = ReadEvents();
            e.Result = events;
        }

        public void BeginPoll()
        {
            doc.Load(eventsFile);

            downloadWorker.RunWorkerAsync();
           

            
        }

        private List<Event> ReadEvents()
        {
            List<Event> retEvents = new List<Event>();
            XmlNode root = doc.DocumentElement;
            try
            {
                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.Name.ToLower().Equals("event"))
                    {
                        int id = -1;
                        string name, description, image;
                        name = description = image = "";
                        DateTime time = new DateTime();
                        List<Category> categories = new List<Category>();

                        Int32.TryParse(node.Attributes.GetNamedItem("id").Value, out id);

                        foreach (XmlNode detail in node.ChildNodes)
                        {
                            if (detail.Name.ToLower().Equals("name"))
                                name = detail.InnerText;

                            if (detail.Name.ToLower().Equals("description"))
                                description = detail.InnerText;

                            if (detail.Name.ToLower().Equals("datetime"))
                            {
                                if (!DateTime.TryParse(detail.InnerText, out time))
                                    time = DateTime.Now;
                            }

                            if (detail.Name.ToLower().Equals("image"))
                                image = detail.InnerText;
                        }

                        retEvents.Add(new Event(id, name, description, time, time.AddHours(1), image, categories));
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

        public event EventHandler<SourceEventArgs> EventsUpdated;

        public void Cleanup()
        {
            
        }



    }
}
