using System.IO;

namespace Start_a_Town_
{
    static class BlockHelper
    {
        public static void Write(this BinaryWriter w, Block block)
        {
            w.Write(block.Hash);
        }
        public static Block ReadBlock(this BinaryReader r)
        {
            return Block.GetBlock(r.ReadInt32());
        }
        public static void Save(this SaveTag tag, Block block, string name)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, name, block.Hash));
        }
        public static Block LoadBlock(this SaveTag tag, string name)
        {
            return Block.GetBlock((int)tag[name].Value);
        }
    }
}
