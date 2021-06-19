using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    class PacketDeleteStockpile : Packet
    {
        public int StockpileID;

        public PacketDeleteStockpile(int stockpileID)
        {
            this.StockpileID = stockpileID;
        }
        public PacketDeleteStockpile(BinaryReader r)
        {
            this.StockpileID = r.ReadInt32();
        }
        public override byte[] Write()
        {
            //byte[] data = Network.Serialize(w =>
            //{
            //    w.Write((int)TownsPacketHandler.Channels.DeleteStockpile);
            //    w.Write(this.StockpileID);
            //});

            byte[] data = BitConverter.GetBytes((int)TownsPacketHandler.Channels.DeleteStockpile)
                .Concat(BitConverter.GetBytes(this.StockpileID)).ToArray().Compress();

            return data;
        }

        static public void Handle(Server server, BinaryReader r, Packet packet)
        {
            var packetDeleteStockpile = new PacketDeleteStockpile(r);
            if (server.Map.GetTown().DeleteStockpile(packetDeleteStockpile.StockpileID))
                server.Enqueue(PacketType.Towns, packet.Payload, SendType.OrderedReliable);
        }

        static public void Handle(Client client, BinaryReader r, Packet packet)
        {
            var packetDeleteStockpile = new PacketDeleteStockpile(r);
            Stockpile stockpile;
            //if(client.Map.GetTown().DeleteStockpile(packetDeleteStockpile.StockpileID, out stockpile))
            //    client.EventOccured(Components.Message.Types.StockpileDeleted, stockpile);
            client.Map.GetTown().DeleteStockpile(packetDeleteStockpile.StockpileID, out stockpile);
        }
    }
}
