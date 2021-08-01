using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GuiDialogLoad : ScrollableBoxNewNew
    {
        readonly ListBoxNoScroll<FileInfo, ButtonNew> List;
        public GuiDialogLoad()
            : base(320, 320)
        {
            this.List = new ListBoxNoScroll<FileInfo, ButtonNew>(save =>
            {
                var btn = ButtonNew.CreateBig(() => this.Load(save), this.Client.Width, () => Path.GetFileNameWithoutExtension(save.Name), () => save.CreationTime.ToString("R"));
                btn.AddControls(IconButton.CreateCloseButton().SetLeftClickAction(b => delete(save)).SetLocation(btn.TopRight).SetAnchor(Vector2.UnitX));
                return btn;
            });
            this.AddControls(this.List);

            void delete(FileInfo save)
            {
                var msgbox = new MessageBox("", $"Delete {save.Name}?", () => delete(save));
                msgbox.ShowDialog();

                void delete(FileInfo save)
                {
                    save.Delete();
                    this.List.RemoveItems(save);
                }
            }
        }
        public void Populate()
        {
            var saves = GameModeStaticMaps.GetSaves();
            this.List.Clear();
            this.List.AddItems(saves);
            //for (int i = 0; i < saves.Length; i++)
            //{
            //    var save = saves[i];
            //    this.List.AddItem(save, );
            //}
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
