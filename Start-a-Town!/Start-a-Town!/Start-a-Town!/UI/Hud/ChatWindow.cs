using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class Chat : Window
    {
        //  ScrollableBox Box_Text;
        ConsoleBoxAsync Box_Text;
        Panel Panel_Text, Panel_Input;
        Slider Sldr_Opacity;
        Stack<string> History = new Stack<string>(16);
        int HistoryIndex;
        Dictionary<Control, float> Timers = new Dictionary<Control, float>();
        public TextBox TextBox;
        static public float FadeDelay = 6 * Engine.TargetFps;// 360;

        public Chat()
        {
            Title = "Log";
            AutoSize = true;
            Closable = false;
            MouseThrough = true;
            Movable = true;

            Box_Text = Net.Client.Console;
            Box_Text.FadeText = true;
            Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black };
            Panel_Text.Controls.Add(Box_Text);

            Panel_Input = new Panel() { AutoSize = true, Location = Panel_Text.BottomLeft, Color = Color.Black };
            Panel_Input.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Label.DefaultHeight);
            Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            TextBox = new TextBox()
            {
                Width = Panel_Input.ClientSize.Width,
                EnterFunc = (a) =>
                {
                    this.Hide();
                    TextBox.Enabled = false;
                    string gotText = TextBox.Text;
                    TextBox.Text = "";

                    if (this.History.Count == 15)
                        this.History.Pop();
                    this.History.Push(gotText);
                    this.HistoryIndex = -1;

                    Panel_Input.Opacity = 0;
                    if (gotText.Length == 0)
                        return;

                    if (gotText[0] == '/')
                    {
                        //server command
                        Net.Client.PlayerCommand(gotText.TrimStart('/'));
                        //Packet.Create(Net.Client.PacketID, PacketType.PlayerServerCommand, Net.Network.Serialize(writer =>
                        //{
                        //    writer.WriteASCII(gotText.TrimStart('/'));
                        //})).BeginSendTo(Net.Client.Host, Net.Client.RemoteIP);
                    }
                    else
                    //if (gotText.Length > 0)
                    {
                        //Log.Command(gotText);
                        Packet.Create(Net.Client.PacketID, PacketType.Chat, Net.Network.Serialize(writer =>
                        {
                            Net.Client.PlayerData.Write(writer);
                            writer.WriteASCII(gotText);
                        })).BeginSendTo(Net.Client.Host, Net.Client.RemoteIP);
                    }
                },
                EscapeFunc = (a) =>
                {
                    TextBox.Enabled = false;
                    TextBox.Text = "";
                    Controls.Remove(Panel_Input);
                }
                ,
                //TextEnterFunc = (a) =>
                InputFunc = (text, ch) =>
                {
                    if (char.IsControl(ch))
                        return text;
                    return text + ch;
                },
                HistoryNextPrev = this.ChatHistory
            };



            Sldr_Opacity = new Slider(this.Label_Title.TopRight, width: 100, min: 0, max: 100, step: 1, value: Box_Text.Opacity * 100) { Name = "Opacity" };
            Sldr_Opacity.ValueChanged += new EventHandler<EventArgs>(Sldr_Opacity_ValueChanged);

            Panel_Input.Controls.Add(TextBox);
            Client.Controls.Add(Panel_Text, Panel_Input);
            Controls.Add(Client, Sldr_Opacity);
            Location = new Vector2(0, UIManager.Height);// - Height);
            Anchor = new Vector2(0, 1);

            this.SetOpacity(0, true, exclude: Box_Text);//.Client);
            TextBox.Enabled = false;
            SetMousethrough(true, true);

        }

        void ChatHistory(int index)
        {
            if (this.History.Count == 0)
                return;
            // TODO: do something
            this.HistoryIndex += index;

            this.HistoryIndex = Math.Max(0, Math.Min(this.History.Count - 1, this.HistoryIndex));
            this.TextBox.Text = this.History.ElementAt(this.HistoryIndex);

        }

        void Sldr_Opacity_ValueChanged(object sender, EventArgs e)
        {
            //Box_Text.SetOpacity(Sldr_Opacity.Value / 100f, true);
            Box_Text.Opacity = Sldr_Opacity.Value / 100f;
        }

        void Log_EntryAdded(object sender, LogEventArgs e)
        {
            //Write("[System] " + e.Entry.ToString());
            Write(e);//.Entry.ToString());
            //Show();
        }


        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb, this.ScreenBounds);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public override void Update()
        {
            base.Update();
            //if (this.Box_Text.Client.Controls.Count != this.Timers.Count)
            //    this.Refresh();

            if (this.TextBox.Enabled)
                return;

            Dictionary<Control, float> copy = new Dictionary<Control, float>(Timers);
            foreach (KeyValuePair<Control, float> timer in copy)
            {
                float newValue = timer.Value - 1;// GlobalVars.DeltaTime;
                float alpha = 10 * (float)Math.Sin(Math.PI * (newValue / (float)FadeDelay));

                Timers[timer.Key] = newValue;
                timer.Key.Opacity = alpha;

                if (timer.Value <= 0)
                {
                    Timers.Remove(timer.Key);
                    timer.Key.Opacity = 0;
                }
            }
        }


        void Timer_Tick(object sender, EventArgs e)
        {
            Hide();
            // Timer.Stop();
        }

        public void Write(LogEventArgs e)
        {
            string text = e.Entry.ToString();
            if (text.Length == 0)
                return;

            Color c = Color.White;
            switch (e.Entry.Type)
            {
                case Log.EntryTypes.System:
                    c = Color.Yellow;
                    break;
                case Log.EntryTypes.Damage:
                    c = Color.Red;
                    break;
                default:
                    break;
            }
            //string fullText = DateTime.Now.ToString("[HH:mm:ss]") + text;// e.Entry;
            //fullText = UIManager.WrapText(fullText, this.Box_Text.Client.Width);

            Label line = this.Box_Text.Write(c, text);
            line.MouseThrough = true;
            //    new Label(fullText)//DateTime.Now.ToString("[HH:mm:ss]") + text)//e.Entry)
            //{
            //    Opacity = 1,
            //    TextColor = c,
            //    Location = this.Box_Text.Client.Controls.Count > 0 ? this.Box_Text.Client.Controls.Last().BottomLeft : Vector2.Zero,// Instance.Panel_Text.Height - UIManager.Font.MeasureString(fullText).Y),
            //    MouseThrough = this.MouseThrough
            //};
            this.Timers.Add(line, FadeDelay);

            //this.Box_Text.Add(line);
            this.Box_Text.Client.ClientLocation.Y = this.Box_Text.Client.Bottom - this.Box_Text.Client.ClientSize.Height;


        }

        public void WriteOld(LogEventArgs e)
        {
            string text = e.Entry.ToString();
            if (text.Length == 0)
                return;

            Color c = Color.White;
            switch (e.Entry.Type)
            {
                case Log.EntryTypes.System:
                    c = Color.Yellow;
                    break;
                case Log.EntryTypes.Damage:
                    c = Color.Red;
                    break;
                default:
                    break;
            }
            string fullText = DateTime.Now.ToString("[HH:mm:ss]") + text;// e.Entry;
            fullText = UIManager.WrapText(fullText, this.Box_Text.Client.Width);

            Label line = new Label(fullText)//DateTime.Now.ToString("[HH:mm:ss]") + text)//e.Entry)
            {
                Opacity = 1,
                TextColor = c,
                Location = this.Box_Text.Client.Controls.Count > 0 ? this.Box_Text.Client.Controls.Last().BottomLeft : Vector2.Zero,// Instance.Panel_Text.Height - UIManager.Font.MeasureString(fullText).Y),
                MouseThrough = this.MouseThrough
            };
            this.Timers.Add(line, FadeDelay);

            this.Box_Text.Add(line);
            this.Box_Text.Client.ClientLocation.Y = this.Box_Text.Client.Bottom - this.Box_Text.Client.ClientSize.Height;


        }
        internal override void OnControlRemoved(Control control)
        {
            //if (this.Timers.ContainsKey(control))
                this.Timers.Remove(control);
        }
        internal void StartTyping()
        {
            //if(this.TextBox.Enabled)
            //{
            //    this.TextBox.EnterFunc(this.TextBox.Text);
            //    this.TextBox.Text = "";
            //    return;
            //}

            this.SetOpacity(1, true, exclude: Box_Text);//.Client);
            SetMousethrough(false, true);
            // Controls.Add(Panel_Input);
            //   Panel_Input.Controls.Add(TextBox);
            TextBox.Enabled = true;
            foreach (Control label in Box_Text.Client.Controls)
            {
                Timers[label] = FadeDelay / 2f;
                // label.Opacity = 1;
            }
            //  Show();
        }

        public override bool Hide()
        {
            // this.Opacity = 0;

            this.SetOpacity(0, true, exclude: Box_Text);//.Client);
            //      Box_Text.Opacity = 1;

            TextBox.Enabled = false;
            SetMousethrough(true, true);
            // return base.Hide();
            return true;
            //Parent.Controls.Remove(this);
            //return true;
        }

        public override void BringToFront()
        {
            //base.BringToFront();
        }

        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.HandleKeyDown(e);
        }


        public override void Validate()
        {
            Refresh();
            base.Validate();
        }
        private void Refresh()
        {
            this.Timers.Clear();
            foreach (var entry in this.Box_Text.Client.Controls)
                this.Timers.Add(entry, FadeDelay);
        }
        
    }

}
