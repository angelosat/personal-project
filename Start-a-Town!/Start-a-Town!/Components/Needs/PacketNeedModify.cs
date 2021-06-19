using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Needs
{
    class PacketNeedModify
    {
        static public void Send(Server server, int agentID, NeedDef needDef, float value)
        {
            server.OutgoingStream.Write((int)PacketType.NeedModifyValue);
            server.OutgoingStream.Write(agentID);
            server.OutgoingStream.Write(needDef.Name);
            server.OutgoingStream.Write(value);

            //server.Enqueue(PacketType.NeedModifyValue, Network.Serialize(w =>
            //{
            //    w.Write(agentID);
            //    w.Write(needID);
            //    w.Write(value);
            //}));
        }
        static public void Receive(IObjectProvider net, BinaryReader r)// byte[] payload)
        {
                var entity = net.GetNetworkObject(r.ReadInt32());
            //var needID = (Need.Types)r.ReadInt32();
            var needName = r.ReadString();
                var needVal = r.ReadSingle();
                Components.NeedsComponent.ModifyNeed(entity, needName, needVal);
                net.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needName, needVal);
        }
    }
}
