using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class HoveredEventArgs : EventArgs
    {
        public Event CurrentEvent {get;set;}
    }
}
