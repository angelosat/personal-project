using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class BlockEntityCompWorkstation : BlockEntityComp
    {
        readonly List<CraftOrderNew> _Orders = new List<CraftOrderNew>();
        public List<CraftOrderNew> Orders => this._Orders;
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
        internal CraftOrderNew GetOrder(string uniqueID)
        {
            return this.Orders.Find(o => o.GetUniqueLoadID() == uniqueID);
        }
        internal CraftOrderNew GetOrder(int uniqueID)
        {
            return this.Orders.Find(o => o.ID == uniqueID);
        }
        internal bool Reorder(int orderID, bool increasePriority)
        {
            var order = this.GetOrder(orderID);
            if (order == null)
                return false;
            var prevIndex = this.Orders.IndexOf(order);
            this.Orders.Remove(order);
            var newIndex = Math.Max(0, Math.Min(this.Orders.Count, prevIndex + (increasePriority ? -1 : 1)));
            this.Orders.Insert(newIndex, order);
            return true;
        }
        
        internal override void GetQuickButtons(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        {
            uISelectedInfo.AddTabAction("Orders", () => ShowUI(map, vector3));
        }

        public void ShowUI(IMap map, Vector3 global)
        {
            if (CraftingWindow != null)
                CraftingWindow.Hide();

            CraftingWindow = new Modules.Crafting.WorkstationInterfaceNew(map, global, this).ToWindow("Crafting");
            CraftingWindow.ToggleSmart();

        }

        internal override void DrawSelected(MySpriteBatch sb, Camera cam, IMap map, Vector3 global)
        {
            // draw workstation operating position
            cam.DrawGridCells(sb, Color.White * .5f, new Vector3[] { global + Block.GetFrontSide(map.GetCell(global).Orientation) });
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
        internal override void MapLoaded(IMap map, Vector3 global)
        {
            foreach (var ord in this._Orders)
                ord.Map = map;
        }
    }
}
