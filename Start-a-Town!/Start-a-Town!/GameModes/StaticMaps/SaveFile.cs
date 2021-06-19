using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes.StaticMaps
{
    static class SaveFile
    {
        internal static void Load(FileInfo file, out StaticMap map)
        {
            using (FileStream stream = new FileStream(file.FullName, System.IO.FileMode.Open))
            {
                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                    //SaveTag worldTag = SaveTag.Read(reader);
                    SaveTag worldTag = SaveTag.ReadWithRefs(reader);

                    var world = new StaticWorld(worldTag["World"]);
                    //map = StaticMap.Load(world, Vector2.Zero, worldTag["Map"] as SaveTag);
                    map = world.Map;
                    //world.Maps.Add(map.Coordinates, map);
                }
            }
        }

        static public void Delete(FileInfo save, Action callback = null)
        {
            var msgbox = new MessageBox("", string.Format("Delete {0}?", save.Name), () => { save.Delete(); callback?.Invoke(); });// this.List.RemoveItem(save); });
            msgbox.ShowDialog();
        }
    }
}
