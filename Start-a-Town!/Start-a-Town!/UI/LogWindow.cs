using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class LogWindow : Window
    {
        ConsoleBoxAsync Box_Text;
        Panel Panel_Text, Panel_Input;
        Slider Sldr_Opacity;

        Dictionary<Control, float> Timers;
        public TextBox TextBox;
        static public float FadeDelay = 6 * Engine.TicksPerSecond;

        static LogWindow _Instance;
        static public LogWindow Instance => _Instance ??= new LogWindow();

        public LogWindow()
        {
            Title = "Log";
            AutoSize = true;
            Closable = false;
            MouseThrough = true;

            Box_Text = Net.Client.Instance.Log;
            Box_Text.FadeText = true;
            Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black }; 
            Panel_Text.Controls.Add(Box_Text);

            Panel_Input = new Panel() { Location = Panel_Text.BottomLeft, Color = Color.Black };
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

                    Panel_Input.Opacity = 0;
                    if (gotText.Length == 0)
                        return;

                    if (gotText.Length > 0)
                    {
                        Packet.Create(Net.Client.Instance.PacketID, PacketType.Chat, Network.Serialize(writer =>
                        {
                            Net.Client.Instance.PlayerData.Write(writer);
                            writer.WriteASCII(gotText);
                        })).BeginSendTo(Net.Client.Instance.Host, Net.Client.Instance.RemoteIP);
                    }
                },
                EscapeFunc = (a) =>
                {
                    TextBox.Enabled = false;
                    TextBox.Text = "";
                    Controls.Remove(Panel_Input);
                },
                TextEnterFunc = (a) =>
                {
                    if (!char.IsControl(a.KeyChar))
                        TextBox.Text += a.KeyChar;
                }
            };

            Timers = new Dictionary<Control, float>();

            Sldr_Opacity = new Slider(this.Label_Title.TopRight, width: 100, min: 0, max: 100, step: 1, value: Box_Text.Opacity * 100) { Name = "Opacity" };
            Sldr_Opacity.ValueChanged += new EventHandler<EventArgs>(Sldr_Opacity_ValueChanged);

            Panel_Input.Controls.Add(TextBox);
            Client.Controls.Add(Panel_Text, Panel_Input);
            Controls.Add(Client, Sldr_Opacity);
            Location = new Vector2(0, UIManager.Height);
            Anchor = new Vector2(0, 1);

            this.SetOpacity(0, true, exclude: Box_Text);
            TextBox.Enabled = false;
            SetMousethrough(true, true);
        }

        void Sldr_Opacity_ValueChanged(object sender, EventArgs e)
        {
            Box_Text.Opacity = Sldr_Opacity.Value / 100f;
        }

        public override void Update()
        {
            base.Update();
            if (this.TextBox.Enabled)
                return;

            Dictionary<Control, float> copy = new Dictionary<Control, float>(Timers);
            foreach (KeyValuePair<Control, float> timer in copy)
            {
                float newValue = timer.Value - 1;
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
        }

        static public void Write(LogEventArgs e)
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
            string fullText = DateTime.Now.ToString("[HH:mm:ss]") + text;
            fullText = UIManager.WrapText(fullText, Instance.Box_Text.Client.Width);
            var line = new Label(fullText)
            {
                
                TextColor = c,
                Location = Instance.Box_Text.Client.Controls.Count > 0 ? Instance.Box_Text.Client.Controls.Last().BottomLeft : Vector2.Zero,
                MouseThrough = Instance.MouseThrough
            };
            Instance.Timers.Add(line, FadeDelay);
            Instance.Box_Text.Add(line);
            Instance.Box_Text.Client.ClientLocation.Y = Instance.Box_Text.Client.Bottom - Instance.Box_Text.Client.ClientSize.Height;
        }

        public override bool Hide()
        {
            this.SetOpacity(0, true, exclude: Box_Text);
            TextBox.Enabled = false;
            SetMousethrough(true, true);
            return true;
        }
    }
}
