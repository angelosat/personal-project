using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI
{
    class AIPacketHandler : IServerPacketHandler, IClientPacketHandler
    {
        public enum Channels { Command, DialogueOption }

        //public static void AIFollowEntity(GameObject ai, GameObject entity)
        //{

        //}

        public void HandlePacket(Server server, Packet packet)
        {
            packet.Payload.Deserialize(r =>
            {
                var channel = (Channels)r.ReadInt32();
                switch (channel)
                {
                    case Channels.Command:
                        var entityID = r.ReadInt32();
                        var action = r.ReadString();
                        var target = TargetArgs.Read(server, r);
                        //server.GetNetworkObject(entityID).PostMessageLocal()
                        server.PostLocalEvent(server.GetNetworkObject(entityID), Components.Message.Types.AICommand, action, target);
                        break;

                    case Channels.DialogueOption:
                        var speakerID = r.ReadInt32();
                        var targetID = r.ReadInt32();
                        var dialogOption = r.ReadString();
                        var speaker = server.GetNetworkObject(speakerID);
                        server.PostLocalEvent(server.GetNetworkObject(targetID), Components.Message.Types.DialogueOption,speaker , dialogOption);
                        server.Enqueue(packet.PacketType, packet.Payload, SendType.OrderedReliable, speaker.Global);
                        break;

                    default:
                        //this.Handle(server, packet);
                        break;
                }
            });
        }

        public void HandlePacket(Client client, Packet packet)
        {
            packet.Payload.Deserialize(r =>
            {
                var channel = (Channels)r.ReadInt32();
                switch (channel)
                {
                    //case Channels.Command:
                    //    var entityID = r.ReadInt32();
                    //    var action = r.ReadString();
                    //    var target = TargetArgs.Read(server, r);
                    //    //server.GetNetworkObject(entityID).PostMessageLocal()
                    //    server.PostLocalEvent(server.GetNetworkObject(entityID), Components.Message.Types.AICommand, action, target);
                    //    break;

                    case Channels.DialogueOption:
                        var speakerID = r.ReadInt32();
                        var targetID = r.ReadInt32();
                        var dialogOption = r.ReadString();
                        client.PostLocalEvent(client.GetNetworkObject(targetID), Components.Message.Types.DialogueOption, client.GetNetworkObject(speakerID), dialogOption);
                        break;

                    default:
                        //this.Handle(client, packet);
                        break;
                }
            });
        }

        //public void Handle(IObjectProvider net, Packet p)
        //{
        //    AI.AIManager.Handle(net as Client, p);
        //}
    }
}
