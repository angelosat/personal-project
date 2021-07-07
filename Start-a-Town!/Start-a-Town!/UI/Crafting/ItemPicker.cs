using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class ItemPicker : PanelLabeled
    {
        static ItemPicker _Instance;
        static public ItemPicker Instance => _Instance ??= new ItemPicker();

        Func<GameObject, bool> Condition { get; set; }
        SlotGrid Slots { get; set; }
        CheckBox ChkBox_MatsAvailable;
        Container Storage;
        List<GameObjectSlot> Sources;
        Action<GameObject> Callback { get; set; }
        ItemPicker()
            : base("Select item")
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

            this.ChkBox_MatsAvailable = new CheckBox("Materials Available");
            this.ChkBox_MatsAvailable.Location = Location = this.Controls.BottomLeft;
            this.ChkBox_MatsAvailable.ValueChangedFunction = (value) =>
                {
                    if (value)
                        this.Slots.Refresh(slot => 
                            (from s in this.Sources
                                 where s.Object != null
                                 where s.Object.IDType == slot.Object.IDType
                                 select s).FirstOrDefault() != null
                            );

                    else
                        this.Slots.Refresh();
                    this.Invalidate();
                    this.ConformToScreen();

                };
            this.Controls.Add(this.ChkBox_MatsAvailable);

        }
        
        public void Show(Vector2 position, Func<GameObject, bool> condition, IEnumerable<GameObjectSlot> sources, Action<GameObject> callback)
        {
            if (!this.Controls.Contains(this.ChkBox_MatsAvailable))
                this.Controls.Add(this.ChkBox_MatsAvailable);
            if (!this.ChkBox_MatsAvailable.Checked)
            {
                this.Show(position, condition, callback);
                return;
            }
            this.Callback = callback;
            this.Location = position;
            this.Controls.Remove(this.Slots);
            this.Condition = condition;
            this.Sources = sources.ToList();
            var all = //sources.All(slot => { if (slot.Object != null) return this.Condition(slot.Object); return false; });
                from slot in sources 
                where slot.Object != null 
                where this.Condition(slot.Object)
                select slot.Object;
                //(from item in ReagentComponent.Registry let obj = GameObject.Objects[item] where this.Reagent.Pass(obj) select obj);
            var allslots = from slot in all select slot.ToSlotLink();
            this.Slots = new SlotGrid(allslots.ToList(), 8, sl =>
            {
                sl.LeftClickAction = () =>
                {
                    this.Callback(sl.Tag.Object);
                    this.Hide();
                };
            }) { Location = this.Controls.BottomLeft };
            //this.Slots = new SlotGrid<Slot<GameObject>, GameObject>(all, 8, (sl, o) =>
            //{
            //    sl.LeftClickAction = () =>
            //    {
            //        this.Callback(o);
            //        this.Hide();
            //    };
            //}) { Location = this.Controls.BottomLeft };
            this.Controls.Add(this.Slots);
            this.Show();
            this.ConformToScreen();
        }
        public void Show(Vector2 position, Func<GameObject, bool> condition, Container sources, Action<GameObject> callback)
        {
            if (!this.Controls.Contains(this.ChkBox_MatsAvailable))
                this.Controls.Add(this.ChkBox_MatsAvailable);
            this.Storage = sources;
            this.Callback = callback;
            this.Location = position;
            this.Controls.Remove(this.Slots);
            this.Condition = condition;
            var all = //sources.All(slot => { if (slot.Object != null) return this.Condition(slot.Object); return false; });
                //from slot in sources.Slots
                //where slot.Object != null
                //where this.Condition(slot.Object)
                //select slot.Object;
                (from obj in GameObject.Objects.Values where this.Condition(obj) select obj);

            //(from item in ReagentComponent.Registry let obj = GameObject.Objects[item] where this.Reagent.Pass(obj) select obj);
            var allslots = from slot in all select slot.ToSlotLink();
            this.Slots = new SlotGrid(allslots.ToList(), 8, sl =>
            {
                sl.LeftClickAction = () =>
                {
                    this.Callback(sl.Tag.Object);
                    this.Hide();
                };
            }) { Location = this.Controls.BottomLeft };
            if (this.ChkBox_MatsAvailable.Checked)
                this.Slots.Refresh(slot => this.Storage.Contains(o => slot.Object.IDType == o.IDType));
            this.Controls.Add(this.Slots);
            this.Show();
            this.ConformToScreen();
        }
        public void Show(Vector2 position, Func<GameObject, bool> condition, Action<GameObject> callback)
        {
            if (this.Controls.Contains(this.ChkBox_MatsAvailable))
                this.Controls.Remove(this.ChkBox_MatsAvailable);
            this.Callback = callback;
            this.Location = position;
            this.Controls.Remove(this.Slots);
            this.Condition = condition;
            var all = //(from item in ReagentComponent.Registry let obj = GameObject.Objects[item] where this.Condition(obj) select obj);
                (from obj in GameObject.Objects.Values where this.Condition(obj) select obj);
            var allslots = from slot in all select slot.ToSlotLink();
            this.Slots = new SlotGrid(allslots.ToList(), 8, sl =>
            {
                sl.LeftClickAction = () =>
                {
                    this.Callback(sl.Tag.Object);
                    this.Hide();
                };
            }) { Location = this.Controls.BottomLeft };
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
