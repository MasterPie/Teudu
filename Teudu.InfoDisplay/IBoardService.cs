using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface IBoardService
    {
        List<Event> Events { get; set; }
        List<Board> Boards { get; }
        Board Current{get;}
        Board Next { get; }
        Board Prev { get; }
        bool AdvanceCurrent();
        bool RegressCurrent();
        void Cleanup();

        event EventHandler BoardsUpdated;
        event EventHandler<BoardEventArgs> BoardRegressed;
        event EventHandler<BoardEventArgs> BoardAdvanced;
    }
}
