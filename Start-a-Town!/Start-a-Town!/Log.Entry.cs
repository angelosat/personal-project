﻿using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    public partial class Log
    {
        public class Entry
        {
            public Color Color;
            public EntryTypes Type;
            public object[] Values;
            public Entry(EntryTypes type, params object[] values)
            {
                Type = type;
                Values = values;
            }
            public override string ToString()
            {
                switch (Type)
                {
                    case EntryTypes.Damage:
                        throw new NotImplementedException();
                    case EntryTypes.Death:
                        return (Values[1] as GameObject).Name + " slain by " + (Values[0] as GameObject).Name;
                    case EntryTypes.Equip:
                        return (Values[0] as GameObject).Name + " equipped " + (Values[1] as GameObject).Name;
                    case EntryTypes.Unequip:
                        return (Values[0] as GameObject).Name + " unequipped " + (Values[1] as GameObject).Name;
                    case EntryTypes.Buff:
                        return (Values[0] as GameObject).Name + " is afflicted with " + (Values[1] as GameObject).Name;
                    case EntryTypes.Debuff:
                        return (Values[0] as GameObject).Name + "'s " + (Values[1] as GameObject).Name + " faded ";
                    case EntryTypes.TooHeavy:
                        return "Too heavy!";
                    case EntryTypes.CellChanged:
                        return "";
                    case EntryTypes.Chat:
                        return "[" + (Values[0] == null ? "" : (Values[0] as GameObject).Name) + "] " + Values[1];
                    case EntryTypes.ChatPlayer:
                        return string.Format("[{0}] {1}", Values[0], Values[1]);
                    default:
                        return $"[{Type}] {Values[0]}";
                }
            }

            public static Entry Warning(string text)
            {
                return new Entry(EntryTypes.Warning, text) { Color = Color.Red };
            }
            public static Entry System(string text)
            {
                return new Entry(EntryTypes.System, text) { Color = Color.Yellow };
            }
        }
    }
}
