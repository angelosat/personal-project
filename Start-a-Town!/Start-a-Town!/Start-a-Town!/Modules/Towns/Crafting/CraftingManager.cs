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

namespace Start_a_Town_.Towns.Crafting
{
    public class CraftingManager : TownComponent
    {
        public override string Name
        {
            get { return "Crafting"; }
        }
        int OrderSequence = 1;
        //Dictionary<int, CraftOrder> PendingOrders = new Dictionary<int, CraftOrder>();
        // TODO: add order priorities
        Dictionary<Vector3, List<CraftOrder>> Orders = new Dictionary<Vector3, List<CraftOrder>>();

        Queue<CraftOrder> LoadedOrders = new Queue<CraftOrder>();

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
                case PacketType.CraftingOrderPlace:
                    msg.Payload.Deserialize(r =>
                    {
                        var craftOp = new CraftOperation(net, r);
                        var server = net as Server;
                        var benchEntity = net.Map.GetBlockEntity(craftOp.WorkstationEntity) as BlockEntityWorkstation;// as Blocks.BlockWorkbench.Entity;
                        benchEntity.ExecutingOrders = true;
                        //benchEntity.EnqueueOrder(net, craftOp);
                        //this.AddOrder(new CraftOrder(this.OrderSequence, craftOp, 1));
                        //this.OrderSequence++;
                        var order = new CraftOrder(this.OrderSequence++, craftOp, 1);
                        this.AddOrder(order);

                        if (server != null)
                        {
                            //this.PendingOrders.Add(this.OrderSequence, new CraftOrder(this.OrderSequence, craftOp, 1));
                            //this.OrderSequence++;
                            server.Enqueue(PacketType.CraftingOrderPlace, msg.Payload, SendType.OrderedReliable, true);
                        }
                    });
                    break;

                case PacketType.CraftingOrderRemove:
                    msg.Payload.Deserialize(r =>
                    {
                        var global = r.ReadVector3();
                        var orderIndex = r.ReadInt32();
                        var benchEntity = net.Map.GetBlockEntity(global) as BlockEntityWorkstation;// Blocks.BlockWorkbench.Entity;
                        //var order = benchEntity.GetQueuedOrders()[orderIndex];
                        var order = this.Orders[global][orderIndex];

                        //benchEntity.RemoveOrder(net, orderIndex);
                        //var removed = benchEntity.RemoveOrder(net, orderIndex);
                        //var order = this.PendingOrders[orderIndex];// this.PendingOrders.First(f => f.Value.Craft == removed);
                        this.RemoveOrder(order);
                        if (order.Job != null)
                            order.Job.Cancel();
                        //this.Town.Map.EventOccured(Message.Types.OrdersUpdated);
                        var server = net as Server;
                        if (server != null)
                            server.Enqueue(PacketType.CraftingOrderRemove, msg.Payload, SendType.OrderedReliable, true);
                    });
                    break;

                case PacketType.CraftingOrdersClear:
                    msg.Payload.Deserialize(r =>
                    {
                        var global = r.ReadVector3();
                        this.Orders.Remove(global);
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

                case PacketType.WorkstationToggle:
                    msg.Payload.Deserialize(r =>
                        {
                            var senderID = r.ReadInt32();
                            var global = r.ReadVector3();
                            var benchEntity = net.Map.GetBlockEntity(global) as BlockEntityWorkstation;
                            var value = r.ReadBoolean();
                            if (value)
                                this.EnableWorkstation(global);
                            else
                                this.DisableWorkstation(global);
                            benchEntity.ExecutingOrders = value;
                            net.Map.EventOccured(Message.Types.WorkstationOrderSet, this);
                            var server = net as Server;
                            if (server != null)
                                server.Enqueue(msg.PacketType, msg.Payload);
                        });
                    break;

                default:
                    break;
            }
        }

        public override void OnUpdate()
        {
            //if (this.Town.Map.Net is Net.Client)
            //    return;
            //this.GenerateWork();
            if (this.LoadedOrders.Count > 0)
            {
                while (this.LoadedOrders.Count > 0)
                    this.AddOrder(this.LoadedOrders.Dequeue());

                this.Town.Map.EventOccured(Message.Types.OrdersUpdated);

            }
        }
 
        AIJob CreateJob(CraftOrder order)
        {
            AIJob job = new AIJob();
            //var i = new Blocks.BlockWorkbench.InteractionCraft();
            var block = this.Town.Map.GetBlock(order.Craft.WorkstationEntity) as BlockWorkstation;// as IBlockWorkstation;
            job.Labor = block.Labor;
            System.Diagnostics.Debug.Assert(block != null);
            var i = block.GetCraftingInteraction();
            AIInstruction instr = new AIInstruction(new TargetArgs(order.Craft.WorkstationEntity), i);
            job.AddStep(instr);
            order.Job = job;
            return job;
        }



        public void AddOrder(CraftOrder order)
        {
            List<CraftOrder> benchOrders;
            if (this.Orders.TryGetValue(order.Craft.WorkstationEntity, out benchOrders))
                benchOrders.Add(order);
            else
                this.Orders[order.Craft.WorkstationEntity] = new List<CraftOrder>() { order };
            this.Town.Map.EventOccured(Message.Types.OrdersUpdated, order.Craft.WorkstationEntity);

            var job = CreateJob(order);

            var workstationEntity = this.Town.Map.GetBlockEntity(order.Craft.WorkstationEntity) as BlockEntityWorkstation;
            //workstationEntity.AssignedOrders.Add(order);

            if (workstationEntity.ExecutingOrders)
            {
                workstationEntity.CurrentOrder = order.Craft;
                this.Town.AddJob(job);
            }
            ////if (order.ID > 0)
            ////{
            ////    return;
            ////}
            //if (order.ID == 0)
            //order.ID = this.OrderSequence++;
            ////this.PendingOrders.Add(order.ID, order);
            //this.PendingOrders[order.ID] = order;
        }
        private void RemoveOrder(CraftOrder order)
        {
            List<CraftOrder> benchOrders = this.Orders[order.Craft.WorkstationEntity];
            benchOrders.Remove(order);
            if (benchOrders.Count == 0)
                this.Orders.Remove(order.Craft.WorkstationEntity);
            var workstationEntity = this.Town.Map.GetBlockEntity(order.Craft.WorkstationEntity) as BlockEntityWorkstation;
            //workstationEntity.AssignedOrders.Remove(order);
            this.Town.RemoveJob(order.Job);
            this.Town.Map.EventOccured(Message.Types.OrdersUpdated);
        }

        //public void RemoveOrder(int orderIndex)
        //{
        //    CraftOrder order;
        //    if(this.PendingOrders.TryGetValue(orderIndex, out order))
        //    {
        //        this.Town.RemoveJob(order.Job);
        //        this.PendingOrders.Remove(orderIndex);
        //    }
        //}

        //public List<CraftOrder> GetOrdersFor(Vector3 workstation)
        //{
        //    return this.PendingOrders.Values.Where(o => o.Craft.WorkstationEntity == workstation).ToList();
        //}

        public override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            tag.Add(this.OrderSequence.Save("IDSequence"));
            var ordersTag = new SaveTag(SaveTag.Types.List, "Orders", SaveTag.Types.List);
            //foreach (var order in this.PendingOrders)
            //    ordersTag.Add(order.Value.Save());
            foreach (var global in this.Orders)
            {
                var globaltag = new SaveTag(SaveTag.Types.List, "", SaveTag.Types.Compound);
                foreach (var order in global.Value)
                {
                    var ordertag = order.Save();
                    globaltag.Add(ordertag);
                }
                ordersTag.Add(globaltag);
            }
            tag.Add(ordersTag);
            return tag;
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("IDSequence", v => this.OrderSequence = v);
            List<SaveTag> globalstag;
            if (tag.TryGetTagValue<List<SaveTag>>("Orders", out globalstag))
                foreach (var globaltag in globalstag)
                {
                    List<SaveTag> orderstag = globaltag.Value as List<SaveTag>;
                    foreach(var ordertag in orderstag)
                    {
                        var order = new CraftOrder(ordertag);
                        this.LoadedOrders.Enqueue(order);
                    }
                    //var order = new CraftOrder(globaltag);
                    //AddOrder(order);
                }
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(this.OrderSequence);
            //w.Write(this.PendingOrders.Count);
            //foreach (var o in this.PendingOrders)
            //    o.Value.Write(w);
            w.Write(this.Orders.Count);
            foreach(var global in this.Orders)
            {
                w.Write(global.Value.Count);
                foreach(var order in global.Value)
                {
                    order.Write(w);
                }
            }
        }
        public override void Read(BinaryReader r)
        {
            this.OrderSequence = r.ReadInt32();
            //var count = r.ReadInt32();
            //for (int i = 0; i < count; i++)
            //{
            //    var order = new CraftOrder(r);
            //    this.AddOrder(order);
            //}
            var workstationsCount = r.ReadInt32();
            for (int i = 0; i < workstationsCount; i++)
            {
                var orderscount = r.ReadInt32();
                for (int j = 0; j < orderscount; j++)
                {
                    var order = new CraftOrder(r);
                    this.LoadedOrders.Enqueue(order);// TODO: enqueue or add order immediately???
                }
            }
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                //case Message.Types.CraftingComplete:
                //    var order = e.Parameters[0] as CraftOrder;
                //    order.State = CraftOrder.States.Finished;
                //    this.RemoveOrder(order);
                //    this.Town.Map.EventOccured(Message.Types.OrdersUpdated);
                //    break;

                case Message.Types.CraftingComplete:
                    Vector3 global = (Vector3)e.Parameters[0];
                    var craft = e.Parameters[1] as CraftOperation;
                    CraftOrder order = this.Orders[global].First(o => o.Craft == craft);
                    order.State = CraftOrder.States.Finished;
                    this.RemoveOrder(order);
                    this.NextOrder(global);
                    this.Town.Map.EventOccured(Message.Types.OrdersUpdated);
                    break;

                //case Message.Types.CraftingComplete:
                //    var craft = e.Parameters[0] as CraftOperation;
                //    var order = this.PendingOrders.Values.FirstOrDefault(o => o.Craft == craft);
                //    order.State = CraftOrder.States.Finished;
                //    this.RemoveOrder(order.ID);
                //    this.Town.Map.EventOccured(Message.Types.OrdersUpdated);
                //    break;

                default:
                    break;
            }
        }

        private void NextOrder(Vector3 global)
        {
            var entity = this.Town.Map.GetBlockEntity(global) as BlockEntityWorkstation;
            if(entity.ExecutingOrders)
            {
                List<CraftOrder> orders;
                if (this.Orders.TryGetValue(global, out orders))
                {
                    var order = orders.First();
                    entity.CurrentOrder = order.Craft;
                    return;
                }
            }
            entity.CurrentOrder = null;
        }

        //internal void AddOrder(CraftOperation order)
        //{
        //    var o = new CraftOrder(this.OrderSequence++, order, 1);
        //    this.AddOrder(o);
        //}

        internal void DisableWorkstation(Vector3 global)
        {
            List<CraftOrder> orders;
            if (!this.Orders.TryGetValue(global, out orders))
                return;
            var entity = this.Town.Map.GetBlockEntity(global) as BlockEntityWorkstation;
            //entity.ExecutingOrders = false;
            foreach(var o in orders)
            {
                this.Town.RemoveJob(o.Job);
            }
        }
        internal void EnableWorkstation(Vector3 global)
        {
            List<CraftOrder> orders;
            if (!this.Orders.TryGetValue(global, out orders))
                return;
            var entity = this.Town.Map.GetBlockEntity(global) as BlockEntityWorkstation;
            //entity.ExecutingOrders = true;
            entity.CurrentOrder = orders.First().Craft;
            foreach (var o in orders)
            {
                this.Town.AddJob(o.Job);
            }
        }
        internal List<CraftOrder> GetOrders(Vector3 workstationGlobal)
        {
            return this.Orders.GetValueOrDefault(workstationGlobal);
        }
    }
}
