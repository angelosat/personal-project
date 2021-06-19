using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class QuestRewardMoney : QuestReward
    {
        public int Amount;
        public QuestRewardMoney(QuestDef parent, int amount) : base(parent)
        {
            this.Amount = amount;
        }

        public override string Text => ItemDefAmount.GetText(ItemDefOf.Coins, this.Amount);
        //public override int Count => this.Amount;
        public override int Count { get => this.Amount; set { this.Amount = Math.Max(0, value); this.Parent.Manager.QuestModified(this.Parent); } }

        public override int Budget => this.Count;

        public override string Label => ItemDefOf.Coins.Label;

        internal override void Award(Actor actor)
        {
            this.AwardFromTownStockpiles(actor);
            //this.AwardFromQuestGiver(actor);
        }
        void AwardFromTownStockpiles(Actor actor)
        {
            var qgiver = this.Parent.Giver;
            var moneyItems = qgiver.Town.StockpileManager
                .FindItems(e => e.Def == ItemDefOf.Coins, this.Amount);
            if (!moneyItems.Any())
                throw new Exception();
            foreach(var i in moneyItems)
            {
                if (i.amount < i.item.StackSize)
                {
                    var split = i.item.Split(i.amount);
                    if (qgiver.NetNew is Net.Server server)
                    {
                        server.SyncInstantiate(split);
                        actor.Inventory.SyncInsert(split);
                    }
                }
                else
                    actor.Inventory.Insert(i.item);
            }
        }
        private void AwardFromQuestGiver(Actor actor)
        {
            var giver = this.Parent.Giver;
            var money = giver.GetMoney();
            if (this.Amount < money.StackSize)
            {
                if (actor.NetNew is Net.Server server)
                {
                    money = money.Split(this.Amount) as Entity;
                    server.SyncInstantiate(money);
                    actor.Inventory.SyncInsert(money);
                }
            }
            else
            {
                giver.Inventory.Remove(money);
                actor.Inventory.Insert(money);
            }
        }

        internal override bool CanAward()
        {
            var qgiver = this.Parent.Giver;
            //var money = qgiver.GetMoney();
            ////return money?.StackSize >= this.Amount;
            //if (money?.StackSize >= this.Amount)
            //    return true;
            return qgiver.Town.StockpileManager
                .FindItems(e => e.Def == ItemDefOf.Coins, this.Amount)
                .Sum(i => i.amount) 
                >= 
                this.Amount;
        }
    }
}
