using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
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
}
