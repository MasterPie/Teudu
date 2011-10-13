﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class MomentaryBoardService : IBoardService
    {
        private List<Event> events;
        private List<Board> boards;
        private int currBoard = 0;

        public MomentaryBoardService()
        {
            boards = new List<Board>();
            boards.Add(new Board("Hot"));
        }

        public List<Event> Events
        {
            get
            {
                return events;
            }
            set
            {
                events = value;
                Sift();
                currBoard = 0;
                if (BoardsUpdated != null)
                    BoardsUpdated(this, new EventArgs());
            }
        }

        public Board Current
        {
            get {
                //if (currBoard >= 0 && currBoard < boards.Count)
                    return boards[(currBoard)%boards.Count];
                //else 
                //    return null;
            }
        }

        public Board Next
        {
            get
            {
                //if (currBoard >= (boards.Count - 1))
                //    return null;
                return boards[(currBoard + 1)%boards.Count];
            }
        }

        public Board Prev
        {
            get
            {
                if (currBoard <= 0)
                    return boards[boards.Count-1];
                return boards[currBoard - 1];
            }
        }

        public bool AdvanceCurrent()
        {
            //if (currBoard < boards.Count - 1)
            //{
                currBoard = (currBoard + 1) % boards.Count;
                return true;
            //}
            //return false;
        }

        public bool RegressCurrent()
        {
            if (currBoard > 0)
            {
                currBoard--;
                
            }
            else
                currBoard = boards.Count;
            //return false;
            return true;
        }

        private void Sift()
        {
            boards.Clear();
            Dictionary<string, Board> boardLookup = new Dictionary<string, Board>();
            boardLookup.Add("hot", new Board("Hot"));

            events.ForEach(x =>
            {
                List<Category> categories = x.Categories;
                categories.ForEach(cat =>
                {
                    if (boardLookup.ContainsKey(cat.Name.ToLower()))
                        boardLookup[cat.Name.ToLower()].Events.Add(x);
                    else
                        boardLookup.Add(cat.Name.ToLower(), new Board(cat.Name) { Events = new List<Event>() {x} });
                });

                if (x.HappeningThisWeek)
                    boardLookup["hot"].Events.Add(x);
            });

            foreach (string key in boardLookup.Keys)
            {
                boards.Add(boardLookup[key]);
            }

            foreach (Board board in boards)
            {
                List<Event> savedEvents = board.Events;
                savedEvents = savedEvents.OrderByDescending(x => x.StartTime).Take(15).ToList<Event>();
                board.Events = savedEvents;
            }
        }

        public void Cleanup()
        {
            //...
        }


        public List<Board> Boards
        {
            get { return boards; }
        }

        public event EventHandler BoardsUpdated;
        public event EventHandler<BoardEventArgs> ActiveBoardChanged;
    }
}
