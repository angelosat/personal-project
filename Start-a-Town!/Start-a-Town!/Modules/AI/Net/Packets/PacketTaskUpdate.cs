using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.AI;

namespace Start_a_Town_.Modules.AI.Net
{
    class PacketTaskUpdate
    {
        static public void Send(Server server, int agentID, string taskString)
        {
            server.OutgoingStream.Write((int)PacketType.AITaskUpdate);
            server.OutgoingStream.Write(agentID);
            server.OutgoingStream.Write(taskString);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var entity = net.GetNetworkObject(r.ReadInt32());
            if (entity == null)
                return;
            var taskString = r.ReadString();
            AIState.GetState(entity).TaskString = taskString;
        }
    }
}
