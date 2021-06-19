﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes.StaticMaps;
using Start_a_Town_.GameModes.StaticMaps.Screens;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps.UI
{
    class StaticMapBrowser : GroupBox
    {
        StaticWorld World;
        PanelLabeled PanelMapList;
        Panel PanelInfo;
        ListBox<IMap, Button> MapList;
        Button BtnCreateMap;
        Action<StaticMap> CallBack;

        public StaticMapBrowser(Action<StaticMap> callback)//StaticWorld world)
        {
            //this.World = world;
            this.CallBack = callback;

            this.PanelMapList = new PanelLabeled("Maps") { AutoSize = true};

            this.MapList = new ListBox<IMap, Button>(new Rectangle(0, 0, 150, 300)) { Location = this.PanelMapList.Label.BottomLeft };
            this.PanelMapList.Controls.Add(this.MapList);

            //this.PanelInfo = new PanelLabeled("Info") { Location = this.PanelMapList.TopRight, Size = this.PanelMapList.Size };
            this.PanelInfo = new Panel() { Location = this.PanelMapList.TopRight, Size = this.PanelMapList.Size };

            //this.MapList = new ScrollableList(Vector2.Zero, this.PanelMapList.Size);//, m => m.Coordinates.ToString());
            
            this.BtnCreateMap = new Button("Create Map", this.PanelMapList.Width * 2) { Location = this.PanelMapList.BottomLeft, LeftClickAction = CreateMap };
            //var box = new Panel() { Location = this.PanelInfo.TopRight, AutoSize = true };
            //box.Controls.Add(this.MapList);
            this.Controls.Add(this.PanelMapList, this.PanelInfo);//
                //box);
            this.Location = this.CenterScreen;
        }

        public void SetWorld(StaticWorld world)
        {
            this.World = world;
            this.Refresh();
        }

        void Refresh()
        {
            this.Controls.Remove(this.BtnCreateMap);
            this.PanelInfo.Controls.Clear();
            //this.PanelMapList.Controls.Clear();
            //this.PanelInfo.Controls.Clear();
            //this.MapList.Build(new IMap[] { });
            this.MapList.Clear();
            if (this.World == null)
                return;
            if (this.World.Maps.Count == 0)
                this.Controls.Add(this.BtnCreateMap);

            //this.MapList.Build(this.World.GetMaps().Values);
            this.MapList.Build(this.World.GetMaps().Values, m => m.Coordinates.ToString(), (map, btn) => btn.LeftClickAction = () => this.SelectMap(map));
            
            //this.Controls.Remove(this.MapList);
            //this.Controls.Add(this.MapList);
        }

        void CreateMap()
        {
            //var map = this.World.CreateMap(Vector2.Zero);
            var map = new StaticMap(this.World, Vector2.Zero.ToString(), Vector2.Zero, StaticMap.MapSize.Default);
            this.World.Maps.Add(Vector2.Zero, map);
            this.Refresh();
        }

        void SelectMap(IMap map)
        {
            this.PanelInfo.Controls.Clear();
            if (map == null)
                return;
            var btnEnter = new Button("Play", this.MapList.Width) { Location = new Vector2(0, this.PanelInfo.GetClientSize().Height), Anchor = Vector2.UnitY, LeftClickAction = ()=> PlayMap(map) };
            this.PanelInfo.Controls.Add(new Label(map.ToString()), btnEnter);
        }

        private void PlayMap(IMap map)
        {
            //Net.Server.LoadMap(map);
            this.CallBack(map as StaticMap);
            //ScreenManager.Add(new ScreenMapLoading(map as StaticMap));
        }
    }
}
