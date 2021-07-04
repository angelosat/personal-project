using Start_a_Town_.UI;

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

            this.LblWorldName = new Label("World Name: ");
            this.LblSeed = new Label("World seed: ");
            this.LblMapName = new Label("Map Name: ");

            this.LblWorldName.Text = "World Name: " + map.World.GetName();
            this.LblSeed.Text = "World seed: " + map.World.GetSeed();
            this.LblMapName.Text = "Map Name: " + map.GetName();

            var panelinfo = new Panel() { AutoSize = true };
            panelinfo.AddControlsVertically(
                this.LblWorldName,
                this.LblSeed,
                this.LblMapName);

            this.Buttons = new Panel() { AutoSize = true };
            this.BtnDelete = new Button("Delete", panelinfo.ClientSize.Width);
            this.BtnEnter = new Button("Enter", panelinfo.ClientSize.Width) { Location = this.BtnDelete.TopRight, LeftClickAction = Enter };
            this.Buttons.AddControlsVertically(
                this.BtnEnter, 
                this.BtnDelete);

            this.Client.AddControlsVertically(
                panelinfo,
                this.Buttons);

            this.SnapToScreenCenter();
            this.Anchor = new Microsoft.Xna.Framework.Vector2(.5f);
        }

        static public WindowSelectedWorldNew Refresh(StaticMap map)
        {
            return new WindowSelectedWorldNew(map);
        }

        private void Enter()
        {
            string localHost = "127.0.0.1";
            UIConnecting.Create(localHost);

            Net.Server.Start();
            Net.Server.InstantiateMap(this.Map); // is this needed??? YES!!! it enumerates all existing entitites in the network
            Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            this.Hide();
        }
    }
}
