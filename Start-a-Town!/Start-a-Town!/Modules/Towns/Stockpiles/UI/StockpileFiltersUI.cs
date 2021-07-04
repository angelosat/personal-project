using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class StockpileFiltersUI : GroupBox
    {
        Stockpile Stockpile;
        List<CheckBoxNew> CheckBoxes = new List<CheckBoxNew>();
        ListBox<GameObject, CheckBoxNew> ListCheckBoxes;
        Dictionary<string, List<GameObject>> Filters = new Dictionary<string, List<GameObject>>();
        const int W = 200, H = 400;
        public StockpileFiltersUI(Stockpile stockpile)
        {
            this.Stockpile = stockpile;
            this.ListCheckBoxes = new ListBox<GameObject, CheckBoxNew>(W, H);
            foreach (var type in Stockpile.Filters)
            {
                var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(type)).ToList();
                this.Filters.Add(type, items);
            }

            this.ListCheckBoxes.BuildCollapsible(this.Filters, f => f.Name, (c, btn) => { }, (c, i) =>
            {
                this.CheckBoxes.Add(i);
            });
            this.AddControls(this.ListCheckBoxes);
            this.Refresh();
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileUpdated:
                    if (e.Parameters[0] as Stockpile != this.Stockpile)
                        break;
                    this.Refresh();
                    break;
                default:
                    break;
            }
        }
    }
}
