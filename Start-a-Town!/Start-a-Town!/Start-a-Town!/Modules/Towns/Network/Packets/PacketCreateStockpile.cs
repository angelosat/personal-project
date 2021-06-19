using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class PacketCreateStockpile : Packet
    {
        public int EntityID;
        public int StockpileID;
        public Vector3 Begin;
        public int Width, Height;

        public PacketCreateStockpile(int entityID, int stockpileID, Vector3 begin, int w, int h)
        {
            this.EntityID = entityID;
            this.StockpileID = stockpileID;
            this.Begin = begin;
            this.Width = w;
            this.Height = h;
        }
        //public PacketCreateStockpile(int entityID, Stockpile stockpile)
        //{
        //    this.EntityID = entityID;
        //    this.StockpileID = stockpile.ID;
        //    this.Begin = stockpile.Begin;
        //    this.Width = stockpile.Width;
        //    this.Height = stockpile.Height;
        //}
        public PacketCreateStockpile(BinaryReader r)
        {
            this.EntityID = r.ReadInt32();
            this.StockpileID = r.ReadInt32();
            this.Begin = r.ReadVector3();
            this.Width = r.ReadInt32();
            this.Height = r.ReadInt32();
        }
        public override byte[] Write()
        {
            //byte[] data = BitConverter.GetBytes((int)TownsPacketHandler.Channels.Stockpile);
            //data.Concat(BitConverter.GetBytes(this.EntityID));
            //data.Concat(this.Begin.GetBytes());
            //data.Concat(BitConverter.GetBytes(this.Width));
            //data.Concat(BitConverter.GetBytes(this.Height));

            //byte[] data = BitConverter.GetBytes((int)TownsPacketHandler.Channels.Stockpile)
            //    .Concat(BitConverter.GetBytes(this.EntityID))
            //    .Concat(this.Begin.GetBytes())
            //    .Concat(BitConverter.GetBytes(this.Width))
            //    .Concat(BitConverter.GetBytes(this.Height)).ToArray();

            byte[] data = Network.Serialize(w =>
            {
                w.Write((int)TownsPacketHandler.Channels.CreateStockpile);
                w.Write(this.EntityID);
                w.Write(this.StockpileID);
                w.Write(this.Begin);
                w.Write(this.Width);
                w.Write(this.Height);
            });

            //byte[] data = BitConverter.GetBytes((int)TownsPacketHandler.Channels.Stockpile);
            //data = data.Concat(BitConverter.GetBytes(this.EntityID)).ToArray();
            //data = data.Concat(this.Begin.GetBytes()).ToArray();
            //data = data.Concat(BitConverter.GetBytes(this.Width)).ToArray();
            //data = data.Concat(BitConverter.GetBytes(this.Height)).ToArray();

            return data;
        }

        static public void Send(int creatorInstanceID, int stockpileID, Vector3 start, int width, int height)
        {
            var p = new PacketCreateStockpile(creatorInstanceID, stockpileID, start, width, height);
            Client.Instance.Send(PacketType.Towns, p.Write());
        }

        static public void Handle(Server server, BinaryReader r, Packet packet)
        {
            var p = new PacketCreateStockpile(r);
            var stockpile = new Stockpile(p.Begin, p.Width, p.Height);
            var town = server.Map.GetTown();
            town.AddStockpile(stockpile);
            //var newpacket = new PacketCreateStockpile(p.EntityID, stockpile.ID, p.Begin, p.Width, p.hei stockpile);
            p.StockpileID = stockpile.ID;
            server.Enqueue(PacketType.Towns, p.Write(), SendType.OrderedReliable);
        }

        static public void Handle(Client client, BinaryReader r, Packet packet)
        {
            var p = new PacketCreateStockpile(r);
            var stockpile = new Stockpile(p.Begin, p.Width, p.Height);
            stockpile.ID = p.StockpileID;
            var town = client.Map.GetTown();
            town.AddStockpile(stockpile);
            client.EventOccured(Components.Message.Types.StockpileCreated, stockpile);
        }
    }
}
