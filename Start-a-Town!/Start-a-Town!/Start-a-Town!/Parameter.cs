using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public class Parameter
    {
        public string Label;
        public object Value;

        public Parameter(string label, object value)
        {
            Label = label;
            Value = value;
        }

        public Parameter() 
        {
            Label = "<undefined>";
        }
    }
}
