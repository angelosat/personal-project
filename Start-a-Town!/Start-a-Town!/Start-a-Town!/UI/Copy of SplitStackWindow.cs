using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class SplitStackEventArgs : EventArgs
    {
        public GameObjectSlot Slot;
        public int Amount;
        public SplitStackEventArgs(GameObjectSlot slot, int amount)
        {
            this.Slot = slot;
            this.Amount = amount;
        }
    }
    class SplitStackWindow : Window
    {
        static SplitStackWindow _Instance;
        static public SplitStackWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SplitStackWindow();
                return _Instance;
            }
        }

        public event EventHandler<SplitStackEventArgs> Done;
        void OnDone(SplitStackEventArgs e)
        {
            if (Done != null)
                Done(this, e);
        }
        Panel Panel_Input;
        TextBox Txt_Amount;
        Button Btn_Split;
        GameObjectSlot Slot;
        public SplitStackWindow()//GameObjectSlot objSlot = null)
        {
            //if (objSlot == null)
            //    return;
            //if (objSlot.Object == null)
            //    throw new ArgumentNullException("objSlot.Object", "Created SplitStackWindow for null object");
            //this.Slot = objSlot;

            Panel_Input = new Panel();
            Panel_Input.AutoSize = true;
            Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            Title = "Split";// objSlot.ToString();
            AutoSize = true;
            float w = Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            Txt_Amount = new TextBox(Vector2.Zero, new Vector2(w, Label.DefaultHeight));
            Txt_Amount.Text = "1";
            Txt_Amount.TextEntered += new EventHandler<TextEventArgs>(Txt_Amount_TextEntered);
            Panel_Input.Controls.Add(Txt_Amount);

            Btn_Split = new Button(new Vector2(0, Panel_Input.Bottom), width: Panel_Input.Width, label: "Split");
            Btn_Split.Click += new UIEvent(Btn_Split_Click);
            Controls.Add(Panel_Input, Btn_Split);

            Location = CenterMouseOnControl(Btn_Split);
        }

        void Btn_Split_Click(object sender, EventArgs e)
        {
            OnDone(new SplitStackEventArgs(Slot, Int16.Parse(Txt_Amount.Text)));
            Hide();
        }

        public override bool Show(params object[] p)
        {

            GameObjectSlot objSlot = p[0] as GameObjectSlot;
            if (objSlot == null)
                return false;
            if (objSlot.Object == null)
                throw new ArgumentNullException("objSlot.Object", "Tried to show SplitStackWindow for null object");
            this.Slot = objSlot;

            //Panel_Input = new Panel();
            //Panel_Input.AutoSize = true;


            Title = objSlot.ToString();
            //AutoSize = true;
            float w = Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            Txt_Amount.Width = (int)w;
            Txt_Amount.Text = "1";
            //Txt_Amount = new TextBox(Vector2.Zero, new Vector2(w, Label.DefaultHeight));
            //Txt_Amount.Text = "1";
            //Txt_Amount.TextEntered += new EventHandler<TextEventArgs>(Txt_Amount_TextEntered);

            //Btn_Split = new Button(new Vector2(0, Txt_Amount.Bottom), width: Txt_Amount.Width, label: "Split");
            
            Panel_Input.Controls.Remove(Txt_Amount);
            Panel_Input.Controls.Add(Txt_Amount);
            Btn_Split.Width = Panel_Input.Width;
            Controls.Remove(Panel_Input); 
            Controls.Remove(Btn_Split);
            Controls.Add(Panel_Input, Btn_Split);

            Location = CenterMouseOnControl(Btn_Split);
            return base.Show(p);
        }

        void Txt_Amount_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == '\r')//13) //enter
            {
                Btn_Split_Click(sender, e);
                return;
            }

            short amount;
            if (!Int16.TryParse(e.Char.ToString(), out amount))
                return;

            Txt_Amount.Text += e.Char;
       //     int max = (int)Slot.Object.GetGui()["StackMax"];
            if (Int16.Parse(Txt_Amount.Text) > Slot.StackSize)
                Txt_Amount.Text = Slot.StackSize.ToString();
        }
    }
}
