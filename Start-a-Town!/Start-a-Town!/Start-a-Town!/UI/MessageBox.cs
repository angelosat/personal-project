using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class MessageBox : Window
    {
        Panel Panel_Text, Panel_Buttons;

        public event EventHandler<EventArgs> Yes;
        void OnYes()
        {
            if (Yes != null)
                Yes(this, EventArgs.Empty);
        }

        Action YesAction, NoAction;
        //public override Rectangle Bounds
        //{
        //    get
        //    {
        //        return new Rectangle(0, 0, Camera.Width, Camera.Height);
        //    }
        //}

        static public MessageBox Create(string title, string text, string b1text, Action b1action)
        {
            return new MessageBox(title, text, b1text, b1action);
        }
        static public MessageBox Create(string title, string text, string b1text, Action b1action, string b2text, Action b2action)
        {
            return new MessageBox(title, text, b1text, b1action, b2text, b2action);
        }
        static public MessageBox Create(string title, string text, Action yesAction = null, Action noAction = null)
        {
            return new MessageBox(title, text, yesAction, noAction);
        }
        MessageBox(string title, string text, string b1text, Action b1action, string b2text, Action b2action)
        {
            Title = title;
            AutoSize = true;
            Closable = false;

            int maxwidth = 600;
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;

            Label label = new Label(new Vector2(0), UIManager.WrapText(text, maxwidth));
            Panel_Text.Controls.Add(label);
            Panel_Text.ClientSize = new Rectangle(0, 0, maxwidth, Math.Max(label.Height, UIManager.SlotSprite.Height));

            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom));
            Panel_Buttons.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Button.DefaultHeight);


            Button b1 = new Button(Vector2.Zero, maxwidth / 2, b1text) { LeftClickAction = b1action };
            Button b2 = new Button(new Vector2(b1.Right, 0), maxwidth / 2, b2text) { LeftClickAction = b2action };
            //Yes.LeftClick += new UIEvent(Yes_Click);
            //No.LeftClick += new UIEvent(No_Click);
            Panel_Buttons.Controls.Add(b1, b2);

            Client.Controls.Add(Panel_Text, Panel_Buttons);

            Location = this.CenterScreen;
            this.YesAction = b1action ?? (() => { });
            this.NoAction = b2action ?? (() => { });

        }
        MessageBox(string title, string text, string b1text, Action b1action)
        {
            this.AutoSize = true;
            Title = title;
            AutoSize = true;
            Closable = false;

            int width = this.ClientSize.Width;
            
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;

            Label label = new Label(new Vector2(0), UIManager.WrapText(text, width));
            Panel_Text.Controls.Add(label);
            Panel_Text.ClientSize = new Rectangle(0, 0, width, Math.Max(label.Height, UIManager.SlotSprite.Height));

            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom));
            Panel_Buttons.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Button.DefaultHeight);

            Button btn = new Button(b1text, Panel_Buttons.ClientSize.Width) { LeftClickAction = b1action };
            //btn.LeftClick += new UIEvent(Yes_Click);

            Panel_Buttons.Controls.Add(btn);
            Client.Controls.Add(Panel_Text, Panel_Buttons);

            YesAction = b1action;
            Location = this.CenterScreen;
        }

        public MessageBox(string title, string text, Action yesAction = null, Action noAction = null)
        {
            Title = title;
            AutoSize = true;
            Closable = false;

            int width = 200;
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;

            Label label = new Label(new Vector2(0), UIManager.WrapText(text, width));
            Panel_Text.Controls.Add(label);
            Panel_Text.ClientSize = new Rectangle(0, 0, width, Math.Max(label.Height, UIManager.SlotSprite.Height));

            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom));
            Panel_Buttons.ClientSize = new Rectangle(0, 0, Panel_Text.ClientSize.Width, Button.DefaultHeight);


            Button Yes = new Button(Vector2.Zero, width / 2, "Yes") { LeftClickAction = yesAction };
            Button No = new Button(new Vector2(Yes.Right, 0), width / 2, "No") { LeftClickAction = yesAction };
            //Yes.LeftClick += new UIEvent(Yes_Click);
            //No.LeftClick += new UIEvent(No_Click);
            Panel_Buttons.Controls.Add(Yes, No);

            Client.Controls.Add(Panel_Text, Panel_Buttons);

            Location = this.CenterScreen;
            this.YesAction = yesAction ?? (() => { });
            this.NoAction = noAction ?? (() => { });

        }
        public MessageBox(string title, string text, params ContextAction[] actions)
        {
            Title = title;
            AutoSize = true;
            Closable = false;

            int width = 200;
            Panel_Text = new Panel();
            Panel_Text.AutoSize = true;
            Panel_Text.BackgroundStyle = UI.BackgroundStyle.TickBox;
            Label label = new Label(new Vector2(0), UIManager.WrapText(text, width));
            Panel_Text.Controls.Add(label);
            Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom)) { AutoSize = true };
            foreach (var action in actions)
            {
                Button btn = new Button(action.Name()) { LeftClickAction = () => { action.Action(); this.Hide(); } };
                //btn.Location = this.Panel_Buttons.Controls.BottomLeft;
                btn.Location = this.Panel_Buttons.Controls.TopRight;
                this.Panel_Buttons.Controls.Add(btn);
            }

            Client.Controls.Add(Panel_Text, Panel_Buttons);

            this.Location = this.CenterScreen;
        }
        //public MessageBox(string title, string text, params Button[] buttons)
        //{
        //    Title = title;
        //    AutoSize = true;
        //    Closable = false;

        //    int width = 200;
        //    Panel_Text = new Panel();
        //    Panel_Text.AutoSize = true;

        //    Label label = new Label(new Vector2(0), UIManager.WrapText(text, width));
        //    Panel_Text.Controls.Add(label);
        //    Panel_Buttons = new Panel(new Vector2(0, Panel_Text.Bottom)) { AutoSize = true };
        //    foreach(var btn in buttons)
        //    {
        //        btn.Location = this.Panel_Buttons.Controls.BottomLeft;
        //        this.Panel_Buttons.Controls.Add(btn);
        //    }

        //    Client.Controls.Add(Panel_Text, Panel_Buttons);

        //    this.Location = this.CenterScreen;
        //}

        //void Yes_Click(object sender, EventArgs e)
        //{
        //    YesAction();
        //    OnYes();
        //    Hide();
        //}
        //void No_Click(object sender, EventArgs e)
        //{
        //    NoAction();
        //    //Close();
        //    Hide();
        //}

        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        //{
        //    //sb.Draw(WindowManager.DimScreen, new Vector2(0), Color.Lerp(Color.White, Color.Transparent, 0.5f));
        //    base.Draw(sb);
        //}
    }
}
