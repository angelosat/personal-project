using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;

namespace Start_a_Town_.Towns
{
    public class StockpilesManagerUI : GroupBox
    {
        StockpilesListUI StockpilesList;
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
                //LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(Player.Actor.Map.Town)// this.Create)
                LeftClickAction = () => ZoneNew.Edit(typeof(Stockpile))// Stockpile.Edit// ToolManager.SetTool(new ToolDesignateZone(town, typeof(Stockpile)))

            };
            panelButtons.Controls.Add(this.BtnDesignate);
            this.Controls.Add(panelButtons);

            PanelLabeled stockpiles = new PanelLabeled("Stockpiles") { Location = this.Controls.BottomLeft, AutoSize = true };
            //this.StockpileUI = new StockpileUI(town) { Location = stockpiles.Controls.BottomLeft };
            //stockpiles.Controls.Add(StockpileUI);
            //this.Controls.Add(stockpiles);

            this.StockpilesList = new StockpilesListUI(this.Town, 100, 150) { Location = stockpiles.Controls.BottomLeft};//this.BtnDesignate.BottomLeft };

            stockpiles.Controls.Add(this.StockpilesList);

            this.SlotsUI = new StockpileSlotsUI(this.Town) { Location = stockpiles.TopRight };

            //var contents = new UIStockpileInventoryIcons() { Location = stockpiles.BottomLeft };
            this.Controls.Add(stockpiles, this.SlotsUI
                //,contents
                );
        }

        public new void Refresh()
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
                    Stockpile stockpile = e.Parameters[0] as Stockpile;
                    // do i want to immediately open the stockpile's interface upon creation?
                    //StockpileUI.GetWindow(stockpile).Show();
                    FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile created", ft => ft.Font = UIManager.FontBold);

                    break;

                case Components.Message.Types.StockpileDeleted:
                    this.Refresh();
                    //Window win;
                    stockpile = e.Parameters[0] as Stockpile;
                    //if (this.OpenWindows.TryGetValue(s, out win))
                    //    win.Hide();
                    //this.OpenWindows.Remove(s);
                    Window win = stockpile.GetWindow();
                    //ScreenManager.CurrentScreen.ToolManager.ActiveTool = null;
                    FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile deleted", ft => ft.Font = UIManager.FontBold);
                    break;

                default:
                    base.OnGameEvent(e);
                    break;
            }
        }

        //public override bool Show()
        //{
        //    this.OnStockpileUpdate();
        //    return base.Show();
        //}
    }

}
