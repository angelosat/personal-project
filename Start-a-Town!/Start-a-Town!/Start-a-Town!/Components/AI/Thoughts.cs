using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class ThoughtCollection : List<Thought> { }// SortedList<DateTime, Thought> { }

    class Thought
    {
        public string Title, Text;
        public Time Time;
    }
}
