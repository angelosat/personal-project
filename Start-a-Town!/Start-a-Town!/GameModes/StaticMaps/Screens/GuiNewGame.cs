using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Core
{
    class GuiNewGame : GroupBox
    {
        public GuiNewGame(Action cancelAction)
        {
            var tab_World = new GroupBox();
            var label_mapsize = new Label("Map size");

            var guiname = TextBox.CreateWithLabel("Name", StaticWorld.GetRandomName(), 150, out _, out var txtboxname);
            txtboxname.InputFilter = char.IsLetterOrDigit;

            var btn_randomName = new IconButton()
            {
                HoverText = "Randomize",
                BackgroundTexture = UIManager.Icon16Background,
                Name = "Randomize",
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                LeftClickAction = () => txtboxname.Text = StaticWorld.GetRandomName()
            };
            guiname.AddControlsTopRight(btn_randomName);
            var defaultSizes = StaticMap.MapSize.GetList();
            var selectedSize = defaultSizes.First();
            var comboSize = new ComboBoxNewNew<StaticMap.MapSize>(defaultSizes, guiname.Width, "Size", s => s.Name, () => selectedSize, s => selectedSize = s);

            var terraformers = Terraformer.Defaults.Select(d => d.Create()).ToList();
            var winTerraformers = terraformers.Select(t => t.GetUI()).ToGroupBoxVertically().ToPanel().ToWindow("Terraformers Properties", closable: true, movable: false);
            var btnadvanced = new Button("Advanced", toggleAdvanced, guiname.Width);

            tab_World.AddControlsVertically(1,
                guiname,
                comboSize,
                btnadvanced);

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
                var btnstart = new Button("Start", () => this.CreateMap(txtboxname.Text, selectedSize, terraformers, actors.ToArray()));
                var btnback = new Button("Back", () => { actorsui.GetWindow().Hide(); this.GetWindow().Show(); });
                actorsCreateBox.AddControlsVertically(0, HorizontalAlignment.Right,
                    actorsui,
                    UIHelper.Wrap(btnstart, btnback)
                    );
                var createActorsWindow = new Window(actorsCreateBox) { Closable = false, Movable = false };
                createActorsWindow.LocationFunc = () => UIManager.Center;
                createActorsWindow.Anchor = Vector2.One * .5f;
                createActorsWindow.Show();
                this.GetWindow().Hide();
            }

            void toggleAdvanced() 
            {
                //var allProps = terraformers.SelectMany(t => t.GetModifiableProperties());
                //var allProps = terraformers.SelectMany(t => t.GetAdjustableParameters());
                //foreach(var p in allProps)
                //{

                //}
                winTerraformers.ToggleDialog();
            };
        }

        void CreateMap(string name, StaticMap.MapSize size, List<Terraformer> terraformers, Actor[] actors)
        {
            var world = new StaticWorld(name, terraformers);
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
