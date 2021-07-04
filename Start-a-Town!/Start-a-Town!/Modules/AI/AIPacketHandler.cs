using Start_a_Town_.Net;
using System;

namespace Start_a_Town_.AI
{
    [Obsolete]
    class AIPacketHandler : IServerPacketHandler, IClientPacketHandler
    {
        public enum Channels { Command, DialogueOption }

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
                    case Channels.DialogueOption:
                        var speakerID = r.ReadInt32();
                        var targetID = r.ReadInt32();
                        var dialogOption = r.ReadString();
                        client.PostLocalEvent(client.GetNetworkObject(targetID), Components.Message.Types.DialogueOption, client.GetNetworkObject(speakerID), dialogOption);
                        break;

                    default:
                        break;
                }
            });
        }
    }
}
