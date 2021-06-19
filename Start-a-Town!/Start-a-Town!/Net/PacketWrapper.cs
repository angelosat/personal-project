using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Net
{
    class PacketWrapper
    {
        public const int Size = 65535;
        public List<Packet> Messages;

        PacketWrapper()
        {
            this.Messages = new List<Packet>();
        }
        PacketWrapper(params Packet[] messages)
        {
            this.Messages = new List<Packet>(messages);
        }
        PacketWrapper(IEnumerable<Packet> messages)
        {
            this.Messages = new List<Packet>(messages);
        }
        static public PacketWrapper Create(params Packet[] messages)
        {
            return new PacketWrapper(messages);
        }
        static public PacketWrapper Create(IEnumerable<Packet> messages)
        {
            return new PacketWrapper(messages);
        }

        public void BeginSendTo(Socket socket, EndPoint ip)
        {
            this.BeginSendTo(socket, ip, ar => { });
        }
        public void BeginSendTo(Socket socket, EndPoint ip, AsyncCallback callback)
        {
            var segments = new List<byte[]>();
            segments.Add(new Action<BinaryWriter>(w => w.Write(this.Messages.Count)).GetBytes());
            foreach (var msg in this.Messages)
                segments.Add(msg.ToArray());
            var fullMsg = new List<byte>();
            segments.ForEach(s => fullMsg.AddRange(s));
            if (fullMsg.Count > Size)
                throw new Exception("Maximum packet size exceeded");
            socket.BeginSendTo(fullMsg.ToArray(), 0, fullMsg.Count, SocketFlags.None, ip, a =>
            {
                socket.EndSend(a);
                callback(a);
            }, socket);
        }

        static public PacketWrapper Create(byte[] data)
        {
            var packet = new PacketWrapper();
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                int msgCount = reader.ReadInt32();
                if (msgCount == 0)
                    throw new Exception("Received empty packet");
                for (int i = 0; i < msgCount; i++)
                    packet.Messages.Add(Packet.Read(reader));
            }
            return packet;
        }
    }
}
