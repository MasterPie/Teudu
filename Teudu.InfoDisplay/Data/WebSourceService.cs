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
using System.Globalization;
using System.Xml.Linq;

namespace Teudu.InfoDisplay
{
    public class WebSourceService: SourceService
    {
        private XElement root;
        private WebClient wc;      
        private string serviceURI;

        private BackgroundWorker XmlDownloadWorker;
        private DispatcherTimer retryTimer;
        private DispatcherTimer eventSyncTimer;

        public override void Initialize()
        {
            base.Initialize();
            serviceURI = ConfigurationManager.AppSettings["EventsServiceURI"].ToString();
            
            wc = new WebClient();
            wc.Encoding = Encoding.UTF8;

            int pollTime = 3600; //In seconds
            Int32.TryParse(ConfigurationManager.AppSettings["EventsServicePollInterval"], out pollTime);

            if (pollTime < 60)
                pollTime = 3600;
            
            eventSyncTimer = new DispatcherTimer();
            eventSyncTimer.Interval = new TimeSpan(0, 0, pollTime);
            eventSyncTimer.Tick += new EventHandler(eventSyncTimer_Tick);

            retryTimer = new DispatcherTimer();
            retryTimer.Interval = TimeSpan.FromSeconds(30);
            retryTimer.Tick += new EventHandler(retryTimer_Tick);

            XmlDownloadWorker = new BackgroundWorker();
            XmlDownloadWorker.DoWork += new DoWorkEventHandler(XmlDownloadWorker_DoWork);
            XmlDownloadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(XmlDownloadWorker_RunWorkerCompleted);
        }

        public override void BeginPoll()
        {
            if (retryTimer.IsEnabled)
                retryTimer.Stop();

            XmlDownloadWorker.RunWorkerAsync();
            eventSyncTimer.Start();
        }

        /// <summary>
        /// Retries event downloads on error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void retryTimer_Tick(object sender, EventArgs e)
        {
            BeginPoll();
        }

        /// <summary>
        /// Asynchronous routine that downloads the Xml from the webservice, and then calls other routines to download the images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void XmlDownloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string xmlResponse = wc.DownloadString(serviceURI);

                root = XElement.Parse(xmlResponse);
                List<Event> events = ReadEvents(root);
                events = DownloadImages(events);
                e.Result = events;
            }
            catch (Exception)
            {
                retryTimer.Start();
            }
        }

        /// <summary>
        /// Runs immediately after all the events are downloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void XmlDownloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
                return;

            OnEventsUpdated(this, new SourceEventArgs() { Events = (List<Event>)e.Result });

            eventSyncTimer.Start();
        }

        /// <summary>
        /// Redownloads events every time timer goes off
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void eventSyncTimer_Tick(object sender, EventArgs e)
        {
            XmlDownloadWorker.RunWorkerAsync();
            eventSyncTimer.Stop();
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

                        if(node.Attributes.GetNamedItem("id") != null)
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
                                if (!DateTime.TryParseExact(detail.InnerText.Replace('-', '/').Replace("UTC","").Trim(), "yyyy/MM/dd HH:mm:ss", culture, DateTimeStyles.AssumeLocal, out time))
                                    time = DateTime.Now;
                            }

                            if (detail.Name.ToLower().Equals("endtime"))
                            {
                                if (!DateTime.TryParseExact(detail.InnerText.Replace('-', '/').Replace("UTC", "").Trim(), "yyyy/MM/dd HH:mm:ss", culture, DateTimeStyles.AssumeLocal, out endTime))
                                    endTime = time;
                            }

                            if (detail.Name.ToLower().Equals("image"))
                                image = detail.InnerText;

                            if (detail.Name.ToLower().Equals("categories"))
                                detail.InnerText.Split(',').Distinct().ToList().ForEach(x => categories.Add(new Category(x.Trim())));
                        }

                        string imageLoc = DownloadImage(image, id);
                        
                        retEvents.Add(new Event(id, name, description, location, time, endTime, imageLoc, categories));
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }
            return retEvents;
        }

        */
        
        /// <summary>
        /// Downloads all the images in parallel
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        private List<Event> DownloadImages(List<Event> events)
        {
            events.AsParallel().ForAll(x => x.Image = DownloadImage(x.Image, x.ID));
            return events;
        }

        /// <summary>
        /// Downloads a single image. If image does not exist (or on error), set path to image as ""
        /// </summary>
        /// <param name="uri">Url to download image from</param>
        /// <param name="id">ID of event</param>
        /// <returns>New path to downloaded image</returns>
        private string DownloadImage(string uri, int id)
        {
            string fileName = uri;
            if(String.IsNullOrEmpty(fileName))
                fileName = "";
            else
            {
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

        public override void Cleanup()
        {
            retryTimer.Stop();
            eventSyncTimer.Stop();
            wc.Dispose();
        }
    }
}
