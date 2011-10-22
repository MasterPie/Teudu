using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class BoardEventArgs : EventArgs
    {
        public Board Previous { get; set; }
        public Board Board { get; set; }
        public Board Next { get; set; }

        public IBoardService BoardService { get; set; }
    }
}
