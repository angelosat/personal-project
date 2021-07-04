using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using System.IO;

namespace Start_a_Town_
{
    public class Tavern : Workplace
    {
        static class Packets
        {
            static int PacketOrderAdd, PacketOrderRemove, PacketOrderSync, PacketOrderUpdateIngredients;//, PacketUpdateWorkerRoles;
            static public void Init()
            {
                PacketOrderAdd = Network.RegisterPacketHandler(HandleAddOrder);
                PacketOrderSync = Network.RegisterPacketHandler(HandleSyncOrder);
                PacketOrderRemove = Network.RegisterPacketHandler(HandleRemoveOrder);
                PacketOrderUpdateIngredients = Network.RegisterPacketHandler(UpdateOrderIngredients);
                //PacketUpdateWorkerRoles = Network.RegisterPacketHandler(UpdateWorkerRoles);
            }
            private static void HandleRemoveOrder(IObjectProvider net, BinaryReader r)
            {
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var orderid = r.ReadInt32();
                var order = tavern.GetOrder(orderid);
                if (net is Client)
                    tavern.RemoveOrder(order);
                else
                    SendRemoveOrder(net, pl, tavern, order);
            }
            public static void SendRemoveOrder(IObjectProvider net, PlayerData player, Tavern tavern, CraftOrderNew order)
            {
                if (net is Server)
                    tavern.RemoveOrder(order);
                net.GetOutgoingStream().Write(PacketOrderRemove, player.ID, tavern.ID, order.ID);
            }
            private static void HandleAddOrder(IObjectProvider net, BinaryReader r)
            {
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var reaction = Reaction.GetReaction(r.ReadInt32());// Def.GetDef<Reaction>(r.ReadString());
                var id = r.ReadInt32();
                if (net is Client)
                    tavern.AddOrder(new CraftOrderNew(reaction) { ID = id });
                else
                    SendAddMenuItem(net, pl, tavern, reaction, id);
            }

            static public void SendAddMenuItem(IObjectProvider net, PlayerData player, Tavern tavern, Reaction reaction, int id = -1)
            {
                if (net is Server)
                {
                    id = tavern.MenuItemIDSequence++;
                    tavern.AddOrder(new CraftOrderNew(reaction) { ID = id });
                    //var order = new CraftOrderNew(reaction) { ID = id };
                    //tavern.Orders.Add(order);
                    //net.EventOccured(Components.Message.Types.TavernMenuChanged, tavern, new CraftOrderNew[] { order }, new CraftOrderNew[] { });
                }
                net.GetOutgoingStream().Write(PacketOrderAdd, player.ID, tavern.ID, reaction.ID, id);
            }

            static public void SendOrderSync(IObjectProvider net, PlayerData player, Tavern tavern, CraftOrderNew order, bool enabled)
            {
                if (net is Server)
                {
                    order.Enabled = enabled;
                }
                net.GetOutgoingStream().Write(PacketOrderSync, player.ID, tavern.ID, order.ID, enabled);
            }
            private static void HandleSyncOrder(IObjectProvider net, BinaryReader r)
            {
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var order = tavern.GetOrder(r.ReadInt32());
                var enabled = r.ReadBoolean();
                if (net is Client)
                {
                    order.Enabled = enabled;
                }
                else
                    net.GetOutgoingStream().Write(PacketOrderSync, pl.ID, tavern.ID, order.ID, enabled);
            }

            public static void UpdateOrderIngredients(IObjectProvider net, PlayerData player, Tavern tavern, CraftOrderNew order, string reagent, ItemDef[] defs, Material[] mats, MaterialType[] matTypes)
            {
                if (net is Server)
                    order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
                var w = net.GetOutgoingStream();
                w.Write(PacketOrderUpdateIngredients);
                w.Write(player.ID);
                w.Write(tavern.ID);
                w.Write(order.ID);
                w.Write(reagent);
                w.Write(defs?.Select(d => d.Name).ToArray());
                w.Write(mats?.Select(d => d.ID).ToArray());
                w.Write(matTypes?.Select(d => d.ID).ToArray());
            }
            private static void UpdateOrderIngredients(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.GetShop<Tavern>(r.ReadInt32());
                var order = tavern.GetOrder(r.ReadInt32());
                var reagent = r.ReadString();
                var defs = r.ReadStringArray().Select(Def.GetDef<ItemDef>).ToArray();
                var mats = r.ReadIntArray().Select(Material.GetMaterial).ToArray();
                var matTypes = r.ReadIntArray().Select(MaterialType.GetMaterialType).ToArray();
                if (net is Client)
                {
                    order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
                }
                else
                {
                    UpdateOrderIngredients(net, player, tavern, order, reagent, defs, mats, matTypes);
                }
            }
        }
        bool NoonPassed;
        public override void Tick()
        {
            var time = this.Town.Map.World.Clock;
            if (!this.NoonPassed && time.Hours == 12)
            {
                //unrent rooms
                foreach (var bedroom in this.GetRooms().Where(r => r.RoomRole == RoomRoleDefOf.Bedroom))
                {
                    bedroom.Owner?.Ownership.Unclaim(bedroom);
                }
                this.NoonPassed = true;
            }
            else if (this.NoonPassed && time.Hours == 0)
            {
                this.NoonPassed = false;
            }
        }

        internal bool IsInnkeeperServicing()
        {
            var map = this.Town.Map;
            return this.WorkerProps.Values.Any(p =>
            {
                var actor = map.Net.GetNetworkObject<Actor>(p.ActorID);
                return
                    p.GetJob(JobInnKeeper).Enabled &&
                    actor.GetState().CurrentTaskBehavior is TaskBehaviorInnKeeper &&
                    //actor.Global.SnapToBlock() == map.GetBehindOfBlock(this.Counter.Value);
                    actor.Cell.Value == map.GetBehindOfBlock(this.Counter.Value);
            });
            //return map.GetObjectsNew(map.GetBehindOfBlock(this.Counter.Value)).OfType<Actor>().Any(a => 
            //    this.ActorHasJob(a, JobInnKeeper) &&
            //    a.GetState().CurrentTaskBehavior is TaskBehaviorInnKeeper
            //);
        }

        internal Room GetFreeBedroom(int maxValue)// = int.MaxValue)
        {
            return this.GetRooms().FirstOrDefault(r =>
                r.RoomRole == RoomRoleDefOf.Bedroom &&
                r.Owner is null &&
                r.Value <= maxValue && // TODO order by value
                !IsReserved(r));
        }

        private bool IsReserved(Room r)
        {
            return this.Customers.Any(c => c.Bedroom == r);
        }

        //static public readonly WorkerRoleDef Waiter = new("TavernPostWaiter", "Waiter");
        //static public readonly WorkerRoleDef Cook = new("TavernPostCook", "Cook");
        //static public readonly WorkerRoleDef InnKeeper = new("TavernPostInnKeeper", "Inn Keeper");

        static public readonly JobDef JobWaiter = new("JobTavernWaiter", new TaskGiverTavernWaiter());
        static public readonly JobDef JobCook = new("JobTavernCook", new TaskGiverTavernCook());
        static public readonly JobDef JobInnKeeper = new("JobTavernInnKeeper", new TaskGiverInnKeeper());

        //static public readonly WorkerRoleDef[] RolesAll = { Waiter, Cook, InnKeeper };
        static public readonly JobDef[] RolesAll = { JobWaiter, JobCook, JobInnKeeper };

        static public readonly HashSet<RoomRoleDef> ValidRooms = new() { RoomRoleDefOf.Bedroom };
        static Tavern()
        {
            Packets.Init();

            //Def.Register(Waiter);
            //Def.Register(Cook);
            //Def.Register(InnKeeper);

            Def.Register(JobWaiter);
            Def.Register(JobCook);
            Def.Register(JobInnKeeper);
        }
        public static void Init() { }
        readonly HashSet<IntVec3> Tables = new();
        readonly HashSet<IntVec3> Workstations = new();
        readonly public List<CustomerTavern> Customers = new();
        //public IntVec3? Counter;

        public bool CanOfferBed => this.Counter.HasValue; // TODO also check for bed availability

        //public Dictionary<WorkerRoleDef, WorkerRole> Roles { get; } = RolesAll.ToDictionary(d => d, d => new WorkerRole(d));
        //readonly Dictionary<WorkerRoleDef, WorkerRole> Roles = RolesAll.ToDictionary(d => d, d => new WorkerRole(d));
        public override bool IsValidRoom(Room room)
        {
            return ValidRooms.Contains(room.RoomRole);
        }
        //public override IEnumerable<WorkerRoleDef> GetRoleDefs()
        //{
        //    foreach (var r in RolesAll)
        //        yield return r;
        //}
        public override IEnumerable<JobDef> GetRoleDefs()
        {
            foreach (var r in RolesAll)
                yield return r;
        }
        //public override WorkerRole GetRole(WorkerRoleDef roleDef)
        //{
        //    return this.Roles[roleDef];
        //}
        readonly List<CraftOrderNew> Orders = new();
        int MenuItemIDSequence = 1;
        public Tavern()
        {

        }
        public Tavern(TownComponent manager, int id) : base(manager, id)
        {
            this.Name = string.Format("Tavern{0}", this.ID);
        }
        public Tavern(TownComponent manager) : base(manager)
        {
        }
        //public override IEnumerable<WorkerRole> GetRoles()
        //{
        //    foreach (var role in Roles.Values)
        //        yield return role;
        //}
        public override CraftOrderNew GetOrder(int orderid)
        {
            return this.Orders.First(o => o.ID == orderid);
        }
        public void AddOrder(CraftOrderNew order)
        {
            this.Orders.Add(order);
            this.Town.Net.EventOccured(Components.Message.Types.TavernMenuChanged, this, new CraftOrderNew[] { order }, new CraftOrderNew[] { });
        }
        public void RemoveOrder(CraftOrderNew order)
        {
            this.Orders.Remove(order);
            this.Town.Net.EventOccured(Components.Message.Types.TavernMenuChanged, this, new CraftOrderNew[] {  }, new CraftOrderNew[] { order });
            this.Town.Net.EventOccured(Components.Message.Types.OrderDeleted, order);
        }
        public void RemoveOrder(int orderid)
        {
            var order = this.GetOrder(orderid);
            this.Orders.Remove(order);
            this.Town.Net.EventOccured(Components.Message.Types.TavernMenuChanged, this, new CraftOrderNew[] { }, new CraftOrderNew[] { order });
            this.Town.Net.EventOccured(Components.Message.Types.OrderDeleted, order);
        }
        public void AddTable(IntVec3 global)
        {
            this.Tables.Add(global);
        }
        public void AddWorkstation(IntVec3 global)
        {
            this.Workstations.Add(global);
        }
        public CustomerTavern AddCustomer(Actor customer, Room freeBedroom)
        {
            var customerProps = new CustomerTavern(this, customer.RefID, freeBedroom);
            this.Customers.Add(customerProps);
            return customerProps;
        }
        public CustomerTavern AddCustomer(Actor customer, IntVec3 table, VisitorCraftRequest request)
        {
            var customerProps = new CustomerTavern(this, customer.RefID, table, request);
            this.Customers.Add(customerProps);
            return customerProps;
        }
        //public CustomerTavern AddCustomer(Actor customer, IntVec3 bed)
        //{
        //    var customerProps = new CustomerTavern(this, customer.InstanceID, bed);
        //    this.Customers.Add(customerProps);
        //    return customerProps;
        //}
        internal void RemoveCustomer(int customerID)
        {
            this.Customers.RemoveAll(p => p.CustomerID == customerID);
        }
        public CustomerTavern GetCustomerProperties(Actor actor)
        {
            return this.Customers.First(c => c.CustomerID == actor.RefID);
        }
        public CustomerTavern GetCustomerProperties(int actorID)
        {
            return this.Customers.First(c => c.CustomerID == actorID);
        }
        public IntVec3? GetAvailableTable()
        {
            //return this.Tables.FirstOrDefault(t => !this.Customers.Any(c => c.Table == t));
            foreach (var table in this.Tables)
                if (!this.Customers.Any(c => c.Table == table))
                    return table;
            return null;
        }
        public bool TryGetAvailableTable(out IntVec3 table)
        {
            //return this.Tables.FirstOrDefault(t => !this.Customers.Any(c => c.Table == t));
            foreach (var t in this.Tables)
                if (!this.Customers.Any(c => c.Table == t))
                {
                    table = t;
                    return true;
                }
            table = default;
            return false;
        }
        public IEnumerable<CraftOrderNew> GetAvailableOrders()
        {
            foreach(var o in this.Orders)
                yield return o;
            //foreach (var ws in this.Workstations)
            //{
            //    var orders = this.Town.CraftingManager.GetOrdersNew(ws);
            //    foreach (var o in orders)
            //        yield return o;
            //}
        }
        public IntVec3? GetAvailableKitchen()
        {
            return this.Workstations.FirstOrDefault(this.Town.ReservationManager.IsReserved);
            //return this.Workstations.First();
        }
        public override bool IsAllowed(Block block)
        {
            return (block.Type == Block.Types.Kitchen) || block is BlockStool || block is BlockShopCounter;
        }
        internal override void AddFacility(IntVec3 global)
        {
            var block = this.Town.Map.GetBlock(global);
            if (block is BlockStool)
                this.Tables.Add(global);
            //else if (block is BlockKitchen)
            else if (block.Type == Block.Types.Kitchen)
                this.Workstations.Add(global);
            else if (block is BlockShopCounter)
                this.Counter = global;
            else
                throw new Exception();
        }
        //public override AITask GetTask(Actor worker)
        //{
        //    //var iswaiter = this.Roles[Waiter].Contains(worker);
        //    //var iscook = this.Roles[Cook].Contains(worker);
        //    //var isinnkeeper = this.Roles[InnKeeper].Contains(worker);

        //    var iswaiter = this.GetWorkerProps(worker).GetJob(JobWaiter).Enabled;
        //    var iscook = this.GetWorkerProps(worker).GetJob(JobCook).Enabled;
        //    var isinnkeeper = this.GetWorkerProps(worker).GetJob(JobInnKeeper).Enabled;

        //    foreach (var customer in this.Customers.ToArray())
        //    {
        //        //if(isinnkeeper && customer.Bedroom is not null && !customer.Customer.Ownership.Owns(customer.Bedroom))
        //        //{
        //        //    return new AITask(typeof(TaskBehaviorInnKeeper), customer.Customer);// { CustomerProps = customer };
        //        //}
        //        //if (iswaiter && customer.IsSeated && !customer.IsOrderTaken)
        //        //{
        //        //    //customer.OrderTakenBy = worker;
        //        //        return new AITask(typeof(TaskBehaviorTavernWorkerTakeOrder), customer.Customer) { ShopID = this.ID };// { CustomerProps = customer };//, worker.Net.GetNetworkObject(customer.CustomerID));
        //        //}
        //        //if (iscook && customer.IsSeated && customer.IsOrderTaken && customer.Dish == null) // TODO && order ready/not ready
        //        //{
        //        //    var availableKitchen = this.GetAvailableKitchen();
        //        //    if (availableKitchen.HasValue)
        //        //    {
        //        //        var order = customer.CraftRequest;
        //        //        var map = worker.Map;
        //        //        var allObjects = map.GetEntities().OfType<Entity>();
        //        //        List<(string reagent, Entity item)> foundIngredients = new();
        //        //        foreach (var (reagentName, item, material) in order.GetPreferences())
        //        //        {
        //        //            var foundItem = allObjects.FirstOrDefault(o => o.Def == item && o.PrimaryMaterial == material && worker.CanReserve(o));
        //        //            if (foundItem == null)
        //        //                return null; // TODO start delivering materials even if not all of them are currently available?
        //        //            foundIngredients.Add((reagentName, foundItem));
        //        //        }
        //        //        var task = new AITask(typeof(TaskBehaviorTavernWorkerPrepareOrder));
        //        //        foreach (var (reagent, item) in foundIngredients)
        //        //            task.AddTarget(TargetIndex.A, item, 1);
        //        //        task.SetTarget(TargetIndex.B, availableKitchen.Value);// this.Workstations.First());
        //        //        task.IngredientsUsed = foundIngredients.ToDictionary(i => i.reagent, i => new ObjectRefIDsAmount(i.item, 1));
        //        //        task.Order = order.Order;
        //        //        task.CustomerProps = customer;
        //        //        task.ShopID = this.ID;
        //        //        return task;
        //        //    }
        //        //}
        //        //if (iswaiter && !customer.IsServed && customer.Dish != null)
        //        //{
        //        //    customer.ServedBy = worker;
        //        //    return new AITask(typeof(TaskBehaviorTavernWorkerServe), customer.Dish, (worker.Map, customer.Table.Above())) { CustomerProps = customer };
        //        //}
        //        //if (customer.IsServed)
        //        //    this.Customers.Remove(customer);
        //    }
        //    return base.GetTask(worker);
        //    return null;
        //}
        
        public CustomerProperties GetCustomer(Actor customer)
        {
            return this.Customers.FirstOrDefault(c => c.CustomerID == customer.RefID);
        }
        public override IEnumerable<IntVec3> GetFacilities()
        {
            foreach (var v in this.Workstations)
                yield return v;
            foreach (var v in this.Tables)
                yield return v;
        }
        static GroupBox OrdersGUI;
        static GroupBox CreateOrdersGUI()
        {
            var ordersBox = new GroupBox() { Name = "Orders" };
            Tavern tav = null;
            var ordersTable = new TableScrollableCompactNewNew<CraftOrderNew>(16)
                .AddColumn(new(), "enabled", UIManager.TextureTickBox.Width, o => new CheckBoxNew() { TickedFunc = () => o.Enabled, LeftClickAction = () => Packets.SendOrderSync(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, o, !o.Enabled) })
                .AddColumn(new(), "name", 130, o => new Label(o.Name, () => o.ShowDetailsUI((o, n, i, m, t) => Packets.UpdateOrderIngredients(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, o, n, i, m, t))), 0)
                .AddColumn(new(), "price", 70 - Icon.Cross.SourceRect.Width, o => new Label("-"))
                .AddColumn(new(), "remove", Icon.Cross.SourceRect.Width, o => IconButton.CreateSmall(Icon.Cross, () => Packets.SendRemoveOrder(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, o)));


            //ordersTable.AddItems(this.Orders);
            ordersBox.SetGetDataAction(t =>
            {
                tav = t as Tavern;
                ordersTable.ClearItems().AddItems(tav.Orders);
            });
            ordersTable.OnGameEventAction = e =>
            {
                switch (e.Type)
                {
                    case Components.Message.Types.TavernMenuChanged:
                        var t = e.Parameters[0] as Tavern;
                        if (tav != t)
                            break;
                        var ordersAdded = e.Parameters[1] as CraftOrderNew[];
                        ordersTable.AddItems(ordersAdded);
                        var ordersRemoved = e.Parameters[2] as CraftOrderNew[];
                        ordersTable.RemoveItems(ordersRemoved);
                        break;

                    default:
                        break;
                }
            };

            var btnAdd = new Button("Add menu item", () => Reaction.ShowRecipeListUI(r => r.ValidWorkshops.Contains(IsWorkstation.Types.Baking), r => Packets.SendAddMenuItem(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, r)));

            ordersBox.AddControlsVertically(ordersTable, btnAdd);
            return ordersBox;
        }
        protected override IEnumerable<GroupBox> GetUI()
        {
            yield return OrdersGUI ??= CreateOrdersGUI();
        }

        protected override void ResolveExtraReferences()
        {
            this.NoonPassed = this.Town.Map.Clock.Hours >= 12;
            this.Tables.RemoveWhere(g => this.Town.Map.GetBlock(g) is not BlockStool);
            this.Workstations.RemoveWhere(g => this.Town.Map.GetBlock(g).Type != Block.Types.Kitchen);
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach(var pos in positions)
            {
                var block = this.Map.GetBlock(pos);
                if (this.Tables.Contains(pos) && block is not BlockStool)
                    this.Tables.Remove(pos);
                else if (this.Workstations.Contains(pos) && block.Type != Block.Types.Kitchen)
                    this.Workstations.Remove(pos);
            }
        }
        protected override void SaveExtra(SaveTag tag)
        {
            this.Tables.Save(tag, "Tables");
            this.Workstations.Save(tag, "Kitchens");
            this.Customers.SaveNewBEST(tag, "Customers");
            this.Orders.SaveNewBEST(tag, "Orders");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Tables);
            w.Write(this.Workstations);
            this.Customers.Write(w);
            this.Orders.Write(w);
            //this.Tables.Write(w);
        }
        protected override void LoadExtra(SaveTag tag)
        {
            this.Tables.Load(tag, "Tables");
            this.Workstations.Load(tag, "Kitchens");
            this.Customers.Load(tag, "Customers", this);
            this.Orders.TryLoad(tag, "Orders");
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Tables.Read(r);
            this.Workstations.Read(r);
            this.Customers.InitializeNew(r, this);
            this.Orders.Read(r);
        }
    }
}
