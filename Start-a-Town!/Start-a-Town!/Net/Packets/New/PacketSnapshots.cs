using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketSnapshots
    {
        static public void Init()
        {
            Server.RegisterPacketHandler(PacketType.Snapshot, Receive);
            Client.RegisterPacketHandler(PacketType.Snapshot, Receive);
        }
        static public void Send(IObjectProvider net, IEnumerable<GameObject> entities)
        {
            var server = net as Server;
            var strem = server.OutgoingStreamUnreliable;
            strem.Write((int)PacketType.Snapshot);
            strem.Write(Server.ServerClock.TotalMilliseconds);
            strem.Write(entities.Count());
            foreach (var obj in entities)
            {
                strem.Write(obj.RefID);
                ObjectSnapshot.Write(obj, strem);
            }
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var client = net as Client;
            //client.ReadSnapshot(msg.Decompressed);
            client.ReadSnapshot(r);
        }
    }
}
