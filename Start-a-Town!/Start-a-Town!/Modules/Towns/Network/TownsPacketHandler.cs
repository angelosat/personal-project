using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Housing;

namespace Start_a_Town_.Towns
{
    [Obsolete]
    class TownsPacketHandler : IServerPacketHandler, IClientPacketHandler
    {
        public enum Channels { CreateStockpile = 1, DeleteStockpile, AddJob, PlayerCreateHouse, AddHouse, PlayerRenameHouse, PlayerRemoveHouse }

        delegate void ServerHandler(Server server, BinaryReader r, Packet p);
        delegate void ClientHandler(Client client, BinaryReader r, Packet p);

        Dictionary<Channels, ServerHandler> ServerHandlers = new Dictionary<Channels, ServerHandler>()
        {
        };
        Dictionary<Channels, ClientHandler> ClientHandlers = new Dictionary<Channels, ClientHandler>()
        {
        };

        internal static void PlayerCreateHouse(string name, Vector3 vector3)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write((int)Channels.PlayerCreateHouse);
                w.Write(PlayerOld.Actor.RefID);
                w.Write(name);
                w.Write(vector3);
            });
            Client.Instance.Send(PacketType.Towns, data);
        }
        internal static void PlayerRenameHouse(House house, string name)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write((int)Channels.PlayerRenameHouse);
                w.Write(PlayerOld.Actor.RefID);
                w.Write(house.ID);
                w.Write(name);
            });
            Client.Instance.Send(PacketType.Towns, data);
        }
        internal static void PlayerRemoveHouse(House house)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write((int)Channels.PlayerRemoveHouse);
                w.Write(PlayerOld.Actor.RefID);
                w.Write(house.ID);
            });
            Client.Instance.Send(PacketType.Towns, data);
        }

        public void Send(Packet packet)
        {
            byte[] data = packet.Write();
            Client.Instance.Send(PacketType.Towns, data);
        }
        

        public void HandlePacket(Server server, Packet packet)
        {
            packet.Payload.Deserialize(r =>
            {
                Channels channel = (Channels)r.ReadInt32();
                switch (channel)
                {
                    case Channels.AddJob:
                        var addJob = new PacketAddJob();
                        addJob.Read(server, r);
                        var interaction = addJob.Target.GetInteraction(server, addJob.InteractionName);
                        if (interaction == null)
                            return;
                        server.Map.GetTown().AddJob(new AI.AIJob(new AI.AIInstruction(addJob.Target, interaction)));
                        break;

                    case Channels.PlayerCreateHouse:
                        var actor = server.GetNetworkObject(r.ReadInt32());
                        var name = r.ReadString();
                        var house = House.FloodFill(server.Map, r.ReadVector3());
                        if (house == null)
                            break;
                        house.Name = string.IsNullOrWhiteSpace(name) ? house.Name : name;
                        server.Map.GetTown().AddHouse(house);

                        byte[] data = Network.Serialize(w =>
                            {
                                w.Write((int)Channels.AddHouse);
                                house.Write(w);
                            });
                        server.Enqueue(PacketType.Towns, data, SendType.OrderedReliable);
                        break;

                    case Channels.PlayerRenameHouse:
                        actor = server.GetNetworkObject(r.ReadInt32());
                        var houseID = r.ReadInt32();
                        name = r.ReadString();
                        // TODO: validate name
                        if (server.Map.GetTown().RenameHouse(houseID, name))
                            server.Enqueue(PacketType.Towns, packet.Payload, SendType.OrderedReliable);
                        break;

                    case Channels.PlayerRemoveHouse:
                        actor = server.GetNetworkObject(r.ReadInt32());
                        houseID = r.ReadInt32();
                        if (server.Map.GetTown().RemoveHouse(houseID))
                            server.Enqueue(PacketType.Towns, packet.Payload, SendType.OrderedReliable);
                        break;

                    default:
                        ServerHandler handler;
                        if (ServerHandlers.TryGetValue(channel, out handler))
                            handler(server, r, packet);
                        break;
                }
            });
        }

        public void HandlePacket(Client client, Packet packet)
        {
            BinaryReader r = new BinaryReader(new MemoryStream(packet.Decompressed));
            Channels channel = (Channels)r.ReadInt32();
            switch (channel)
            {
                case Channels.AddJob:
                    var addJob = new PacketAddJob();
                    addJob.Read(client, r);
                    var interaction = addJob.Target.GetInteraction(client, addJob.InteractionName);
                    if (interaction == null)
                        return;
                    client.Map.GetTown().AddJob(new AI.AIJob(new AI.AIInstruction(addJob.Target, interaction)));
                    break;

                case Channels.AddHouse:
                    var house = new House(client.Map.GetTown(), r);
                    client.Map.GetTown().AddHouse(house);
                    Client.Instance.Log.Write(house.Name + " created");
                    break;

                case Channels.PlayerRenameHouse:
                    var actor = client.GetNetworkObject(r.ReadInt32());
                    var houseID = r.ReadInt32();
                    // TODO: validate name
                    var name = r.ReadString();
                    var oldName = client.Map.GetTown().GetHouse(houseID).Name;
                    if (client.Map.GetTown().RenameHouse(houseID, name))
                        Client.Instance.Log.Write(oldName + " renamed to " + name);
                    break;

                case Channels.PlayerRemoveHouse:
                    actor = client.GetNetworkObject(r.ReadInt32());
                    houseID = r.ReadInt32();
                    if (client.Map.GetTown().RemoveHouse(houseID, out house))
                        Client.Instance.Log.Write(house.Name + " removed");
                    break;

                default:
                    ClientHandler handler;
                    if (ClientHandlers.TryGetValue(channel, out handler))
                        handler(client, r, packet);
                    break;
            }
        }
    }
}
