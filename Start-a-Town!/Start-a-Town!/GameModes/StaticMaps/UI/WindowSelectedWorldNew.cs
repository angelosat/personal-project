using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes.StaticMaps.Screens;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class WindowSelectedWorldNew : Window
    {
        Label LblWorldName, LblMapName, LblSeed;
        Button BtnDelete, BtnEnter;
        Panel Buttons;
        StaticMap Map;

        WindowSelectedWorldNew(StaticMap map)
        {
            this.Map = map;
            this.Title = "Selected World";
            this.Movable = false;
            this.AutoSize = true;
            //this.Size = new Microsoft.Xna.Framework.Rectangle(0, 0, 200, 300);

            this.LblWorldName = new Label("World Name: ");
            this.LblSeed = new Label("World seed: ");// { Location = this.LblWorldName.BottomLeft };
            this.LblMapName = new Label("Map Name: ");// { Location = this.LblSeed.BottomLeft };

            this.LblWorldName.Text = "World Name: " + map.World.GetName();
            this.LblSeed.Text = "World seed: " + map.World.GetSeed();
            this.LblMapName.Text = "Map Name: " + map.GetName();

            var panelinfo = new Panel() { AutoSize = true };
            panelinfo.AddControlsVertically(
                this.LblWorldName,
                this.LblSeed,
                this.LblMapName);

            this.Buttons = new Panel() { AutoSize = true };// new GroupBox() { Location = this.LblMapName.BottomLeft }; ;
            this.BtnDelete = new Button("Delete", panelinfo.ClientSize.Width);// { Location = this.LblMapName.BottomLeft };
            this.BtnEnter = new Button("Enter", panelinfo.ClientSize.Width) { Location = this.BtnDelete.TopRight, LeftClickAction = Enter };
            //this.Buttons.Controls.Add(this.BtnDelete, this.BtnEnter);
            this.Buttons.AddControlsVertically(
                this.BtnEnter, 
                this.BtnDelete);

            this.Client.AddControlsVertically(
                panelinfo,
                this.Buttons);//, this.Buttons);

            //this.Location = this.CenterScreen;
            this.SnapToScreenCenter();
            this.Anchor = new Microsoft.Xna.Framework.Vector2(.5f);
        }
        //public WindowSelectedWorldNew(StaticMap map)
        //{
        //    this.Map = map;

        //    this.Title = "Selected World";
        //    this.Movable = false;
        //    this.AutoSize = true;
        //    this.Size = new Microsoft.Xna.Framework.Rectangle(0, 0, 200, 300);

        //    var w = map.World;
        //    this.LblWorldName = new Label("World Name: " + w.GetName());
        //    this.LblSeed = new Label("World seed: " + w.GetSeed()) { Location = this.LblWorldName.BottomLeft };
        //    this.LblMapName = new Label("Map Name: " + map.GetName()) { Location = this.LblSeed.BottomLeft };

        //    this.Buttons = new GroupBox() { Location = this.LblMapName.BottomLeft }; ;
        //    this.BtnDelete = new Button("Delete");// { Location = this.LblMapName.BottomLeft };
        //    this.BtnEnter = new Button("Enter") { Location = this.BtnDelete.TopRight, LeftClickAction = Enter };
        //    this.Buttons.Controls.Add(this.BtnDelete, this.BtnEnter);

        //    this.Client.Controls.Add(this.LblWorldName, this.LblMapName, this.LblSeed, this.Buttons);


        //    //this.Location = this.CenterScreen;
        //    this.CenterScreen();

        //}

        static public WindowSelectedWorldNew Refresh(StaticMap map)
        {
            return new WindowSelectedWorldNew(map);
        }
        //public WindowSelectedWorldNew Refresh(StaticMap map)
        //{
        //    this.Map = map;
        //    // TODO: do something if map is invalid

        //    var w = map.World;

        //    this.LblWorldName.Text = "World Name: " + w.GetName();
        //    this.LblSeed.Text = "World seed: " + w.GetSeed();
        //    this.LblMapName.Text = "Map Name: " + map.GetName();
        //    this.Client.Controls.Remove(this.Buttons);
        //    this.Client.Controls.Add(this.Buttons);

        //    return this;
        //}

        private void Enter()
        {
            //"loading map...".ToConsole();
            //Stopwatch watch = Stopwatch.StartNew();
            ////this.Map.LoadAllChunks();
            //this.Map.InitChunks();
            //("map loaded in " + watch.Elapsed.ToString()).ToConsole();
            //watch.Stop();
            string localHost = "127.0.0.1";
            UIConnecting.Create(localHost);

            Net.Server.Start();
            Net.Server.InstantiateMap(this.Map); // is this needed??? YES!!! it enumerates all existing entitites in the network
            //Client.SessionCharacter = this.Window_Character.Tag as GameObject;
            Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            this.Hide();
        }
    }
}
