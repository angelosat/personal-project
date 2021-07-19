using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Crafting
{
    public abstract class BlockEntityWorkstation : BlockEntity
    {
        /// <summary>
        /// make this a property that returns first item from this.getorders(this.global) ?
        /// </summary>
        public GameObject CurrentWorker;
        public List<CraftOrderNew> Orders = new List<CraftOrderNew>();
        public bool ExecutingOrders;
        public abstract Container Input { get; }
        public abstract IsWorkstation.Types Type { get; }

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
        
        public override List<GameObjectSlot> GetChildren()
        {
            if (this.Input != null)
                return this.Input.Slots;
            return new List<GameObjectSlot>();
        }

        public bool Insert(Entity material)
        {
            return this.Input.Insert(material);
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.TrySaveRefs(this.Orders, "Orders");
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryLoadRefs<CraftOrderNew>("Orders", ref this.Orders);
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            this.Orders.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Orders.Read(r);
        }
    }
}
