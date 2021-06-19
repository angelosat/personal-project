using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    

    public class ComponentPropertyCollection : Dictionary<string, object>
    {
        public T GetParameter<T>(string name)// where T : ComponentParameter
        {
            //return (T)Collection[name];
            return (T)this[name];
        }

        public void SetParameter(string name, object value)
        {
            this[name] = value;
        }

        //public void Add(Stat.Types stat, ComponentProperty parameter)
        //{
        //    Add(Stat.StatDB[stat].Name, parameter);
        //}

        public override string ToString()
        {
            string text = "";
            foreach (KeyValuePair<string, object> property in this)
            {
                string propText = (property.Value != null ? property.Value.ToString() : "<null>");
                //text += "[" + property.Key + ": " + (propText.Contains('\n') ? "\n" : "") + propText + "]\n";
                text += "[" + property.Key + ": " + propText + "]\n";
            }

            return text.TrimEnd('\n'); ;
        }

        public void Add(string name, object value = null)
        {
            //base.Add(name, value != null ? value : GlobalVars.Undefined);
            base.Add(name, value);
        }
    }

}
