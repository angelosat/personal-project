using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class ConsoleBox : ScrollableBox
    {
        #region Filtering
        public FilterType FilterType = FilterType.Exclude;
        public HashSet<ConsoleMessageTypes> Filters = new HashSet<ConsoleMessageTypes>() { ConsoleMessageTypes.Acks };

        public bool Filter(ConsoleMessageTypes i)
        {
            if (Filters.Contains(i))
                return FilterType == FilterType.Include;
            return FilterType == FilterType.Exclude;
        }
        #endregion

        public bool FadeText { get; set; }
        public bool TimeStamp { get; set; }
        public ConsoleBox(Rectangle bounds)
            : base(bounds)
        {
            this.TimeStamp = true;
            BackgroundColor = Color.Black;
            Opacity = 0.5f;
        }
        public void Write(string name, string text)
        {
            Write(Color.White, name, text);
        }
        public Label Write(string text)
        {
            return Write(Color.White, text);
        }

        public Label Write(Color c, string text)
        {
                if (TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]: "));
                text = UIManager.WrapText(text, Client.Width);
                Label line = new Label(Client.Controls.BottomLeft, text) { TextColorFunc = () => c };

                Add(line);
                Client.ClientLocation.Y = Client.Bottom - Client.ClientSize.Height;
                return line;
        }
        public System.Collections.Concurrent.ConcurrentBag<string> Entries = new System.Collections.Concurrent.ConcurrentBag<string>();
        public Label Write(ConsoleMessageTypes type, Color c, string name, string text)
        {
            this.Entries.Add(text);
            if (!Filter(type))
                return null;
                if (TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]" + "[" + name + "]: "));
                text = UIManager.WrapText(text, Client.Width);
                Label line = new Label(Client.Controls.BottomLeft, text) { TextColorFunc = () => c };

                Add(line);
                Client.ClientLocation.Y = Client.Bottom - Client.ClientSize.Height;
                return line;
        }
        public Label Write(Color c, string name, string text)
        {
                if (TimeStamp)
                    text = text.Insert(0, DateTime.Now.ToString("[HH:mm:ss]" + "[" + name + "]: "));
                text = UIManager.WrapText(text, Client.Width);
                Label line = new Label(Client.Controls.BottomLeft, text) { TextColorFunc = () => c };

                Add(line);
                Client.ClientLocation.Y = Client.Bottom - Client.ClientSize.Height;
                return line;
        }

        public override void Add(params Control[] controls)
        {
            foreach (var c in controls)
                c.Tag = LogWindow.FadeDelay;
            base.Add(controls);
        }

        public override void Update()
        {
            if (FadeText)
                foreach (var label in
                    from c in this.Client.Controls//.ToList() // collection might be modified asynchronously, maybe add new new entries to a queue?
                    where c is Label
                    where c.Tag is float
                    select c)
                {
                    float value = (float)label.Tag - 1;
                    label.Tag = value;
                    label.Opacity = 10 * (float)Math.Sin(Math.PI * (value / (float)LogWindow.FadeDelay));
                    if (value <= 0)
                        label.Tag = null;
                }
            base.Update();
        }
    }
}
