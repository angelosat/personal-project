using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public partial class UIChat : Window
    {
        static public UIChat Instance;
        static UIChat()
        {
            Instance = new UIChat();
        }
        readonly GroupBox BoxButtons;
        public ConsoleBoxAsync Console;
        readonly Panel Panel_Text, Panel_Input;
        readonly SliderNew Sldr_Opacity;
        readonly Stack<string> History = new(16);
        int HistoryIndex;
        public TextBox TextBox;
        readonly IconButton BtnSettings;
        readonly UIChatSettings UISettings;
        UIChat()
        {
            Title = "Log";
            AutoSize = true;
            Closable = false;
            MouseThrough = true;
            Movable = true;

            Console = Net.Client.Instance.ConsoleBox;
            Console.FadeText = true;
            Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black };
            Panel_Text.Controls.Add(Console);

            Panel_Input = new Panel() { AutoSize = true, Location = Panel_Text.BottomLeft, Color = Color.Black };
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

            Sldr_Opacity = new SliderNew(() => Console.Opacity * 100, v => this.Console.Opacity = v / 100f, width: 100, min: 0, max: 100, step: 1) { Name = "Opacity" };
            Sldr_Opacity.Location = this.Label_Title.TopRight;
            Panel_Input.Controls.Add(TextBox);
            this.BoxButtons = new GroupBox();
            this.BtnSettings = new IconButton(Icon.ArrowUp) { BackgroundTexture = IconButton.Small, Location = this.TopRight, Anchor = Vector2.UnitX, LeftClickAction = ToggleOptions };
            this.BoxButtons.AddControls(this.BtnSettings);

            this.Client.AddControlsVertically(this.BoxButtons, Panel_Text, Panel_Input);

            this.UISettings = new UIChatSettings(this);

            Location = new Vector2(0, UIManager.Height);
            Anchor = new Vector2(0, 1);

            this.SetOpacity(0, true, this.Client);
            TextBox.Enabled = false;
            SetMousethrough(true, true);
        }

        private void ToggleOptions()
        {
            this.UISettings.Location = this.BtnSettings.ScreenLocation;
            this.UISettings.Anchor = Vector2.UnitY;
            this.UISettings.Toggle();
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
            if (this.TextBox.Enabled)
                return;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Hide();
        }

        public void Write(string text)
        {
            this.Write(Log.EntryTypes.Default, text);
        }
        public void Write(Log.EntryTypes type, string text)
        {
            this.Write(new Log.Entry(type, new object[] { text }));
        }
        public void Write(Log.Entry e)
        {
            string text = e.ToString();
            if (text.Length == 0)
                return;

            //var line = this.Console.Write(e.Color, text);
            var line = new Entry(e.Color, text);
            this.Console.Write(line);
            //line.MouseThrough = true;

            this.Console.Client.ClientLocation.Y = this.Console.Client.Bottom - this.Console.Client.ClientSize.Height;
        }

        internal void StartTyping()
        {
            this.Client.SetOpacity(1, true, exclude: Console);

            SetMousethrough(false, true);
            TextBox.Enabled = true;
        }

        internal void StartOrFinishTyping()
        {
            if (this.TextBox.Enabled)
            {
                this.TextBox.EnterFunc(this.TextBox.Text);
                return;
            }

            this.Client.SetOpacity(1, true, exclude: Console);

            SetMousethrough(false, true);
            TextBox.Enabled = true;
        }
        public override bool Hide()
        {
            this.Client.SetOpacity(0, true, exclude: Console);
            this.UISettings.Hide();
            TextBox.Enabled = false;
            SetMousethrough(true, true);
            return true;
        }

        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.HandleKeyDown(e);
        }
    }
    //public partial class UIChat : Window
    //{
    //    static public UIChat Instance;
    //    static UIChat()
    //    {
    //        Instance = new UIChat();
    //    }
    //    readonly GroupBox BoxButtons;
    //    public ConsoleBoxAsync Console;
    //    readonly Panel Panel_Text, Panel_Input;
    //    readonly SliderNew Sldr_Opacity;
    //    readonly Stack<string> History = new(16);
    //    int HistoryIndex;
    //    readonly Dictionary<Control, float> Timers = new();
    //    public TextBox TextBox;
    //    static public readonly float FadeDelay = 10 * Engine.TicksPerSecond;
    //    readonly IconButton BtnSettings;
    //    readonly UIChatSettings UISettings;
    //    UIChat()
    //    {
    //        Title = "Log";
    //        AutoSize = true;
    //        Closable = false;
    //        MouseThrough = true;
    //        Movable = true;

    //        Console = Net.Client.Instance.ConsoleBox;
    //        Console.FadeText = true;
    //        Panel_Text = new Panel() { AutoSize = true, Name = "Panel_Text", Color = Color.Black };
    //        Panel_Text.Controls.Add(Console);

    //        Panel_Input = new Panel() { AutoSize = true, Location = Panel_Text.BottomLeft, Color = Color.Black };
    //        Panel_Input.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Label.DefaultHeight);
    //        TextBox = new TextBox()
    //        {
    //            Width = Panel_Input.ClientSize.Width,
    //            EnterFunc = (a) =>
    //            {
    //                this.Hide();
    //                TextBox.Enabled = false;
    //                string gotText = TextBox.Text;
    //                TextBox.Text = "";

    //                if (this.History.Count == 15)
    //                    this.History.Pop();
    //                this.History.Push(gotText);
    //                this.HistoryIndex = -1;

    //                Panel_Input.Opacity = 0;
    //                if (gotText.Length == 0)
    //                    return;

    //                if (gotText[0] == '/')
    //                {
    //                    //server command
    //                    Net.Client.PlayerCommand(gotText.TrimStart('/'));
    //                }
    //                else
    //                {
    //                    PacketChat.Send(Net.Client.Instance, Net.Client.Instance.PlayerData.ID, gotText);
    //                }
    //            },
    //            EscapeFunc = (a) =>
    //            {
    //                TextBox.Enabled = false;
    //                TextBox.Text = "";
    //                Controls.Remove(Panel_Input);
    //            }
    //            ,
    //            InputFunc = (text, ch) =>
    //            {
    //                if (char.IsControl(ch))
    //                    return text;
    //                return text + ch;
    //            },
    //            HistoryNextPrev = this.ChatHistory
    //        };

    //        Sldr_Opacity = new SliderNew(() => Console.Opacity * 100, v => this.Console.Opacity = v / 100f, width: 100, min: 0, max: 100, step: 1) { Name = "Opacity" };
    //        Sldr_Opacity.Location = this.Label_Title.TopRight;
    //        Panel_Input.Controls.Add(TextBox);
    //        this.BoxButtons = new GroupBox();
    //        this.BtnSettings = new IconButton(Icon.ArrowUp) { BackgroundTexture = IconButton.Small, Location = this.TopRight, Anchor = Vector2.UnitX, LeftClickAction = ToggleOptions };
    //        this.BoxButtons.AddControls(this.BtnSettings);

    //        this.Client.AddControlsVertically(this.BoxButtons, Panel_Text, Panel_Input);

    //        this.UISettings = new UIChatSettings(this);

    //        Location = new Vector2(0, UIManager.Height);
    //        Anchor = new Vector2(0, 1);

    //        this.SetOpacity(0, true,this.Client);
    //        TextBox.Enabled = false;
    //        SetMousethrough(true, true);
    //    }

    //    private void ToggleOptions()
    //    {
    //        this.UISettings.Location = this.BtnSettings.ScreenLocation;
    //        this.UISettings.Anchor = Vector2.UnitY;
    //        this.UISettings.Toggle();
    //    }

    //    void ChatHistory(int index)
    //    {
    //        if (this.History.Count == 0)
    //            return;
    //        // TODO: do something
    //        this.HistoryIndex += index;
    //        this.HistoryIndex = Math.Max(0, Math.Min(this.History.Count - 1, this.HistoryIndex));
    //        this.TextBox.Text = this.History.ElementAt(this.HistoryIndex);
    //    }

    //    public override void Draw(SpriteBatch sb)
    //    {
    //        base.Draw(sb, this.BoundsScreen);
    //    }

    //    public override void Draw(SpriteBatch sb, Rectangle viewport)
    //    {
    //        base.Draw(sb, viewport);
    //    }

    //    public override void Update()
    //    {
    //        base.Update();

    //        if (this.TextBox.Enabled)
    //            return;

    //        Dictionary<Control, float> copy = new Dictionary<Control, float>(Timers);
    //        foreach (KeyValuePair<Control, float> timer in copy)
    //        {
    //            float newValue = timer.Value - 1;// GlobalVars.DeltaTime;
    //            float alpha = 10 * (float)Math.Sin(Math.PI * (newValue / (float)FadeDelay));

    //            Timers[timer.Key] = newValue;
    //            timer.Key.Opacity = alpha;

    //            if (timer.Value <= 0)
    //            {
    //                Timers.Remove(timer.Key);
    //                timer.Key.Opacity = 0;
    //            }
    //        }
    //    }


    //    void Timer_Tick(object sender, EventArgs e)
    //    {
    //        Hide();
    //    }

    //    public void Write(string text)
    //    {
    //        this.Write(Log.EntryTypes.Default, text);
    //    }
    //    public void Write(Log.EntryTypes type, string text)
    //    {
    //        this.Write(new Log.Entry(type, new object[] { text }));
    //    }
    //    public void Write(Log.Entry e)
    //    {
    //        string text = e.ToString();
    //        if (text.Length == 0)
    //            return;

    //        var line = this.Console.Write(e.Color, text);
    //        line.MouseThrough = true;

    //        this.Timers.Add(line, FadeDelay);

    //        this.Console.Client.ClientLocation.Y = this.Console.Client.Bottom - this.Console.Client.ClientSize.Height;
    //    }

    //    internal override void OnControlRemoved(Control control)
    //    {
    //            this.Timers.Remove(control);
    //    }
    //    internal void StartTyping()
    //    {
    //        this.Client.SetOpacity(1, true, exclude: Console);

    //        SetMousethrough(false, true);
    //        TextBox.Enabled = true;
    //        foreach (Control label in Console.Client.Controls)
    //        {
    //            Timers[label] = FadeDelay / 2f;
    //        }
    //    }

    //    internal void StartOrFinishTyping()
    //    {
    //        if (this.TextBox.Enabled)
    //        {
    //            this.TextBox.EnterFunc(this.TextBox.Text);
    //            return;
    //        }

    //        this.Client.SetOpacity(1, true, exclude: Console);

    //        SetMousethrough(false, true);
    //        TextBox.Enabled = true;
    //        foreach (Control label in Console.Client.Controls)
    //        {
    //            Timers[label] = FadeDelay / 2f;
    //        }
    //    }
    //    public override bool Hide()
    //    {
    //        this.Client.SetOpacity(0, true, exclude: Console);
    //        this.UISettings.Hide();
    //        TextBox.Enabled = false;
    //        SetMousethrough(true, true);
    //        return true;
    //    }

    //    public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
    //    {
    //        base.HandleKeyDown(e);
    //    }
    //}
}
