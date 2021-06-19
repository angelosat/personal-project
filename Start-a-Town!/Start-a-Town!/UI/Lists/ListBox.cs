using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBox<TObject, TControl> : ScrollableBox
        where TControl : ButtonBase, new()
        where TObject : class
    {
        
        public TObject SelectedItem { get { return (SelectedControl == null ? default(TObject) : SelectedControl.Tag as TObject); } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
            this.btn_Click(this.SelectedControl);
        }

        public List<TControl> Items
        {
            get
            {
                return this.Client.Controls.Cast<TControl>().ToList();
            }
        }

        public Action<TObject> ItemChangedFunc = (item) => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBox(int width, int height) : base(new Rectangle(0,0,width,height)) { }//MouseThrough = true; }
        public ListBox(Rectangle bounds) : base(bounds) { }//MouseThrough = true; }

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged(object control)
        {
            ItemChangedFunc((control as TControl).Tag as TObject);
            if (SelectedItemChanged != null)
                SelectedItemChanged(control, EventArgs.Empty);
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
            //TControl btn;
            foreach (var obj in list)
            {
                var btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                  //  Width = this.Width,// Client.Width,
                    Tag = obj,
                    //Text = nameFunc(obj),
                    //Name = nameFunc(obj),
                    TextFunc = () => nameFunc(obj),
                    NameFunc = () => nameFunc(obj),
                    HoverText = htv(obj),
                    Active = true
                };
                //btn.LeftClick += new UIEvent(btn_Click);
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
        public ListBox<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            this.SelectedTextFunc = nameFunc;
            this.OnControlInit = onControlInit;
            this.List = list;
        //    Func<TObject, string> htv = hoverTextFunc ?? (foo => "");
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                TControl btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                   // Width = this.Width,// Client.Width,
                    Tag = obj,
                    //Text = nameFunc(obj),
                    //Name = nameFunc(obj),
                    TextFunc = () => nameFunc(obj),
                    NameFunc = () => nameFunc(obj),
                    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                 //   HoverText = htv(obj),
                    Active = true
                };
                onControlInit(obj, btn as TControl);
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    action();
                    btn_Click(btn);
                };
                //btn.LeftClick += new UIEvent(btn_Click);
                // this.Add(btn);
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            // SelectedItem = default(TObject);
            SelectedControl = null;
            return this;
        }
        public ListBox<TObject, TControl> BuildCollapsible(Dictionary<string, List<TObject>> list, Func<TObject, string> nameFunc, Action<string, TControl> onCategoryInit, Action<TObject, TControl> onControlInit)
        {
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var inner = new GroupBox();// new List<TControl>();
                foreach(var item in obj.Value)
                {
                    TControl btninner = new TControl();
                    btninner.Tag = item;
                    btninner.Name = btninner.Text;
                    btninner.TooltipFunc = (tt) => { if (item is ITooltippable) (item as ITooltippable).GetTooltipInfo(tt); };
                    btninner.Text = nameFunc(item);
                    btninner.Active = true;
                    btninner.Location = inner.Controls.BottomLeft;// +new Vector2(16, 0);
                    btninner.LeftClickAction = () => Toggle(btninner);
                    inner.Controls.Add(btninner);
                }

                TControl btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = inner,
                    Text = obj.Key,
                    Name = obj.Key,
                    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                    Active = true
                };
                //onControlInit(obj, btn as TControl);
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    this.Toggle(btn);
                };
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            SelectedControl = null;
            return this;
        }

        void Toggle(TControl btn)
        {
            var inner = btn.Tag as GroupBox;
            if (this.Client.Controls.Contains(inner))
                this.Collapse(btn);
            else
                this.Expand(btn);
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
            //this.Client.Controls.Add(inner);
            this.Add(inner);
            //Remeasure();
        }
        private void Collapse(TControl btn)
        {
            var inner = btn.Tag as GroupBox;
            this.Remove(inner);

            var btnIndex = this.Client.Controls.FindIndex(c => c == btn);
            for (int i = btnIndex + 1; i < this.Client.Controls.Count; i++)
            {
                var control = this.Client.Controls[i];
                control.Location.Y -= inner.Height;
            }
            Remeasure();
        }

        public override void Remeasure()
        {
            base.Remeasure();
            // TODO: lol
            //int oldH = Client.Height;
            //Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
            //Client.Height = oldH;
            foreach (var btn in Client.Controls)
            {
                //btn.Width = Client.Width;
                btn.Width = Math.Max(btn.Width, Client.Width);
            }
        }

        //void btn_Click(object sender, EventArgs e)
        //{
        //    TControl ctrl = sender as TControl;
        ////    this.SelectedItem = ctrl.Tag as TObject;
        //    if (!this.SelectedControl.IsNull())
        //        this.SelectedControl.BackgroundColor = Color.Transparent;
        //    this.SelectedControl = ctrl;
        //    ctrl.BackgroundColor = Color.White * 0.5f;
        //    OnSelectedItemChanged(sender);
        //}
        void btn_Click(TControl ctrl)
        {
            //TControl ctrl = sender as TControl;
            //    this.SelectedItem = ctrl.Tag as TObject;
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

