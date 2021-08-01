using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class BlockEntityCompWorkstation : BlockEntityComp
    {
        readonly ObservableCollection<CraftOrder> _Orders = new();
        public ObservableCollection<CraftOrder> Orders => this._Orders;
        readonly HashSet<IsWorkstation.Types> WorkstationTypes;
        static Window CraftingWindow;

        public BlockEntityCompWorkstation(params IsWorkstation.Types[] types)
        {
            this.WorkstationTypes = new HashSet<IsWorkstation.Types>(types);
        }
        public bool IsWorkstationType(IsWorkstation.Types type)
        {
            return this.WorkstationTypes.Contains(type);
        }

        internal CraftOrder GetOrder(string uniqueID)
        {
            return this.Orders.First(o => o.GetUniqueLoadID() == uniqueID);
        }
        internal CraftOrder GetOrder(int uniqueID)
        {
            return this.Orders.First(o => o.ID == uniqueID);
        }
        internal bool RemoveOrder(string orderID)
        {
            return this.Orders.Remove(this.GetOrder(orderID));
        }
        internal CraftOrder RemoveOrder(int orderID)
        {
            var order = this.GetOrder(orderID);
            this.Orders.Remove(order);
            return order;
        }
        internal bool Reorder(int orderID, bool increasePriority)
        {
            var order = this.GetOrder(orderID);
            var prevIndex = this.Orders.IndexOf(order);
            this.Orders.Remove(order);
            var newIndex = Math.Max(0, Math.Min(this.Orders.Count, prevIndex + (increasePriority ? -1 : 1)));
            this.Orders.Insert(newIndex, order);
            return true;
        }
        
        internal override void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3)
        {
            uISelectedInfo.AddTabAction("Orders", () => ShowUI(map, vector3));
        }

        public void ShowUI(MapBase map, IntVec3 global)
        {
            if (CraftingWindow != null)
                CraftingWindow.Hide();

            CraftingWindow = new Modules.Crafting.WorkstationGui(map, global, this).ToWindow("Crafting");
            CraftingWindow.ToggleSmart();
        }

        internal override void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
            // draw workstation operating position
            cam.DrawGridCells(sb, Color.White * .5f, new IntVec3[] { global + map.GetCell(global).Front });
        }

        public override void AddSaveData(SaveTag tag)
        {
            tag.Add(this._Orders.SaveNewBEST("Orders"));
        }

        public override void Load(SaveTag tag)
        {
            this._Orders.TryLoadMutable(tag, "Orders");
        }

        public override void Write(BinaryWriter w)
        {
            this._Orders.Write(w);
        }
        public override ISerializable Read(BinaryReader r)
        {
            this._Orders.ReadMutable(r);
            return this;
        }
        internal override void MapLoaded(MapBase map, IntVec3 global)
        {
            foreach (var ord in this._Orders)
                ord.Map = map;
        }
    }
}
