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
    public class StockpilesManagerUI : GroupBox
    {
        StockpilesListInterface StockpilesList;
        StockpileSlotsUI SlotsUI;
        //SlotGrid<SlotDefault> Slots;
        IconButton BtnDesignate;
        //Dictionary<Stockpile, Window> OpenWindows = new Dictionary<Stockpile, Window>();

        //IEnumerable<Stockpile> StockpileCollection;
        Town Town;
        public StockpilesManagerUI(Town town)
        {
            //this.Town = town;
            //this.StockpilesInterface = new StockpilesInterface(this.Town);
            //this.Controls.Add(this.StockpilesInterface);


            //this.Title = "Town";
            //this.AutoSize = true;
            //this.Movable = true;
            this.Town = town;

            Panel panelButtons = new Panel() { Location = this.Controls.BottomLeft, AutoSize = true };
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate stockpiles\n\nLeft click & drag: Add stockpile\nCtrl+Left click: Remove stockpile",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(Engine.Map.GetTown())// this.Create)
            };
            panelButtons.Controls.Add(this.BtnDesignate);
            this.Controls.Add(panelButtons);

            PanelLabeled stockpiles = new PanelLabeled("Stockpiles") { Location = this.Controls.BottomLeft, AutoSize = true };
            //this.StockpileUI = new StockpileUI(town) { Location = stockpiles.Controls.BottomLeft };
            //stockpiles.Controls.Add(StockpileUI);
            this.Controls.Add(stockpiles);

            this.StockpilesList = new StockpilesListInterface(this.Town, 100, 150) { Location = stockpiles.Controls.BottomLeft};//this.BtnDesignate.BottomLeft };

            stockpiles.Controls.Add(this.StockpilesList);

            this.SlotsUI = new StockpileSlotsUI(this.Town) { Location = stockpiles.TopRight };

            //this.Controls.Add(this.StockpilesInterface, this.SlotsUI);
            this.Controls.Add(stockpiles, this.SlotsUI);
        }

        public void Refresh()
        {
            this.SlotsUI.Refresh();
            this.StockpilesList.Refresh();
            this.Invalidate(true);
            
            ////this.Controls.Clear();
            //var stockpiles = this.Town.Stockpiles.Values.ToList();
            //if (stockpiles.Count == 0)
            //{
            //    this.Controls.Add(new Label("No stockpiles created"));
            //    return;
            //}

            //// TODO: update slots here
            //List<GameObject> allContents = new List<GameObject>();
            //foreach (var item in stockpiles)
            //    allContents.AddRange(item.GetContents());

            //// merge objects
            //Dictionary<int, int> inventory = new Dictionary<int, int>();
            //foreach (var item in allContents)
            //    inventory.AddOrUpdate(item.GetInfo().ID, item.StackSize, (key, val) => { return val += item.StackSize; });

            ////var objSlots = from item in inventory select new GameObjectSlot(GameObject.Objects[item.Key], item.Value); // I WAS CHANGING THE STACKSIZE OF THE ORIGINAL OBJECT
            //var objSlots = from item in inventory select new GameObjectSlot(GameObject.Objects[item.Key].Clone(), item.Value);

            //this.Slots = new SlotGrid<SlotDefault>(objSlots, 4);

            ////this.StockpilesInterface.Refresh();
            //this.StockpilesInterface.Location = this.Slots.BottomLeft;
            //this.Controls.Remove(this.Slots);
            //this.Controls.Remove(this.StockpilesInterface);
            //this.Controls.Add(this.Slots, this.StockpilesInterface);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileCreated:
                    this.Refresh();
                    //this.StockpilesInterface.Refresh();
                    //this.StockpilesInterface.Location = this.Slots.BottomLeft;
                    StockpileUI.GetWindow(e.Parameters[0] as Stockpile).Show();
                    break;

                case Components.Message.Types.StockpileDeleted:
                    this.Refresh();
                    //Window win;
                    var s = e.Parameters[0] as Stockpile;
                    //if (this.OpenWindows.TryGetValue(s, out win))
                    //    win.Hide();
                    //this.OpenWindows.Remove(s);
                    Window win = s.GetWindow();
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = null;
                    break;

                default:
                    base.OnGameEvent(e);
                    break;
            }
        }

        //public override bool Show(params object[] p)
        //{
        //    this.OnStockpileUpdate();
        //    return base.Show(p);
        //}
    }

}
