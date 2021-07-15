using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketSnapshots
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, IEnumerable<GameObject> entities)
        {
            var server = net as Server;
            var strem = server.OutgoingStreamUnreliable;
            strem.Write(p);
            strem.Write(Server.ServerClock.TotalMilliseconds);
            strem.Write(entities.Count());
            foreach (var obj in entities)
            {
                strem.Write(obj.RefID);
                ObjectSnapshot.Write(obj, strem);
            }
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            var client = net as Client;
            client.ReadSnapshot(r);
        }
    }
}
