﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class LogEventArgs : EventArgs
    {
        public Log.Entry Entry;
        public LogEventArgs(Log.Entry entry)
        {
            Entry = entry;
        }
    }
    public partial class Log
    {
        static readonly Queue<Entry> EntryQueue = new();
        public enum EntryTypes
        {
            Damage, Death, Equip, Unequip, Default, TooHeavy, Buff, Debuff, Skill, CellChanged,
            /// <summary>
            /// <para>arg0: object</para>
            /// <para>arg1: text</para>
            /// </summary>
            Chat,
            System, Dialogue, DialogueOption, DialogueEnd, Jobs,
            ChatPlayer
        }
        static Log _Instance;
        static public Log Instance => _Instance ??= new Log();
        
        static Dictionary<EntryTypes, List<EventHandler<LogEventArgs>>> Events = new();

        List<Entry> Entries;
        void OnEntryAdded(LogEventArgs e)
        {
            List<EventHandler<LogEventArgs>> list;
            if (Events.TryGetValue(e.Entry.Type, out list))
                foreach (EventHandler<LogEventArgs> handler in list)
                    handler.Invoke(this, e);
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
            Instance.OnEntryAdded(new LogEventArgs(entry));
            if (entry.ToString().Length == 0)
                return;
            Instance.Entries.Add(entry);
            LogWindow.Write(new LogEventArgs(entry));
        }
        static void Write(EntryTypes type, params object[] values)
        {
            Entry entry = new Entry(type, values);
            Instance.OnEntryAdded(new LogEventArgs(entry));
            if (entry.ToString().Length == 0)
                return;
            Instance.Entries.Add(entry);
        }
        readonly static StreamWriter LogFile = new("Log.txt", false) { AutoFlush = true };
     
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
                    DialogueOption[] options = (DialogueOption[])p[3];
                    SpeechBubbleOld.Create(time, speaker, text, options.ToArray());
                    Log.Enqueue(EntryTypes.Chat, speaker, text);
                    break;

                case EntryTypes.DialogueOption:
                    speaker = p[0] as GameObject;
                    option = (DialogueOption)p[2];
                    SpeechBubbleOld.Create(speaker, option.Text);
                    throw new NotImplementedException();

                case EntryTypes.DialogueEnd:
                    speaker = p[0] as GameObject;
                    SpeechBubbleOld.Hide(speaker);
                    break;

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
            Queue<string> wordQueue = new Queue<string>(words);
            string msg = wordQueue.Dequeue();
            try
            {
                switch (msg)
                {
                    case "/set":
                        Set(wordQueue);
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
                        Log.Enqueue(Log.EntryTypes.Chat, PlayerOld.Actor, gotText);
                        SpeechBubbleOld.Create(PlayerOld.Actor, gotText);
                        break;
                }
            }
            catch (Exception) { 
                Log.Write(EntryTypes.Default, "Catastrophic Failure!"); 
            }
        }

        private static void RefreshStats()
        {
            PlayerOld.Actor.PostMessage(Message.Types.ChunkLoaded);
        }

        private static void UpdateMapThumpnail()
        {
            string worldPath = @"/Saves/Worlds/" + Engine.Map.GetName() + "/";

            using Texture2D thumbnail = Engine.Map.GetThumbnail();
            using (FileStream stream = new(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmall.png", FileMode.OpenOrCreate))
            {
                thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                stream.Close();
            }
            using (FileStream stream = new(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmaller.png", FileMode.OpenOrCreate))
            {
                thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                stream.Close();
            }
            using (FileStream stream = new(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmallest.png", FileMode.OpenOrCreate))
            {
                thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                stream.Close();
            }
        }

        private static void Set(Queue<string> words)
        {
            string setWhat = words.Dequeue();
            switch (setWhat)
            {
                case "ambience":
                    string colorName = words.Peek();
                    Color color;
                    try
                    {
                        color = (Color)typeof(Color).GetProperty(colorName).GetValue(null, null);
                        Engine.Map.SetAmbientColor(color);
                        break;
                    }
                    catch (Exception e)
                    {
                    }
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
                    break;
                case "skill":
                    string skillName = words.Dequeue();
                    float value = Convert.ToSingle(words.Dequeue());
                    if (!PlayerOld.Actor["Skills"].Properties.ContainsKey(skillName))
                        Log.Write(EntryTypes.Default, "Invalid Skill");
                    float old = PlayerOld.Actor["Skills"].GetProperty<float>(skillName);
                    PlayerOld.Actor["Skills"][skillName] = value;
                    Log.Write(EntryTypes.Skill, skillName + " set to " + value);
                    break;
                case "hour":
                case "time":
                    throw new Exception();
                    int t = int.Parse(words.Dequeue());
                    foreach (var ch in Engine.Map.GetActiveChunks())
                        ch.Value.LightCache.Clear();
                    Enqueue(EntryTypes.System, "Hour set to " + t);
                    break;
                case "need":
                    try
                    {
                        string target, need;
                        Need.Types needType;
                        switch (words.Count)
                        {
                            case 5:
                                target = words.Dequeue();
                                need = words.Dequeue();
                                value = float.Parse(words.Dequeue());
                                GameObject targetAgent = NpcComponent.NpcDirectory.First(foo => foo.Name == target);
                                needType = (Need.Types)Int32.Parse(need);
                                targetAgent["Needs"].GetProperty<NeedsCollection>("Needs")[needType].Value = value;
                                break;
                            case 4:
                                need = words.Dequeue();
                                value = float.Parse(words.Dequeue());
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
                    Log.Enqueue(EntryTypes.System, "Invalid command");
                    break;
            }
        }
    }
}
