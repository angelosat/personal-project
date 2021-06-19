using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;

namespace Start_a_Town_.Towns.Stockpiles
{
    class StockpilesListInterface : GroupBox
    {
        ListBox<Stockpile, Label> ListStockpiles;
        Town Town;
        public StockpilesListInterface(Town town, int w, int h)
        {
            this.Town = town;
            this.ListStockpiles = new ListBox<Stockpile, Label>(w, h);
            this.Controls.Add(this.ListStockpiles);
            this.Refresh();
        }

        public StockpilesListInterface Refresh()
        {
            this.Controls.Clear();
            this.ListStockpiles.Build(this.Town.Stockpiles.Values.ToList(), s => s.Name, (s, b) => b.LeftClickAction = () => OpenStockpile(s));
            this.Controls.Add(this.ListStockpiles);
            this.Invalidate();
            return this;
        }
        //DialogInput DialogRename;
        private void OpenStockpile(Stockpile s)
        {
            StockpileUI.GetWindow(s).Show();
        }

        //private void Rename(Stockpile stockpile, string newname)
        //{
        //    Net.Client.Instance.Send(Net.PacketType.StockpileRename, PacketStockpileRename.Write(stockpile.ID, newname));
        //    this.DialogRename.Hide();
        //    //Net.Client.Instance.Send(Net.PacketType.StockpileRename, Net.Network.Serialize(w =>
        //    //{
        //    //    w.Write(stockpile.ID);
        //    //    w.Write(newname);
        //    //}));
        //}

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.StockpileUpdated:
                    //foreach (var s in this.OpenWindows)
                    //    if (s.Key == e.Parameters[0] as Stockpile)
                    //        s.Value.Title = s.Key.Name;
                    this.Refresh();
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }
    }
}
