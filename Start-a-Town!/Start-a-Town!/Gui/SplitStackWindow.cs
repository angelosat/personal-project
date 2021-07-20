using System;
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
        static public SplitStackWindow Instance => _Instance ??= new SplitStackWindow();

        SliderNew Sldr_Amount;
        Action<int> SplitAction;
        Panel Panel_Input;
        TextBox Txt_Amount;
        Button Btn_Split;
        new public GameObjectSlot Tag;
        GameObject Entity;
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
                int count = short.Parse(Txt_Amount.Text);
                DragDropManager.Create(new DragDropSlot(Entity, slotTarget, new TargetArgs(new GameObjectSlot(slotTarget.Slot.Object.Clone(), count)), DragDropEffects.Move | DragDropEffects.Link));
            };
          
            this.Tag = slotTarget.Slot;
            this.Copy = new GameObjectSlot(slotTarget.Slot.Object, slotTarget.Slot.StackSize);

            Title = slotTarget.Slot.ToString();
            int w = (int)Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            int amount = (int)(slotTarget.Slot.StackSize / 2f);
            Txt_Amount.Width = w;
            Txt_Amount.Text = amount.ToString();//"1";

            Panel_Input.Controls.Remove(Txt_Amount);
            Panel_Input.Controls.Add(Txt_Amount);

            Sldr_Amount = new SliderNew(() => amount, v => amount = (int)v, Panel_Input.Width, 1, slotTarget.Slot.StackSize, 1)
            {
                Location = Panel_Input.BottomLeft,
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
                    DragDropManager.Create(new DragDropSlot(Entity, objSlot, new GameObjectSlot(objSlot.Object.Clone(), count), DragDropEffects.Move | DragDropEffects.Link));
                });
        }
        public bool Show(GameObjectSlot objSlot, GameObject parent, Action<int> splitAction)
        {
            this.SplitAction = splitAction;
            Entity = parent;
            if (objSlot == null)
                return false;
            if (objSlot.Object == null)
                throw new ArgumentNullException("objSlot.Object", "Tried to show SplitStackWindow for null object");
            this.Tag = objSlot;
            this.Copy = new GameObjectSlot(objSlot.Object, objSlot.StackSize);

            Title = objSlot.ToString();
            int w = (int)Math.Max(70, TitleLocation.X + UIManager.Font.MeasureString(Title).X + CloseButton.Width);
            int amount = (int)(objSlot.StackSize / 2f);
            Txt_Amount.Width = w;
            Txt_Amount.Text = amount.ToString();

            Panel_Input.Controls.Remove(Txt_Amount);
            Panel_Input.Controls.Add(Txt_Amount);

            Sldr_Amount = new SliderNew(() => amount, v => amount = (int)v, Panel_Input.Width, 1, objSlot.StackSize, 1)
            {
                Location = Panel_Input.BottomLeft,
                ValueChangedFunc = () => Txt_Amount.Text = Sldr_Amount.Value.ToString(),
                HoverFunc = () => "Split to: " + Sldr_Amount.Value.ToString(),
            };

            Btn_Split.Location = Sldr_Amount.BottomLeft;
            Btn_Split.Width = Panel_Input.Width;

            Client.Controls.Clear();

            Client.Controls.Add(Panel_Input, Btn_Split, Sldr_Amount);
            Txt_Amount.Enabled = true;
            Location = CenterMouseOnControl(Btn_Split);

            return base.Show();
        }

        void Txt_Amount_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == '\r')//13) //enter
            {
                Btn_Split.PerformLeftClick();
                return;
            }

            int amount;
            string newAmount = Txt_Amount.Text + (Char.IsDigit(e.Char) ? e.Char.ToString() : "");
            if (!int.TryParse(newAmount, out amount))
                amount = 1;
            else
                amount = (int)MathHelper.Clamp((float)Int16.Parse(newAmount), 1, (float)Tag.StackSize);

            Txt_Amount.Text = amount.ToString();
            Sldr_Amount.Value = amount;
        }
    }
}
