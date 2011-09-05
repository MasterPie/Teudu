using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface ISourceService
    {
        void Initialize();
        void BeginPoll();
        event EventHandler<SourceEventArgs> EventsUpdated;
        void Cleanup();

    }
}
