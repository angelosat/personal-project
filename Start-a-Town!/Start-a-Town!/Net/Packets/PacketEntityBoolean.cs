using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net.Packets
{
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
