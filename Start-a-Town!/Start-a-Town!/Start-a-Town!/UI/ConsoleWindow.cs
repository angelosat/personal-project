using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    class ConsoleWindow : Window
    {
        RenderTarget2D Texture;
        List<Label> Lines;
        PictureBox Chatbox;
        Timer2 Timer;
        public TextBox TextBox;

        static ConsoleWindow _Instance;
        static public ConsoleWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConsoleWindow();
                return _Instance;
            }
        }

        public ConsoleWindow()
        {
            Title = "Console";
            AutoSize = true;
            Opacity = 0;
            Panel panel = new Panel(new Rectangle(0,0,400, 100));
            Texture = new RenderTarget2D(Game1.Instance.GraphicsDevice, 400, 100);
            Chatbox = new PictureBox(Vector2.Zero, Texture, panel.ClientSize, TextAlignment.Left);
            Lines = new List<Label>();
            panel.Controls.Add(Chatbox);

            Panel panel_input = new Panel(new Vector2(0, panel.Bottom));
            panel_input.ClientSize = new Rectangle(0, 0, panel.ClientSize.Width, Label.DefaultHeight);

            TextBox = new TextBox();
            //TextBox.KeyPress += new EventHandler<KeyPressEventArgs>(TextBox_KeyPress);
            TextBox.TextEntered += new EventHandler<TextEventArgs>(TextBox_TextEntered);
            TextBox.Width = panel.ClientSize.Width;
            panel_input.Controls.Add(TextBox);

            Timer = new Timer2();
            Timer.Interval = 60 * 10;
            Timer.Tick +=new EventHandler<EventArgs>(Timer_Tick);
            //Timer.Start();

            Controls.AddRange(new Control[] { panel_input, panel });
            Controls.Add(panel);
            //Location = new Vector2(0, Camera.Height - Size.Height);
            Location = new Vector2(0, QuickBar.Instance.Top - Height);
            Update();
        }

        void TextBox_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == 13)
            {
                Write(TextBox.Text);
                TextBox.Text = "";
            }
            else
                TextBox.Text += e.Char;
        }

        void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Enter:
                    TextBox.Enabled = false;
                    Write(TextBox.Text);
                    TextBox.Text = "";
                    break;
                default:
                    TextBox.Text += e.Key;
                    break;
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Close();
            Timer.Stop();
        }

        public override bool Close()
        {
            TextBox.Enabled = false; 
            return base.Close();
        }

        

        public override void Paint()
        {
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            gfx.SetRenderTarget(Texture);
            gfx.Clear(Color.Transparent);
            SpriteBatch sb = Game1.Instance.spriteBatch;
            sb.Begin();
            foreach (Label label in Lines)
                label.Draw(sb);
            sb.End();
            gfx.SetRenderTarget(null);
            base.Paint();
        }

        static public void Write(string text)
        {
            Label line = new Label(new Vector2(0, Instance.Chatbox.Height - Label.DefaultHeight), DateTime.Now.ToString("[HH:mm:ss] ") + text);
            line.Location.Y = Instance.Chatbox.Height - Label.DefaultHeight;
            foreach (Label label in Instance.Lines)
                label.Location.Y -= Label.DefaultHeight;
            Instance.Lines.Add(line);
            if (Instance.Lines.Count > 5)
                Instance.Lines.RemoveAt(0);
            Instance.Paint();
            Instance.Show();
            Instance.Timer.Restart();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            //sb.Draw(Texture, ScreenLocation, Color.White);
        }




        //public override void HandleInput(InputState input)
        //{
        //    Keys[] keys = input.CurrentKeyboardState.GetPressedKeys();
        //    if (keys.Contains(Keys.Enter))
        //    {
        //        Write(TextBox.Text);
        //        TextBox.Text = "";
        //    }
        //    base.HandleInput(input);
        //}

        //protected override void WindowManager_KeyPress(object sender, KeyEventArgs e)
        //{
        //    if (e.KeysNew.Contains(Keys.Enter))
        //    {
        //        Paint();

        //        if (TextBox.Text.Length > 0)
        //        {
        //            Write(TextBox.Text);
        //            TextBox.Text = "";
        //        }

        //    }
        //    base.WindowManager_KeyPress(sender, e);
        //}
    }
}
