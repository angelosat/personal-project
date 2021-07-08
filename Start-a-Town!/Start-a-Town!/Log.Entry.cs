using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public partial class Log
    {
        public class Entry
        {
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
                        Attack attack = Values[2] as Attack;
                        StatCollection dmg = attack.Damage;
                        string text = (Values[0] as GameObject).Name + " hits " + (Values[1] as GameObject).Name + " for " + attack.Value + " damage";
                        bool first = true;
                        foreach (var d in dmg)
                        {
                            if (d.Value == 0)
                                continue;
                            text += " (" + d.Value + " " + d.Key + ")";
                            first = false;
                        }
                        return text;
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

                    case EntryTypes.System:
                        return "[System] " + Values[0];
                    default:
                        return Values[0].ToString();
                }
            }
        }
    }
}
