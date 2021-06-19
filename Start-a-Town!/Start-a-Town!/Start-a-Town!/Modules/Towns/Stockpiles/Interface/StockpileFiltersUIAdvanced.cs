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
    class StockpileFiltersUIAdvanced : GroupBox// ScrollableBox
    {
        Stockpile Stockpile;
        //List<CheckBoxNew> CheckBoxes = new List<CheckBoxNew>();
        //ListBox<GameObject, CheckBoxNew> ListCheckBoxes;
        ListBox<string, Label> ListCategories;
        Panel PanelFilters;
        Dictionary<string, ListBox<GameObject, CheckBoxNew>> Filters = new Dictionary<string, ListBox<GameObject, CheckBoxNew>>();
        const int W = 200, H = 400;
        public StockpileFiltersUIAdvanced(Stockpile stockpile)
        {
            this.Stockpile = stockpile;
            //var panel = new Panel(Vector2.Zero, new Vector2(500,500));//{AutoSize = true};//, new Vector2(100, 200));

            //var filters = new List<string>() { ReagentComponent.Name};//, typeof(SeedComponent), typeof(GearComponent) };
            //this.ListCheckBoxes = new ListBox<GameObject, CheckBoxNew>(W, H);
            //foreach (var type in Stockpile.Filters)
            //{
            //    var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(type)).ToList();
            //    var listitems = new ListBox<GameObject, CheckBoxNew>(200, 200);
            //    listitems.Build(items, f => f.Name, (f, btn) => btn.LeftClickAction = () => this.ToggleFilter(f));
            //    this.Filters.Add(type, listitems);
            //}

            this.ListCategories = new ListBox<string, Label>(120,400);
            this.ListCategories.Build(Stockpile.Filters, f => f, (c, btn) => btn.LeftClickAction = () => this.OpenCategory(c));
            var panelCategories = new Panel() { AutoSize = true };
            panelCategories.AddControls(this.ListCategories);

            var btnW = Panel.GetClientLength(200);
            var btnAll = new Button("All", btnW) { LeftClickAction = SelectAll };
            var btnNone = new Button("None", btnW) { Location = btnAll.BottomLeft, LeftClickAction = SelectNone };
            var btnInvert = new Button("Invert", btnW) { Location = btnNone.BottomLeft, LeftClickAction = SelectInverse };
            var btnBasic = new Button("Basic", btnW) { Location = btnInvert.BottomLeft, LeftClickAction = GoBasic };

            var panelbuttons = new Panel() { AutoSize = true };
            panelbuttons.AddControls(btnAll, btnNone, btnInvert, btnBasic);

            this.PanelFilters = new Panel(panelCategories.TopRight, new Vector2(200, panelCategories.Height - panelbuttons.Height));
            panelbuttons.Location = this.PanelFilters.BottomLeft;

            //foreach (var item in this.Filters.Values)
            //    item.Size = this.PanelFilters.ClientSize;
            foreach (var type in Stockpile.Filters)
            {
                var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(type)).ToList();
                var listitems = new ListBox<GameObject, CheckBoxNew>(this.PanelFilters.ClientSize.Width, this.PanelFilters.ClientSize.Height);
                listitems.Build(items, f => f.Name, (f, btn) =>
                {
                    //this.CheckBoxes.Add(btn);
                    var id= (int)f.ID;
                    btn.Tag = id;
                    btn.LeftClickAction = () => this.SelectFilter(id, !btn.Value);
                });
                //listitems.Tag = items;
                this.Filters.Add(type, listitems);
            }
            //this.ListCheckBoxes.Build(this.Filters, f => f.Name, (c, btn) => { }, (c, i) =>
            //{
            //    this.CheckBoxes.Add(i);
            //});
            //this.AddControls(this.ListCheckBoxes);
            this.AddControls(panelCategories, this.PanelFilters, panelbuttons);
            this.Refresh();
        }
        private void GoBasic()
        {
            var win = new StockpileFiltersUIBasic(this.Stockpile).ToWindow("Advanced Filters");
            var thiswin = this.GetWindow();
            win.Location = thiswin.Location;
            win.Show();
            thiswin.Hide();
        }

        private void SelectInverse()
        {
            if (this.CurrentCategory == null)
                return;
            var filters = this.CurrentCategory.Items;
            Dictionary<int, bool> values = new Dictionary<int, bool>();
            foreach(var f in filters) //TODO: optimize this? i can let the stockpile invert its filters client-side
            {
                int id = (int)f.Tag;
                values[id] = !f.Value;
            }
            Net.Client.Instance.Send(PacketType.StockpileFilters, PacketStockpileFiltersNew.Write(this.Stockpile.ID, values));
        }

        private void SelectNone()
        {
            if (this.CurrentCategory == null)
                return;
            var filters = this.CurrentCategory.Items;
            Dictionary<int, bool> values = new Dictionary<int, bool>();
            foreach (var f in filters) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                int id = (int)f.Tag;
                values[id] = false;
            }
            Net.Client.Instance.Send(PacketType.StockpileFilters, PacketStockpileFiltersNew.Write(this.Stockpile.ID, values));
        }

        private void SelectAll()
        {
            if (this.CurrentCategory == null)
                return;
            var filters = this.CurrentCategory.Items;
            Dictionary<int, bool> values = new Dictionary<int, bool>();
            foreach (var f in filters) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                int id = (int)f.Tag;
                values[id] = true;
            }
            Net.Client.Instance.Send(PacketType.StockpileFilters, PacketStockpileFiltersNew.Write(this.Stockpile.ID, values));
        }

        private void SelectFilter(int filter, bool value)
        {
            Dictionary<int, bool> values = new Dictionary<int, bool>();
            values.Add(filter, value);
            Net.Client.Instance.Send(PacketType.StockpileFilters, PacketStockpileFiltersNew.Write(this.Stockpile.ID, values));
        }

        //void Refresh()
        //{
        //    foreach (var check in this.CheckBoxes)//from c in this.PanelFilters.Controls where c is CheckBoxNew select c as CheckBoxNew)
        //        check.Value = this.Stockpile.CurrentFilters.Contains((string)check.Tag);
        //}
        void Refresh()
        {
            //foreach (var check in this.CheckBoxes)//from c in this.PanelFilters.Controls where c is CheckBoxNew select c as CheckBoxNew)
            foreach (var f in this.Filters)
                foreach (var check in f.Value.Items)
                    check.Value = this.Stockpile.CurrentFilters.Contains((int)check.Tag);
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

        ListBox<GameObject, CheckBoxNew> CurrentCategory;
        private void OpenCategory(string category)
        {
            this.PanelFilters.Controls.Clear();
            var list = this.Filters[category];
            this.PanelFilters.Controls.Add(list);
            this.CurrentCategory = list;//.Tag as List<GameObject>;
        }

    }
}
