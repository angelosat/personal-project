using Start_a_Town_.UI;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class UIDialogLoad : GroupBox
    {
        ListBoxNew<FileInfo, ButtonNew> List;
        public UIDialogLoad()
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
                var msgbox = new MessageBox("", string.Format("Delete {0}?", save.Name), () => { save.Delete(); this.List.RemoveItem(save); });
                msgbox.ShowDialog();
            }

            for (int i = 0; i < saves.Length; i++)
            {
                var save = saves[i];
                this.List.AddItem(save, s =>
                {
                    var btn = ButtonNew.CreateBig(() => Load(s), this.List.Client.Width, () => Path.GetFileNameWithoutExtension(save.Name), () => save.CreationTime.ToString("R"));
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
                Net.Server.InstantiateMap(map); // is this needed??? YES!!! it enumerates all existing entities in the network
                Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
                this.Hide();
            });
        }
    }
}
