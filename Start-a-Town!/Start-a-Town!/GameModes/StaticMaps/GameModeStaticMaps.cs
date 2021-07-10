﻿using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GameModeStaticMaps : GameMode
    {
        CommandParser Parser = new CommandParser();

        UIDialogLoad DialogLoad;

        public GameModeStaticMaps()
        {
            this.Name = "Static Map";

            this.GameComponents.Add(new TownsManager());
            this.GameComponents.Add(new AI.AIManager());

            PacketChunkRequest.Init();
            PacketPlayerSetSpeed.Init();
            PacketChunkReceived.Init();
            PacketMap.Init();
            PacketWorld.Init();
        }

        
        public override void OnIngameMenuCreated(IngameMenu menu)
        {
            Button btn_save = new Button("Save", width: menu.PanelButtons.ClientSize.Width) { LeftClickAction = IngameSave };
            menu.PanelButtons.Controls.Insert(0, btn_save);
            menu.PanelButtons.AlignVertically();
            menu.SizeToControl(menu.PanelButtons);
        }
        internal override void OnMainMenuCreated(MainMenuWindow mainmenu)
        {
        }

        void IngameSave()
        {
            // TODO: store the map reference here? or pass something like a game state as an argument so i can select what to save?
            var map = Server.Instance.Map as StaticMap;
            DialogInput getSaveName = null;

            var groupBox = new GroupBox();
            var table = new TableScrollableCompactNew<FileInfo>(16, false, ScrollableBoxNew.ScrollModes.Vertical)
                .AddColumn(null, "filename", 160, f => new Label(Path.GetFileNameWithoutExtension(f.Name)), 0)
                .AddColumn(null, "datetime", 160, f => new Label(f.CreationTime.ToString("R")), 0)
                .AddColumn(null, "overwrite", Button.GetWidth(UIManager.Font, "Overwrite"), f => new Button("Overwrite") { LeftClickAction = () => saveNew(Path.GetFileNameWithoutExtension(f.Name)) }, 0)
                .AddColumn(null, "delete", 16, (t, f) => IconButton.CreateCloseButton().SetLeftClickAction(b => SaveFile.Delete(f, () => t.RemoveItem(f))), 0)
                ;
            var saves = GetSaves();
            table.Build(saves);
            var tablePanel = table.ToPanel();

            getSaveName = new DialogInput("Enter save name", saveNew, 300, map.World.Name);
           
            groupBox.AddControlsVertically(tablePanel, new Button("Create new save", tablePanel.Width) { LeftClickAction = () => getSaveName.ShowDialog() });
            groupBox.ToWindow("Save", true, false).ShowDialog();

            void saveNew(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, "Save");
                var world = map.World as StaticWorld; // TODO: add methods to interface instead of casting them

                name = name.IsNullEmptyOrWhiteSpace() ? world.Name : name;

                string directory = GlobalVars.SaveDir + @"/Worlds/";
                string worldPath = @"/Saves/Worlds/";
                string fullPath = worldPath + name + ".sat";
                var workingDir = Directory.GetCurrentDirectory();
                if (File.Exists(workingDir + fullPath))
                {
                    var msgBoxOverwrite = new MessageBox("", string.Format("{0} already exists. Overwrite?", fullPath), save);
                    msgBoxOverwrite.ShowDialog();
                }
                else
                    save();

                void save()
                {
                    Server.StartSaving();
                    tag.Add(world.SaveToTag());

                    using (MemoryStream stream = new())
                    {
                        BinaryWriter writer = new(stream);
                        tag.WriteWithRefs(writer);

                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        Chunk.Compress(stream, workingDir + fullPath);

                        stream.Close();
                    }
                    Server.FinishSaving();
                    getSaveName.Hide();
                    groupBox.GetWindow().Hide();
                }
            }
        }

        public override bool IsPlayerWithinRangeForPacket(PlayerData player, Vector3 packetEventGlobal)
        {
            return true;
        }
    
        public override void ParseCommand(IObjectProvider net, string command)
        {
            this.Parser.Execute(net, command);
        }

        internal override void PlayerConnected(Server server, PlayerData player)
        {
            PacketWorld.Send(server, player);
            PacketMap.Send(server, player);
        }
        internal override void PlayerIDAssigned(Client client)
        {
            PacketChunkRequest.Send(client, Client.Instance.PlayerData.ID);
        }
        internal override void ChunkReceived(Server server, int playerid, Vector2 vec)
        {
            var player = server.GetPlayer(playerid);
            player.SentChunks.Remove(vec);
        }
        internal override void MapReceived(MapBase map)
        {
            this.ChunksPending = new List<Vector2>();
            var size = (map as StaticMap).Size.Chunks;
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    this.ChunksPending.Add(new Vector2(i, j));
        }
        ConcurrentQueue<Chunk> IncomingChunks = new ConcurrentQueue<Chunk>();
        List<Vector2> ChunksPending = new List<Vector2>();
        internal override void Update(Client client)
        {
            Chunk chunk;
            while (this.IncomingChunks.TryDequeue(out chunk))
            {
                client.ReceiveChunk(chunk);
                this.ChunksPending.Remove(chunk.MapCoords);
                ("chunk received " + chunk.MapCoords.ToString()).ToConsole();
                Client.Instance.Send(PacketType.ChunkReceived, Network.Serialize(w => w.Write(chunk.MapCoords)));

                // change screen when player entity is assigned instead of here?
                if (this.ChunksPending.Count == 0)
                {
                    // all chunks received, enter world
                    "all chunks loaded!".ToConsole();
                    (client.Map as StaticMap).Regions.Init();
                    (client.Map as StaticMap).FinishLoading();
                    client.EnterWorld(PlayerOld.Actor);
                    Rooms.Ingame ingame = Rooms.Ingame.Instance;
                    ScreenManager.Add(ingame.Initialize(client)); // TODO: find out why there's a freeze when ingame screen begins (and causing rendertargets during ingame.initialize() not work
                }
            }
        }
        internal override void Update(Server server)
        {
            SendPendingChunks(server);
        }
        private void SendPendingChunks(Server server)
        {
            foreach (var pl in server.Players.GetList())
            {
                if (pl.SentChunks.Count > 0)
                    continue;
                if (pl.PendingChunks.Count == 0)
                    continue;
                var pending = pl.PendingChunks;
                var first = pending.First();
                pending.Remove(first.Key);
                pl.SentChunks.Add(first.Key);
                PacketChunk.Send(server, first.Key, first.Value, pl);
                ("sending chunk " + first.Key.ToString()).ToConsole();
            }
        }

        internal override void AllChunksReceived(IObjectProvider net)
        {
            // all chunks received, enter world
            "all chunks loaded!".ToConsole();
            net.EventOccured(Components.Message.Types.ChunksLoaded);
            var map = net.Map as GameModes.StaticMaps.StaticMap;
            map.Regions.Init();
            map.InitUndiscoveredAreas();
            map.Init();
            map.FinishLoading();
            map.ResolveReferences();

            var client = net as Client;

            var ingame = Rooms.Ingame.Instance.Initialize(client);

            map.CameraRecenter(); // TODO: save camera position
            ScreenManager.Add(ingame); // TODO: find out why there's a freeze when ingame screen begins (and causing rendertargets during ingame.initialize() not work
        }

        internal override Control Load()
        {
            if (DialogLoad == null)
                DialogLoad = new UIDialogLoad();
            DialogLoad.Populate();
            return DialogLoad;
        }
        internal override Control NewGame()
        {
            return new UINewGame();
        }
        static string SavePath = GlobalVars.SaveDir + "/Worlds/";
        internal static FileInfo[] GetSaves()
        {
            DirectoryInfo directory = new DirectoryInfo(SavePath);
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetFiles().OrderByDescending(s => s.CreationTime).ToArray();
        }

        public override Rooms.GameScreen GetWorldSelectScreen(IObjectProvider net)
        {
            throw new System.NotImplementedException();
        }
    }
}
