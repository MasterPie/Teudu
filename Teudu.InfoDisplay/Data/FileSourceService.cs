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

        /// <summary>
        /// Worker method that runs when the event data has been read from the Xml document
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">e.Result contains a List of events that were parsed from the Xml</param>
        void downloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnEventsUpdated(this, new SourceEventArgs(){ Events = (List<Event>)e.Result });
        }

        /// <summary>
        /// Asynchronous worker method that reads the Xml document describing the event data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void downloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Event> events = ReadEvents(doc);
            e.Result = events;
        }

        public override void BeginPoll()
        {
            doc = XElement.Load(eventsFile);

            IOWorker.RunWorkerAsync();
        }


        public override void Cleanup()
        {
            //do nothing?
        }
    }
}
