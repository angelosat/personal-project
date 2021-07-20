using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GuiDialogLoad : GroupBox
    {
        readonly ListBoxNew<FileInfo, ButtonNew> List;
        public GuiDialogLoad()
        {
            this.AutoSize = true;
            this.List = new ListBoxNew<FileInfo, ButtonNew>(200, 300);
            this.AddControls(this.List);
        }
        public void Populate()
        {
            var saves = GameModeStaticMaps.GetSaves();

            void delete(FileInfo save)
            {
                var msgbox = new MessageBox("", $"Delete {save.Name}?", () => delete(save));
                msgbox.ShowDialog();

                void delete(FileInfo save)
                {
                    save.Delete();
                    this.List.RemoveItem(save);
                }
            }

            for (int i = 0; i < saves.Length; i++)
            {
                var save = saves[i];
                this.List.AddItem(save, s =>
                {
                    var btn = ButtonNew.CreateBig(() => this.Load(s), this.List.Client.Width, () => Path.GetFileNameWithoutExtension(save.Name), () => save.CreationTime.ToString("R"));
                    btn.AddControls(IconButton.CreateCloseButton().SetLeftClickAction(b => delete(save)).SetLocation(btn.TopRight).SetAnchor(Vector2.UnitX));
                    return btn;
                });
            }
        }
        private void Load(FileInfo item)
        {
            Net.Server.Start();

            StaticMap map = null;
            DialogLoading.AddTask(
                "Loading map",
                () => SaveFile.Load(item, out map));
            DialogLoading.AddTask(
                "Initializing undiscovered areas",
                () => map.InitUndiscoveredAreas()); // TEMP UNTIL I START SAVING IT
            DialogLoading.AddTask(
               "Initializing",
               () => map.Init());
            DialogLoading.Start(() =>
            {
                map.CameraRecenter();
                string localHost = "127.0.0.1";
                UIConnecting.Create(localHost);
                Net.Server.SetMap(map); // is this needed??? YES!!! it enumerates all existing entities in the network
                Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
                this.Hide();
            });
        }
    }
}
