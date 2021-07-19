using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    [Obsolete]
    public class ListBox<TObject, TControl> : ScrollableBox
        where TControl : ButtonBase, new()
        where TObject : class
    {
        public TObject SelectedItem => SelectedControl == null ? default(TObject) : SelectedControl.Tag as TObject;
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
            this.btn_Click(this.SelectedControl);
        }

        public Action<TObject> ItemChangedFunc = item => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBox(int width, int height) : base(new Rectangle(0,0,width,height)) { }
        public ListBox(Rectangle bounds) : base(bounds) { }

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged(object control)
        {
            ItemChangedFunc((control as TControl).Tag as TObject);
            SelectedItemChanged?.Invoke(control, EventArgs.Empty);
        }

        public Func<TObject, string> SelectedTextFunc;
        public Action<TObject, TControl> OnControlInit;

        public void Clear()
        {
            this.Client.Controls.Clear();
        }
        public ListBox<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> hoverTextFunc = null)
        {
            return this.Build(list, obj => obj.ToString(), hoverTextFunc);
        }
        public ListBox<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Func<TObject, string> hoverTextFunc = null)
        {
            this.List = list;
            Func<TObject, string> htv = hoverTextFunc ?? (foo => "");
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = obj,
                    TextFunc = () => nameFunc(obj),
                    NameFunc = () => nameFunc(obj),
                    HoverText = htv(obj),
                    Active = true
                };
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    action();
                    btn_Click(btn);
                };
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            SelectedControl = null;
            return this;
        }
        public ListBox<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            this.SelectedTextFunc = nameFunc;
            this.OnControlInit = onControlInit;
            this.List = list;
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                TControl btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = obj,
                    TextFunc = () => nameFunc(obj),
                    NameFunc = () => nameFunc(obj),
                    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                    Active = true
                };
                onControlInit(obj, btn as TControl);
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    action();
                    btn_Click(btn);
                };
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            // SelectedItem = default(TObject);
            SelectedControl = null;
            return this;
        }

        private void Expand(TControl btn)
        {
            var inner = btn.Tag as GroupBox;
            inner.Location = btn.BottomLeft + new Vector2(16,0);
            var btnIndex = this.Client.Controls.FindIndex(c => c == btn);
            for (int i = btnIndex + 1; i < this.Client.Controls.Count; i++)
            {
                var control = this.Client.Controls[i];
                control.Location.Y += inner.Height;
            }
            this.Add(inner);
        }

        public override void Remeasure()
        {
            base.Remeasure();
            foreach (var btn in Client.Controls)
            {
                btn.Width = Math.Max(btn.Width, Client.Width);
            }
        }

        void btn_Click(TControl ctrl)
        {
            if (this.SelectedControl != null)
                this.SelectedControl.BackgroundColor = Color.Transparent;
            this.SelectedControl = ctrl;
            ctrl.BackgroundColor = Color.White * 0.5f;
            OnSelectedItemChanged(ctrl);
        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}

