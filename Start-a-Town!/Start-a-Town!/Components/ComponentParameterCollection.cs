using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components
{
    [Obsolete]
    public class ComponentPropertyCollection : Dictionary<string, object>
    {
        public T GetParameter<T>(string name)
        {
            return (T)this[name];
        }

        public void SetParameter(string name, object value)
        {
            this[name] = value;
        }

        public override string ToString()
        {
            string text = "";
            foreach (KeyValuePair<string, object> property in this)
            {
                string propText = (property.Value != null ? property.Value.ToString() : "<null>");
                text += "[" + property.Key + ": " + propText + "]\n";
            }

            return text.TrimEnd('\n'); ;
        }

        public void Add(string name, object value = null)
        {
            base.Add(name, value);
        }
    }
}
