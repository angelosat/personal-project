using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class DialogInput : Window
    {
        Panel Panel_Input, Panel_Buttons;
        TextBox Txt_Input;
        Button Btn_Accept, Btn_Cancel;
        public string Input { get { return this.Txt_Input.Text; } }

        public DialogInput(string title, Action<string> callback, int maxlength = int.MaxValue, string initialText = "")
        {
            this.Title = title;
            this.AutoSize = true;

            Panel_Input = new Panel();
            Panel_Input.AutoSize = true;
            Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Buttons = new Panel(Panel_Input.BottomLeft);
            Panel_Buttons.AutoSize = true;

            //Txt_Input = new TextBox(Vector2.Zero, new Vector2(200, Label.DefaultHeight));
            Txt_Input = new TextBox(Vector2.Zero, new Vector2(300, Label.DefaultHeight)) { MaxLength = maxlength, Text = initialText, EnterFunc = callback };

            //Txt_Input.TextEntered += new EventHandler<TextEventArgs>(Txt_Name_TextEntered);
            Txt_Input.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Input.Controls.Add(Txt_Input);

            Panel_Buttons = new Panel(Panel_Input.BottomLeft);
            Panel_Buttons.AutoSize = true;
            Btn_Accept = new Button(Vector2.Zero, 50, "Done") { LeftClickAction = () => callback(this.Txt_Input.Text) };
            Btn_Cancel = new Button(Btn_Accept.TopRight, 50, "Cancel");
            //Btn_Accept.LeftClick += new UIEvent(Btn_Accept_Click);
            //Btn_Cancel.LeftClick += new UIEvent(Btn_Cancel_Click);
            Panel_Buttons.Controls.Add(Btn_Accept);//, Btn_Cancel);

            Panel_Buttons.Location = Panel_Input.BottomCenter;
            Panel_Buttons.Anchor = Vector2.UnitX / 2;

            this.Client.Controls.Add(Panel_Input, Panel_Buttons);
            this.Location = CenterScreen;
        }
        public DialogInput(string title, Action<DialogInput> callback, int maxlength = int.MaxValue, string initialText = "")
        {
            this.Title = title;
            this.AutoSize = true;

            Panel_Input = new Panel();
            Panel_Input.AutoSize = true;
            Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Buttons = new Panel(Panel_Input.BottomLeft);
            Panel_Buttons.AutoSize = true;

            //Txt_Input = new TextBox(Vector2.Zero, new Vector2(200, Label.DefaultHeight));
            Txt_Input = new TextBox(Vector2.Zero, new Vector2(300, Label.DefaultHeight)) { MaxLength = maxlength, Text = initialText, EnterFunc = (txt) => callback(this) };

            //Txt_Input.TextEntered += new EventHandler<TextEventArgs>(Txt_Name_TextEntered);
            Txt_Input.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Input.Controls.Add(Txt_Input);

            Panel_Buttons = new Panel(Panel_Input.BottomLeft);
            Panel_Buttons.AutoSize = true;
            Btn_Accept = new Button(Vector2.Zero, 50, "Done") { LeftClickAction = () => callback(this) };
            Btn_Cancel = new Button(Btn_Accept.TopRight, 50, "Cancel");
            //Btn_Accept.LeftClick += new UIEvent(Btn_Accept_Click);
            //Btn_Cancel.LeftClick += new UIEvent(Btn_Cancel_Click);
            Panel_Buttons.Controls.Add(Btn_Accept);//, Btn_Cancel);

            Panel_Buttons.Location = Panel_Input.BottomCenter;
            Panel_Buttons.Anchor = Vector2.UnitX / 2;

            this.Client.Controls.Add(Panel_Input, Panel_Buttons);
            this.Location = CenterScreen;
        }

        public override bool Show(params object[] p)
        {
            this.Txt_Input.Enabled = true;
            return base.Show(p);
        }
    }
}
