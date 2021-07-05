﻿using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class GameObjectSlotCollection : List<GameObjectSlot>, IInterfaceable
    {
        public GameObjectSlotCollection()
        {
        }
        public GameObjectSlotCollection(List<GameObjectSlot> list)
        {
            this.AddRange(list);
        }

        public override string ToString()
        {
            string text = "";
            for (int i = 0; i < this.Count; i++)
                text += "[" + i + "]: " + this[i].ToString() + "\n";
            return text.TrimEnd('\n');
        }

        public void GetUI(UI.Control ui)
        {
            int i = 0;
            foreach (var obj in this)
            {
                Slot slot = new Slot(new Vector2(0, i * Slot.DefaultHeight)) { Tag = obj };
                Label label = new Label(slot.CenterRight, obj.HasValue ? obj.Object.Name : "<empty>", HorizontalAlignment.Left, VerticalAlignment.Center);
                ui.Controls.Add(slot, label);
            }
        }
    }
}
