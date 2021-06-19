using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class Shop : Workplace//, ISerializable, ISaveable
    {
        public int OwnerID;

        //static public readonly WorkerRoleDef RoleRegistry = new("ShopRegistry", "Registry");
        //static public readonly WorkerRoleDef RoleRestock = new("ShopRestock", "Restock");

        static public readonly JobDef JobRegistry = new("JobShopRegistry", new TaskGiverTradingOverCounter());
        static public readonly JobDef JobeRestock = new("JobShopRestock");
        static public readonly JobDef[] RolesAll = { JobRegistry, JobeRestock };

        //readonly Dictionary<WorkerRoleDef, WorkerRole> Roles = RolesAll.ToDictionary(d => d, d => new WorkerRole(d));
        //public override IEnumerable<WorkerRoleDef> GetRoleDefs()
        //{
        //    yield return RoleRegistry;
        //    yield return RoleRestock;
        //}
        public override IEnumerable<JobDef> GetRoleDefs()
        {
            foreach (var r in RolesAll)
                yield return r;
        }
        //public override IEnumerable<WorkerRole> GetRoles()
        //{
        //    foreach (var role in Roles.Values)
        //        yield return role;
        //}
        //public override WorkerRole GetRole(WorkerRoleDef roleDef)
        //{
        //    return this.Roles[roleDef];
        //}
        Actor NextCustomer;
        readonly Queue<Actor> CustomerQueue = new();

        readonly Dictionary<Actor, Entity> PendingSales = new();
        readonly Dictionary<Actor, Transaction> PendingTransactions = new();

        public Shop(TownComponent manager, int id) : base(manager, id)
        {
            this.Name = string.Format("Shop{0}", this.ID);
        }
        public Shop(TownComponent manager) : base(manager)
        {

        }
      
        //public override AITask GetTask(Actor actor)
        //{
        //    //if(this.GetRole(RoleRegistry).Contains(actor))
        //    if(this.GetWorkerProps(actor).GetJob(JobRegistry).Enabled)
        //    {
        //        if (!this.IsValid())
        //            return null;
        //        if (!this.TryGetNextTransaction(out var transaction))
        //            return null;
        //        if (!this.CanExecuteTransaction(actor, transaction))
        //            return null;
        //        if (transaction.Type == Transaction.Types.Buy)
        //            return new AITask(typeof(TaskBehaviorAcceptSellOverCounter), (actor.Map, this.Counter.Value));
        //        else if (transaction.Type == Transaction.Types.Sell)
        //            return new AITask(typeof(TaskBehaviorAcceptBuyOverCounter)) { ShopID = this.ID, Transaction = transaction }; // shop holds value for counter so no need to pass it to the task as a target
        //        else
        //            throw new Exception();
        //    }
        //    return null;
        //}
        internal override void AddFacility(IntVec3 global)
        {
            var block = this.Town.Map.GetBlock(global);
            if (block is not BlockShopCounter)
                throw new Exception();
            this.Counter = global;
        }

        internal bool HandleCustomer(Actor actor, Entity item)
        {
            if (this.NextCustomer != null)
                return false;
            this.NextCustomer = actor;
            this.PendingSales.Add(actor, item);

            return true;
        }
        public bool RequestTransactionBuy(Actor customer, Entity item, int cost)
        {
            this.PendingTransactions.Add(customer, new Transaction(customer, Transaction.Types.Buy, item, cost));
            return true;
        }
        public bool RequestTransactionSell(Actor customer, Entity item, int cost, out Transaction transaction)
        {
            transaction = new Transaction(customer, Transaction.Types.Sell, item, cost);
            this.PendingTransactions.Add(customer, transaction);
            this.CustomerQueue.Enqueue(customer);
            return true;
        }
      
        public Transaction GetTransaction(Actor customer)
        {
            return this.PendingTransactions[customer];
        }
        internal bool TryGetNextTransaction(out Transaction transaction)
        {
            var customer = this.GetNextCustomer();
            if (customer != null)
            {
                transaction = this.PendingTransactions[customer];
                return true;
            }
            transaction = default;
            return false;
        }

        internal bool CanExecuteTransaction(Actor actor, Transaction transaction)
        {
            if (transaction.Type == Transaction.Types.Sell)
                return actor.HasMoney(transaction.Cost);
            else
                return true;
        }
        public Actor GetNextCustomer()
        {
            return this.CustomerQueue.Count > 0 ? this.CustomerQueue.Peek() : null;
            //return this.NextCustomer;
        }
        public bool HasCustomer()
        {
            return this.NextCustomer != null;
        }
        public void RemoveCustomer(Actor actor)
        {
            if (this.GetNextCustomer() != actor)
                return;
            this.CustomerQueue.Dequeue();
            this.PendingTransactions.Remove(actor);
        }
        public void RemoveCustomer(int actorID)
        {
            var actor = this.Town.Net.GetNetworkObject(actorID) as Actor;
            if (this.GetNextCustomer() != actor)
                return;
            this.CustomerQueue.Dequeue();
            this.PendingTransactions.Remove(actor);
            return;

        }
        public Entity GetSaleItem(Actor actor)
        {
            return this.PendingSales[actor];
        }
        public Entity GetNextSaleItem()
        {
            return this.PendingSales[this.NextCustomer];
        }
      

        public override bool IsAllowed(Block block)
        {
            return block is BlockShopCounter;
        }
        public void AddStockpile(Stockpile stockpile)
        {
            if(this.Stockpiles.Contains(stockpile.ID))
            {
                this.RemoveStockpile(stockpile);
                return;
            }
            this.Town.ShopManager.FindShop<Shop>(stockpile)?.RemoveStockpile(stockpile);
            this.Stockpiles.Add(stockpile.ID);
            this.Town.Net.EventOccured(Components.Message.Types.ShopUpdated, this, new[] { stockpile });
        }

        public void RemoveStockpile(Stockpile stockpile)
        {
            this.Stockpiles.Remove(stockpile.ID);
            this.Town.Net.EventOccured(Components.Message.Types.ShopUpdated, this, new[] { stockpile });
        }
        
        public IEnumerable<Entity> GetItems()
        {

            return this.GetItems(i => true);
        }
        public IEnumerable<Entity> GetItems(Func<Entity, bool> condition)
        {
            foreach (var stID in this.Stockpiles)
            {
                var st = this.Town.StockpileManager.GetStockpile(stID);
                var items = st.GetContents().Select(i => i as Entity);
                foreach (var i in items)
                    if (condition(i))
                        yield return i;
            }
        }
        public override IEnumerable<IntVec3> GetFacilities()
        {
            if (this.Counter.HasValue)
                yield return this.Counter.Value;
            else
                yield break;
        }
        

        public override bool IsValid()
        {
            return this.Counter.HasValue && this.Town.ShopManager.ShopExists(this);
        }

    }
}
