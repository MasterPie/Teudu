using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface ISourceService
    {
        /// <summary>
        /// Initializes the SourceService
        /// </summary>
        void Initialize();
        /// <summary>
        /// Starts the polling cycle. The SourceService will download a new event listing periodically based on the polling interval specified in config
        /// </summary>
        void BeginPoll();
        /// <summary>
        /// Event raised when new events have been downloaded
        /// </summary>
        event EventHandler<SourceEventArgs> EventsUpdated;
        /// <summary>
        /// Releases all resources
        /// </summary>
        void Cleanup();

    }
}
