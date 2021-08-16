using Microsoft.Xna.Framework;
using System.Linq;
using Start_a_Town_.UI;

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
            public ConsoleEntry GetGui()
            {
                return new ConsoleEntry(this.Color, this.ToString());
            }
            public override string ToString()
            {
                return this.ConvertToString();
            }
            string ConvertToString()
            {
                return $"[{this.Source}] {string.Join(" ", this.Values.Select(v => v is string ? v.ToString() : $"[{v}]"))}";
            }
            public static Entry Notification(string text)
            {
                return new Entry(EntryTypes.Notification, new object[] { text }) { Color = Color.Goldenrod };
            }
            public static Entry Warning(string text)
            {
                return new Entry(EntryTypes.Warning, new object[] { text }) { Color = Color.Orange };
            }
            public static Entry Error(string text)
            {
                return new Entry(EntryTypes.Error, new object[] { text }) { Color = Color.Red };
            }
            public static Entry System(string text)
            {
                return new Entry(EntryTypes.System, new object[] { text }) { Color = Color.Yellow };
            }
            public static Entry Chat(object source, string text)
            {
                return new Entry(EntryTypes.Chat, source, new object[] { text });
            }
            public static Entry Network(object source, string text)
            {
                return new Entry(EntryTypes.Network, source, new object[] { text }) { Color = Color.Lime };
            }
        }
    }
}
