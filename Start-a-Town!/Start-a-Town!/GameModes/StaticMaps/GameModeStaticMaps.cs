﻿using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;
using System;
using System.IO;
using System.Linq;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GameModeStaticMaps : GameMode
    {
        readonly CommandParser Parser = new CommandParser();

        GuiDialogLoad DialogLoad;

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
            var btn_save = new Button("Save", width: menu.PanelButtons.ClientSize.Width) { LeftClickAction = IngameSave };
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
            var box = new GroupBox();
            ScrollableBoxNewNew groupBox = null;
            var table = new TableObservable<FileInfo>();
            table
                .AddColumn("filename", 160, f => new Label(Path.GetFileNameWithoutExtension(f.Name)))
                .AddColumn("datetime", 160, f => new Label(f.CreationTime.ToString("R")))
                .AddColumn("overwrite", Button.GetWidth(UIManager.Font, "Overwrite"), f => new Button("Overwrite") { LeftClickAction = () => saveNew(Path.GetFileNameWithoutExtension(f.Name)) })
                .AddColumn("delete", 16, f => IconButton.CreateCloseButton().SetLeftClickAction(b => SaveFile.Delete(f, () => table.RemoveItem(f))))
                ;

            var saves = GetSaves();
            table.AddItems(saves);
            //table.BackgroundColor = Color.Red;
            //groupBox = ScrollableBoxNewNew.FromClientSize(table.RowWidth, 16 * table.RowHeight, ScrollModes.Vertical);
            groupBox = ScrollableBoxNewNew.FromClientSize(table.RowWidth, table.GetHeightFromRowCount(16), ScrollModes.Vertical);
            groupBox.AddControls(table);
            var tablePanel = groupBox.ToPanel();

            getSaveName = new DialogInput("Enter save name", saveNew, 300, map.World.Name);

            box.AddControlsVertically(
                tablePanel, 
                //table,
                new Button("Create new save", tablePanel.Width) { LeftClickAction = delegate { getSaveName.ShowDialog(); } });

            box.ToWindow("Save", true, false).ShowDialog();

            void saveNew(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, "Save");
                var world = map.World as StaticWorld; // TODO: add methods to interface instead of casting them?

                name = name.IsNullEmptyOrWhiteSpace() ? world.Name : name;

                string directory = GlobalVars.SaveDir + @"/Worlds/";
                string worldPath = @"/Saves/Worlds/";
                string fullPath = worldPath + name + ".sat";
                var workingDir = Directory.GetCurrentDirectory();
                if (File.Exists(workingDir + fullPath))
                {
                    var msgBoxOverwrite = new MessageBox("", $"{fullPath} already exists. Overwrite?", save);
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
                    box.GetWindow().Hide();
                }
            }
        }

        //void IngameSave()
        //{
        //    // TODO: store the map reference here? or pass something like a game state as an argument so i can select what to save?
        //    var map = Server.Instance.Map as StaticMap;
        //    DialogInput getSaveName = null;

        //    var groupBox = new GroupBox();
        //    var table = new TableScrollableCompactNewNew<FileInfo>(16, false, ScrollModes.Vertical)
        //        .AddColumn(null, "filename", 160, f => new Label(Path.GetFileNameWithoutExtension(f.Name)), 0)
        //        .AddColumn(null, "datetime", 160, f => new Label(f.CreationTime.ToString("R")), 0)
        //        .AddColumn(null, "overwrite", Button.GetWidth(UIManager.Font, "Overwrite"), f => new Button("Overwrite") { LeftClickAction = () => saveNew(Path.GetFileNameWithoutExtension(f.Name)) }, 0)
        //        .AddColumn(null, "delete", 16, (t, f) => IconButton.CreateCloseButton().SetLeftClickAction(b => SaveFile.Delete(f, () => t.RemoveItem(f))), 0)
        //        ;
        //    var saves = GetSaves();
        //    table.Build(saves);
        //    var tablePanel = table.ToPanel();

        //    getSaveName = new DialogInput("Enter save name", saveNew, 300, map.World.Name);

        //    groupBox.AddControlsVertically(tablePanel, new Button("Create new save", tablePanel.Width) { LeftClickAction = delegate { getSaveName.ShowDialog(); } });

        //    groupBox.ToWindow("Save", true, false).ShowDialog();

        //    void saveNew(string name = "")
        //    {
        //        var tag = new SaveTag(SaveTag.Types.Compound, "Save");
        //        var world = map.World as StaticWorld; // TODO: add methods to interface instead of casting them?

        //        name = name.IsNullEmptyOrWhiteSpace() ? world.Name : name;

        //        string directory = GlobalVars.SaveDir + @"/Worlds/";
        //        string worldPath = @"/Saves/Worlds/";
        //        string fullPath = worldPath + name + ".sat";
        //        var workingDir = Directory.GetCurrentDirectory();
        //        if (File.Exists(workingDir + fullPath))
        //        {
        //            var msgBoxOverwrite = new MessageBox("", $"{fullPath} already exists. Overwrite?", save);
        //            msgBoxOverwrite.ShowDialog();
        //        }
        //        else
        //        {
        //            save();
        //        }

        //        void save()
        //        {
        //            Server.StartSaving();
        //            tag.Add(world.SaveToTag());

        //            using (MemoryStream stream = new())
        //            {
        //                BinaryWriter writer = new(stream);
        //                tag.WriteWithRefs(writer);
        //                if (!Directory.Exists(directory))
        //                    Directory.CreateDirectory(directory);
        //                Chunk.Compress(stream, workingDir + fullPath);
        //                stream.Close();
        //            }
        //            Server.FinishSaving();
        //            getSaveName.Hide();
        //            groupBox.GetWindow().Hide();
        //        }
        //    }
        //}

        public override bool IsPlayerWithinRangeForPacket(PlayerData player, Vector3 packetEventGlobal)
        {
            return true;
        }

        public override void ParseCommand(INetwork net, string command)
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

        internal override void Update(Server server)
        {
            this.SendPendingChunks(server);
        }
        private void SendPendingChunks(Server server)
        {
            foreach (var pl in server.Players.GetList())
            {
                if (pl.SentChunks.Count > 0)
                {
                    continue;
                }

                if (pl.PendingChunks.Count == 0)
                {
                    continue;
                }

                var pending = pl.PendingChunks;
                var first = pending.First();
                pending.Remove(first.Key);
                pl.SentChunks.Add(first.Key);
                PacketChunk.Send(server, first.Key, first.Value, pl);
                ("sending chunk " + first.Key.ToString()).ToConsole();
            }
        }

        internal override void AllChunksReceived(INetwork net)
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

            var ingame = Ingame.Instance.Initialize(client);

            map.CameraRecenter(); // TODO: save camera position
            ScreenManager.Add(ingame); // TODO: find out why there's a freeze when ingame screen begins (and causing rendertargets during ingame.initialize() not work
        }

        internal override Control Load()
        {
            if (this.DialogLoad is null)
                this.DialogLoad = new GuiDialogLoad();

            this.DialogLoad.Populate();
            return this.DialogLoad;
        }
        internal override Control GetNewGameGui(Action cancelAction)
        {
            return new GuiNewGame(cancelAction);
        }
        static readonly string SavePath = GlobalVars.SaveDir + "/Worlds/";
        internal static FileInfo[] GetSaves()
        {
            DirectoryInfo directory = new DirectoryInfo(SavePath);
            if (!Directory.Exists(directory.FullName))
            {
                Directory.CreateDirectory(directory.FullName);
            }

            return directory.GetFiles().OrderByDescending(s => s.CreationTime).ToArray();
        }

        public override GameScreen GetWorldSelectScreen(INetwork net)
        {
            throw new System.NotImplementedException();
        }
    }
}
