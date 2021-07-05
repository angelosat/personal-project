using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketControlNpc
    {
        static int PType;
        internal static void Init()
        {
            // TODO: update
            PType = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(IObjectProvider net, int entityid)
        {
            if (net is Server)
                throw new Exception();
            var plid = net.GetPlayer().ID;
            Send(net, plid, entityid);
        }
        internal static void Send(IObjectProvider net, int playerid, int entityid)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(playerid);
            w.Write(entityid);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var player = net.GetPlayer(r.ReadInt32());
            var entityid = r.ReadInt32();
            var entity = entityid != -1 ? net.GetNetworkObject(entityid) as Actor : null;
            if (entity?.IsPlayerControlled ?? false)
                return;
            var lastEntity = player.ControllingEntity;
            player.ControllingEntity = entity;
            net.EventOccured(Components.Message.Types.PlayerControlNpc, player, entity, lastEntity);

            if (entity != null)
                net.Write(string.Format("{0} is controlling over {1}", player.Name, entity.Name));
            else
                net.Write(string.Format("{0} no longer controlling {1}", player.Name, lastEntity.Name));

            if (net is Server)
            {
                if (lastEntity != null)
                    lastEntity.GetComponent<AIComponent>().Enabled = true;
                if (entity != null)
                    entity.GetComponent<AIComponent>().Enabled = false;
                Send(net, player.ID, entityid);
            }
        }
    }
}
