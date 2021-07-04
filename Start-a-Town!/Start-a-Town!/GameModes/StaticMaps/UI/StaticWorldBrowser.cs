using System;
using System.Linq;
using System.IO;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps.UI
{
    class StaticWorldBrowser : GroupBox
    {
        ListBox<DirectoryInfo, Button> List_Worlds;
        Action<IWorld> Callback;
        public StaticWorldBrowser(int width, int height, Action<IWorld> callback)
        {

            this.Callback = callback;
            List_Worlds = new ListBox<DirectoryInfo, Button>(new Rectangle(0, 0, width, height));
            this.Controls.Add(this.List_Worlds);

        }
        public new void Refresh()
        {
            DirectoryInfo[] worlds = StaticWorld.GetWorlds();
            List_Worlds.Build(worlds, foo => foo.Name, onControlInit: (item, ctrl) =>
            {
                ctrl.IdleColor = Color.Black;
                ctrl.ColorFunc = () => new Color(0.5f, 0.5f, 0.5f, 1f);
                ctrl.LeftClickAction = () => SelectWorld(item);
            });
        }
        void SelectWorld(DirectoryInfo worldDir)
        {
            DirectoryInfo worldDirectory = worldDir;
            FileInfo[] worldFiles = worldDirectory.GetFiles("*.world.sat", SearchOption.TopDirectoryOnly);
            if (worldFiles.Length == 0)
                throw (new Exception("World file missing!"));
            FileInfo worldSave = worldFiles.First();
            var world = StaticWorld.Load(worldDirectory);
            this.Callback(world);
        }
    }
}
