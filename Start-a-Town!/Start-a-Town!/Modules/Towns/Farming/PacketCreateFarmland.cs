using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class PacketCreateFarmland : Packet
    {
        public int EntityID;
        public int FarmlandID;
        public Vector3 Begin;
        public int Width, Height;
        public bool Remove;
        public PacketCreateFarmland(int entityID, int farmID, Vector3 begin, int w, int h, bool remove)
        {
            this.EntityID = entityID;
            this.FarmlandID = farmID;
            this.Begin = begin;
            this.Width = w;
            this.Height = h;
            this.Remove = remove;
        }
        public PacketCreateFarmland(IObjectProvider net, BinaryReader r)
        {
            this.Read(net, r);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.FarmlandID);
            w.Write(this.Begin);
            w.Write(this.Width);
            w.Write(this.Height);
            w.Write(this.Remove);
        }
        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.EntityID = r.ReadInt32();
            this.FarmlandID = r.ReadInt32();
            this.Begin = r.ReadVector3();
            this.Width = r.ReadInt32();
            this.Height = r.ReadInt32();
            this.Remove = r.ReadBoolean();
        }
        //static public void Handle(Server server, BinaryReader r, Packet packet)
        //{
        //    var p = new PacketCreateFarmland(server, r);
        //    var farm = new Farmland(p.Begin, p.Width, p.Height);
        //    var town = server.Map.GetTown();
        //}
        //static public void Handle(Client client, BinaryReader r, Packet packet)
        //{

        //}
    }
}
