using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    class NeedsCollection : Dictionary<Need.Types, Need>
    {
        //public Need this[string name] { get { return ByName[name]; } }
        //public Dictionary<string, Need> ByName { get { return this.ToDictionary(foo => foo.Name); } }

        public NeedsCollection(params Need[] needs)
        {
            foreach (var need in needs)
                //this.Add(need.Name, need);
                this.Add(need.ID, need);
        }

        static public NeedsCollection Empty { get { return new NeedsCollection(); } }

        public override string ToString()
        {
            string text = "";
            foreach (var need in this.Values)
                text += "[" + need.Name + ": " + need.Value.ToString() + " (" + (need.Decay > 0 ? "+" : "-") + need.Decay.ToString("n2") + ")]\n";
            return text;
        }
    }
    
}
