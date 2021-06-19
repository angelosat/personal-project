using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityRequestSpawn
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.SpawnEntity, Receive);
            Server.RegisterPacketHandler(PacketType.SpawnEntityFromTemplate, ReceiveTemplate);
            //Client.Instance.RegisterPacketHandler(PacketType.SpawnEntity, Receive);
        }
        internal static void SendTemplate(IObjectProvider net, int templateID, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.SpawnEntityFromTemplate);
            w.Write(templateID);
            target.Write(w);
        }
        internal static void Send(IObjectProvider net, int entityType, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.SpawnEntity);
            w.Write(entityType);
            target.Write(w);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var server = net as Server;
            var type = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            server.SpawnRequest(type, target);
            //net.DisposeObject(id);
            //if (net is Server)
            //    Send(net, id);
        }
        internal static void ReceiveTemplate(IObjectProvider net, BinaryReader r)
        {
            var server = net as Server;
            var templateID = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            server.SpawnRequestFromTemplate(templateID, target);
            //net.DisposeObject(id);
            //if (net is Server)
            //    Send(net, id);
        }
        
    }
}
