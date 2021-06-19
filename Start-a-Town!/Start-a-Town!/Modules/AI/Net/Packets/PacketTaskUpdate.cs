using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //server.Enqueue(PacketType.AITaskUpdate, Network.Serialize(w =>
            //{
            //    w.Write(agentID);
            //    w.Write(taskString);
            //}));
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            //payload.Deserialize(r =>
            //{
                var entity = net.GetNetworkObject(r.ReadInt32());
                if (entity == null)
                    return;
                var taskString = r.ReadString();
                AIState.GetState(entity).TaskString = taskString;
                //Components.NeedsComponent.ModifyNeed(entity, needID, needVal);
                //net.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needID, needVal);
            //});
        }
    }
}
