using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class PlayerList
    {
        #region Events
        public event EventHandler PlayersChanged;
        public void OnPlayersChanged()
        {
            if (!PlayersChanged.IsNull())
                PlayersChanged(this, EventArgs.Empty);
        }
        #endregion

        List<PlayerData> List = new List<PlayerData>();
        public List<PlayerData> GetList()
        {
            return List.ToList();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.List.Count);
            foreach (var player in this.List)
                player.Write(writer);
        }

        static public PlayerList Read(BinaryReader reader)
        {
            PlayerList list = new PlayerList();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                list.Add(PlayerData.Read(reader));
            return list;
        }

        public void Add(PlayerData player)
        {
            this.List.Add(player);
            OnPlayersChanged();
        }
        public void Remove(PlayerData player)
        {
            this.List.Remove(player);
            OnPlayersChanged();
        }

    }

    public class PlayerData
    {
        long _packetSeq = 1;
        public long PacketSequence
        {
            get { return _packetSeq++; }
        }
        //long _orderedReliableSeq = 1;
        public long OrderedReliableSequence = 1;
        //{
        //    get { return _orderedReliableSeq++; }
        //}
        public ConcurrentQueue<Packet> Incoming = new ConcurrentQueue<Packet>();
        //public Dictionary<long, PacketMessage> WaitingForAck = new Dictionary<long, PacketMessage>();
        public ConcurrentDictionary<long, Packet> WaitingForAck = new ConcurrentDictionary<long, Packet>();
        public Queue<Packet> OrderedPackets = new Queue<Packet>();
        public UdpConnection Connection { get; set; }
        public EndPoint IP { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public int CharacterID { get; set; }
        public GameObject Character { get; set; }
        public int Ping { get; set; }
        public bool IsActive { get; set; }

        public ConcurrentQueue<Packet> OutUnreliable = new ConcurrentQueue<Packet>();
        //public ConcurrentQueue<PacketMessage> OutOrdered = new ConcurrentQueue<PacketMessage>();
        public ConcurrentQueue<Packet> OutReliable = new ConcurrentQueue<Packet>();
        //public ConcurrentQueue<PacketMessage> OutReliableOrdered = new ConcurrentQueue<PacketMessage>();

        public bool TryGetNextPacket(out Packet packet)
        {
            packet = OrderedPackets.FirstOrDefault();//.Peek();

            //if queue empty return
            if (packet.IsNull())
                return false;

            //if currently waiting for an ack for this packet, it means it has been sent at least once; return because there's an active callback chain that resends it 
            if (WaitingForAck.ContainsKey(packet.ID))
                return false;
            //this.WaitingForAck[packet.ID] = packet;
            //otherwise it means that this is a fresh reliable packet that needs to be sent
            // WHY DO I DEQUEUE IT? I NEED IT TO BE PROCESS IT, ORDERED PACKETS ARE RELIABLE IMPLICITLY
            //OrderedPackets.Dequeue();
            return true;
        }
        public bool TryGetNextPacketOld(out Packet packet)
        {
            packet = OrderedPackets.FirstOrDefault();//.Peek();

            //if queue empty return
            if (packet.IsNull())
                return false;

            //if currently waiting for an ack for this packet, it means it has been sent at least once; return because there's an active callback chain that resends it 
            if (WaitingForAck.ContainsKey(packet.ID)) 
                return false;

            //otherwise it means that this is a fresh reliable packet that needs to be sent
            // WHY DO I DEQUEUE IT? I NEED IT TO BE PROCESS IT, ORDERED PACKETS ARE RELIABLE IMPLICITLY
            OrderedPackets.Dequeue();
            return true;
        }

        public PlayerData(EndPoint ip)
        {
           // this.Outgoing = new ConcurrentQueue<PacketMessage>();
            this.CharacterID = 0;
            this.Name = ip.ToString();
            this.IP = ip;
        }

        public PlayerData(string name)
        {
            //this.Outgoing = new ConcurrentQueue<PacketMessage>();
            this.CharacterID = 0;
            this.Name = name;
        }

        static public PlayerData Read(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            int namelength = reader.ReadInt32();
            string name = Encoding.ASCII.GetString(reader.ReadBytes(namelength));
            int charID = reader.ReadInt32();
            int rtt = reader.ReadInt32();
            //GameObject charObj = charID > 0 ? GameObject.Create(reader) : null;

            return new PlayerData(name) { ID = id, CharacterID = charID, Ping = rtt};//, Character = charObj };


        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(this.ID);
            byte[] encoded = Encoding.ASCII.GetBytes(this.Name);
            writer.Write(encoded.Length);
            writer.Write(encoded);
            writer.Write(CharacterID);
            writer.Write(Ping);
            //if (CharacterID > 0)
            //    Character.Write(writer);

        }

        //static public Func<PlayerData, bool> LoadedChunksContains(GameObject entity)
        //{
        //    return (p) => Vector2.Distance(p.Character.GetChunk().MapCoords, entity.GetChunk().MapCoords) <= Engine.ChunkRadius;
        //}
        //static public Func<PlayerData, bool> IsWithin(Vector3 global, int radius = Engine.ChunkRadius)
        //{
        //    return (p) => Vector2.Distance(p.Character.GetChunk().MapCoords, global.GetChunk(p.Character.Net.Map).MapCoords) <= radius;
        //}

        //public Func<PlayerData, bool> IsWithin(Vector3 global, int radius = Engine.ChunkRadius)
        //{
        //    return (p) => Vector2.Distance(p.Character.GetChunk().MapCoords, global.GetChunk(p.Character.Net.Map).MapCoords) <= radius;
        //}
        public bool IsWithin(Vector3 global, int radius = Engine.ChunkRadius)
        {
            var playerchunk = this.Character.GetChunk();
            //var targetchunk = global.GetChunk(this.Character.Net.Map);
            var targetchunk = this.Character.Net.Map.GetChunk(global);

            return Vector2.Distance(playerchunk.MapCoords, targetchunk.MapCoords) <= radius;
        }

        //static public void Read(byte[] data, out string name)
        //{
        //    using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
        //    {
        //        int namelength = reader.ReadInt32();
        //        name = Encoding.ASCII.GetString(reader.ReadBytes(namelength));
        //    }
        //}

        //public void SendOutgoing(Socket socket)
        //{
        //    PacketMessage packet;
        //        if (this.Outgoing.TryDequeue(out packet))
        //        {
        //            if (packet.Retries > 0)
        //            {
        //                int rtt = 5000;
        //                BeginReliableSend(packet, rtt);
        //            }
        //            if (packet.SendType == SendType.Ordered)
        //                packet.Player.OrderedPackets.Enqueue(packet);
        //            else
        //            {
        //                packet.BeginSendTo(socket, packet.Player.IP, ar =>
        //                {
        //                    // WHY CALL TO SEND AGAIN IF I HAVE A TIMER THAT SENDS?
        //                    // BECAUSE: in case the queue is empty, the callback chain breaks and i need a loop that checks for new outgoing packets
        //                    SendOutgoing(socket);
        //                });
        //            }
        //        }
        //}
        //void BeginReliableSend(Socket socket, PacketMessage packet, int rtt)
        //{
        //    //packet.Player.WaitingForAck.Add(packet.ID, packet); // TODO: figure out why it threw null reference exception //maybe because it tried again?
        //    this.WaitingForAck[packet.ID] = packet;
        //    packet.RTT.Restart();


        //    Timer timer = new Timer(a =>
        //    {
        //        PacketMessage p = (PacketMessage)a;
        //        //if ack already received, stop timer;
        //        //if (!WaitingForAck.Contains(p.ID))
        //        if (!packet.Player.WaitingForAck.ContainsKey(p.ID))
        //        {
        //            p.ResendTimer.Change(Timeout.Infinite, Timeout.Infinite);
        //            //  ("ack received for packet " + p).ToConsole();
        //            Server.Console.Write(UI.ConsoleMessageTypes.Acks, Color.Lime, "SERVER", "Ack received for packet " + p);
        //            return;
        //        }
        //        if (--p.Retries < 0)
        //        {
        //            p.ResendTimer.Change(Timeout.Infinite, Timeout.Infinite);
        //            packet.Player.WaitingForAck.Remove(p.ID);
        //            //if (!WaitingForAck.TryRemove(p.ID, out p))
        //            //    throw new Exception("packet doesn't exist");

        //            //  ("maximum send retries exceeded for packet " + p).ToConsole();
        //            Server.Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Send attempts exceeded maximum for packet " + p);
        //            Server.Console.Write(Color.Red, "SERVER", p.Player.Name + " timed out");
        //            CloseConnection(p.Player.Connection);
        //            return;
        //        }
        //        //("resending packet " + p).ToConsole();
        //        Server.Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Resending packet " + p);
        //        packet.BeginSendTo(socket, packet.Player.IP);
        //    }, packet, 0, rtt);
        //    packet.ResendTimer = timer;
        //}

    }

}
