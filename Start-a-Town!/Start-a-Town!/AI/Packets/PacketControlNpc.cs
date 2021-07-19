using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketControlNpc
    {
        static readonly int PType;
        static PacketControlNpc()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        internal static void Init() { }
        internal static void Send(INetwork net, int entityid)
        {
            if (net is Server)
                throw new Exception();
            var plid = net.GetPlayer().ID;
            Send(net, plid, entityid);
        }
        internal static void Send(INetwork net, int playerid, int entityid)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(playerid);
            w.Write(entityid);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var player = net.GetPlayer(r.ReadInt32());
            var entityid = r.ReadInt32();
            var entity = entityid != -1 ? net.GetNetworkObject(entityid) as Actor : null;
            if (entity?.IsPlayerControlled ?? false)
                return;
            var lastEntity = player.ControllingEntity;
            player.ControllingEntity = entity;
            net.EventOccured(Components.Message.Types.PlayerControlNpc, player, entity, lastEntity);

            if (entity is not null)
                net.Write($"{player.Name} is controlling {entity.Name}");
            else
                net.Write($"{player.Name} no longer controlling {lastEntity.Name}");

            if (net is Server)
            {
                if (lastEntity is not null)
                    lastEntity.GetComponent<AIComponent>().Enabled = true;
                if (entity is not null)
                    entity.GetComponent<AIComponent>().Enabled = false;
                Send(net, player.ID, entityid);
            }
        }
    }
}
