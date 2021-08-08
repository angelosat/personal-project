﻿using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    public class Shop : Workplace
    {
        public int OwnerID;

        static public readonly JobDef JobRegistry = new("JobShopRegistry", new TaskGiverTradingOverCounter());
        static public readonly JobDef JobeRestock = new("JobShopRestock");
        static public readonly JobDef[] RolesAll = { JobRegistry, JobeRestock };

        public override IEnumerable<JobDef> GetRoleDefs()
        {
            foreach (var r in RolesAll)
                yield return r;
        }
       
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
                var st = this.Town.ZoneManager.GetZone<Stockpile>(stID);
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
