using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teudu.InfoDisplay
{
    public class Board
    {
        private string name;
        private List<Event> events;

        public Board(string boardName)
        {
            Name = boardName;
            events = new List<Event>();
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Event> Events
        {
            get { return events; }
            set { events = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
