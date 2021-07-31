using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Start_a_Town_
{
    public partial class Log
    {
        public class Entry
        {
            public Color Color = Color.White;
            public object Source;
            public EntryTypes Type;
            public object[] Values;
            public Entry(EntryTypes type, object[] values)
            {
                this.Type = type;
                this.Source = type;
                this.Values = values;
            }
            public Entry(EntryTypes type, object source, object[] values)
            {
                this.Type = type;
                this.Source = source;
                this.Values = values;
            }
           
            public override string ToString()
            {
                return this.ConvertToString();
            }
            string ConvertToString()
            {
                return $"[{this.Source}] {string.Join(" ", this.Values.Select(v => v is string ? v.ToString() : $"[{v}]"))}";
            }
            public static Entry Warning(string text)
            {
                return new Entry(EntryTypes.Warning, new object[] { text }) { Color = Color.Red };
            }
            public static Entry System(string text)
            {
                return new Entry(EntryTypes.System, new object[] { text }) { Color = Color.Yellow };
            }
            public static Entry Chat(object source, string text)
            {
                return new Entry(EntryTypes.Chat, source, new object[] { text });
            }
        }
    }
}
