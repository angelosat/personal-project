using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBoxCollapsible<TObject, TControl> : ScrollableBox
        where TControl : ButtonBase, new()
        where TObject : class
    {
        
        public TObject SelectedItem { get { return (SelectedControl.IsNull() ? default(TObject) : SelectedControl.Tag as TObject); } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
            this.btn_Click(this.SelectedControl);
        }

        public Action<TObject> ItemChangedFunc = (item) => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBoxCollapsible(int width, int height) : base(new Rectangle(0,0,width,height)) { }//MouseThrough = true; }
        public ListBoxCollapsible(Rectangle bounds) : base(bounds) { }//MouseThrough = true; }

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged(object control)
        {
            ItemChangedFunc((control as TControl).Tag as TObject);
            if (SelectedItemChanged != null)
                SelectedItemChanged(control, EventArgs.Empty);
        }
        public void Clear()
        {
            this.Client.Controls.Clear();
        }

        //public ListBox<TObject, TControl> Build(Dictionary<string, List<TObject>> list, Func<TObject, string> nameFunc, Action<string, TControl> onCategoryInit, Action<TObject, TControl> onControlInit)
        //{
        //    this.Client.Controls.Clear();
        //    foreach (var obj in list)
        //    {
        //        TControl btn = new TControl()
        //        {
        //            Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
        //            Tag = obj.Value,
        //            Text = obj.Key,
        //            Name = obj.Key,
        //            TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
        //            Active = true
        //        };
        //        //onControlInit(obj, btn as TControl);
        //        var action = btn.LeftClickAction;
        //        btn.LeftClickAction = () =>
        //        {
        //            this.Expand(btn);
        //        };
        //        this.Client.Controls.Add(btn);
        //    }
        //    Remeasure();
        //    Client.ClientLocation = Vector2.Zero;
        //    SelectedControl = null;
        //    return this;
        //}

        private void Expand(TControl btn)
        {
            List<TObject> list = btn.Tag as List<TObject>;

        }
        public override void Remeasure()
        {
            base.Remeasure();
            // TODO: lol
            int oldH = Client.Height;
            Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
            Client.Height = oldH;
            foreach (var btn in Client.Controls)
            {
                btn.Width = Client.Width;
              //  btn.Invalidate();
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
            if (!this.SelectedControl.IsNull())
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

