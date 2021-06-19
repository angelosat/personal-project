using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class MaterialPicker : PanelLabeled
    {
        static MaterialPicker _Instance;
        static public MaterialPicker Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new MaterialPicker();
                return _Instance;
            }
        }

        Reaction.Reagent Reagent { get; set; }
        SlotGrid<Slot<GameObject>, GameObject> Slots { get; set; }
        //SlotGrid Slots { get; set; }

        Action<GameObject> Callback { get; set; }
        MaterialPicker()
            : base("Materials")
        {
            this.AutoSize = true;
            IconButton btn = new IconButton()
            {
                BackgroundTexture = UIManager.Icon16Background,
                Icon = new Icon(UIManager.Icons16x16, 0, 16),
                LeftClickAction = () => this.Hide(),
                LocationFunc = ()=>new Vector2(this.Width - 16 - UIManager.BorderPx - this.ClientLocation.X, UIManager.BorderPx - this.ClientLocation.Y)
            };

            this.Controls.Add(btn);
        }
        //public MaterialPicker(Reaction.Reagent reagent, Action<GameObject> callback)
        public void Show(Vector2 position, Reaction.Reagent reagent, Action<GameObject> callback)
        //public void Refresh(Reaction.Reagent reagent, Action<GameObject> callback)
        {
            this.Callback = callback;
            this.Location = position;
            //this.Controls.Clear();
            this.Controls.Remove(this.Slots);
            this.Reagent = reagent;
            //var mats = (from mat in ReagentComponent.Registry let obj = GameObject.Objects[mat] where this.Reagent.Pass(obj) select obj.ToSlot());
            var mats = (from mat in ReagentComponent.Registry let obj = GameObject.Objects[mat] where this.Reagent.Filter(obj) select obj);

            //this.Slots = new SlotGrid(mats, null, 8, sl=>
            this.Slots = new SlotGrid<Slot<GameObject>, GameObject>(mats, 8, (sl, o) =>
            {
                sl.LeftClickAction = () =>
                {
                    this.Callback(o);
                    this.Hide();
                };
            }) { Location = this.Controls.BottomLeft };

            //this.Slots = new SlotGrid(mats.ToList(), 8, (sl) =>
            //{
            //    sl.LeftClickAction = () =>
            //    {
            //        this.Callback(sl.Tag.Object);
            //        this.Hide();
            //    };
            //}) { Location = this.Controls.BottomLeft };
            this.Controls.Add(this.Slots);
            this.Show();
            this.ConformToScreen();
        }
        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Hide();
            base.HandleRButtonDown(e);
        }
        
    }
}
