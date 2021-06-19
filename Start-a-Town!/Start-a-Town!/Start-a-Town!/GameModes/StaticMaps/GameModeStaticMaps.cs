using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes.StaticMaps.Screens;
using Start_a_Town_.GameModes.StaticMaps.Packets;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Modules.Base;
using Start_a_Town_.Towns;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GameModeStaticMaps : GameMode
    {
        CommandParser Parser = new CommandParser();

        public GameModeStaticMaps()
        {
            this.Name = "Static Map";
            this.ServerEventHandler = new StaticMapsServerEventHandler();
            this.ClientEventHandler = new StaticMapsClientEventHandler();
            
            //this.ServerPacketHandler = new StaticMapsServerPacketHandler();
            //this.ClientPacketHandler = new staticmapsclie
            this.PacketHandler = new StaticMapsPacketHandler();

            //this.GameComponents.Add(new GameManager());
            this.GameComponents.Add(new ConstructionManager());
            this.GameComponents.Add(new TownsManager());
            this.GameComponents.Add(new AI.AIManager());
        }
        
        public override Rooms.GameScreen GetWorldSelectScreen()
        {
            return StaticWorldScreen.Instance.Initialize();
        }

        public override void OnIngameMenuCreated(IngameMenu menu)
        {
            Button btn_save = new Button("Save", width: menu.PanelButtons.ClientSize.Width) { LeftClickAction = IngameSave };
            //menu.PanelButtons.AutoSize = true;
            menu.PanelButtons.Controls.Insert(0, btn_save);
            menu.PanelButtons.AlignVertically();
            menu.SizeToControl(menu.PanelButtons);
        }

        void IngameSave()
        {
            // TODO: store the map reference here? or pass something like a game state as an argument so i can select what to save?
            var map = Net.Server.Instance.Map as StaticMap;
            //map.ForceSaveChunks();
            map.Save();
            List<LoadingTask> tasks = new List<LoadingTask>();
            foreach (var chunk in map.ActiveChunks.Values)
                tasks.Add(new LoadingTask("Saving chunks...", chunk.SaveToFile));
            //var loading = new LoadingOperation("Saving chunks...", tasks);
            tasks.Add(new LoadingTask("Saving player...", Net.Client.SavePlayerCharacter));
            // TODO: pause game while saving!!!
            LoadingOperation.StartNew("Saving", tasks);
        }

        public override bool IsPlayerWithinRangeForPacket(GameObject actor, Vector3 packetEventGlobal)
        {
            return true;
        }

        public override IEnterior GetEnterior(IMap map, Vector3 global)
        {
            return map.GetTown().GetHouseAt(global);
        }

        public override void ParseCommand(Net.IObjectProvider net, string command)
        {
            this.Parser.Execute(net, command);
        }
    }
}
