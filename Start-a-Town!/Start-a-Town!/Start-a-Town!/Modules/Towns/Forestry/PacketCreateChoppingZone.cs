using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Forestry
{
    class PacketCreateChoppingZone : Packet
    {
        public int EntityID;
        public Vector3 Global;
        public int Width;
        public int Height;

        public PacketCreateChoppingZone(int entityID, Vector3 global, int w, int h)
        {
            // TODO: Complete member initialization
            this.EntityID = entityID;
            this.Global = global;
            this.Width = w;
            this.Height = h;
        }

        public PacketCreateChoppingZone(BinaryReader r)
        {
            this.EntityID = r.ReadInt32();
            this.Global = r.ReadVector3();
            this.Width = r.ReadInt32();
            this.Height = r.ReadInt32();
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.Global);
            w.Write(this.Width);
            w.Write(this.Height);
        }
        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.EntityID = r.ReadInt32();
            this.Global = r.ReadVector3();
            this.Width = r.ReadInt32();
            this.Height = r.ReadInt32();
        }
    }
}
