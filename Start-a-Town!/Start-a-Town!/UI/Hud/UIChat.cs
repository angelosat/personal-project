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
    public class UIChat : Window
    {
        //  ScrollableBox Box_Text;
        static public UIChat Instance;
        static UIChat()
        {
            Instance = new UIChat();
        }
        readonly GroupBox BoxButtons;
        public ConsoleBoxAsync Console;
        readonly Panel Panel_Text, Panel_Input;
        readonly Slider Sldr_Opacity;
        readonly Stack<string> History = new(16);
        int HistoryIndex;
        readonly Dictionary<Control, float> Timers = new();
        public TextBox TextBox;
        static public readonly float FadeDelay = 10 * Engine.TicksPerSecond;// 6;
        readonly IconButton BtnSettings;
        readonly UIChatSettings UISettings;
        UIChat()
        {
            Title = "Log";
            AutoSize = true;
            Closable = false;
            MouseThrough = true;
            Movable = true;


            Console = Net.Client.Instance.Log;
            Console.FadeText = true;
            Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black };
            Panel_Text.Controls.Add(Console);

            Panel_Input = new Panel() { AutoSize = true, Location = Panel_Text.BottomLeft, Color = Color.Black };
            //Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;
            Panel_Input.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Label.DefaultHeight);
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
                    }
                    else
                    {
                        PacketChat.Send(Net.Client.Instance, Net.Client.Instance.PlayerData.ID, gotText);
                    }
                },
                EscapeFunc = (a) =>
                {
                    TextBox.Enabled = false;
                    TextBox.Text = "";
                    Controls.Remove(Panel_Input);
                }
                ,
                InputFunc = (text, ch) =>
                {
                    if (char.IsControl(ch))
                        return text;
                    return text + ch;
                },
                HistoryNextPrev = this.ChatHistory
            };

            Sldr_Opacity = new Slider(this.Label_Title.TopRight, width: 100, min: 0, max: 100, step: 1, value: Console.Opacity * 100) { Name = "Opacity", ValueChangedFunc = Sldr_Opacity_ValueChanged };

            

            Panel_Input.Controls.Add(TextBox);
            //Client.AddControls(Panel_Text, Panel_Input);
            this.BoxButtons = new GroupBox();
            this.BtnSettings = new IconButton(Icon.ArrowUp) { BackgroundTexture = IconButton.Small, Location = this.TopRight, Anchor = Vector2.UnitX, LeftClickAction = ToggleOptions };
            this.BoxButtons.AddControls(this.BtnSettings);

            this.Client.AddControlsVertically(this.BoxButtons, Panel_Text, Panel_Input);

            this.UISettings = new UIChatSettings(this);
            //Controls.Add(
            //    //Client, 
            //    Sldr_Opacity);

            Location = new Vector2(0, UIManager.Height);// - Height);
            Anchor = new Vector2(0, 1);

            this.SetOpacity(0, true,this.Client);// Box_Text, this.IconSettings);//.Client);
            TextBox.Enabled = false;
            SetMousethrough(true, true);
            //LoadConfig();

        }

        //private void LoadConfig()
        //{
        //    Engine.Config.Root.TryGetValue("Timestamps", v =>
        //    {
        //        bool parsed;
        //        if (bool.TryParse(v, out parsed))
        //            this.Console.TimeStamp = parsed;
        //    });
        //}

        private void ToggleOptions()
        {
            this.UISettings.Location = this.BtnSettings.ScreenLocation;
            this.UISettings.Anchor = Vector2.UnitY;
            this.UISettings.Toggle();
            //UIChatSettings.Refresh(this);
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
      
        private void Sldr_Opacity_ValueChanged()
        {
            this.Console.Opacity = Sldr_Opacity.Value / 100f;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb, this.BoundsScreen);
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
        //internal void Write(Log.EntryTypes type, string source, string text)
        //{
        //    throw new NotImplementedException();
        //}
        public void Write(string text)
        {
            this.Write(Log.EntryTypes.Default, text);
        }
        public void Write(Log.EntryTypes type, string text)
        {
            this.Write(new Log.Entry(type, text));
            //if (text.Length == 0)
            //    return;

            //Color c = Color.White;
            //switch (type)
            //{
            //    case Log.EntryTypes.System:
            //        c = Color.Yellow;
            //        break;
            //    case Log.EntryTypes.Damage:
            //        c = Color.Red;
            //        break;
            //    default:
            //        break;
            //}

            //Label line = this.Box_Text.Write(c, text);
            //line.MouseThrough = true;
            
            //this.Timers.Add(line, FadeDelay);

            //this.Box_Text.Client.ClientLocation.Y = this.Box_Text.Client.Bottom - this.Box_Text.Client.ClientSize.Height;

        }
        public void Write(Log.Entry e)
        {
            string text = e.ToString();
            if (text.Length == 0)
                return;

            Color c = Color.White;
            switch (e.Type)
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

            Label line = this.Console.Write(c, text);
            line.MouseThrough = true;
            
            this.Timers.Add(line, FadeDelay);

            this.Console.Client.ClientLocation.Y = this.Console.Client.Bottom - this.Console.Client.ClientSize.Height;

        }

        internal override void OnControlRemoved(Control control)
        {
                this.Timers.Remove(control);
        }
        internal void StartTyping()
        {

            //this.SetOpacity(1, true, exclude: Box_Text);//.Client);
            this.Client.SetOpacity(1, true, exclude: Console);//.Client);

            SetMousethrough(false, true);
            TextBox.Enabled = true;
            foreach (Control label in Console.Client.Controls)
            {
                Timers[label] = FadeDelay / 2f;
            }
        }

        public override bool Hide()
        {
            //this.SetOpacity(0, true, exclude: Box_Text);//.Client);
            this.Client.SetOpacity(0, true, exclude: Console);//.Client);
            this.UISettings.Hide();
            TextBox.Enabled = false;
            SetMousethrough(true, true);
            return true;
        }

        public override void BringToFront()
        {
            //base.BringToFront();
        }

        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.HandleKeyDown(e);
        }


        public override void Validate(bool cascade = false)
        {
            //Refresh();
            base.Validate(cascade);
        }
        private void Refresh()
        {
            this.Timers.Clear();
            foreach (var entry in this.Console.Client.Controls)
                this.Timers.Add(entry, FadeDelay / 2f);
        }

        
    }
}
