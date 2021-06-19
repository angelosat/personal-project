using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SlotGridNew<TSlot, TItem> : GroupBox
        where TSlot : Slot<TItem>, new()
        where TItem : class, ISlottable
    {
        public SlotGridNew(IEnumerable<TItem> items, int lineMax, Action<TSlot, TItem> slotInit = null)
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

    class SlotGridNewCustom<TSlot, TItem> : GroupBox
        where TSlot : SlotCustom<TItem>, new()
        where TItem : class, ISlottable
    {
        public SlotGridNewCustom(IEnumerable<TItem> items, int lineMax, Action<TSlot, TItem> slotInit = null)
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

    class SlotGridNew<T> : GroupBox where T : Slot, new()
    {
        public SlotGridNew(IEnumerable<GameObjectSlot> objSlots, int lineMax)
            : this(objSlots.ToList(), lineMax, (c) => { })
        {
        }
        public SlotGridNew(List<GameObjectSlot> objSlots, int lineMax)
            : this(objSlots, lineMax, (c) => { })
        {
        }

        public SlotGridNew(List<GameObjectSlot> objSlots, int lineMax, Action<T> slotInit)
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
        public SlotGridNew(List<GameObjectSlot> objSlots, int lineMax, Func<GameObjectSlot, T> slotCtor)
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
        public SlotGridNew(int count, int lineMax)
            : this(count, lineMax, s => { })
        {

        }
        public SlotGridNew(int count, int lineMax, Action<T> slotInit)
        {
            for (int i = 0; i < count; i++)
            {
                var slot = new T() { Tag = GameObjectSlot.Empty, Location = new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height) };
                slotInit(slot);
                this.Controls.Add(slot);
            }
        }
    }

    class SlotGridNew : GroupBox
    {
        List<Slot> All = new List<Slot>();
        int LineMax;
        Action<Slot> SlotInit = (s) => { };
        public SlotGridNew(List<GameObjectSlot> objSlots, GameObject parent, int lineMax)
            : this(objSlots, parent, lineMax, (c) => { })
        {
        }
        public SlotGridNew(int count, int lineMax, Action<Slot> slotInit = null)
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
       
        public SlotGridNew(List<GameObjectSlot> objSlots, GameObject parent, int lineMax, Action<InventorySlot> slotInit)
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

        public SlotGridNew(List<GameObjectSlot> objSlots, int lineMax, Action<Slot> slotInit = null)
            : base()
        {
            this.SlotInit = slotInit ?? new Action<Slot>((s) => { });
            this.LineMax = lineMax;
            for (int i = 0; i < objSlots.Count; i++)
            {
                var objslot = objSlots[i];
                var slot = new Slot(objslot) { Location = FindPosition(lineMax, i) };
                if (slotInit != null)
                    slotInit(slot);
                this.All.Add(slot);
                this.Controls.Add(slot);
            }
        }

        private static Vector2 FindPosition(int lineMax, int i)
        {
            return new Vector2((i % lineMax) * UIManager.SlotSprite.Width, (i / lineMax) * UIManager.SlotSprite.Height);
        }

        public void Add(GameObject obj)
        {
            var loc = FindPosition(this.LineMax, this.All.Count);
            var slot = new Slot(obj.ToSlotLink()) { Location = loc };
            this.SlotInit(slot);
            this.All.Add(slot);
            this.Controls.Add(slot);
        }
        public void Add(GameObjectSlot objSlot)
        {
            var loc = FindPosition(this.LineMax, this.All.Count);
            var slot = new Slot(objSlot) { Location = loc };
            this.SlotInit(slot);
            this.All.Add(slot);
            this.Controls.Add(slot);
        }
        public void Remove(GameObject obj)
        {
            var slot = this.All.First(f => f.Tag.Object == obj);
            this.Controls.Remove(slot);
            this.All.Remove(slot);
            this.Rearrange();
        }
        public void Remove(GameObjectSlot objSlot)
        {
            var slot = this.All.First(f => f.Tag == objSlot);
            this.Controls.Remove(slot);
            this.All.Remove(slot);
            this.Rearrange();
        }
        void Rearrange()
        {
            for (int i = 0; i < this.All.Count; i++)
                this.All[i].Location = FindPosition(this.LineMax, i);
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
