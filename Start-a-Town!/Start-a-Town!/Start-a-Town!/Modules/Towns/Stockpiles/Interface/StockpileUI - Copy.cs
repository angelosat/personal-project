using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class StockpileUI : GroupBox
    {
        static Dictionary<Stockpile, Window> OpenWindows = new Dictionary<Stockpile, Window>();

        Stockpile Stockpile;
        StockpileFiltersUI FiltersUI;
        Panel PanelSlots, PanelFilters, PanelFiltersButtons;
        SlotGrid Slots;
        Window FiltersUINew;

        public StockpileUI(Stockpile stockpile)
        {
            this.Stockpile = stockpile;
            //this.AutoSize = false;
            this.PanelFilters = new Panel(Vector2.Zero) {AutoSize = true};//, new Vector2(100, 200));
            this.FiltersUI = new StockpileFiltersUI(this.Stockpile);
            this.PanelFilters.Controls.Add(this.FiltersUI);

            this.FiltersUINew = new StockpileFiltersUIBasic(this.Stockpile).ToWindow("Filters");

            this.PanelFiltersButtons = new Panel(this.PanelFilters.BottomLeft) { AutoSize = true };
            var btnAll = new Button("All", this.PanelFilters.ClientSize.Width) { LeftClickAction = SelectAll };
            var btnNone = new Button("None", this.PanelFilters.ClientSize.Width) { Location = btnAll.BottomLeft, LeftClickAction = SelectNone };
            var btnInvert = new Button("Invert", this.PanelFilters.ClientSize.Width) { Location = btnNone.BottomLeft, LeftClickAction = SelectInverse };

            var btnFilters = new Button("Filters", this.PanelFilters.ClientSize.Width)
            {
                Location = btnInvert.BottomLeft,
                LeftClickAction = () =>
                {
                    this.FiltersUINew.ToggleSmart();
                    this.GetWindow().Location.ToConsole();
                    this.FiltersUINew.Location.ToConsole();
                }
            };

            this.PanelFiltersButtons.Controls.Add(btnFilters); //btnAll, btnNone, btnInvert, 

            this.PanelSlots = new Panel(this.PanelFilters.TopRight, new Vector2(200, 200));
            //this.Size = new Rectangle(0, 0, 100, 100);
            //this.InitFilters();
           
            this.Refresh();
            this.Controls.Add(this.PanelFilters, this.PanelSlots, this.PanelFiltersButtons);

            var btnEdit = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Edit stockpile",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolDesignatePositions(this.Edit, () => stockpile.Positions.Select(g => g - Vector3.UnitZ).ToList()) { ValidityCheck = IsPositionValid }// this.Create)
            };
            btnEdit.Location = this.BottomRight;
            btnEdit.Anchor = Vector2.One;
            //var btnCenterCamera = new IconButton()
            //{
            //    Tag = (int)0,
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = new Icon(UIManager.Icons32, 12, 32),
            //    HoverFunc = () => "Center Camera"//,// "Add/Remove stockpiles",
            //    //LeftClickAction = () => btnCenterCamera(Tag };
            //};
            //btnCenterCamera.LeftClickAction = () => this.CenterCamera((int)btnCenterCamera.Tag);
            this.Controls.Add(btnEdit);
        }

        //private object CenterCamera(int p)
        //{
        //    throw new NotImplementedException();
        //}

        private bool IsPositionValid(Vector3 global)
        {
            return Block.IsBlockSolid(this.Stockpile.Town.Map, global) && !Block.IsBlockSolid(this.Stockpile.Town.Map, global + Vector3.UnitZ);
        }

        private void Edit(Vector3 begin, Vector3 end, bool value)
        {
            Client.Instance.Send(PacketType.StockpileEdit, PacketStockpileEdit.Write(this.Stockpile.ID, begin + Vector3.UnitZ, end + Vector3.UnitZ, value));
        }

        //private void Edit(Vector3 global, int w, int h, bool value)
        //{
        //    Client.Instance.Send(PacketType.StockpileEdit, PacketStockpileEdit.Write(this.Stockpile.ID, global, w, h, value));
        //}

        //void UpdateFilters()
        //{
        //    foreach (var check in from c in this.PanelFilters.Controls where c is CheckBoxNew select c as CheckBoxNew)
        //        check.Value = this.Stockpile.CurrentFilters.Contains((string)check.Tag);
        //}
        //void InitFilters()
        //{
        //    this.PanelFilters.Controls.Clear();
        //    //var filters = new List<string>() { ReagentComponent.Name};//, typeof(SeedComponent), typeof(GearComponent) };
        //    foreach (var type in Stockpile.Filters)
        //    {
        //        CheckBoxNew box = new CheckBoxNew(type.ToString(), this.PanelFilters.Controls.BottomLeft);
        //        box.Tag = type;
        //        box.Value = Stockpile.CurrentFilters.Contains(type);
        //        //box.ValueChangedFunction = (v) => this.SelectFilter(type, v);// this.Stockpile.FilterToggle(v, type);
        //        box.LeftClickAction = () => this.SelectFilter(type, !box.Value);// this.Stockpile.FilterToggle(v, type);
        //        this.PanelFilters.Controls.Add(box);
        //    }
        //}
        //void InitFilters()
        //{
        //    this.PanelFilters.Controls.Clear();
        //    //var filters = new List<string>() { ReagentComponent.Name};//, typeof(SeedComponent), typeof(GearComponent) };
        //    foreach (var type in Stockpile.Filters)
        //    {
        //        CheckBox box = new CheckBox(type.ToString(), this.PanelFilters.Controls.BottomLeft);
        //        box.Tag = type;
        //        box.Checked = Stockpile.CurrentFilters.Contains(type);
        //        //box.ValueChangedFunction = (v) => this.SelectFilter(type, v);// this.Stockpile.FilterToggle(v, type);
        //        box.LeftClickAction = () => this.SelectFilter(type, box.Checked);// this.Stockpile.FilterToggle(v, type);
        //        this.PanelFilters.Controls.Add(box);
        //    }
        //}

        public void Refresh()
        {
            var list = from obj in this.Stockpile.QueryPositions() select obj.ToSlot();
            this.PanelSlots.Controls.Clear();
            this.Slots = new SlotGrid(list.ToList(), 4);
            this.PanelSlots.Controls.Add(this.Slots);
        }
        public override bool Show(params object[] p)
        {
            this.Refresh();
            return base.Show(p);
        }

        private void SelectFilter(string filter, bool value)
        {
            PacketStockpileFilters p = new PacketStockpileFilters(this.Stockpile.ID);
            p.Add(filter, value);
            Net.Client.Instance.Send(PacketType.StockpileFilters, Network.Serialize(p.Write));
        }
        private void SelectAll()
        {
            PacketStockpileFilters p = new PacketStockpileFilters(this.Stockpile.ID);
            foreach(var f in Stockpile.Filters)
                p.Add(f, true);
            Net.Client.Instance.Send(PacketType.StockpileFilters, Network.Serialize(p.Write));
            //this.Stockpile.FilterEnable(Stockpile.Filters.ToArray());
            //UpdateFilters();
        }
        private void SelectNone()
        {
            PacketStockpileFilters p = new PacketStockpileFilters(this.Stockpile.ID);
            foreach (var f in Stockpile.Filters)
                p.Add(f, false);
            Net.Client.Instance.Send(PacketType.StockpileFilters, Network.Serialize(p.Write));
            //this.Stockpile.FilterDisable(Stockpile.Filters.ToArray());
            //UpdateFilters();
        }
        private void SelectInverse()
        {
            //PacketStockpileFilters p = new PacketStockpileFilters(this.Stockpile.ID);
            //foreach (var f in Stockpile.Filters)
            //    p.Add(f, !this.Stockpile.CurrentFilters.Contains(f));
            //Net.Client.Instance.Send(PacketType.StockpileFilters, Network.Serialize(p.Write));
        }

        internal override void OnGameEvent(GameEvent e)
        {
            var s = e.Parameters[0] as Stockpile;
            switch(e.Type)
            {
                case Message.Types.StockpileUpdated:
                    if (s != this.Stockpile)
                        break;
                    //this.UpdateFilters();
                    this.GetWindow().Title = s.Name;
                    break;

                case Message.Types.StockpileDeleted:
                    //var s = e.Parameters[0] as Stockpile;
                    if (s == this.Stockpile)
                    {
                        this.GetWindow().Hide();
                    }
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }

        private new Window ToWindow(string name = "")
        {
            Window existing;
            if (OpenWindows.TryGetValue(this.Stockpile, out existing))
                return existing;

            var win = base.ToWindow(name);
            win.AutoSize = true;
            win.Movable = true;
            win.Client.Controls.Add(this);


            var dialogRename = new DialogInput("Rename " + this.Stockpile.Name, Rename, 16, this.Stockpile.Name);
            win.Label_Title.LeftClickAction = () => dialogRename.ShowDialog();
            win.Label_Title.MouseThrough = false;
            win.Label_Title.Active = true;

            win.HideAction = () => OpenWindows.Remove(this.Stockpile);
            win.ShowAction = () => OpenWindows[this.Stockpile] = win;

            return win;
        }

        private void Rename(DialogInput dialog)
        {
            Net.Client.Instance.Send(Net.PacketType.StockpileRename, PacketStockpileRename.Write(this.Stockpile.ID, dialog.Input));
            dialog.Hide();
        }

        internal static Window GetWindow(Stockpile stockpile)
        {
            Window existing;
            if (OpenWindows.TryGetValue(stockpile, out existing))
                return existing;
            return new StockpileUI(stockpile).ToWindow(stockpile.Name);
        }
    }
}
