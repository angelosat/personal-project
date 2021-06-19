using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.AI;

namespace Start_a_Town_.Modules.AI.Net.Packets
{
    class PacketAILogWrite
    {
        static public void Send(Server server, int agentID, string entry)
        {
            server.OutgoingStream.Write((int)PacketType.AILogWrite);
            server.OutgoingStream.Write(agentID);
            server.OutgoingStream.Write(entry);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)// byte[] payload)
        {
            //payload.Deserialize(r =>
            //{
                var entity = net.GetNetworkObject(r.ReadInt32()) as Actor;
                var entry = r.ReadString();
            entity.Log.Write(entry);
                //AILog.SyncWrite(entity, entry);
            //});
        }
    }
}
