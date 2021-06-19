using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;

namespace Start_a_Town_.Towns.Stockpiles.Interface
{
    public class StockpileUIOld : GroupBox
    {
        StockpilesListInterface StockpilesInterface;
        SlotGrid<SlotDefault> Slots;
        //IEnumerable<Stockpile> StockpileCollection;
        Town Town;
        public StockpileUIOld(Town town)
        {
            this.Town = town;
            this.StockpilesInterface = new StockpilesListInterface(this.Town,150, 75);
            this.Controls.Add(this.StockpilesInterface);
        }

        public void Refresh()
        {
            this.GetWindow().Invalidate(true);

            this.Controls.Clear();
            var stockpiles = this.Town.Stockpiles.Values.ToList();
            if (stockpiles.Count == 0)
            {
                this.Controls.Add(new Label("No stockpiles created"));
                return;
            }

            // TODO: update slots here
            List<GameObject> allContents = new List<GameObject>();
            foreach (var item in stockpiles)
                allContents.AddRange(item.GetContents());

            // merge objects
            Dictionary<int, int> inventory = new Dictionary<int, int>();
            foreach (var item in allContents)
                inventory.AddOrUpdate(item.GetInfo().ID, item.StackSize, (key, val) => { return val += item.StackSize; });

            //var objSlots = from item in inventory select new GameObjectSlot(GameObject.Objects[item.Key], item.Value); // I WAS CHANGING THE STACKSIZE OF THE ORIGINAL OBJECT
            var objSlots = from item in inventory select new GameObjectSlot(GameObject.Objects[item.Key].Clone(), item.Value);

            this.Slots = new SlotGrid<SlotDefault>(objSlots, 4);

            //this.StockpilesInterface.Refresh();
            this.StockpilesInterface.Location = this.Slots.BottomLeft;

            this.Controls.Add(this.Slots, this.StockpilesInterface);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileCreated:
                case Components.Message.Types.StockpileDeleted:
                    this.Refresh();
                    this.StockpilesInterface.Refresh();
                    this.StockpilesInterface.Location = this.Slots.BottomLeft;
                    break;

                default:
                    break;
            }
        }

        //public override bool Show(params object[] p)
        //{
        //    this.OnStockpileUpdate();
        //    return base.Show(p);
        //}
    }

}
