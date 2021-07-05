using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public partial class Server
    {
        class ServerCommandParser
        {
            readonly Server Server;
            public ServerCommandParser(Server server)
            {
                this.Server = server;
            }
                public void Command(string command)
            {
                var queue = new Queue<string>(command.Split(' '));
                try
                {
                    switch (queue.Dequeue())
                    {
                        case "hello":
                            Server.Log.Write("SERVER", "how are you?");
                            break;

                        case "loadworld":
                            string worldName = queue.Dequeue();
                            break;

                        case "unloadworld":
                            UnloadWorld();
                            break;

                        case "broadcast":
                            string message = queue.Dequeue();
                            byte[] data = Network.Serialize(w => w.WriteASCII(message));
                            foreach (var p in Instance.Players.GetList())
                                Instance.Enqueue(p, Packet.Create(p, PacketType.ServerBroadcast, data));

                            Server.Log.Write(Color.Orange, "SERVER", message);
                            break;

                        case "kick":
                            int plid;
                            if (int.TryParse(queue.Peek(), out plid))
                                KickPlayer(plid);
                            break;

                        case "acks":
                        case "ack":
                            if (!Instance.Log.Filters.Remove(UI.ConsoleMessageTypes.Acks))
                            {
                                Server.Log.Write("SERVER", "ACK reporting on");
                                Instance.Log.Filters.Add(UI.ConsoleMessageTypes.Acks);
                            }
                            else
                                Server.Log.Write("SERVER", "ACK reporting off");
                            break;

                        case "savechunk":
                            try
                            {
                                int x = int.Parse(queue.Dequeue());
                                int y = int.Parse(queue.Dequeue());
                                var pos = new Vector2(x, y);
                                if (!Instance.Map.GetActiveChunks().TryGetValue(pos, out Chunk chunk))
                                {
                                    Server.Log.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
                                    break;
                                }
                                Server.Log.Write("SERVER", "Saving chunk " + pos.ToString());
                                chunk.SaveToFile();
                            }
                            catch (Exception) { Server.Log.Write("SERVER", "Syntax error in: " + command); }
                            break;

                        case "savechunks":

                            Server.Log.Write("SERVER", "Saving all active chunks");
                            foreach (var ch in Instance.Map.GetActiveChunks().Values)
                                ch.SaveToFile();
                            break;

                        case "save":
                            Server.Log.Write("SERVER", "Saving...");
                            Save();
                            break;

                        case "savethumb":
                            Instance.Map.GenerateThumbnails();
                            break;

                        case "set":
                            Set(command);
                            break;

                        default:
                            Server.Log.Write("SERVER", "Unknown command " + command);
                            break;
                    }
                }
                catch (Exception) { Server.Log.Write("SERVER", "Syntax error in: " + command); }
            }

            private void Set(string command)
            {
                var p = command.Split(' ');
                var type = p[1];
                switch (type)
                {
                    case "time":
                        throw new Exception();
                        int t = int.Parse(p[2]);
                        foreach (var ch in this.Server.Map.GetActiveChunks())
                            ch.Value.LightCache.Clear();

                        this.Server.Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        break;

                    default: break;
                }
            }
        }
    }
}
