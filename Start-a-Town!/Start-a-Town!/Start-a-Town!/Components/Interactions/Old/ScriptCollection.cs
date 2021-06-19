using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.Interactions
{
    public class ScriptCollection : Dictionary<Script.Types, Script>
    {
        public override string ToString()
        {
            string text = "\n";
            this.Values.ToList().ForEach(s => text += s.Name + "\n");
            return text.TrimEnd('\n');
        }
    }
}
