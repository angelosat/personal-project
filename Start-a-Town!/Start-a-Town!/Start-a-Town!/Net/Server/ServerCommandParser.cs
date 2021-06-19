using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public partial class Server
    {
        class ServerCommandParser
        {
            Server Server;
            public ServerCommandParser(Server server)
            {
                this.Server = server;
            }
            //static
                public void Command(string command)
            {
                Queue<string> queue = new Queue<string>(command.Split(' '));
                try
                {
                    switch (queue.Dequeue())
                    {
                        case "hello":
                            Server.Console.Write("SERVER", "how are you?");
                            break;

                        case "loadworld":
                            string worldName = queue.Dequeue();
                            //SetWorld(worldName);
                            //Server.Console.Write(Color.Red, "SERVER", "World " + worldName + " doesn't exist");
                            break;

                        case "unloadworld":
                            UnloadWorld();
                            break;

                        case "broadcast":
                            string message = queue.Dequeue();
                            byte[] data = Network.Serialize(w => w.WriteASCII(message));
                            foreach (var p in Instance.Players.GetList())
                                //p.Outgoing.
                                Instance.Enqueue(p, Packet.Create(p, PacketType.ServerBroadcast, data));

                            Server.Console.Write(Color.Orange, "SERVER", message);
                            break;

                        case "kick":
                            //int plid = int.Parse(queue.Dequeue());
                            int plid;
                            if (int.TryParse(queue.Peek(), out plid))
                                KickPlayer(plid);
                            //else 
                            //    KickPlayer(queue.Peek());


                            break;

                        case "acks":
                        case "ack":
                            if (!Console.Filters.Remove(UI.ConsoleMessageTypes.Acks))
                            {
                                Server.Console.Write("SERVER", "ACK reporting on");
                                Console.Filters.Add(UI.ConsoleMessageTypes.Acks);
                            }
                            else
                                Server.Console.Write("SERVER", "ACK reporting off");
                            break;

                        case "updatechunkedges":
                            if (queue.Count == 0)
                                if (Player.Actor != null)
                                {
                                    //var pos = Player.Actor.Global.GetChunk(Instance.Map).MapCoords;
                                    var pos = Instance.Map.GetChunk(Player.Actor.Global).MapCoords;

                                    //Instance.Map.UpdateChunkEdges(pos);
                                    Instance.Enqueue(PacketType.UpdateChunkEdges, Network.Serialize(w => w.Write(pos)), SendType.OrderedReliable);
                                    Server.Console.Write("SERVER", "Updating chunk edges at player's location");
                                }
                            break;

                        //case "resetlight":
                        //    try
                        //    {
                        //        if (queue.Count == 0)
                        //        {
                        //            if (Player.Actor != null)
                        //            {
                        //                //ResetLight(Player.Actor.Global.GetChunk(Instance.Map));
                        //                ResetLight(Instance.Map.GetChunk(Player.Actor.Global));

                        //                Server.Console.Write("SERVER", "Resetting light of chunk at player's location");
                        //            }
                        //            break;
                        //        }
                        //        if (queue.Peek() == "all")
                        //        {
                        //            Server.Console.Write("SERVER", "Resetting light of all active chunks");
                        //            //foreach (var ch in Instance.Map.ActiveChunks.Values)
                        //            foreach (var ch in Instance.Map.GetActiveChunks().Values)
                        //                ResetLight(ch);
                        //            break;
                        //        }
                        //        int x = int.Parse(queue.Dequeue());
                        //        int y = int.Parse(queue.Dequeue());
                        //        Vector2 pos = new Vector2(x, y);
                        //        Chunk chunk;
                        //        if (!Instance.Map.GetActiveChunks().TryGetValue(pos, out chunk))
                        //        {
                        //            Server.Console.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
                        //            break;
                        //        }
                        //        Server.Console.Write("SERVER", "Resetting light of chunk " + pos.ToString());
                        //        ResetLight(chunk);
                        //    }
                        //    catch (Exception) { Server.Console.Write("SERVER", "Error in command " + command); }
                        //    break;

                        case "savechunk":
                            try
                            {
                                //if (queue.Count == 0)
                                //{
                                //    Server.Console.Write("SERVER", "Saving all active chunks");
                                //    foreach (var ch in Instance.Map.ActiveChunks.Values)
                                //        ch.SaveServer();
                                //    break;
                                //}
                                int x = int.Parse(queue.Dequeue());
                                int y = int.Parse(queue.Dequeue());
                                Vector2 pos = new Vector2(x, y);
                                Chunk chunk;
                                if (!Instance.Map.GetActiveChunks().TryGetValue(pos, out chunk))
                                {
                                    Server.Console.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
                                    break;
                                }
                                Server.Console.Write("SERVER", "Saving chunk " + pos.ToString());
                                chunk.SaveToFile();
                            }
                            catch (Exception) { Server.Console.Write("SERVER", "Syntax error in: " + command); }
                            break;

                        case "savechunks":

                            Server.Console.Write("SERVER", "Saving all active chunks");
                            foreach (var ch in Instance.Map.GetActiveChunks().Values)
                                ch.SaveToFile();


                            break;

                        case "save":
                            Server.Console.Write("SERVER", "Saving...");
                            Save();
                            break;

                        case "savethumb":
                            Instance.Map.GenerateThumbnails();
                            break;

                        case "set":
                            Set(command);
                            break;

                        default:
                            Server.Console.Write("SERVER", "Unknown command " + command);
                            break;
                    }
                }
                catch (Exception) { Server.Console.Write("SERVER", "Syntax error in: " + command); }
            }

            private void Set(string command)
            {
                //var type = queue.Dequeue();
                //string[] p = new string[queue.Count];
                //for (int i = 0; i < queue.Count; i++)
                //    p[i] = queue.Dequeue();
                var p = command.Split(' ');
                var type = p[1];
                switch (type)
                {
                    case "time":
                        int t = int.Parse(p[2]);
                        this.Server.Map.Time = new TimeSpan(this.Server.Map.Time.Days, t, this.Server.Map.Time.Minutes, this.Server.Map.Time.Seconds);
                        foreach (var ch in this.Server.Map.GetActiveChunks())
                            ch.Value.LightCache.Clear();

                        //Client.Instance.Map.Time = this.Server.Map.Time;
                        //foreach (var ch in Client.Instance.Map.GetActiveChunks())
                        //    ch.Value.LightCache.Clear();

                        this.Server.Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        //Enqueue(EntryTypes.System, "Hour set to " + t);
                        break;

                    default: break;
                }
            }

        }
    }
}
