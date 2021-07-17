using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SlotGridNew : GroupBox
    {
        List<Slot> All = new List<Slot>();
        int LineMax;
        Action<Slot> SlotInit = (s) => { };
        
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
