using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    // TODO: register all and make a factory
    public abstract class PacketTranslator
    {
        public enum Types : byte
        {
            Byte,
            Short,
            Int32,
            Long,
            Single,
            Double,
            ByteArray,
            String,
            UInt16,
            UInt32,
            Bool,
            Vector2,
            Vector3
        }

        public abstract PacketTranslator Translate(IObjectProvider objProvider, byte[] data);
        public abstract PacketTranslator Translate(IObjectProvider objProvider, object[] parameters);
    }
}
