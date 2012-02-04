using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public interface IBoardService
    {
        /// <summary>
        /// List of events that are candidates for displaying on the boards
        /// </summary>
        List<Event> Events { get; set; }
        /// <summary>
        /// The various boards (categories) that the interface is divided up into
        /// </summary>
        List<Board> Boards { get; }
        /// <summary>
        /// The current board selected
        /// </summary>
        Board Current{get;}
        /// <summary>
        /// The next board (if exists) that is adjacent to the currently selected board
        /// </summary>
        Board Next { get; }
        /// <summary>
        /// The previous board (if exists) that is adjacent to the currently selected board
        /// </summary>
        Board Prev { get; }
        /// <summary>
        /// Resets the BoardService
        /// </summary>
        /// <returns></returns>
        bool Reset();
        /// <summary>
        /// Advances the currently selected board to the next board in succession
        /// </summary>
        /// <returns>true if a next board was selected</returns>
        bool AdvanceCurrent();
        /// <summary>
        /// Regresses the currently selected board to the previous board
        /// </summary>
        /// <returns>true if a previous board was selected</returns>
        bool RegressCurrent();
        /// <summary>
        /// Releases all resources
        /// </summary>
        void Cleanup();
        /// <summary>
        /// Event raised when the boards are regenerated
        /// </summary>
        event EventHandler BoardsUpdated;
        /// <summary>
        /// Event raised when a previous board becomes the currently selected board
        /// </summary>
        event EventHandler<BoardEventArgs> BoardRegressed;
        /// <summary>
        /// Event raised when a next board becomes the currently selected board
        /// </summary>
        event EventHandler<BoardEventArgs> BoardAdvanced;
    }
}
