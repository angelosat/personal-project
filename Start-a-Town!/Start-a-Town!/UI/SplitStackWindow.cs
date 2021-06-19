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
        #region Singleton
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
        #endregion

        Func<bool> CancelFunc;

        public event EventHandler<SplitStackEventArgs> Done;
        void OnDone(SplitStackEventArgs e)
        {
            if (Done != null)
                Done(this, e);
        }

        Slider Sldr_Amount;
        Action<int> SplitAction;
        Panel Panel_Input;
        TextBox Txt_Amount;
        Button Btn_Split;
        new public GameObjectSlot Tag;
        GameObject Parent;
        GameObjectSlot Copy;
        SplitStackWindow()
        {
            AutoSize = true;

            Panel_Input = new Panel();
            Panel_Input.AutoSize = true;
            Panel_Input.BackgroundStyle = BackgroundStyle.TickBox;

            Title = "Split";

            float w = Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            Txt_Amount = new TextBox(Vector2.Zero, new Vector2(w, Label.DefaultHeight));
            Txt_Amount.Text = "1";
            Txt_Amount.TextEntered += new EventHandler<TextEventArgs>(Txt_Amount_TextEntered);
            Panel_Input.Controls.Add(Txt_Amount);

            Btn_Split = new Button(new Vector2(0, Panel_Input.Bottom), width: Panel_Input.Width, label: "Split")
            {
                LeftClickAction = () => this.Split()
                //{
                //    int amount = Int16.Parse(Txt_Amount.Text);
                //  //  DragDropManager.Create(new DragDropSlot(parent, this.Tag, this.Tag, DragDropEffects.Move | DragDropEffects.Link));
                //    //DragDropManager.Create(new GameObjectSlot(this.Tag.Object, amount), this.Tag, DragDropEffects.Move | DragDropEffects.Link);

                //    // MUST REQUEST SERVER TO INSTANTIATE NEW OBJECT
                //    // send same object (with same netID) so the server knows to duplicate it in another slot?
                //    GameObjectSlot newSlot = new GameObjectSlot(this.Tag.Object, amount);// .Clone()
                //    this.Tag.StackSize -= amount;
                //    Client.RequestNewObject(this.Tag.Object, (byte)amount);

                //    //DragDropManager.Create(new DragDropSlot(this.Parent, this.Tag, newSlot, DragDropEffects.Move | DragDropEffects.Link));
                //    //DragDropManager.Create(newSlot, this.Parent, DragDropEffects.Move | DragDropEffects.Link);
                //    //DragDropManager.Create(new GameObjectSlot(this.Tag.Object, amount), this.Parent, DragDropEffects.Move | DragDropEffects.Link);
                //    OnDone(new SplitStackEventArgs(Tag, amount));
                //    Hide();
                //}
            };
            Client.Controls.Add(Panel_Input, Btn_Split);
            Location = CenterMouseOnControl(Btn_Split);
        }

        void Split()
        {
            this.SplitAction(Int16.Parse(Txt_Amount.Text));
            this.Hide();
        }

        public override void Update()
        {
            base.Update();
            if((Tag.Object != Copy.Object) ||
                (Tag.StackSize != Copy.StackSize))
                this.Hide();
        }

        public SplitStackWindow Refresh(TargetArgs slotTarget)
        {
            this.SplitAction = (a) =>
            {
                int count = Int16.Parse(Txt_Amount.Text);
                DragDropManager.Create(new DragDropSlot(Parent, slotTarget, new TargetArgs(new GameObjectSlot(slotTarget.Slot.Object.Clone(), count)), DragDropEffects.Move | DragDropEffects.Link));
            };
            //Parent = parent;// p[1] as GameObject;
            //if (objSlot == null)
            //    return false;
            //if (objSlot.Object == null)
            //    throw new ArgumentNullException("objSlot.Object", "Tried to show SplitStackWindow for null object");
            this.Tag = slotTarget.Slot;
            this.Copy = new GameObjectSlot(slotTarget.Slot.Object, slotTarget.Slot.StackSize);

            Title = slotTarget.Slot.ToString();
            int w = (int)Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            int amount = (int)(slotTarget.Slot.StackSize / 2f);
            Txt_Amount.Width = w;
            Txt_Amount.Text = amount.ToString();//"1";

            Panel_Input.Controls.Remove(Txt_Amount);
            Panel_Input.Controls.Add(Txt_Amount);

            Sldr_Amount = new Slider(Panel_Input.BottomLeft, Panel_Input.Width, 1, slotTarget.Slot.StackSize, 1, amount)
            {
                ValueChangedFunc = () => Txt_Amount.Text = Sldr_Amount.Value.ToString(),
                HoverFunc = () => "Split to: " + Sldr_Amount.Value.ToString(),
            };

            Btn_Split.Location = Sldr_Amount.BottomLeft;
            Btn_Split.Width = Panel_Input.Width;

            Client.Controls.Clear();

            Client.Controls.Add(Panel_Input, Btn_Split, Sldr_Amount);
            Txt_Amount.Enabled = true;
            Location = CenterMouseOnControl(Btn_Split);

            return this;
        }

        public bool Show(GameObjectSlot objSlot, GameObject parent)
        {
            return this.Show(objSlot, parent, (amount) =>
                {
                    int count = Int16.Parse(Txt_Amount.Text);
                    //DragDropManager.Create(new DragDropSlot(Parent, objSlot, new GameObjectSlot(objSlot.Object.Clone(), count), DragDropEffects.Move | DragDropEffects.Link));
                    DragDropManager.Create(new DragDropSlot(Parent, objSlot, new GameObjectSlot(objSlot.Object.Clone(), count), DragDropEffects.Move | DragDropEffects.Link));
                });
        }
        public bool Show(GameObjectSlot objSlot, GameObject parent, Action<int> splitAction)//params object[] p)
        {
           // GameObjectSlot objSlot = p[0] as GameObjectSlot;
            //this.Btn_Split.LeftClickAction = splitAction;
            this.SplitAction = splitAction;
            Parent = parent;// p[1] as GameObject;
            if (objSlot == null)
                return false;
            if (objSlot.Object == null)
                throw new ArgumentNullException("objSlot.Object", "Tried to show SplitStackWindow for null object");
            this.Tag = objSlot;
            this.Copy = new GameObjectSlot(objSlot.Object, objSlot.StackSize);
            //this.CancelFunc = ()=>this.Slot.Object != objSlot.

            Title = objSlot.ToString();
            int w = (int)Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            int amount = (int)(objSlot.StackSize / 2f);
            Txt_Amount.Width = w;
            Txt_Amount.Text = amount.ToString();//"1";

            Panel_Input.Controls.Remove(Txt_Amount);
            Panel_Input.Controls.Add(Txt_Amount);

            Sldr_Amount = new Slider(Panel_Input.BottomLeft, Panel_Input.Width, 1, objSlot.StackSize, 1, amount)
            {
                ValueChangedFunc = () => Txt_Amount.Text = Sldr_Amount.Value.ToString(),
                HoverFunc = () => "Split to: " + Sldr_Amount.Value.ToString(),
            };

            Btn_Split.Location = Sldr_Amount.BottomLeft;
            Btn_Split.Width = Panel_Input.Width;

            Client.Controls.Clear();

            Client.Controls.Add(Panel_Input, Btn_Split, Sldr_Amount);
            Txt_Amount.Enabled = true;
            Location = CenterMouseOnControl(Btn_Split);

            //Btn_Split.LeftClickAction = () =>
            //{
            //    int count = Int16.Parse(Txt_Amount.Text);
            //    DragDropManager.Create(new DragDropSlot(Parent, objSlot, new GameObjectSlot(objSlot.Object.Clone(), count), DragDropEffects.Move | DragDropEffects.Link));
            //    this.Hide();
            //};
            return base.Show();
            //return base.Show();
        }

        void Txt_Amount_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == '\r')//13) //enter
            {
              //  Btn_Split_Click(sender, e);
                Btn_Split.PerformLeftClick();
                return;
            }
            //if (!Char.IsDigit(e.Char))
            //    return;

            int amount;
            string newAmount = Txt_Amount.Text + (Char.IsDigit(e.Char) ? e.Char.ToString() : "");
            //string newAmount = Txt_Amount.Text.Substring(Math.Max(Txt_Amount.Text.Length, 1) - Slot.StackSize.ToString().Length, Slot.StackSize.ToString().Length - 1) + e.Char;
           // string newAmount = Txt_Amount.Text.Substring(1, Slot.StackSize.ToString().Length - 1) + e.Char;
            if (!int.TryParse(newAmount, out amount))
                amount = 1;
            else
                amount = (int)MathHelper.Clamp((float)Int16.Parse(newAmount), 1, (float)Tag.StackSize);

            Txt_Amount.Text = amount.ToString();
            Sldr_Amount.Value = amount;
        }
    }
}
