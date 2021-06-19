using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SlotGrid<TSlot, TItem> : GroupBox
        where TSlot : Slot<TItem>, new()
        where TItem : class, ISlottable
    {
        public SlotGrid(IEnumerable<TItem> items, int lineMax, Action<TSlot, TItem> slotInit = null)
        {
            Action<TSlot, TItem> _slotInit = slotInit ?? ((slot, item) => { });
            int count = items.Count();
            int i = 0;
            foreach (var item in items)
            {
                var slot = new TSlot() { Tag = item, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                _slotInit(slot, item);
                this.Controls.Add(slot);
                i++;
            }
        }
    }

    class SlotGridCustom<TSlot, TItem> : GroupBox
        where TSlot : SlotCustom<TItem>, new()
        where TItem : class, ISlottable
    {
        public SlotGridCustom(IEnumerable<TItem> items, int lineMax, Action<TSlot, TItem> slotInit = null)
        {
            Action<TSlot, TItem> _slotInit = slotInit ?? ((slot, item) => { });
            int count = items.Count();
            int i = 0;
            foreach (var item in items)
            {
                var slot = new TSlot() { Tag = item, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                _slotInit(slot, item);
                this.Controls.Add(slot);
                i++;
            }
        }
    }

    class SlotGrid<T> : GroupBox where T : Slot, new()
    {
        public SlotGrid(IEnumerable<GameObjectSlot> objSlots, int lineMax)
            : this(objSlots.ToList(), lineMax, (c) => { })
        {
        }
        public SlotGrid(List<GameObjectSlot> objSlots, int lineMax)
            : this(objSlots, lineMax, (c) => { })
        {
        }

        public SlotGrid(List<GameObjectSlot> objSlots, int lineMax, Action<T> slotInit)
            : base()
        {
            for (int i = 0; i < objSlots.Count; i++)
            {
                //var slot = new InventorySlot(objSlots[i], parent) { Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                var objslot = objSlots[i];
                var slot = new T() { Tag = objslot, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                slotInit(slot);
                this.Controls.Add(slot);
            }
        }
        public SlotGrid(List<GameObjectSlot> objSlots, int lineMax, Func<GameObjectSlot, T> slotCtor)
            : base()
        {
            for (int i = 0; i < objSlots.Count; i++)
            {
                //var slot = new InventorySlot(objSlots[i], parent) { Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                var objslot = objSlots[i];
                var slot = slotCtor(objslot);
                slot.Tag = objslot;
                slot.Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height);
                //slotInit(slot);
                this.Controls.Add(slot);
            }
        }
        public SlotGrid(int count, int lineMax)
            : this(count, lineMax, s => { })
        {

        }
        public SlotGrid(int count, int lineMax, Action<T> slotInit)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = new T() { Tag = GameObjectSlot.Empty, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                slotInit(slot);
                this.Controls.Add(slot);
            }
        }
    }

    class SlotGrid : GroupBox
    {
        List<Slot> All = new List<Slot>();
        int LineMax;

        public SlotGrid(List<GameObjectSlot> objSlots, GameObject parent, int lineMax)
            : this(objSlots, parent, lineMax, (c) => { })
        {
        }
        public SlotGrid(int count, int lineMax, Action<Slot> slotInit = null)
        {
            this.AutoSize = false;
            this.LineMax = lineMax;
            Action<Slot> _slotInit = slotInit ?? (c => { });
            for (int i = 0; i < count; i++)
            {
                var slot = new Slot() { Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                _slotInit(slot);
                this.All.Add(slot);
                this.Controls.Add(slot);
            }
        }
       
        public SlotGrid(List<GameObjectSlot> objSlots, GameObject parent, int lineMax, Action<InventorySlot> slotInit)
            : base()
        {
            this.LineMax = lineMax;
            for (int i = 0; i < objSlots.Count; i++)
            {
                var slot = new InventorySlot(objSlots[i], parent) { Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                slotInit(slot);
                this.All.Add(slot);
                this.Controls.Add(slot);
            }
        }

        public SlotGrid(List<GameObjectSlot> objSlots, int lineMax, Action<InventorySlot> slotInit = null)
            : base()
        {
            this.LineMax = lineMax;
            for (int i = 0; i < objSlots.Count; i++)
            {
                var objslot = objSlots[i];
                var slot = new InventorySlot(objslot, objslot.Parent) { Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                if (slotInit != null)
                    slotInit(slot);
                this.All.Add(slot);
                this.Controls.Add(slot);
            }
        }

        public void Refresh(Predicate<GameObjectSlot> filter)
        {
            this.Controls.Clear();
            var i = 0;
            foreach(var slot in this.All)
            {
                if (!filter(slot.Tag))
                    continue;
                slot.Location = new Vector2((i % this.LineMax) * UIManager.SlotSprite.Width, (i / this.LineMax) * UIManager.SlotSprite.Height);
                i++;
                this.Controls.Add(slot);
            }
        }
        public void Refresh()
        {
            this.Controls.Clear();
            var i = 0;
            foreach (var slot in this.All)
            {
                slot.Location = new Vector2((i % this.LineMax) * UIManager.SlotSprite.Width, (i / this.LineMax) * UIManager.SlotSprite.Height);
                i++;
                this.Controls.Add(slot);
            }
        }
    }
}
