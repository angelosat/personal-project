using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class StockpileFiltersUI : GroupBox// ScrollableBox
    {
        Stockpile Stockpile;
        List<CheckBoxNew> CheckBoxes = new List<CheckBoxNew>();
        ListBox<GameObject, CheckBoxNew> ListCheckBoxes;
        Dictionary<string, List<GameObject>> Filters = new Dictionary<string, List<GameObject>>();
        const int W = 200, H = 400;
        public StockpileFiltersUI(Stockpile stockpile)
           // : base(new Rectangle(0, 0, W, H))
        {
            this.Stockpile = stockpile;
            //var panel = new Panel(Vector2.Zero, new Vector2(500,500));//{AutoSize = true};//, new Vector2(100, 200));

            //var filters = new List<string>() { ReagentComponent.Name};//, typeof(SeedComponent), typeof(GearComponent) };
            this.ListCheckBoxes = new ListBox<GameObject, CheckBoxNew>(W, H);
            foreach (var type in Stockpile.Filters)
            {
                var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(type)).ToList();
                this.Filters.Add(type, items);

                //CheckBoxNew box = new CheckBoxNew(type.ToString(), panel.Controls.BottomLeft);
                //box.Tag = type;
                //box.Value = Stockpile.CurrentFilters.Contains(type);
                ////box.ValueChangedFunction = (v) => this.SelectFilter(type, v);// this.Stockpile.FilterToggle(v, type);
                //box.LeftClickAction = () => this.SelectFilter(type, !box.Value);// this.Stockpile.FilterToggle(v, type);
                //this.CheckBoxes.Add(box);
                //panel.Controls.Add(box);
            }

            this.ListCheckBoxes.Build(this.Filters, f => f.Name, (c, btn) => { }, (c, i) =>
            {
                this.CheckBoxes.Add(i);
            });
            //panel.AddControls(this.ListCheckBoxes);
            //this.AddControls(panel);
            this.AddControls(this.ListCheckBoxes);
            this.Refresh();
        }

        private void SelectFilter(string filter, bool value)
        {
            PacketStockpileFilters p = new PacketStockpileFilters(this.Stockpile.ID);
            p.Add(filter, value);
            Net.Client.Instance.Send(PacketType.StockpileFilters, Network.Serialize(p.Write));
        }

        void Refresh()
        {
            //foreach (var check in this.CheckBoxes)//from c in this.PanelFilters.Controls where c is CheckBoxNew select c as CheckBoxNew)
            //    check.Value = this.Stockpile.CurrentFilters.Contains((string)check.Tag);
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
