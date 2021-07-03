using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class LogEventArgs:EventArgs
    {
        public Log.Entry Entry;
        public LogEventArgs(Log.Entry entry)
        {
            Entry = entry;
        }
    }
    public class Log
    {
        static Queue<Entry> EntryQueue = new();
        public enum EntryTypes
        {
            Damage, Death, Equip, Unequip, Default, TooHeavy, Buff, Debuff, Skill, CellChanged,
            /// <summary>
            /// <para>arg0: object</para>
            /// <para>arg1: text</para>
            /// </summary>
            Chat,
            System, Dialogue, DialogueOption, DialogueEnd, Inventory, Thoughts, Jobs, Needs,
            ChatPlayer
        }
        static Log _Instance;
        static public Log Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Log();
                return _Instance;
            }
        }

        static Dictionary<EntryTypes, List<EventHandler<LogEventArgs>>> Events = new Dictionary<EntryTypes,List<EventHandler<LogEventArgs>>>();
        static public void Subscribe(EntryTypes entryType, EventHandler<LogEventArgs> handler)
        {
            List<EventHandler<LogEventArgs>> list;// = Subscribers[entryType];
            if (Events.TryGetValue(entryType, out list))
                list.Add(handler);
            else
                Events[entryType] = new List<EventHandler<LogEventArgs>>() { handler };
        }
        static public void Unsubscribe(EntryTypes entryType, EventHandler<LogEventArgs> handler)
        {
            List<EventHandler<LogEventArgs>> list = Events[entryType];
            list.Remove(handler);
            if (list.Count == 0)
                Events.Remove(entryType);
        }

        //static Dictionary<EntryTypes, List<EventHandler>> Subscribers;
        //static public void Subscribe(EntryTypes entryType, EventHandler handler)
        //{
        //    List<EventHandler> list;// = Subscribers[entryType];
        //    if (Subscribers.TryGetValue(entryType, out list))
        //        list.Add(handler);
        //    else
        //        Subscribers[entryType] = new List<EventHandler>() { handler };
        //}
        //static public void Unsubscribe(EntryTypes entryType, EventHandler handler)
        //{
        //    List<EventHandler> list = Subscribers[entryType];
        //    list.Remove(handler);
        //    if (list.Count == 0)
        //        Subscribers.Remove(entryType);
        //}


        List<Entry> Entries;
        public event EventHandler<LogEventArgs> EntryAdded;
        void OnEntryAdded(LogEventArgs e)
        {
            if (EntryAdded != null)
                EntryAdded(this, e);
            List<EventHandler<LogEventArgs>> list;// = Events[e.Entry.Type];
            if (Events.TryGetValue(e.Entry.Type, out list))
                foreach (EventHandler<LogEventArgs> handler in list)
                    handler.Invoke(this, e);
        }

        public class Entry
        {
            public EntryTypes Type;
            public object[] Values;
            public Entry(EntryTypes type, params object[] values)
            {
                Type = type;
                Values = values;
            }
            static public string Format(Log.EntryTypes type, params object[] items)
            {
                switch (type)
                {
                    //case EntryTypes.Default:
                    //  return Values[0].ToString();
                    case EntryTypes.Damage:
                        Attack attack = items[2] as Attack;
                        //StatsComponent dmg = attack.Values;
                        StatCollection dmg = attack.Damage;
                        string text = (items[0] as GameObject).Name + " hits " + (items[1] as GameObject).Name + " for " + attack.Value + " damage";
                        //bool first = true;
                        foreach (var d in dmg)//.Properties)
                        {
                            if (d.Value == 0)
                                continue;
                            text += " (" + d.Value + " " + d.Key + ")";
                            //first = false;
                        }
                        return text;
                    case EntryTypes.Death:
                        return (items[1] as GameObject).Name + " slain by " + (items[0] as GameObject).Name;
                    case EntryTypes.Equip:
                        return (items[0] as GameObject).Name + " equipped " + (items[1] as GameObject).Name;
                    case EntryTypes.Unequip:
                        return (items[0] as GameObject).Name + " unequipped " + (items[1] as GameObject).Name;
                    case EntryTypes.Buff:
                        //return (Values[0] as GameObject).Name + " is afflicted with " + Stat.StatDB[(Values[1] as ConditionComponent).GetProperty<Stat.Types>("Type")].Name;
                        return (items[0] as GameObject).Name + " is afflicted with " + (items[1] as GameObject).Name;
                    case EntryTypes.Debuff:
                        // return (Values[0] as GameObject).Name + "'s " + Stat.StatDB[(Values[1] as ConditionComponent).GetProperty<Stat.Types>("Type")].Name + " faded ";
                        return (items[0] as GameObject).Name + "'s " + (items[1] as GameObject).Name + " faded ";
                    case EntryTypes.TooHeavy:
                        //return "Not enough " + (Values[0] as Stat).Name;
                        return "Too heavy!";
                    case EntryTypes.CellChanged:
                        return "";
                    case EntryTypes.Chat:
                        //return "[" + (Values[0] as GameObject).Name + "] " + Values[1];
                        return "[" + (items[0] == null ? "" : (items[0] as GameObject).Name) + "] " + items[1];
                    case EntryTypes.ChatPlayer:
                        return string.Format("[{0}] {1}", items[0], items[1]);

                    case EntryTypes.System:
                        return "[System] " + items[0];
                    default:
                        return items[0].ToString();//return "<undefined>";
                }
            }
            public override string ToString()
            {
                //return Type + " " + Values;
                switch (Type)
                {
                    //case EntryTypes.Default:
                      //  return Values[0].ToString();
                    case EntryTypes.Damage:
                        Attack attack = Values[2] as Attack;
                        //StatsComponent dmg = attack.Values;
                        StatCollection dmg = attack.Damage;
                        string text = (Values[0] as GameObject).Name + " hits " + (Values[1] as GameObject).Name + " for " + attack.Value + " damage";
                        bool first = true;
                        foreach (var d in dmg)//.Properties)
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
                        //return (Values[0] as GameObject).Name + " is afflicted with " + Stat.StatDB[(Values[1] as ConditionComponent).GetProperty<Stat.Types>("Type")].Name;
                        return (Values[0] as GameObject).Name + " is afflicted with " + (Values[1] as GameObject).Name;
                    case EntryTypes.Debuff:
                       // return (Values[0] as GameObject).Name + "'s " + Stat.StatDB[(Values[1] as ConditionComponent).GetProperty<Stat.Types>("Type")].Name + " faded ";
                        return (Values[0] as GameObject).Name + "'s " + (Values[1] as GameObject).Name + " faded ";
                    case EntryTypes.TooHeavy:
                        //return "Not enough " + (Values[0] as Stat).Name;
                        return "Too heavy!";
                    case EntryTypes.CellChanged:
                        return "";
                    case EntryTypes.Chat:
                        //return "[" + (Values[0] as GameObject).Name + "] " + Values[1];
                        return "[" + (Values[0] == null ? "" : (Values[0] as GameObject).Name) + "] " + Values[1];
                    case EntryTypes.ChatPlayer:
                        return string.Format("[{0}] {1}", Values[0], Values[1]);

                    case EntryTypes.System:
                        return "[System] " + Values[0];
                    default:
                        return Values[0].ToString();//return "<undefined>";
                }
            }
        }

        static public void Update()
        {
            while (EntryQueue.Count > 0)
            {
                Write(EntryQueue.Dequeue());
            }
        }

        public Log()
        {
            Entries = new List<Entry>();
        }
        static public void Enqueue(EntryTypes type, params object[] values)
        {
            EntryQueue.Enqueue(new Entry(type, values));
        }
        static void Write(Entry entry)
        {
            //      if(Log.Count>0)
            //         Log[Log.Count-1]+
            Instance.OnEntryAdded(new LogEventArgs(entry));
            if (entry.ToString().Length == 0)
                return;
            Instance.Entries.Add(entry);
            LogWindow.Write(new LogEventArgs(entry));
            //  Console.WriteLine(entry);

        }
        static void Write(EntryTypes type, params object[] values)
        {
            Entry entry = new Entry(type, values);
      //      if(Log.Count>0)
       //         Log[Log.Count-1]+
            Instance.OnEntryAdded(new LogEventArgs(entry));
            if (entry.ToString().Length == 0)
                return;
            Instance.Entries.Add(entry);
          //  Console.WriteLine(entry);
            
        }
        readonly static StreamWriter LogFile = new("Log.txt", false) { AutoFlush = true };//, append: true);
        //public static async Task WriteToFile(string text)
        //{
        //    await LogFile.WriteLineAsync(text);
        //}
        public static void WriteToFile(string text)
        {
            var textStamped = $"[{DateTime.Now}] {text}";
            textStamped.ToConsole();
            LogFile.WriteLine(textStamped);
        }
        static void WriteHelp()
        {
            Log.Write(EntryTypes.Default,
                            @"
List of available commands:
/querycell
/querychunk
/give
/set
/updatelight
/updatethumb
/refreshstats
/help
");
        }

        public override string ToString()
        {
            bool first = true;
            string text = "";
            foreach (Entry entry in Entries)
            {
                if (!first)
                    text += "\n";
                text += entry.ToString();
            }
            return text;
        }

        internal static void Command(EntryTypes type, params object[] p)
        {
            GameObject speaker, target;
            string text;
            DialogueOption option;
            switch (type)
            {
                case EntryTypes.Chat:
                    Log.Enqueue(EntryTypes.Chat, p);
                    SpeechBubbleOld.Create(p[0] as GameObject, (string)p[1]);
                    break;

                case EntryTypes.Dialogue:
                    DateTime time = (DateTime)p[0];
                    speaker = p[1] as GameObject;
                    text = (string)p[2];
                    //string[] options = (string[])p[3];
                    //ConversationOptionCollection options = p[3] as ConversationOptionCollection;
                    DialogueOption[] options = (DialogueOption[])p[3];
                    SpeechBubbleOld.Create(time, speaker, text, options.ToArray());
                    Log.Enqueue(EntryTypes.Chat, speaker, text);
                    break;

                case EntryTypes.DialogueOption:
                    speaker = p[0] as GameObject;
                    target = p[1] as GameObject;
                    option = (DialogueOption)p[2];
                    SpeechBubbleOld.Create(speaker, option.Text);
                    throw new NotImplementedException();

                case EntryTypes.DialogueEnd:
                    speaker = p[0] as GameObject;
                    SpeechBubbleOld.Hide(speaker);
                    break;

                case EntryTypes.Inventory:
                    InvWindow.Show(p[0] as GameObject);
                    break;

                case EntryTypes.Needs:
                    NeedsWindow.Show(p[0] as GameObject);
                    break;

                case EntryTypes.Thoughts:
                    ThoughtsWindow.Show(p[0] as GameObject);
                    break;

                //case EntryTypes.Jobs:
                //    NpcJobsWindow.Show(p[0] as GameObject, p[1] as List<JobEntry>);
                //    break;

                default:
                    break;
            }
        }

        internal static void Command(string gotText)
        {
            List<string> words = new List<string>();
            string[] text = gotText.Split('\'');
            for (int i = 0; i < text.Length; i++)
            {
                if (i % 2 == 0)
                    words.AddRange(text[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                else
                    words.Add(text[i]);
            }
           // List<string> words = new List<string>(gotText.Split(' '));
            Queue<string> wordQueue = new Queue<string>(words);
            string msg = wordQueue.Dequeue();
            try
            {
                switch (msg)
                {
                    case "/querycell":
                        QueryCell(words);
                        break;
                    
                    case "/set":
                        Set(wordQueue);
                        break;
                    case "/updatelight":
                        UpdateChunkLight(words);
                        break;
                    case "/updatethumb":
                        UpdateMapThumpnail();
                        break;
                    case "/refreshstats":
                        RefreshStats();
                        break;
                    case "/help":
                        Log.WriteHelp();
                        break;
                    default:
                       // Log.Write(EntryTypes.Default, "Invalid Command");
                        //Log.Help();
                        Log.Enqueue(Log.EntryTypes.Chat, PlayerOld.Actor, gotText);
                        SpeechBubbleOld.Create(PlayerOld.Actor, gotText);
                       // GameObject.PostMessage(Player.Actor, Message.Types.Speak, null, gotText);
                        break;
                }
            }
            catch (Exception) { 
                Log.Write(EntryTypes.Default, "Catastrophic Failure!"); 
                //Log.Help();
            }
        }

        private static void RefreshStats()
        {
            PlayerOld.Actor.PostMessage(Message.Types.ChunkLoaded);
        }

        //private static void Invalidate(List<string> words)
        //{
        //    Chunk chunk;
        //    Vector3 global;
        //    switch (words[1])
        //    {
        //        case "this":
        //            if (Player.Actor == null)
        //                return;
        //            global = Player.Actor.Global;
        //            if (!Position.TryGetChunk(global, out chunk))
        //                return;
        //            chunk.Invalidate();
        //            break;
        //        default:
        //            //global = new Vector3(Convert.ToSingle(words[1]), Convert.ToSingle(words[2]), 0);
        //            if (!ChunkLoader.TryRequest(new Vector2(Convert.ToSingle(words[1]), Convert.ToSingle(words[2])), out chunk))
        //                return;
        //            chunk.Invalidate();
        //            break;
        //    }
        //    Log.Enqueue(EntryTypes.Default, "Chunk " + chunk.MapCoords.ToString() + " invalidated.");
        //}

        private static void UpdateMapThumpnail()
        {
            string worldPath = @"/Saves/Worlds/" + Engine.Map.GetName() + "/";
           // using (Texture2D thumbnail = Map.Instance.GetThumbnail())
           // {
                //using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnail.png", FileMode.OpenOrCreate))
                //{
                //    thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height); //(int)(5 * Chunk.Size * 16), (int)(5 * Chunk.Size * 8));// + 4 * Map.MaxHeight));thumbnail.Width, thumbnail.Height); //thumbnail.Width / 2, thumbnail.Height / 2);
                //    stream.Close();
                //}

            using (Texture2D thumbnail = Engine.Map.GetThumbnail())
                {
                    using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmall.png", FileMode.OpenOrCreate))
                    {
                        thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                        stream.Close();
                    }
                    using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmaller.png", FileMode.OpenOrCreate))
                    {
                        thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                        stream.Close();
                    }
                    using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmallest.png", FileMode.OpenOrCreate))
                    {
                        thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                        stream.Close();
                    }
                }
            //}
        }

        private static void UpdateChunkLight(List<string> words)
        {
            //ChunkLighter.Enqueue(Engine.Map.ActiveChunks[new Vector2(Convert.ToSingle(words[1]), Convert.ToSingle(words[2]))].ResetHeightMap());
        }

        //private static void QueryChunk(List<string> words)
        //{
        //    Chunk chunk;
        //    Vector3 global;
        //    GameObject obj = new GameObject(GameObject.Types.Chunk, "Chunk", "Object representation of a map chunk.");
        //    if (words.Count > 1)
        //        global = new Vector3(Convert.ToSingle(words[1]), Convert.ToSingle(words[2]), 0);
        //    else
        //        global = Player.Actor.Global;
        //    //Position.TryGetChunk(Engine.Map, new Vector3(global.X, global.Y, 0), out chunk);
        //    Engine.Map.TryGetChunk(new Vector3(global.X, global.Y, 0), out chunk);

        //    obj["Chunk"] = chunk;
        //    DebugQueryWindow win = new DebugQueryWindow(obj);
        //    win.Title = "Chunk " + chunk.MapCoords;
        //    win.Show();
        //}

        private static void QueryCell(List<string> words)
        {
            throw new Exception();
            
        }

      
        private static void Set(Queue<string> words)
        {
            string setWhat = words.Dequeue();// words[1];
            switch (setWhat)
            {
                case "ambience":
                    string colorName = words.Peek();//Dequeue();
                    Color color;
                    try
                    {
                        color = (Color)typeof(Color).GetProperty(colorName).GetValue(null, null);
                        Engine.Map.SetAmbientColor(color);
                        break;
                    }
                    catch (Exception e)
                    {
                         //break;
                 //       Enqueue(EntryTypes.System, colorName + " not a valid color");
                    }
                    ////System.Reflection.
                    try
                    {
                        int r = int.Parse(words.Dequeue()),
                            g = int.Parse(words.Dequeue()),
                            b = int.Parse(words.Dequeue());
                        Engine.Map.SetAmbientColor(new Color(r, g, b));
                    }
                    catch (Exception e)
                    {
                        Enqueue(EntryTypes.System, "Invalid color");
                    }
                    //if (!Enum.TryParse(colorName, out color))
                    //{
                    //    Enqueue(EntryTypes.System, colorName + " not a valid color");
                    //    break;
                    //}
                    
                    break;
                case "skill":
                    string skillName = words.Dequeue();//words[2];
                    float value = Convert.ToSingle(words.Dequeue());//words[3]);
                    if (!PlayerOld.Actor["Skills"].Properties.ContainsKey(skillName))
                        Log.Write(EntryTypes.Default, "Invalid Skill");
                    float old = PlayerOld.Actor["Skills"].GetProperty<float>(skillName);
                    PlayerOld.Actor["Skills"][skillName] = value;
                    Log.Write(EntryTypes.Skill, skillName + " set to " + value);
                    break;
                case "hour":
                case "time":
                    throw new Exception();
                    int t = int.Parse(words.Dequeue());//words[2]);
                    //Engine.Map.Clock = new TimeSpan(Engine.Map.Clock.Days, t, Engine.Map.Clock.Minutes, Engine.Map.Clock.Seconds);
                    foreach (var ch in Engine.Map.GetActiveChunks())
                        ch.Value.LightCache.Clear();
                    Enqueue(EntryTypes.System, "Hour set to " + t);
                    break;
                //case "darkness":
                //    Map.Instance.SkyDarkness = byte.Parse(words[2]);
                //    Enqueue(EntryTypes.System, "Sky darkness set to " + Map.Instance.SkyDarkness);
                //    break;
                case "need":
                    try
                    {
                        string target, need;
                        Need.Types needType;
                        switch (words.Count)
                        {
                            case 5:
                                target = words.Dequeue();//words[2];
                                need = words.Dequeue();//words[3];
                                value = float.Parse(words.Dequeue());//words[4]);
                                GameObject targetAgent = NpcComponent.NpcDirectory.First(foo => foo.Name == target);
                                needType = (Need.Types)Int32.Parse(need);
                                targetAgent["Needs"].GetProperty<NeedsCollection>("Needs")[needType].Value = value;
                                break;
                            case 4:
                                need = words.Dequeue();//words[2];
                                value = float.Parse(words.Dequeue());//words[3]);
                                needType = (Need.Types)Int32.Parse(need);
                                PlayerOld.Actor["Needs"].GetProperty<NeedsCollection>("Needs")[needType].Value = value;
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception) { Enqueue(EntryTypes.System, "Syntax: /give '[receiver]' '[object]' [amount = 1]"); }
                    break;
                default:
                    //Log.Write(EntryTypes.System, "Invalid Command");
                    Log.Enqueue(EntryTypes.System, "Invalid command");
                    break;
            }
        }
    }
}
