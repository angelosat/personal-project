using System.IO;
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

        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var entity = net.GetNetworkObject(r.ReadInt32());
            var needName = r.ReadString();
            var needVal = r.ReadSingle();
            Components.NeedsComponent.ModifyNeed(entity, needName, needVal);
            net.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needName, needVal);
        }
    }
}
