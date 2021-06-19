using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class PacketEntityInteract
    {
        static readonly int PacketInteract;
        static PacketEntityInteract()
        {
            PacketInteract = Network.RegisterPacketHandler(Receive);
        }
        internal static void Init()
        {
        }
        internal static void EndInteraction(IObjectProvider net, GameObject entity, bool success)
        {
            var server = net as Server;
            //var w = server.GetOutgoingStream();
            var w = server.OutgoingStreamTimestamped;
            w.Write(PacketInteract);
            w.Write(entity.RefID);
            var map = net.Map;

            w.Write(false);
            w.Write(success);
        }
        internal static void Send(IObjectProvider net, GameObject entity, Interaction action, TargetArgs target)
        {
            //entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, action, target));
            
            var server = net as Server;
            //var w = server.GetOutgoingStream();
            var w = server.OutgoingStreamTimestamped;
            w.Write(PacketInteract);
            w.Write(entity.RefID);
            w.Write(true);
            target.Write(w);
            w.Write(action.GetType().FullName);
            action.Write(w);
            w.Write(entity.Global);
            w.Write(entity.Velocity);
            w.Write(entity.Direction);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var entity = net.GetNetworkObject(r.ReadInt32());
            var map = net.Map;
            if(!r.ReadBoolean())
            {
                //endinteraction
                WorkComponent.End(entity, r.ReadBoolean());
                return;
            }
            //var target = TargetArgs.Create(net, r);
            var target = TargetArgs.Read(net.Map, r);

            var action = Activator.CreateInstance(Type.GetType(r.ReadString())) as Interaction;
            action.Read(r);
            var global = r.ReadVector3();
            var velocity = r.ReadVector3();
            var dir = r.ReadVector3();
            action.Synced(net.Map);
            entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, action, target));
        }
    }
}
