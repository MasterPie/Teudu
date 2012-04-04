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
        private string serviceImageRoot;

        private BackgroundWorker XmlDownloadWorker;
        private DispatcherTimer retryTimer;
        private DispatcherTimer eventSyncTimer;

        public override void Initialize()
        {
            base.Initialize();
            serviceURI = ConfigurationManager.AppSettings["EventsServiceURI"].ToString();
            serviceImageRoot = ConfigurationManager.AppSettings["EventsServiceImageRoot"].ToString();

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
                wc.Dispose();
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
                    using (WebClient downloadClient = new WebClient())
                    {
                        downloadClient.DownloadFile(serviceImageRoot + uri, imageDirectory + fileName);
                    }
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
