using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public class Operand
    {
        public string Label;
        public byte Value;

        public Operand(string label, byte value)
        {
            Label = label;
            Value = value;
        }

        public Operand(string label)
        {
            Label = label;
            Value = 0x00;
        }

        public Operand() 
        {
            Label = "<undefined>";
            Value = 0x00;
        }
    }
}
