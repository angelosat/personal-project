using System;
using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes.StaticMaps
{
    static class SaveFile
    {
        internal static void Load(FileInfo file, out StaticMap map)
        {
            using FileStream stream = new FileStream(file.FullName, FileMode.Open);
            using MemoryStream decompressedStream = Chunk.Decompress(stream);
            BinaryReader reader = new BinaryReader(decompressedStream);
            SaveTag worldTag = SaveTag.ReadWithRefs(reader);
            var world = new StaticWorld(worldTag["World"]);
            map = world.Map;
        }

        static public void Delete(FileInfo save, Action callback = null)
        {
            var msgbox = new MessageBox("", $"Delete {save.Name}?", () => delete(save, callback));
            msgbox.ShowDialog();

            static void delete(FileInfo save, Action callback)
            {
                save.Delete(); 
                callback?.Invoke();
            }
        }
    }
}
