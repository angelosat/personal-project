using System;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps.UI
{
    class StaticMapBrowser : GroupBox
    {
        StaticWorld World;
        PanelLabeled PanelMapList;
        Panel PanelInfo;
        ListBox<MapBase, Button> MapList;
        Button BtnCreateMap;
        Action<StaticMap> CallBack;

        public StaticMapBrowser(Action<StaticMap> callback)
        {
            this.CallBack = callback;

            this.PanelMapList = new PanelLabeled("Maps") { AutoSize = true};

            this.MapList = new ListBox<MapBase, Button>(new Rectangle(0, 0, 150, 300)) { Location = this.PanelMapList.Label.BottomLeft };
            this.PanelMapList.Controls.Add(this.MapList);

            this.PanelInfo = new Panel() { Location = this.PanelMapList.TopRight, Size = this.PanelMapList.Size };

            this.BtnCreateMap = new Button("Create Map", this.PanelMapList.Width * 2) { Location = this.PanelMapList.BottomLeft, LeftClickAction = CreateMap };
            this.Controls.Add(this.PanelMapList, this.PanelInfo);//
            this.SnapToScreenCenter();

        }

        public void SetWorld(StaticWorld world)
        {
            this.World = world;
            this.Refresh();
        }

        public override void Refresh()
        {
            this.Controls.Remove(this.BtnCreateMap);
            this.PanelInfo.Controls.Clear();
            this.MapList.Clear();
            if (this.World == null)
                return;
            if (this.World.Maps.Count == 0)
                this.Controls.Add(this.BtnCreateMap);

            this.MapList.Build(this.World.GetMaps().Values, m => m.Coordinates.ToString(), (map, btn) => btn.LeftClickAction = () => this.SelectMap(map));
        }

        void CreateMap()
        {
            var map = new StaticMap(this.World, Vector2.Zero.ToString(), Vector2.Zero, StaticMap.MapSize.Default);
            this.World.Maps.Add(Vector2.Zero, map);
            this.Refresh();
        }

        void SelectMap(MapBase map)
        {
            this.PanelInfo.Controls.Clear();
            if (map == null)
                return;
            var btnEnter = new Button("Play", this.MapList.Width) { Location = new Vector2(0, this.PanelInfo.GetClientSize().Height), Anchor = Vector2.UnitY, LeftClickAction = ()=> PlayMap(map) };
            this.PanelInfo.Controls.Add(new Label(map.ToString()), btnEnter);
        }

        private void PlayMap(MapBase map)
        {
            this.CallBack(map as StaticMap);
        }
    }
}
