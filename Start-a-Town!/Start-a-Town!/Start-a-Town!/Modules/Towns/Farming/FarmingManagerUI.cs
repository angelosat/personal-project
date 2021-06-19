using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Farming;

namespace Start_a_Town_.Towns.Farming
{
    class FarmingManagerUI : GroupBox
    {
        FarmingManager Manager;
        IconButton BtnDesignate;
        Panel PanelList;
        ListBox<Farmland, Label> ListFarms;
        Dictionary<Farmland, Window> OpenWindows = new Dictionary<Farmland, Window>();

        public FarmingManagerUI(FarmingManager manager)
        {
            this.Manager = manager;

            this.PanelList = new Panel() { AutoSize = true };
            this.ListFarms = new ListBox<Farmland, Label>(80, 150);
            RefreshFarmList();
            this.PanelList.AddControls(this.ListFarms);

            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate farmland\n\nLeft click & drag: Add farmland\nCtrl+Left click: Remove farmland",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new ToolZoningPositions(
                    CreateFarm
                , manager.Town.FarmingManager.GetPositions),// manager.Town.GetZones
                Location = this.PanelList.TopRight
            };
            this.Controls.Add(this.PanelList, this.BtnDesignate);
        }

        private void RefreshFarmList()
        {
            this.ListFarms.Build(this.Manager.Farmlands.Values.ToList(), f => f.Name, (f, c) => c.LeftClickAction = () => OpenFarm(f));
        }

        private void OpenFarm(Farmland farm)
        {
            farm.GetWindow().Show();//.Toggle();
            return;

            Window win;
            if (this.OpenWindows.TryGetValue(farm, out win))
                return;
            var window = new Window();
            window.Title = farm.Name;
            window.AutoSize = true;
            window.Movable = true;
            var farmui = farm.GetInterface();
            window.Client.Controls.Add(farmui);
            window.HideAction = () => this.OpenWindows.Remove(farm);
            window.ShowAction = () => this.OpenWindows.Add(farm, window);
            window.Location = UIManager.Mouse;// ScreenManager.CurrentScreen.Camera.GetScreenPosition(global);
            window.Toggle();

        }

        private static void CreateFarm(Vector3 global, int w, int h, bool remove)
        {
            Client.Instance.Send(PacketType.FarmCreate, new PacketCreateFarmland(Player.Actor.Network.ID, 0, global, w, h, remove).Write());
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.FarmCreated:
                    var farm = e.Parameters[0] as Farmland;
                    //FloatingText.Manager.Create(() => farm.Begin, "Farm created", ft => ft.Font = UIManager.FontBold);
                    FloatingText.Manager.Create(() => farm.Positions.First(), "Farm created", ft => ft.Font = UIManager.FontBold);
                    RefreshFarmList();
                    OpenFarm(farm);
                    break;

                case Components.Message.Types.FarmRemoved:
                    RefreshFarmList();
                    Window win;
                    farm = e.Parameters[0] as Farmland;
                    if (this.OpenWindows.TryGetValue(farm, out win))
                        win.Hide();
                    this.OpenWindows.Remove(farm);
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = null;
                    break;

                case Components.Message.Types.FarmSeedChanged:
                    farm = e.Parameters[0] as Farmland;
                    var seed = e.Parameters[1] as GameObject;
                    //FloatingText.Manager.Create(() => farm.Begin, "Farm seed set to: " + seed.Name, ft => ft.Font = UIManager.FontBold);
                    FloatingText.Manager.Create(() => farm.Positions.First(), "Farm seed set to: " + (seed == null ? "None" : seed.Name), ft => ft.Font = UIManager.FontBold);
                    break;

                case Components.Message.Types.FarmUpdated:
                    this.RefreshFarmList();
                    break;


                default:
                    break;
            }
        }
    }
}
