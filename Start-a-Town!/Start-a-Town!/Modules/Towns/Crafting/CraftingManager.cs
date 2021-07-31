using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;
using Start_a_Town_.Modules.Crafting;

namespace Start_a_Town_.Towns.Crafting
{
    public class CraftingManager : TownComponent
    {
        static int pReaget, pPriority, pQuantity, pRestrictions;
        static CraftingManager()
        {
            pReaget = Network.RegisterPacketHandler(CraftingOrderToggleReagent);
            pPriority = Network.RegisterPacketHandler(CraftingOrderModifyPriority);
            pQuantity = Network.RegisterPacketHandler(CraftingOrderModifyQuantity);
            pRestrictions = Network.RegisterPacketHandler(SetOrderRestrictions);

            PacketOrderAdd.Init();
            PacketOrderRemove.Init();
            PacketCraftingOrderSync.Init();
            PacketCraftOrderToggleHaul.Init();
            PacketCraftOrderChangeMode.Init();
        }
        
        internal IEnumerable<KeyValuePair<IntVec3, ICollection<CraftOrder>>> ByWorkstationNew()
        {
            //return this.Map.GetBlockEntitiesCache().Where(e => e.Value.HasComp<BlockEntityCompWorkstation>()).Select(r => new KeyValuePair<IntVec3, List<CraftOrderNew>>(r.Key, r.Value.GetComp<BlockEntityCompWorkstation>().Orders));
            return this.Map.GetBlockEntitiesCache()
                .Where(e => e.Value.HasComp<BlockEntityCompWorkstation>())
                .Select(r => new KeyValuePair<IntVec3, ICollection<CraftOrder>>(r.Key, r.Value.GetComp<BlockEntityCompWorkstation>().Orders));
        }
        internal BlockEntityCompWorkstation GetWorkstation(IntVec3 global)
        {
            return this.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompWorkstation>();
        }

        public override string Name => "Crafting";
        int OrderSequence = 1;

        // TODO: add order priorities
        readonly Dictionary<IntVec3, List<CraftOrder>> OrdersNew = new();
        readonly List<CraftOrder> OrdersNewNew = new();

        public CraftingManager(Town town)
        {
            this.Town = town;
        }
        
        public static void SetOrderRestrictions(CraftOrder order, string reagent, ItemDef[] defs, MaterialDef[] mats, MaterialType[] matTypes)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            w.Write(pRestrictions);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(reagent);
            w.Write(defs?.Select(d => d.Name).ToArray());
            w.Write(mats?.Select(d => d.ID).ToArray());
            w.Write(matTypes?.Select(d => d.ID).ToArray());
        }
        private static void SetOrderRestrictions(INetwork net, BinaryReader r)
        {
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(r.ReadIntVec3());
            var order = benchEntity.GetOrder(r.ReadInt32());
            var reagent = r.ReadString();
            var defs = r.ReadStringArray().Select(Def.GetDef<ItemDef>).ToArray();
            var mats = r.ReadIntArray().Select(MaterialDef.GetMaterial).ToArray();
            var matTypes = r.ReadIntArray().Select(MaterialType.GetMaterialType).ToArray();
            order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
            if(net is Server)
            {
                SetOrderRestrictions(order, reagent, defs, mats, matTypes);
            }
        }


        static void CraftingOrderToggleReagent(INetwork net, BinaryReader r)
        {
            var global = r.ReadIntVec3();
            var orderID = r.ReadInt32();
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderID);

            var reagent = r.ReadString();
            var itemID = r.ReadInt32();
            var add = r.ReadBoolean();
            order.ToggleReagentRestriction(reagent, itemID, add);
            net.EventOccured(Message.Types.OrderParametersChanged, order);

            if (net is Server server)
                WriteOrderToggleReagent(server.OutgoingStream, order, reagent, itemID, add);
        }
        static void CraftingOrderModifyPriority(INetwork net, BinaryReader r)
        {
            var global = r.ReadIntVec3();
            var orderIndex = r.ReadInt32();
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderIndex);
            var increase = r.ReadBoolean();
            if (!benchEntity.Reorder(orderIndex, increase))
                return;
            net.EventOccured(Message.Types.OrdersUpdatedNew, benchEntity);
            if (net is Server server)
                WriteOrderModifyPriority(server.OutgoingStream, order, increase);
        }
        static void CraftingOrderModifyQuantity(INetwork net, BinaryReader r)
        {
            var global = r.ReadIntVec3();
            var orderid = r.ReadString();
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderid);

            var quantity = r.ReadInt32();
            var lastquantity = order.Quantity;
            order.Quantity += quantity;
            order.Quantity = Math.Max(order.Quantity, 0);
            if (order.Quantity == lastquantity)
                return;

            if (net is Server server)
                WriteOrderModifyQuantityParams(server.OutgoingStream, order, quantity);
        }

        static public void WriteOrderModifyQuantityParams(BinaryWriter w, CraftOrder order, int quantity)
        {
            w.Write(pQuantity);
            w.Write(order.Workstation);
            w.Write(order.GetUniqueLoadID());
            w.Write(quantity);
        }
        static public void WriteOrderToggleReagent(BinaryWriter w, CraftOrder order, string reagent, int item, bool add)
        {
            w.Write(pReaget);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(reagent);
            w.Write(item);
            w.Write(add);
        }

        internal static void WriteOrderModifyPriority(BinaryWriter w, CraftOrder order, bool increase)
        {
            w.Write(pPriority);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(increase);
        }
       
        public void AddOrderNew(CraftOrder order)
        {
            this.OrdersNewNew.Add(order);
            this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, order.Workstation);
        }
        
        internal bool RemoveOrder(IntVec3 station, string orderID)
        {
            var bench = this.GetWorkstation(station);
            return bench.RemoveOrder(orderID);
        }
        internal CraftOrder RemoveOrder(IntVec3 station, int orderID)
        {
            var bench = this.GetWorkstation(station);
            return bench.RemoveOrder(orderID);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Message.Types.BlocksChanged:
                    var cells = e.Parameters[1] as IEnumerable<IntVec3>;
                    foreach (var global in cells)
                        this.OrdersNew.Remove(global); //if a block containing a workstation is changed to another type, immediately clear all corresponding orders, no need to check for anything
                    // TODO: close crafting window if it's open for the removed station
                    break;

                default:
                    break;
            }
        }
       
        internal void AddOrder(IntVec3 station, int reactionID)
        {
            var order = new CraftOrder(this.OrderSequence++, reactionID, this.Town.Map, station);
            var benchEntity = this.Map.GetBlockEntity(station).GetComp<BlockEntityCompWorkstation>();
            benchEntity.Orders.Add(order);
            this.Town.Map.EventOccured(Message.Types.OrdersUpdatedNew, benchEntity);
        }
        
        internal bool OrderExists(CraftOrder craftOrderNew)
        {
            var orders = this.GetOrdersNew(craftOrderNew.Workstation);
            if (orders == null)
                return false;
            return orders.Contains(craftOrderNew);
        }
        public CraftOrder GetOrder(IntVec3 benchGlobal, int orderIndex)
        {
            return this.OrdersNew[benchGlobal][orderIndex];
        }
        internal List<CraftOrder> GetOrdersNew(IntVec3 workstationGlobal)
        {
            var benchEntity = this.Map.GetBlockEntity(workstationGlobal).GetComp<BlockEntityCompWorkstation>();
            return benchEntity.Orders.ToList();
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.OrderSequence.Save("IDSequence"));
            var ordersTag = new SaveTag(SaveTag.Types.List, "Orders", SaveTag.Types.List);

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
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("IDSequence", v => this.OrderSequence = v);
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
                this.OrdersNewNew.Add(new CraftOrder(this.Town.Map, r));
            }
        }
    }
}
