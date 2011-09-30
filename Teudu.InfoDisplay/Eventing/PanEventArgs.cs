using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class PanEventArgs : EventArgs
    {
        public double HorizontalOffset { get; set; }
        public double VerticalOffset { get; set; }
    }
}
