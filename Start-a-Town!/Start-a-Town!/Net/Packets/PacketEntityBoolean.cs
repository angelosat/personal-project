using System;
using System.IO;

namespace Start_a_Town_.Net.Packets
{
    [Obsolete]
    class PacketEntityBoolean : Packet
    {
        public int EntityID;
        public bool Value;

        public PacketEntityBoolean(int entityID, bool value)
        {
            this.EntityID = entityID;
            this.Value = value;
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.Value);
        }
    }
}
