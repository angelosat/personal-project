using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.AI;
using Start_a_Town_.Crafting;
using Start_a_Town_.Modules.Crafting;

namespace Start_a_Town_.Towns.Crafting
{
    public class CraftingManager : TownComponent
    {
        static CraftingManager()
        {
            Server.RegisterPacketHandler(PacketType.CraftingOrderToggleReagent, CraftingOrderToggleReagent);
            Client.RegisterPacketHandler(PacketType.CraftingOrderToggleReagent, CraftingOrderToggleReagent);

            Server.RegisterPacketHandler(PacketType.CraftingOrderModifyPriority, CraftingOrderModifyPriority);
            Client.RegisterPacketHandler(PacketType.CraftingOrderModifyPriority, CraftingOrderModifyPriority);

            Server.RegisterPacketHandler(PacketType.CraftingOrderModifyQuantity, CraftingOrderModifyQuantity);
            Client.RegisterPacketHandler(PacketType.CraftingOrderModifyQuantity, CraftingOrderModifyQuantity);

            Server.RegisterPacketHandler(PacketType.OrderSetRestrictions, SetOrderRestrictions);
            Client.RegisterPacketHandler(PacketType.OrderSetRestrictions, SetOrderRestrictions);

            PacketOrderAdd.Init();
            PacketOrderRemove.Init();
            PacketCraftingOrderSync.Init();
            PacketCraftOrderToggleHaul.Init();
            PacketCraftOrderChangeMode.Init();


        }
        

        internal IEnumerable<KeyValuePair<Vector3, List<CraftOrderNew>>> ByWorkstationNew()
        {
            //return this.Map.GetBlockEntities<BlockEntityWorkstation>().Select(r => new KeyValuePair<Vector3, List<CraftOrderNew>>(r.Key, r.Value.Orders));

            return this.Map.GetBlockEntitiesCache().Where(e => e.Value.HasComp<BlockEntityCompWorkstation>()).Select(r => new KeyValuePair<Vector3, List<CraftOrderNew>>(r.Key, r.Value.GetComp<BlockEntityCompWorkstation>().Orders));

        }
        internal BlockEntityCompWorkstation GetWorkstation(Vector3 global)
        {
            return this.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompWorkstation>();
        }

        public override string Name
        {
            get { return "Crafting"; }
        }
        int OrderSequence = 1;

        // TODO: add order priorities
        readonly Dictionary<Vector3, List<CraftOrderNew>> OrdersNew = new();
        readonly List<CraftOrderNew> OrdersNewNew = new();

        public CraftingManager(Town town)
        {
            this.Town = town;
        }

        

        internal void PlaceOrder(int reactionID, List<ItemRequirement> materials, Vector3 workstationGlobal)
        {
            var craftOp = new CraftOperation(reactionID, materials, workstationGlobal);
            Client.Instance.Send(PacketType.CraftingOrderPlace, Network.Serialize(craftOp.WriteOld));
        }
        public override void Handle(IObjectProvider net, Packet msg)
        {
            switch (msg.PacketType)
            {
                

                case PacketType.CraftingOrdersClear:
                    msg.Payload.Deserialize(r =>
                    {
                        var global = r.ReadVector3();
                        //this.Orders.Remove(global);
                        var entity = this.Town.Map.GetBlockEntity(global) as BlockEntityWorkstation;
                        entity.ExecutingOrders = false;
                        entity.CurrentOrder = null;
                        this.Town.Map.EventOccured(Message.Types.OrdersUpdated);
                        net.Forward(msg);
                    });
                    break;                    

                case PacketType.WorkstationSetCurrent:
                    msg.Payload.Deserialize(r =>
                        {
                            var senderID = r.ReadInt32();
                            //var global = r.ReadVector3();
                            //var product = new Reaction.Product.ProductMaterialPair(r);
                            var craft = new CraftOperation(r);
                            var benchEntity = net.Map.GetBlockEntity(craft.WorkstationEntity) as BlockEntityWorkstation;
                            benchEntity.SetCurrentProject(net, craft);
                            var server = net as Server;
                            if (server != null)
                                server.Enqueue(msg.PacketType, msg.Payload);
                        });
                    break;


                default:
                    break;
            }
        }

        public static void SetOrderRestrictions(CraftOrderNew order, string reagent, ItemDef[] defs, Material[] mats, MaterialType[] matTypes)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            w.Write((int)PacketType.OrderSetRestrictions);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(reagent);
            w.Write(defs?.Select(d => d.Name).ToArray());
            w.Write(mats?.Select(d => d.ID).ToArray());
            w.Write(matTypes?.Select(d => d.ID).ToArray());
        }
        private static void SetOrderRestrictions(IObjectProvider net, BinaryReader r)
        {
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(r.ReadVector3());
            var order = benchEntity.GetOrder(r.ReadInt32());
            var reagent = r.ReadString();
            var defs = r.ReadStringArray().Select(Def.GetDef<ItemDef>).ToArray();
            var mats = r.ReadIntArray().Select(Material.GetMaterial).ToArray();
            var matTypes = r.ReadIntArray().Select(MaterialType.GetMaterialType).ToArray();
            order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
            if(net is Server)
            {
                SetOrderRestrictions(order, reagent, defs, mats, matTypes);
            }
        }


        static void CraftingOrderToggleReagent(IObjectProvider net, BinaryReader r)
        {
            var global = r.ReadVector3();
            var orderID = r.ReadInt32();
            //var benchEntity = net.Map.GetBlockEntity(global) as BlockEntityWorkstation;// Blocks.BlockWorkbench.Entity;
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            //var order = net.Map.Town.CraftingManager.OrdersNew[global][orderIndex];
            var order = benchEntity.GetOrder(orderID);

            var reagent = r.ReadString();
            var itemID = r.ReadInt32();
            var add = r.ReadBoolean();
            order.ToggleReagentRestriction(reagent, itemID, add);
            net.EventOccured(Message.Types.OrderParametersChanged, order);

            if (net is Server server)
                WriteOrderToggleReagent(server.OutgoingStream, order, reagent, itemID, add);
        }
        static void CraftingOrderModifyPriority(IObjectProvider net, BinaryReader r)
        {
            var global = r.ReadVector3();
            var orderIndex = r.ReadInt32();
            //var benchEntity = net.Map.GetBlockEntity(global) as BlockEntityWorkstation;// Blocks.BlockWorkbench.Entity;
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);

            //var manager = net.Map.Town.CraftingManager;
            //var order = manager.OrdersNew[global][orderIndex];
            var order = benchEntity.GetOrder(orderIndex);
            var increase = r.ReadBoolean();

            //manager.OrdersNew[global].Remove(order);
            //var newIndex = Math.Max(0, Math.Min(manager.OrdersNew[global].Count, orderIndex + (increase ? -1 : 1)));
            //manager.OrdersNew[global].Insert(newIndex, order);
            if (!benchEntity.Reorder(orderIndex, increase))
                return;
            net.EventOccured(Message.Types.OrdersUpdatedNew, benchEntity);
            if (net is Server server)
                WriteOrderModifyPriority(server.OutgoingStream, order, increase);// global, orderIndex, increase);
        }
        static void CraftingOrderModifyQuantity(IObjectProvider net, BinaryReader r)
        {
            var global = r.ReadVector3();
            //var benchEntity = net.Map.GetBlockEntity(global) as BlockEntityWorkstation;// Blocks.BlockWorkbench.Entity;
            var orderid = r.ReadString();
            //var order = benchEntity.Orders.First(o=>o.GetUniqueLoadID()==r.ReadString());
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderid);//.First(o => o.GetUniqueLoadID() == orderid);

            var quantity = r.ReadInt32();
            var lastquantity = order.Quantity;
            order.Quantity += quantity;
            order.Quantity = Math.Max(order.Quantity, 0);
            if (order.Quantity == lastquantity)
                return;

            //var benchEntity = net.Map.GetBlockEntity(order.Workstation) as BlockEntityWorkstation;// Blocks.BlockWorkbench.Entity;

            //if (order.Quantity == 0)
            //    benchEntity.CurrentWorker = null; // ugly , do i need this?

            net.EventOccured(Message.Types.OrdersUpdated, benchEntity);

            var server = net as Server;
            if (server != null)
                WriteOrderModifyQuantityParams(server.OutgoingStream, order, quantity);
                //WriteOrderModifyQuantityParams(server.OutgoingStream, global, orderIndex, quantity);
        }

        private CraftOrderNew GetOrderNew(string v)
        {
            return this.OrdersNewNew.First(o => o.GetUniqueLoadID() == v);
        }

        static public void WriteOrderModifyQuantityParams(BinaryWriter w, CraftOrderNew order, int quantity)
        {
            w.Write(PacketType.CraftingOrderModifyQuantity);
            //w.Write(bench);
            //w.Write(orderIndex);
            w.Write(order.Workstation);
            w.Write(order.GetUniqueLoadID());
            w.Write(quantity);
        }
        static public void WriteOrderToggleReagent(BinaryWriter w, CraftOrderNew order, string reagent, int item, bool add)
        {
            w.Write(PacketType.CraftingOrderToggleReagent);
            //w.Write(actor.InstanceID);
            w.Write(order.Workstation);

            //var index = this.Town.CraftingManager.GetOrdersNew(order.Workstation).FindIndex(c => c == order);
            //w.Write(index);
            w.Write(order.ID);// GetIndex());
            w.Write(reagent);
            w.Write(item);
            w.Write(add);
        }

        internal static void WriteOrderModifyPriority(BinaryWriter w, CraftOrderNew order, bool increase)
        {
            w.Write(PacketType.CraftingOrderModifyPriority);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(increase);
        }
        
        
        
        public bool GetClosestFreeStation(Vector3 global, out Vector3 closest)
        {
            //var closestStation = this.OrdersNew.Keys.OrderBy(k => Vector3.DistanceSquared(k, global)).FirstOrDefault();
            //return (from k in this.OrdersNew.Keys
            //        where !this.ActiveJobs.ContainsKey(k)
            //        orderby Vector3.DistanceSquared(k, global)
            //        select k).FirstOrDefault();

            var benches = (from k in this.OrdersNew.Keys
                       let e = this.Town.Map.GetBlockEntity(k) as BlockEntityWorkstation
                       where e.CurrentWorker == null // does this work?
                       orderby Vector3.DistanceSquared(k, global)
                       select k).ToList();
            if (benches.Count == 0)
            {
                closest = Vector3.Zero;
                return false;
            }
            closest = benches.First();
            return true;

            //closest = (from k in this.OrdersNew.Keys
            //           let e = this.Town.Map.GetBlockEntity(k) as BlockEntityWorkstation
            //           where e.CurrentWorker == null // does this work?
            //           orderby Vector3.DistanceSquared(k, global)
            //           select k).FirstOrDefault();
            //return closest != null;
        }


        
        
        
        public void AddOrderNew(CraftOrderNew order)
        {
            this.OrdersNewNew.Add(order);
            this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, order.Workstation);

            return;

            //List<CraftOrderNew> benchOrders;
            //var benchGlobal = order.Workstation;
            //if (this.OrdersNew.TryGetValue(benchGlobal, out benchOrders))
            //    benchOrders.Add(order);
            //else
            //    this.OrdersNew[benchGlobal] = new List<CraftOrderNew>() { order };
            //this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, benchGlobal);
        }
        
        internal bool RemoveOrder(Vector3 station, string orderID)
        {
            //var bench = this.Map.GetBlockEntity(station) as BlockEntityWorkstation;
            var bench = this.GetWorkstation(station);
            if (bench.Orders.RemoveAll(r => r.GetUniqueLoadID() == orderID) > 0)
            {
                this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, bench);
                return true;
            }
            return false;

            //var order = this.OrdersNewNew.FirstOrDefault(o => o.GetUniqueLoadID() == orderID);
            //if (order == null)
            //    return false;
            ////this.OrdersNewNew.RemoveAll(o => o.Name == orderID);
            //this.OrdersNewNew.Remove(order);
            //this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, order.Workstation);
            //return true;
        }
        
       
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                

                case Message.Types.BlockChanged:
                    var global = (Vector3)e.Parameters[1];
                    if(this.OrdersNew.Remove(global)) //if a block containing a workstation is changed to another type, immediately clear all corresponding orders, no need to check for anything
                        this.Town.Map.EventOccured(Message.Types.OrdersUpdated); // is this necessary?
                    // TODO: close crafting window if it's open for the removed station
                    break;

                default:
                    break;
            }
        }

       

        
        internal List<CraftOrderNew> GetOrdersNew(Vector3 workstationGlobal)
        {
            var benchEntity = this.Map.GetBlockEntity(workstationGlobal).GetComp<BlockEntityCompWorkstation>();
            return benchEntity.Orders.ToList();
            return this.OrdersNewNew.Where(o => o.Workstation == workstationGlobal).ToList();
            //return this.OrdersNew.GetValueOrDefault(workstationGlobal);
        }

        internal IEnumerable<Vector3> GetWorkstations()
        {
            return this.OrdersNew.Keys;
        }
        public IEnumerable<IGrouping<Vector3, CraftOrderNew>> ByWorkstation()
        {
            return this.OrdersNewNew.GroupBy(i => i.Workstation);
        }
        internal void AddOrder(Vector3 station, int reactionID)
        {
            var order = new CraftOrderNew(this.OrderSequence++, reactionID, this.Town.Map, station);
            //var benchEntity = this.Map.GetBlockEntity(station) as BlockEntityWorkstation;
            //benchEntity.ExecutingOrders = true;
            //benchEntity.Orders.Add(order);
            //this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, benchEntity);
            var benchEntity = this.Map.GetBlockEntity(station).GetComp<BlockEntityCompWorkstation>();
            //benchEntity.ExecutingOrders = true;
            benchEntity.Orders.Add(order);
            this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, benchEntity);

            //this.AddOrderNew(order);
        }
        internal void RemoveOrder(Vector3 station, int orderIndex)
        {
            throw new Exception();
            //var order = this.OrdersNew[station][orderIndex];
            //this.RemoveOrderNew(order);
        }

        internal bool OrderExists(CraftOrderNew craftOrderNew)
        {
            var orders = this.GetOrdersNew(craftOrderNew.Workstation);
            if (orders == null)
                return false;
            return orders.Contains(craftOrderNew);
        }
        public CraftOrderNew GetOrder(Vector3 benchGlobal, int orderIndex)
        {
            return this.OrdersNew[benchGlobal][orderIndex];
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.OrderSequence.Save("IDSequence"));
            var ordersTag = new SaveTag(SaveTag.Types.List, "Orders", SaveTag.Types.List);

            //tag.TrySaveRefs(this.OrdersNewNew, "Orders");


            foreach (var global in this.OrdersNew)
            {
                // error handling here, skip any orders for any coordinates that aren't workstations anymore and for some reason haven't been removed
                if (this.Town.Map.GetBlockEntity(global.Key) as BlockEntityWorkstation == null)
                    continue;
                var globaltag = new SaveTag(SaveTag.Types.List, "", SaveTag.Types.Compound);
                foreach (var order in global.Value)
                {
                    var ordertag = order.Save("");
                    globaltag.Add(ordertag);
                }
                ordersTag.Add(globaltag);
            }
            //tag.Add(ordersTag);

            //return tag;
        }
        public override void Load(SaveTag tag)
        {
            //tag.TryLoadRefs("Orders", ref this.OrdersNewNew);
            tag.TryGetTagValue<int>("IDSequence", v => this.OrderSequence = v);


            //List<SaveTag> ordersTag;
            //if (tag.TryGetTagValue<List<SaveTag>>("Orders", out ordersTag))
            //    foreach (var globaltag in ordersTag)
            //    {
            //        List<SaveTag> orderstag = globaltag.Value as List<SaveTag>;
            //        foreach (var ordertag in orderstag)
            //        {
            //            //var order = new CraftOrderNew(this.Town.Map, ordertag);
            //            var order = CraftOrderNew.Load(this.Town.Map, ordertag);
            //            this.AddOrderNew(order);
            //        }
            //        //var order = new CraftOrder(globaltag);
            //        //AddOrder(order);
            //    }
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(this.OrderSequence);
            w.Write(this.OrdersNewNew.Count);
            for (int i = 0; i < this.OrdersNewNew.Count; i++)
            {
                this.OrdersNewNew[i].Write(w);
            }


        }
        public override void Read(BinaryReader r)
        {
            this.OrderSequence = r.ReadInt32();
            var orderCount = r.ReadInt32();
            for (int i = 0; i < orderCount; i++)
            {
                this.OrdersNewNew.Add(new CraftOrderNew(this.Town.Map, r));
            }

            //var workstationsCount = r.ReadInt32();
            //for (int i = 0; i < workstationsCount; i++)
            //{
            //    var orderscount = r.ReadInt32();
            //    for (int j = 0; j < orderscount; j++)
            //    {
            //        var order = new CraftOrderNew(this.Town.Map, r);
            //        this.AddOrderNew(order);// TODO: enqueue or add order immediately???
            //    }
            //}
        }

    }
}
