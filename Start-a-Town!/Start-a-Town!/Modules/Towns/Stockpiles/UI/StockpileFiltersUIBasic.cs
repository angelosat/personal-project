using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class StockpileFiltersUIBasic : GroupBox
    {
        Stockpile Stockpile;
        ListBox<string, CheckBoxNew> ListCategories;
        const int W = 200, H = 400;
        public StockpileFiltersUIBasic(Stockpile stockpile)
        {
            this.Stockpile = stockpile;

            var btnW = 120;
            var btnAll = new Button("All", btnW) { LeftClickAction = SelectAll };
            var btnNone = new Button("None", btnW) { Location = btnAll.BottomLeft, LeftClickAction = SelectNone };
            var btnInvert = new Button("Invert", btnW) { Location = btnNone.BottomLeft, LeftClickAction = SelectInverse };
            var btnAdvanced = new Button("Advanced", btnW) { Location = btnInvert.BottomLeft, LeftClickAction = GoAdvanced };
            var panelbuttons = new Panel() { AutoSize = true };
            panelbuttons.AddControls(btnAll, btnNone, btnInvert, btnAdvanced);

            var panelCategories = new Panel(Vector2.Zero, new Vector2(panelbuttons.Width, 400 - panelbuttons.Height));
            this.ListCategories = new ListBox<string, CheckBoxNew>(panelCategories.ClientSize.Width, panelCategories.ClientSize.Height);
            this.ListCategories.Build(Stockpile.Filters, f => f, (c, btn) =>
            {
                btn.Tag = c;
                btn.LeftClickAction = () => this.SelectFilter(c, !btn.Value);
            });
            panelCategories.AddControls(this.ListCategories);

            panelbuttons.Location = panelCategories.BottomLeft;

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
            Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        private void SelectNone()
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            foreach (var f in this.ListCategories.Items) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                string id = (string)f.Tag;
                values[id] = false;
            }
            Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        private void SelectAll()
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();

            foreach (var f in this.ListCategories.Items) //TODO: optimize this? i can let the stockpile select filters client-side
            {
                string id = (string)f.Tag;
                values[id] = true;
            }
            Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
        }

        private void SelectFilter(string filter, bool value)
        {
            Dictionary<string, bool> values = new Dictionary<string, bool>();
            values.Add(filter, value);
            Client.Instance.Send(PacketType.StockpileFiltersCategories, PacketStockpileFiltersToggleCategories.Write(this.Stockpile.ID, values));
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
