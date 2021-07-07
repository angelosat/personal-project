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
        public CraftOperation CurrentOrder;
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

        public bool MaterialsPresent(CraftOperation craft)
        {
            var input = this.Input;

            var mats = craft.Materials;
            foreach (var req in mats)
                if (input.GetAmount(o => (int)o.IDType == req.ObjectID) < req.AmountRequired)
                    return false;
            return true;
        }

        public CraftOperation GetCurrentOrder()
        {
            return this.CurrentOrder;
        }

        internal void SetCurrentProject(IObjectProvider net, CraftOperation craft)
        {
            this.ExecutingOrders = false;
            this.CurrentOrder = craft;
            net.Map.EventOccured(Message.Types.WorkstationOrderSet, this);
        }
        
        public bool Insert(Entity material)
        {
            return this.Input.InsertObject(material);
        }

        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global)
        {
            var craft = this.GetCurrentOrder();
            if (craft != null)
                Bar.Draw(sb, cam, global, "", craft.CraftProgress.Percentage, cam.Zoom * .2f);
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
