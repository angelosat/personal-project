﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net.Packets
{
    class PacketEntityVector3 : Packet
    {
        public int EntityID;
        public Vector3 Vector;
        public PacketEntityVector3()
        {
            this.PacketType = Net.PacketType.Chunk;
        }
        public PacketEntityVector3(byte[] data)
            : this()
        {

        }
        public PacketEntityVector3(int entityID, Vector3 direction)
        {
            this.EntityID = entityID;
            this.Vector = direction;
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.Vector);
        }

        //public override byte[] Write()
        //{
        //    return Write(this.EntityID);
        //}
        //public override void Read(byte[] data)
        //{
        //    data.Translate(r => this.EntityID = r.ReadInt32());
        //}
        //static public byte[] Write(int entityID)
        //{
        //    return BitConverter.GetBytes(entityID);
        //}
        //static public void Write(BinaryWriter io, int entityID)
        //{
        //    io.Write(entityID);
        //}
        //static public void Read(BinaryReader io, out int entityID)
        //{
        //    entityID = io.ReadInt32();
        //}
    }
}
