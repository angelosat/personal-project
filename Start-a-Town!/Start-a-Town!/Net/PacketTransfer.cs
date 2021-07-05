using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Net
{
    class PacketTransfer
    {
        class PacketPartial
        {
            PacketType Type;

            Dictionary<int, byte[]> Segments;
            //public PacketPartial(long packetSequenceID, PacketType type, int partCount)
            public PacketPartial(PacketType type, int partCount)
            {
                //this.PacketSequenceID = packetSequenceID;
                this.Type = type;
                this.Segments = new Dictionary<int, byte[]>();
                for (int i = 0; i < partCount; i++)
                {
                    this.Segments.Add(i, null);
                }
            }
            public bool Receive(int seg, byte[] data)
            {
                if(!this.Segments.ContainsKey(seg))
                    throw new Exception("Invalid segment index");//Segment already exists")
                if (this.Segments[seg] != null)
                    throw new Exception("Segment already received");
                this.Segments[seg] = data;
                return this.Segments.Values.All(value => !value.IsNull());
            }
            byte[] GetData()
            {
                byte[] fulldata = new byte[0];
                for (int i = 0; i < this.Segments.Count; i++)
                {
                    fulldata.Concat(this.Segments[i]);
                }
                return fulldata;
            }
            public Packet GetPacket(long id)
            {
                byte[] data = GetData();
                Packet packet = new Packet(id, this.Type, data.Length, data);
                return packet;
            }
        }
        Dictionary<long, PacketPartial> PartialPackets = new Dictionary<long, PacketPartial>();
        Action<Packet> Callback;
        public PacketTransfer(Action<Packet> callback)
        {
            this.Callback = callback;
        }
        public void Receive(Packet packet)
        {
            Network.Deserialize(packet.Payload, r=>{
                PacketType type = (PacketType)r.ReadInt32();
                int partialIndex = r.ReadInt32();
                int segmentCount = r.ReadInt32();
                int segmentIndex = r.ReadInt32();
                int dataLength = r.ReadInt32();
                byte[] data = r.ReadBytes(dataLength);

                PacketPartial partial;
                if (!this.PartialPackets.TryGetValue(partialIndex, out partial))
                {
                    partial = new PacketPartial(type, segmentCount);
                    this.PartialPackets[partialIndex] = partial;//new PacketPartial(packetID, type, segmentCount);
                }
                if(partial.Receive(segmentIndex, data))
                {
                    this.PartialPackets.Remove(partialIndex);
                    this.Callback(partial.GetPacket(partialIndex));
                }
            });
        }
        static public List<byte[]> Split(long id, byte[] payload)
        {
            int length = payload.Length;
            int length1 = length / 2;
            int length2 = length - length1;
            byte[] data1 = new byte[length1];
            byte[] data2 = new byte[length2];
            Array.Copy(payload, data1, length1);
            Array.Copy(payload, length1, data2, 0, length2);
            byte[] payload1 = Network.Serialize(w =>
            {
                w.Write(id);
                w.Write(2);
                w.Write(0);
                w.Write(data1.Length);
                w.Write(data1);
            });
            byte[] payload2 = Network.Serialize(w =>
            {
                w.Write(id);
                w.Write(2);
                w.Write(1);
                w.Write(data2.Length);
                w.Write(data2);
            });
            return new List<byte[]>() { payload1, payload2 };
            //PacketMessage pack1 = new PacketMessage(id1, PacketType.Partial, payload1.Length, payload1);
            //PacketMessage pack2 = new PacketMessage(id2, PacketType.Partial, payload2.Length, payload2);
            //return new List<PacketMessage>() { pack1, pack2 };
            //PacketMessage packet1 = new PacketMessage()
        }
        static public List<byte[]> Split(Packet packet, long id1, long id2)
        {
            int length = packet.Payload.Length;
            int length1 = length / 2;
            int length2 = length - length1;
            byte[] data1 = new byte[length1];
            byte[] data2 = new byte[length2];
            Array.Copy(packet.Payload, data1, length1);
            Array.Copy(packet.Payload, length1, data2, 0, length2);
            byte[] payload1 = Network.Serialize(w =>
            {
                w.Write(packet.ID);
                w.Write(2);
                w.Write(0);
                w.Write(data1.Length);
                w.Write(data1);
            });
            byte[] payload2 = Network.Serialize(w =>
            {
                w.Write(packet.ID);
                w.Write(2);
                w.Write(1);
                w.Write(data2.Length);
                w.Write(data2);
            });
            return new List<byte[]>() { payload1, payload2 };
            //PacketMessage pack1 = new PacketMessage(id1, PacketType.Partial, payload1.Length, payload1);
            //PacketMessage pack2 = new PacketMessage(id2, PacketType.Partial, payload2.Length, payload2);
            //return new List<PacketMessage>() { pack1, pack2 };
            //PacketMessage packet1 = new PacketMessage()
        }
        //static public PacketTransfer BeginReceive(int partCount, Action<Packet> callback)
        //{
        //    return new PacketTransfer(partCount, callback);
        //}
        //public PacketTransfer(int partCount)
        //{
        //    this.Partial = new PacketPartial(partCount);
        //}
        //public PacketTransfer(int partCount, Action<Packet> callback)
        //{
        //    this.Partial = new PacketPartial(partCount);
        //    this.Callback = callback;
        //}
    }
}
