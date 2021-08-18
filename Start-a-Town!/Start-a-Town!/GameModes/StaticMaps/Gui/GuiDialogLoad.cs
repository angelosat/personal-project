using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System.IO;
using System.Linq;

namespace Start_a_Town_.Core
{
    class GuiDialogLoad : ScrollableBoxNewNew
    {
        readonly ListBoxNoScroll<SaveFile, ButtonNew> List;
        public GuiDialogLoad()
            : base(320, 320)
        {
            this.List = new ListBoxNoScroll<SaveFile, ButtonNew>(save =>
            {
                var btn = ButtonNew.CreateBig(() => this.Load(save), this.Client.Width, () => Path.GetFileNameWithoutExtension(save.File.Name), () => save.Header.Version); //() => save.Header.CreationTime.ToString("R")); //
                btn.AddControls(IconButton.CreateCloseButton().SetLeftClickAction(b => showDeleteDialogue(save)).SetLocation(btn.TopRight).SetAnchor(Vector2.UnitX).ShowOnParentFocus(true));
                return btn;
            });
            this.AddControls(this.List);
            void showDeleteDialogue(SaveFile save)
            {
                var msgbox = new MessageBox("", $"Delete {save.File.Name}?", () => delete(save));
                msgbox.ShowDialog();

                void delete(SaveFile save)
                {
                    save.File.Delete();
                    this.List.RemoveItems(save);
                }
            }
        }
        public void Populate()
        {
            var saves = GameModeStaticMaps.GetSaves().Select(f => SaveFile.Load(f));
            this.List.Clear();
            this.List.AddItems(saves);
        }
        private void Load(SaveFile item)
        {
            Net.Server.Start();

            StaticMap map = null;
            DialogLoading.AddTask(
                "Loading map",
                () => map = item.World.Map);
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
        private void LoadOld(FileInfo item)
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

    //class GuiDialogLoad : ScrollableBoxNewNew
    //{
    //    readonly ListBoxNoScroll<FileInfo, ButtonNew> List;
    //    public GuiDialogLoad()
    //        : base(320, 320)
    //    {
    //        this.List = new ListBoxNoScroll<FileInfo, ButtonNew>(save =>
    //        {
    //            var btn = ButtonNew.CreateBig(() => this.Load(save), this.Client.Width, () => Path.GetFileNameWithoutExtension(save.Name), () => save.CreationTime.ToString("R"));
    //            btn.AddControls(IconButton.CreateCloseButton().SetLeftClickAction(b => delete(save)).SetLocation(btn.TopRight).SetAnchor(Vector2.UnitX).ShowOnParentFocus(true));
    //            return btn;
    //        });

    //        this.AddControls(this.List);

    //        void delete(FileInfo save)
    //        {
    //            var msgbox = new MessageBox("", $"Delete {save.Name}?", () => delete(save));
    //            msgbox.ShowDialog();

    //            void delete(FileInfo save)
    //            {
    //                save.Delete();
    //                this.List.RemoveItems(save);
    //            }
    //        }
    //    }
    //    public void Populate()
    //    {
    //        var saves = GameModeStaticMaps.GetSaves();
    //        this.List.Clear();
    //        this.List.AddItems(saves);
    //    }

    //    private void Load(FileInfo item)
    //    {
    //        Net.Server.Start();

    //        StaticMap map = null;
    //        DialogLoading.AddTask(
    //            "Loading map",
    //            () => SaveFile.Load(item, out map));
    //        DialogLoading.AddTask(
    //            "Initializing undiscovered areas",
    //            () => map.InitUndiscoveredAreas()); // TEMP UNTIL I START SAVING IT
    //        DialogLoading.AddTask(
    //           "Initializing",
    //           () => map.Init());
    //        DialogLoading.Start(() =>
    //        {
    //            map.CameraRecenter();
    //            string localHost = "127.0.0.1";
    //            UIConnecting.Create(localHost);
    //            Net.Server.SetMap(map); // is this needed??? YES!!! it enumerates all existing entities in the network
    //            Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
    //            this.Hide();
    //        });
    //    }
    //}

}
