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
    class StockpileFiltersUIBasic : GroupBox// ScrollableBox
    {
        Stockpile Stockpile;
        ListBox<string, CheckBoxNew> ListCategories;
        Panel PanelFilters;
        Dictionary<string, ListBox<GameObject, CheckBoxNew>> Filters = new Dictionary<string, ListBox<GameObject, CheckBoxNew>>();
        const int W = 200, H = 400;
        public StockpileFiltersUIBasic(Stockpile stockpile)
        {
            this.Stockpile = stockpile;

            var btnW = 120;// Panel.GetClientLength(panelCategories.Width);
            var btnAll = new Button("All", btnW) { LeftClickAction = SelectAll };
            var btnNone = new Button("None", btnW) { Location = btnAll.BottomLeft, LeftClickAction = SelectNone };
            var btnInvert = new Button("Invert", btnW) { Location = btnNone.BottomLeft, LeftClickAction = SelectInverse };
            var btnAdvanced = new Button("Advanced", btnW) { Location = btnInvert.BottomLeft, LeftClickAction = GoAdvanced };
            var panelbuttons = new Panel() { AutoSize = true };
            panelbuttons.AddControls(btnAll, btnNone, btnInvert, btnAdvanced);

            var panelCategories = new Panel(Vector2.Zero, new Vector2(panelbuttons.Width, 400 - panelbuttons.Height));// { AutoSize = true };
            this.ListCategories = new ListBox<string, CheckBoxNew>(panelCategories.ClientSize.Width, panelCategories.ClientSize.Height);// (120, 400 - panelbuttons.Height);
            this.ListCategories.Build(Stockpile.Filters, f => f, (c, btn) =>
            {
                btn.Tag = c;// GetObjectsFromCategory(c);
                btn.LeftClickAction = () => this.SelectFilter(c, !btn.Value);// this.SelectCategory(c);
            });
            panelCategories.AddControls(this.ListCategories);

            panelbuttons.Location = panelCategories.BottomLeft;

            //this.PanelFilters = new Panel(panelCategories.TopRight, new Vector2(200, panelCategories.Height - panelbuttons.Height));
            //panelbuttons.Location = this.PanelFilters.BottomLeft;

            //foreach (var type in Stockpile.Filters)
            //{
            //    var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(type)).ToList();
            //    var listitems = new ListBox<GameObject, CheckBoxNew>(this.PanelFilters.ClientSize.Width, this.PanelFilters.ClientSize.Height);
            //    listitems.Build(items, f => f.Name, (f, btn) =>
            //    {
            //        var id= (int)f.ID;
            //        btn.Tag = id;
            //        btn.LeftClickAction = () => this.SelectFilter(id, !btn.Value);
            //    });
            //    this.Filters.Add(type, listitems);
            //}
            this.AddControls(panelCategories, panelbuttons);
            this.Refresh();
        }

        private void GoAdvanced()
        {
            var win = new StockpileFiltersUIAdvanced(this.Stockpile).ToWindow("Advanced Filters");
            var thiswin = this.GetWindow();
            win.Location = thiswin.Location;
            win.Show();
            thiswin.Hide();
        }

        private void SelectInverse()
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach(var f in this.ListCategories.Items) //TODO: optimize this? i can let the stockpile invert its filters client-side
            {
                string id = (string)f.Tag;
                values[id] = !f.Value;
            }
            Net.Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        private void SelectNone()
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var f in this.ListCategories.Items) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                string id = (string)f.Tag;
                values[id] = false;
            }
            Net.Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        private void SelectAll()
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();

            foreach (var f in this.ListCategories.Items) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                string id = (string)f.Tag;
                values[id] = true;
            }
            Net.Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        private void SelectFilter(string filter, bool value)
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            values.Add(filter, value);
            Net.Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        //void Refresh()
        //{
        //    foreach (var check in this.CheckBoxes)//from c in this.PanelFilters.Controls where c is CheckBoxNew select c as CheckBoxNew)
        //        check.Value = this.Stockpile.CurrentFilters.Contains((string)check.Tag);
        //}
        void Refresh()
        {
            //foreach (var f in Stockpile.Filters)
            //{
            //    var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(f)).Select(o => (int)o.ID).ToArray();
            //}
            foreach (var item in this.ListCategories.Items)
            {
                var f = (string)item.Tag;
                var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(f)).Select(o => (int)o.ID).ToArray();
                if (items.All(i => this.Stockpile.CurrentFilters.Contains(i)))
                    item.Value = true;
                else
                    item.Value = false;
            }
            return;
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

        //ListBox<GameObject, CheckBoxNew> CurrentCategory;
        //private void SelectCategory(string category)
        //{
        //    this.PanelFilters.Controls.Clear();
        //    var list = this.Filters[category];
        //    this.PanelFilters.Controls.Add(list);
        //    this.CurrentCategory = list;//.Tag as List<GameObject>;
        //}

        //static List<GameObject> GetObjectsFromCategory(string cat)
        //{
        //    var items = GameObject.Objects.Values.Where(o => o.Components.ContainsKey(cat)).ToList();
        //    return items;
        //}
    }
}
