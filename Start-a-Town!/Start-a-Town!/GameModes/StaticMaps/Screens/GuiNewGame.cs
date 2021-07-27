﻿using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GuiNewGame : GroupBox
    {
        public GuiNewGame(Action cancelAction)
        {
            var tab_World = new GroupBox();
            var label_mapsize = new Label("Map size");

            var lbl_seed = new Label("Seed");
            var txt_Seed = new TextBox(150);
            txt_Seed.Text = Path.GetRandomFileName().Replace(".", "");
            txt_Seed.InputFilter = char.IsLetterOrDigit;

            IconButton btn_random = new()
            {
                HoverText = "Randomize",
                BackgroundTexture = UIManager.Icon16Background,
                Name = "Randomize seed",
                Location = txt_Seed.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                LeftClickAction = () => txt_Seed.Text = Path.GetRandomFileName().Replace(".", "")
            };
            var seedBox = new GroupBox();
            seedBox.AddControls(
                txt_Seed, btn_random);

            var defaultSizes = StaticMap.MapSize.GetList();
            var selectedSize = defaultSizes.First();
            //var comboSize = new ComboBoxLatest<StaticMap.MapSize>(defaultSizes.ToArray(), seedBox.Width, defaultSizes.Count, (c, s) => selectedSize = s, c => selectedSize);
            var comboSize = new ComboBoxNewNew<StaticMap.MapSize>(defaultSizes, seedBox.Width, "Size", s => s.Name, () => selectedSize, s => selectedSize = s);

            tab_World.AddControlsVertically(
                seedBox.ToPanelLabeled("Seed"),
                comboSize.ToPanelLabeled("Map Size"));

            var btn_create = new Button("Create", openActorCreationGui);
            var btn_cancel = new Button("Cancel", cancelAction);

            this.AddControlsVertically(0, HorizontalAlignment.Right,
                tab_World.ToPanel(),
                UIHelper.Wrap(btn_create, btn_cancel)
                );

            void openActorCreationGui()
            {
                var actorsCreateBox = new GroupBox();
                var actors = new List<Actor>();
                var actorsui = new GuiActorCreation(actors);
                var btnstart = new Button("Start") { LeftClickAction = () => this.CreateMap(txt_Seed.Text, selectedSize, actors.ToArray()) };
                var btnback = new Button("Back") { LeftClickAction = () => { actorsui.GetWindow().Hide(); this.GetWindow().Show(); } };
                actorsCreateBox.AddControlsVertically(0, HorizontalAlignment.Right, 
                    actorsui,
                    UIHelper.Wrap(btnstart, btnback)
                    );
                var createActorsWindow = new Window(actorsCreateBox) { Closable = false, Movable = false, Previous = this.GetWindow() };
                createActorsWindow.LocationFunc = () => UIManager.Center;
                createActorsWindow.Anchor = Vector2.One * .5f;
                createActorsWindow.Show();
                this.GetWindow().Hide();
            }
        }

        void CreateMap(string seedString, StaticMap.MapSize size, Actor[] actors)
        {
            var world = new StaticWorld(seedString, Terraformer.Defaults);
            var map = new StaticMap(world, "test", Vector2.Zero, size);
            this.Hide();
            Server.Start();
            map.Generate(showDialog: true)
                .ContinueWith(_ => OnMapGenerated(actors, world, map));
        }

        private static void OnMapGenerated(Actor[] actors, StaticWorld world, StaticMap map)
        {
            map.AddStartingActors(actors);
            world.Maps.Add(map.Coordinates, map);
            string localHost = "127.0.0.1";
            UIConnecting.Create(localHost);
            map.CameraRecenter();
            Server.SetMap(map);
            foreach(var a in actors)
                map.Town.AddCitizen(a);
            Client.Instance.Connect(localHost, "host", a => LobbyWindow.Instance.Console.Write($"Connected to {localHost}")); // TODO dont manipulate the gui in concurrent threads!!!!!
        }
    }
}
