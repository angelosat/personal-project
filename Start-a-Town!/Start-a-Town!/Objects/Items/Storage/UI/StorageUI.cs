using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class StorageUI : GroupBox
    {
        Stockpile Stockpile;
        ListBoxNew<ItemCategory, Label> ListCategories;
        Panel PanelFilters;
        Dictionary<ItemCategory, ListBox<StorageFilter, CheckBoxNew>> Filters = new Dictionary<ItemCategory, ListBox<StorageFilter, CheckBoxNew>>();
        const int W = 200, H = 400;
        ComboBoxNewNew<StoragePriority> ComboPriority;
        public StorageUI()
        {
            this.ListCategories = new ListBoxNew<ItemCategory, Label>(120, 400);
            this.ListCategories.Build(ItemCategory.All, f => f.Name, (c, btn) => btn.LeftClickAction = () => this.OpenCategory(c));

            var panelCategories = new Panel() { AutoSize = true };
            panelCategories.AddControls(this.ListCategories);

            var btnW = Panel.GetClientLength(200);
            var btnAll = new Button("All", btnW) { LeftClickAction = SelectAll };
            var btnNone = new Button("None", btnW) { Location = btnAll.BottomLeft, LeftClickAction = SelectNone };
            var btnInvert = new Button("Invert", btnW) { Location = btnNone.BottomLeft, LeftClickAction = SelectInverse };
            var btnBasic = new Button("Basic", btnW) { Location = btnInvert.BottomLeft, LeftClickAction = GoBasic };

            var panelbuttons = new Panel() { AutoSize = true };
            panelbuttons.AddControls(btnAll, btnNone, btnInvert
                );

            this.PanelFilters = new Panel(panelCategories.TopRight, new Vector2(200, panelCategories.Height - panelbuttons.Height));
            panelbuttons.Location = this.PanelFilters.BottomLeft;

            foreach (var type in ItemCategory.All)
            {
                var listitems = new ListBox<StorageFilter, CheckBoxNew>(this.PanelFilters.ClientSize.Width, this.PanelFilters.ClientSize.Height);
                foreach (var filter in type.Filters)
                {
                    //var items = GameObject.Objects.Values.Where(filter.Condition).ToList();
                    listitems.Build(type.Filters, f => f.Label, (f, btn) =>
                    {
                        btn.Tag = f;// id;
                        btn.LeftClickAction = () => this.SelectFilter(f, !btn.Value);
                    });
                }
                this.Filters.Add(type, listitems);
            }
            this.AddControls(panelCategories, this.PanelFilters, panelbuttons);

            //this.ComboPriority = new ComboBoxNewNew<StoragePriority>(StoragePriority.All, 100, p => p.Name, (p, b) => b.LeftClickAction = () => SetPriority(p), null) { Location = panelbuttons.BottomLeft };
            var priolabel = new Label("Priority: ") { Location = panelbuttons.BottomLeft };
            this.ComboPriority = new ComboBoxNewNew<StoragePriority>(StoragePriority.All, 150, p => p.ToString(), SetPriority, null) { Location = priolabel.TopRight };
            this.AddControls(priolabel, this.ComboPriority);
        }

        private void SetPriority(StoragePriority p)
        {
            //this.Stockpile.Settings.Priority = p;
            PacketStockpileSync.Send(this.Stockpile, p);

        }

        //public StorageUI(Stockpile stockpile)
        //{
        //    this.Stockpile = stockpile;

        //    this.ListCategories = new ListBoxNew<StorageCategory, Label>(120, 400);
        //    this.ListCategories.Build(StorageCategory.All, f => f.Name, (c, btn) => btn.LeftClickAction = () => this.OpenCategory(c));

        //    var panelCategories = new Panel() { AutoSize = true };
        //    panelCategories.AddControls(this.ListCategories);

        //    var btnW = Panel.GetClientLength(200);
        //    var btnAll = new Button("All", btnW) { LeftClickAction = SelectAll };
        //    var btnNone = new Button("None", btnW) { Location = btnAll.BottomLeft, LeftClickAction = SelectNone };
        //    var btnInvert = new Button("Invert", btnW) { Location = btnNone.BottomLeft, LeftClickAction = SelectInverse };
        //    var btnBasic = new Button("Basic", btnW) { Location = btnInvert.BottomLeft, LeftClickAction = GoBasic };

        //    var panelbuttons = new Panel() { AutoSize = true };
        //    panelbuttons.AddControls(btnAll, btnNone, btnInvert
        //        );

        //    this.PanelFilters = new Panel(panelCategories.TopRight, new Vector2(200, panelCategories.Height - panelbuttons.Height));
        //    panelbuttons.Location = this.PanelFilters.BottomLeft;

        //    foreach (var type in StorageCategory.All)
        //    {
        //        var listitems = new ListBox<StorageFilter, CheckBoxNew>(this.PanelFilters.ClientSize.Width, this.PanelFilters.ClientSize.Height);
        //        foreach (var filter in type.Filters)
        //        {
        //            var items = GameObject.Objects.Values.Where(filter.Condition).ToList();
        //            listitems.Build(type.Filters, f => f.Name, (f, btn) =>
        //            {
        //                //var id = (int)f.IDType;
        //                btn.Tag = f;// id;
        //                btn.LeftClickAction = () => this.SelectFilter(f, !btn.Value);
        //            });
        //        }
        //        this.Filters.Add(type, listitems);
        //    }
        //    this.AddControls(panelCategories, this.PanelFilters, panelbuttons);
        //    this.Refresh();
        //}
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
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var chkBox in filters) //TODO: optimize this? i can let the stockpile invert its filters client-side
            {
                var id = (StorageFilter)chkBox.Tag;
                values[id.Name] = !chkBox.Value;
            }
            PacketStockpileFiltersNew.Send(this.Stockpile, values);
        }
        
        private void SelectNone()
        {
            if (this.CurrentCategory == null)
                return;
            var filters = this.CurrentCategory.Items;
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var chk in filters) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                var f = (StorageFilter)chk.Tag;
                values[f.Name] = false;
            }
            PacketStockpileFiltersNew.Send(this.Stockpile, values);
        }
        
        private void SelectAll()
        {
            if (this.CurrentCategory == null)
                return;
            var filters = this.CurrentCategory.Items;
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var chk in filters) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                var f = (StorageFilter)chk.Tag;
                values[f.Name] = true;
            }
            PacketStockpileFiltersNew.Send(this.Stockpile, values);
        }
        private void SelectFilter(int filter, bool value)
        {
            Dictionary<int, bool> values = new Dictionary<int, bool>();
            values.Add(filter, value);
            Client.Instance.Send(PacketType.StockpileFilters, PacketStockpileFiltersNew.Write(this.Stockpile.ID, values));
        }
        private void SelectFilter(StorageFilter filter, bool value)
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            values.Add(filter.Name, value);
            PacketStockpileFiltersNew.Send(this.Stockpile, values);
        }

        public void Refresh(Stockpile stockpile)
        {
            this.Stockpile = stockpile;
            foreach (var f in this.Filters)
                foreach (var check in f.Value.Items)
                    check.Value = this.Stockpile.Settings.ActiveFilters.Contains((StorageFilter)check.Tag);
            //throw new NotImplementedException();
            //this.ComboPriority.Select(stockpile.Settings.Priority);
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileUpdated:
                    if (e.Parameters[0] as Stockpile != this.Stockpile)
                        break;
                    this.Refresh(this.Stockpile);
                    break;
                default:
                    break;
            }
        }

        ListBox<StorageFilter, CheckBoxNew> CurrentCategory;

        private void OpenCategory(ItemCategory category)
        {
            this.PanelFilters.Controls.Clear();
            var list = this.Filters[category];
            this.PanelFilters.Controls.Add(list);
            this.CurrentCategory = list;//.Tag as List<GameObject>;
        }
    }
}
