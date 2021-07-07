using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public enum ConsoleMessageTypes { Acks }

    public class ConsoleBoxAsync : ScrollableBox
    {
        public readonly object Lock = new object();
        public FilterType FilterType = FilterType.Exclude;
        public HashSet<ConsoleMessageTypes> Filters = new HashSet<ConsoleMessageTypes>() { ConsoleMessageTypes.Acks };

        public bool Filter(ConsoleMessageTypes i)
        {
            if (Filters.Contains(i))
                return FilterType == FilterType.Include;
            return FilterType == FilterType.Exclude;
        }

        public bool FadeText;
        public bool TimeStamp = true;
        public ConsoleBoxAsync(Rectangle bounds)
            : base(bounds)
        {
            LoadConfig();
            BackgroundColor = Color.Black;
            Opacity = 0f;
        }

        public ConcurrentBag<string> Entries = new System.Collections.Concurrent.ConcurrentBag<string>();

        public Label Write(string name, string text)
        {
            return Write(Color.White, name, text);
        }
        public Label Write(string text)
        {
            return Write(Color.White, text);
        }
        public Label Write(Color c, string text)
        {
            // TODO i don't want to write directly here. write to the logwindow that has this consoleboxasync bound
            this.Entries.Add(text);
#if DEBUG
            text.ToConsole();
#endif
            lock (Lock)
            {
                if (TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]"));
                text = UIManager.WrapText(text, Client.Width);

                if (this.Client.Controls.Count >= 16)
                {
                    this.Remove(this.Client.Controls.First());
                }
                var line = new Label(text) {  TextColorFunc = () => c };
                line.BackgroundColor = Color.Black * .5f;

                Add(line); //add line after removing oldest one so that the box height doesn't increase?
                this.AlignTopToBottom();

                Client.ClientLocation.Y = Client.Bottom - Client.ClientSize.Height;
                return line;
            }
        }
        public Label Write(ConsoleMessageTypes type, Color c, string name, string text)
        {
            this.Entries.Add(text);
            if (!Filter(type))
                return null;
            lock (Lock)
            {
                if (TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]" + "[" + name + "]: "));
                text = UIManager.WrapText(text, Client.Width);
                var line = new Label(Client.Controls.BottomLeft, text) { TextColorFunc = () => c };

                Add(line);
                Client.ClientLocation.Y = Client.Bottom - Client.ClientSize.Height;
                return line;
            }
        }
        public Label Write(Color c, string name, string text)
        {
            this.Entries.Add(text);
            lock (Lock)
            {
                text = text.Insert(0, DateTime.Now.ToString("[" + name + "]: "));
                return this.Write(c, text);
            }
        }

        public override void Add(params Control[] controls)
        {
            foreach (var c in controls)
                c.Tag = LogWindow.FadeDelay;
            base.Add(controls);
        }
        private void LoadConfig()
        {
            Engine.Config.Root.TryGetValue("Timestamps", v =>
            {
                bool parsed;
                if (bool.TryParse(v, out parsed))
                    this.TimeStamp = parsed;
            });
        }
    }
}
