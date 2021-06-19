using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Workbench;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;
using Start_a_Town_.Graphics;
using Start_a_Town_.Crafting;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Tokens;
using Start_a_Town_.AI;

namespace Start_a_Town_.Blocks
{
    partial class BlockWorkbench : BlockWorkstation
    {
        public class Entity : BlockEntityWorkstation// BlockEntity, IBlockEntityWorkstation
        {
            public override Container Input
            {
                get { return this.Storage; }
            }
            public override IsWorkstation.Types Type { get { return IsWorkstation.Types.Workbench; } }
            public Queue<CraftOperation> QueuedOrders = new Queue<CraftOperation>();

            //List<CraftOrder> PendingOrders = new List<CraftOrder>();
            //public bool MaterialsPresent(CraftOperation craft)
            //{
            //    //var mats = this.CurrentOrder.Materials;
            //    var mats = craft.Materials;
            //    foreach (var req in mats)
            //        if (this.Storage.GetAmount(o => (int)o.ID == req.ObjectID) < req.Max)
            //            return false;
            //    return true;
            //}
            //public void EnqueueOrder(IObjectProvider net, CraftOperation order)
            //{
            //    var o = new CraftOrder(0, order, 1);
            //    this.PendingOrders.Add(o);
            //}
            //public void RemoveOrder(IObjectProvider net, int index)
            //{
            //    this.PendingOrders.RemoveAt(index);
            //}
            //public void RemoveOrder(IObjectProvider net, CraftOrder order)
            //{
            //    this.PendingOrders.Remove(order);// All(o => o.ID == index);
            //}
            //public List<CraftOrder> GetQueuedOrders()
            //{
            //    return this.PendingOrders;
            //}
            //public CraftOperation GetCurrentOrder()
            //{
            //    var order = this.PendingOrders.FirstOrDefault();
            //    if (order == null)
            //        return null;
            //    return order.Craft;// this.PendingOrders.FirstOrDefault();
            //    //var town = map.GetTown();
            //    //var order = town.CraftingManager.GetOrdersFor(global).FirstOrDefault();
            //    //if (order == null)
            //    //    return null;
            //    //return order.Craft;
            //}
            //public override Container GetMaterialsContainer()
            //{
            //    return this.Storage;
            //}
            //public CraftOperation CurrentOrder { get { return this.QueuedOrders.FirstOrDefault(); } }
            //public int CurrentOrderID;

            public Container Blueprints { get; private set; }
            public Container Storage { get; private set; }
            public Entity()
            {
                this.Blueprints = new Container(8);
                this.Storage = new Container(8);
            }
            public override object Clone()
            {
                return new Entity();
            }
            public bool Insert(GameObjectSlot material)
            {
                return this.Storage.InsertObject(material);
            }
            public bool Insert(GameObject material)
            {
                return this.Storage.InsertObject(material);
            }
            public List<GameObjectSlot> GetReagentSlots(GameObject actor)
            {
                var contents = actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.Storage.Slots).ToList();
                return contents;
            }



            public bool IsMaterialValid(CraftOperation craft, GameObject obj)
            {
                //var mats = this.CurrentOrder.Materials;
                var mats = craft.Materials;
                if (obj == null)
                    return false;
                // TODO: check if the req amount has been met?
                var valid = mats.FirstOrDefault(r => r.ObjectID == (int)obj.ID) != null;
                return valid;
            }
            public override void Update(IObjectProvider net, Vector3 global)
            {
                this.AdvertiseCurrentOrders(net);
            }
            private void AdvertiseCurrentOrders(IObjectProvider net)
            {
                //foreach (var order in this.PendingOrders)//.Where(o=>o.ID == 0))
                //    net.Map.GetTown().CraftingManager.AddOrder(order);
            }
            public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global)
            {
                var craft = this.GetCurrentOrder();
                if (craft != null)
                    Bar.Draw(sb, cam, global + Vector3.UnitZ, "", craft.CraftProgress.Percentage, cam.Zoom * .2f);
            }

            //internal void EnqueueOrder(Net.IObjectProvider net, CraftOperation craftOp)
            //{
            //    this.QueuedOrders.Enqueue(craftOp);
            //    net.Map.EventOccured(Message.Types.OrdersUpdated, this);
            //}

            //internal CraftOperation RemoveOrder(Net.IObjectProvider net, int index)
            //{
            //    var list = this.QueuedOrders.ToList();
            //    var op = list[index];
            //    list.RemoveAt(index);
            //    this.QueuedOrders = new Queue<CraftOperation>(list);
            //    net.Map.EventOccured(Message.Types.OrdersUpdated, this);
            //    return op;
            //}

            //public override SaveTag Save(string name)
            //{
            //    var tag = new SaveTag(SaveTag.Types.Compound, name);
            //    tag.Add(this.CurrentOrderID.Save("CurrentOrderID"));
            //    //var orders = new SaveTag(SaveTag.Types.List, "Orders", SaveTag.Types.Compound);
            //    //foreach (var order in this.QueuedOrders)
            //    //    orders.Add(order.Save());
            //    //tag.Add(orders);
            //    return tag;
            //}
            //public override void Load(SaveTag tag)
            //{
            //    tag.TryGetTagValue<int>("CurrentOrderID", v => this.CurrentOrderID = v);
            //    //tag.TryGetTagValue<List<SaveTag>>("Orders", v =>
            //    //{
            //    //    foreach (var order in v)
            //    //    {
            //    //        this.QueuedOrders.Enqueue(new CraftOperation(order));
            //    //    }
            //    //});
            //}
            //public override void Write(BinaryWriter w)
            //{
            //    w.Write(this.CurrentOrderID);
            //    //w.Write(this.QueuedOrders.Count);
            //    //foreach (var o in this.QueuedOrders)
            //    //    o.Write(w);
            //}
            //public override void Read(BinaryReader r)
            //{
            //    this.CurrentOrderID = r.ReadInt32();
            //    //var count = r.ReadInt32();
            //    //for (int i = 0; i < count; i++)
            //    //{
            //    //    this.QueuedOrders.Enqueue(new CraftOperation(r));
            //    //}
            //}
        }       
    }
}
