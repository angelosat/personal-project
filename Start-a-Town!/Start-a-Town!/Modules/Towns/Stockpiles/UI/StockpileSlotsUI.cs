using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;

namespace Start_a_Town_.Towns
{
    class StockpileSlotsUI : GroupBox
    {
        Town Town;
        SlotGrid<SlotDefault> Slots;
        public StockpileSlotsUI(Town town)
        {
            this.Town = town;
        }
        public new void Refresh()
        {
            this.Controls.Clear();
            var stockpiles = this.Town.StockpileManager.Stockpiles.Values.ToList();
            if (stockpiles.Count == 0)
            {
                //this.Controls.Add(new Label("No stockpiles created"));
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

            this.Controls.Add(this.Slots);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            base.OnGameEvent(e);
            switch (e.Type)
            {
                case Components.Message.Types.StockpileUpdated:
                    this.Refresh();
                    break;

                default:
                    break;
            }
        }
    }
}
