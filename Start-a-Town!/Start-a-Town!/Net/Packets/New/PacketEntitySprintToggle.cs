using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntitySprintToggle
    {
        static readonly PacketType PType = PacketType.EntitySprintToggle;
        internal static void Init()
        {
            Server.RegisterPacketHandler(PType, Receive);
            Client.RegisterPacketHandler(PType, Receive);
        }
        internal static void Send(IObjectProvider net, int entityID, bool toggle)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.SprintToggle(toggle);

            if (net is Server)
                Send(net, entity.RefID, toggle);
        }
    }
}
