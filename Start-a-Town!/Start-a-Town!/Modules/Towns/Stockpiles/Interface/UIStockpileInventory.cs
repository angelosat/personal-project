using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Stockpiles
{
    class UIStockpileInventory : GroupBox
    {
        StockpileContentTracker Tracker;
        TableScrollableCompact<KeyValuePair<int, int>> Table;
        Panel TablePanel;
        public UIStockpileInventory(StockpileContentTracker tracker)
        {
            this.Tracker = tracker;
        }
        public UIStockpileInventory()
        {
            this.TablePanel = new Panel() { AutoSize = true };
            this.Table = new TableScrollableCompact<KeyValuePair<int, int>>(5) { ShowColumnLabels = false };// new TableScrollableCompact<GameObject>(50);
            Table.AddColumn(null, "Item", 150, (pair) => new Label(GameObject.Objects[pair.Key].Name), showColumnLabels:false);
            Table.AddColumn(null, "Count", 30, (pair) => new Label(pair.Value.ToString()), showColumnLabels: false);

            this.TablePanel.AddControls(this.Table);
            this.AddControls(this.TablePanel);

        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.StockpileContentsUpdated:
                    //Dictionary<int, int> inv = e.Parameters[0] as Dictionary<int, int>;
                    //this.Refresh(inv);
                    this.Refresh(e.Parameters[0] as Dictionary<int, int>, e.Parameters[1] as Dictionary<int, int>, e.Parameters[2] as Dictionary<int, int>);
                    break;

                default:
                    break;
            }
        }

        private void Refresh(Dictionary<int, int> added, Dictionary<int, int> removed, Dictionary<int, int> updated)
        {
            this.Table.AddItems(added);
            this.Table.RemoveItems(i => removed.ContainsKey(i.Key));
            foreach(var up in updated)
            {
                var element = this.Table.GetItem(p => p.Key == up.Key, "Count") as Label;
                element.Text = up.Value.ToString();
            }
        }

        //private void Refresh(Dictionary<int, int> inv)
        //{
        //    this.Controls.Remove(this.TablePanel);
        //    this.TablePanel.ClearControls();
        //    this.Table = new TableScrollable<KeyValuePair<int, int>>(5);// new TableScrollableCompact<GameObject>(50);
        //    Table.AddColumn("Item", "Item", 150, (pair) => new Label(GameObject.Objects[pair.Key].Name));
        //    Table.AddColumn("Count", "Count", 30, (pair) => new Label(pair.Value.ToString()));
        //    this.Table.Build(inv);
        //    this.TablePanel.AddControls(this.Table);
        //    this.AddControls(this.TablePanel);
        //}
    }
}
