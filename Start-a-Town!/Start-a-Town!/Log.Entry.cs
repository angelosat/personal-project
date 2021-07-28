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
                switch (this.Type)
                {
                    case EntryTypes.Damage:
                        throw new NotImplementedException();
                    case EntryTypes.Death:
                        return (this.Values[1] as GameObject).Name + " slain by " + (this.Values[0] as GameObject).Name;
                    case EntryTypes.Equip:
                        return (this.Values[0] as GameObject).Name + " equipped " + (this.Values[1] as GameObject).Name;
                    case EntryTypes.Unequip:
                        return (this.Values[0] as GameObject).Name + " unequipped " + (this.Values[1] as GameObject).Name;
                    case EntryTypes.Buff:
                        return (this.Values[0] as GameObject).Name + " is afflicted with " + (this.Values[1] as GameObject).Name;
                    case EntryTypes.Debuff:
                        return (this.Values[0] as GameObject).Name + "'s " + (this.Values[1] as GameObject).Name + " faded ";
                    case EntryTypes.TooHeavy:
                        return "Too heavy!";
                    case EntryTypes.CellChanged:
                        return "";
                    case EntryTypes.Chat:
                        return "[" + (this.Values[0] == null ? "" : (this.Values[0] as GameObject).Name) + "] " + this.Values[1];
                    case EntryTypes.ChatPlayer:
                        return string.Format("[{0}] {1}", this.Values[0], this.Values[1]);
                    default:
                        return $"[{this.Type}] {this.Values[0]}";
                }
            }
            string ConvertToString()
            {
                return $"[{this.Source}] {string.Join(" ", this.Values.Select(v => v is string ? v.ToString() : $"[{v}]"))}";
                //return $"[{this.Type}] {string.Join(" ", this.Values.Select(v => v is string ? v.ToString() : $"[{v}]"))}";
            }
            public static Entry Warning(string text)
            {
                return new Entry(EntryTypes.Warning, new object[] { text }) { Color = Color.Red };
            }
            public static Entry System(string text)
            {
                return new Entry(EntryTypes.System, new object[] { text }) { Color = Color.Yellow };
            }
            public static Entry Chat(object source, string text)// object[] values)
            {
                return new Entry(EntryTypes.Chat, source, new object[] { text });
                //return new Entry(EntryTypes.Chat, text) { Values= new object[] { player, text }, Color = Color.Yellow };
            }
        }
    }
}
