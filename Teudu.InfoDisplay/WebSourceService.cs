using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Teudu.InfoDisplay
{
    public class WebSourceService: ISourceService
    {
        private WebClient wc;
        public void Initialize()
        {
            wc = new WebClient();
        }

        public void BeginPoll()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<SourceEventArgs> EventsUpdated;

        public void Cleanup()
        {
            throw new NotImplementedException();
        }
    }
}
