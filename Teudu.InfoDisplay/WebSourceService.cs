using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;

namespace Teudu.InfoDisplay
{
    public class WebSourceService: ISourceService
    {
        private XmlDocument doc;
        private WebClient wc;
        private string imageDirectory;
        private string xmlResponse;
        private string serviceURI;

        private BackgroundWorker downloadWorker;
        private DispatcherTimer timer;

        public void Initialize()
        {
            imageDirectory = AppDomain.CurrentDomain.BaseDirectory + @"\" + ConfigurationManager.AppSettings["CachedImageDirectory"] + @"\";
            doc = new XmlDocument();
            serviceURI = ConfigurationManager.AppSettings["EventsServiceURI"].ToString();
            wc = new WebClient();

            int pollTime = 60*60;
            Int32.TryParse(ConfigurationManager.AppSettings["EventsServicePollInterval"], out pollTime);

            if (pollTime < 60)
                pollTime = 60 * 60;

            pollTime = 5;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, pollTime);
            timer.Tick += new EventHandler(timer_Tick);

            downloadWorker = new BackgroundWorker();
            downloadWorker.DoWork += new DoWorkEventHandler(downloadWorker_DoWork);
            downloadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(downloadWorker_RunWorkerCompleted);
        }

        void downloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                return;

            if (EventsUpdated != null)
                EventsUpdated(this, new SourceEventArgs() { Events = (List<Event>)e.Result });

            timer.Start();
        }

        void downloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            xmlResponse = wc.DownloadString(serviceURI);

            doc.LoadXml(xmlResponse);

            List<Event> events = ReadEvents();
            e.Result = events;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            downloadWorker.RunWorkerAsync();
            timer.Stop();
        }

        public void BeginPoll()
        {
            downloadWorker.RunWorkerAsync();
            //timer.Start();
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

                        if(node.Attributes.GetNamedItem("id") != null)
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

                        string imageLoc = DownloadImage(image, id);
                        
                        retEvents.Add(new Event(id, name, description, time, imageLoc, categories));
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
            return retEvents;
        }

        private string DownloadImage(string uri, int id)
        {
            string fileName = uri;
            if(String.IsNullOrEmpty(fileName))
                fileName = "";
            else{
                fileName = id.ToString();

                try
                {
                    wc.DownloadFile(uri, imageDirectory + fileName);
                }
                catch (Exception)
                {
                    fileName = "";
                }
            }
            return fileName;
        }

        public event EventHandler<SourceEventArgs> EventsUpdated;

        public void Cleanup()
        {
            timer.Stop();
            wc.Dispose();
        }
    }
}
