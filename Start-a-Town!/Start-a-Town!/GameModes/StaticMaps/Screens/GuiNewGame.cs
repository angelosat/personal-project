using Microsoft.Xna.Framework;
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

            var guiname = TextBox.CreateWithLabel("Name", 150, out _, out var txtboxname);
            txtboxname.Text = StaticWorld.GetRandomName();
            txtboxname.InputFilter = char.IsLetterOrDigit;

            var seedBox = TextBox.CreateWithLabel("Seed", 150, out var lbl_seed, out var txt_Seed);
            txt_Seed.Text = Path.GetRandomFileName().Replace(".", "");
            txt_Seed.InputFilter = char.IsLetterOrDigit;

            var btn_randomSeed = new IconButton()
            {
                HoverText = "Randomize",
                BackgroundTexture = UIManager.Icon16Background,
                Name = "Randomize seed",
                Location = txt_Seed.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                LeftClickAction = () => txt_Seed.Text = Path.GetRandomFileName().Replace(".", "")
            };
            var btn_randomName = new IconButton()
            {
                HoverText = "Randomize",
                BackgroundTexture = UIManager.Icon16Background,
                Name = "Randomize seed",
                Location = txt_Seed.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                LeftClickAction = () => txtboxname.Text = StaticWorld.GetRandomName()
            };
            seedBox.AddControls(
                btn_randomSeed);
            guiname.AddControlsTopRight(btn_randomName);
            var defaultSizes = StaticMap.MapSize.GetList();
            var selectedSize = defaultSizes.First();
            var comboSize = new ComboBoxNewNew<StaticMap.MapSize>(defaultSizes, seedBox.Width, "Size", s => s.Name, () => selectedSize, s => selectedSize = s);

            tab_World.AddControlsVertically(1,
                guiname,
                seedBox,
                comboSize);//.ToPanelLabeled("Map Size"));

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
                var btnstart = new Button("Start", () => this.CreateMap(txt_Seed.Text, selectedSize, actors.ToArray()));
                var btnback = new Button("Back", () => { actorsui.GetWindow().Hide(); this.GetWindow().Show(); });
                actorsCreateBox.AddControlsVertically(0, HorizontalAlignment.Right,
                    actorsui,
                    UIHelper.Wrap(btnstart, btnback)
                    );
                var createActorsWindow = new Window(actorsCreateBox) { Closable = false, Movable = false };//, Previous = this.GetWindow() };
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
            foreach (var a in actors)
                map.Town.AddCitizen(a);
            Client.Instance.Connect(localHost, "host", a => LobbyWindow.Instance.Console.Write($"Connected to {localHost}")); // TODO dont manipulate the gui in concurrent threads!!!!!
        }
    }
}
