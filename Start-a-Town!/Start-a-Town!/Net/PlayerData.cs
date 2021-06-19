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
    
    public class PlayerData
    {
        public Vector2 MousePosition;
        long _packetSeq = 1;
        public long PacketSequence
        {
            get { return _packetSeq++; }
        }
        //long _orderedReliableSeq = 1;
        public long OrderedReliableSequence = 1;
        public Color Color;
        public ConcurrentQueue<Packet> Incoming = new ConcurrentQueue<Packet>();
        //public Dictionary<long, PacketMessage> WaitingForAck = new Dictionary<long, PacketMessage>();
        public ConcurrentDictionary<long, Packet> WaitingForAck = new ConcurrentDictionary<long, Packet>();
        public Queue<Packet> OrderedPackets = new Queue<Packet>();
        public UdpConnection Connection;
        public EndPoint IP;
        public int ID;
        public string Name;
        public int CharacterID;
        public Actor ControllingEntity;
        public int Ping;
        public bool IsActive;
        //public Queue<Chunk> PendingChunks = new Queue<Chunk>();
        public Dictionary<Vector2, byte[]> PendingChunks = new Dictionary<Vector2, byte[]>();
        public HashSet<Vector2> SentChunks = new HashSet<Vector2>();
        public Vector2 CameraPosition;
        public ControlTool CurrentTool = ToolManager.GetDefaultTool();
        public float CameraZoom;
        public TargetArgs Target = TargetArgs.Null;
        public Vector2? LastPointer; // dont store this in the player class?

        //public TargetArgs[] LastTargets = new TargetArgs[2] { TargetArgs.Null, TargetArgs.Null };
        public int SuggestedSpeed = 1;
        static readonly Random Random = new Random();
        public ConcurrentQueue<Packet> OutUnreliable = new();
        //public ConcurrentQueue<PacketMessage> OutOrdered = new ConcurrentQueue<PacketMessage>();
        public ConcurrentQueue<Packet> OutReliable = new();

        //public ConcurrentQueue<PacketMessage> OutReliableOrdered = new ConcurrentQueue<PacketMessage>();

        public bool TryGetNextPacket(out Packet packet)
        {
            packet = OrderedPackets.FirstOrDefault();//.Peek();

            //if queue empty return
            if (packet == null)
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
            if (packet == null)
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
            this.Color = Random.GetColor();
        }

        public PlayerData(string name)
        {
            //this.Outgoing = new ConcurrentQueue<PacketMessage>();
            this.CharacterID = 0;
            this.Name = name;
            this.Color = Random.GetColor();
        }

        static public PlayerData Read(BinaryReader reader)
        {
            int id = reader.ReadInt32();
            int namelength = reader.ReadInt32();
            string name = Encoding.ASCII.GetString(reader.ReadBytes(namelength));
            int charID = reader.ReadInt32();
            int rtt = reader.ReadInt32();
            //GameObject charObj = charID > 0 ? GameObject.Create(reader) : null;
            var speed = reader.ReadInt32();
            var col = reader.ReadColor();
            return new PlayerData(name) { ID = id, CharacterID = charID, Ping = rtt, SuggestedSpeed = speed, Color = col };//, Character = charObj };
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            byte[] encoded = Encoding.ASCII.GetBytes(this.Name);
            w.Write(encoded.Length);
            w.Write(encoded);
            w.Write(CharacterID);
            w.Write(Ping);
            w.Write(this.SuggestedSpeed);
            w.Write(this.Color);
            //if (CharacterID > 0)
            //    Character.Write(writer);

        }
        public bool IsWithin(Vector3 global, int radius = Engine.ChunkRadius)
        {
            return GameModes.GameMode.Current.IsPlayerWithinRangeForPacket(this, global);

            //var playerchunk = this.ControllingEntity.GetChunk();
            ////var targetchunk = global.GetChunk(this.Character.Net.Map);
            //var targetchunk = this.ControllingEntity.Net.Map.GetChunk(global);

            //return Vector2.Distance(playerchunk.MapCoords, targetchunk.MapCoords) <= radius;
        }
        public Vector2 GetMousePosition(Camera camera)
        {
            throw new Exception();
            //return (UI.UIManager.Size / 2 + (this.CameraPosition - camera.Coordinates + this.MousePosition / this.CameraZoom) * camera.Zoom).Floor();
        }
        static public Vector2 GetMousePosition(Vector2 cameraPos, Vector2 mousePos, float zoom, Camera camera)
        {
            throw new Exception();
            //return (UI.UIManager.Size / 2 + (cameraPos - camera.Coordinates + mousePos / zoom) * camera.Zoom).Floor();
        }


        internal void UpdateTarget(TargetArgs target)
        {
            this.Target = target;
        }
    }
}
