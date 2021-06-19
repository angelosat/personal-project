using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    interface ICommandParser
    {
        bool Execute(IObjectProvider net, string command);
    }
    class CommandParser
    {
        static public bool Execute(IObjectProvider net, PlayerData player, string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                //case "set":
                //    switch (p[1])
                //    {
                //        case "time":
                //            int t = int.Parse(p[2]);
                //            //net.Map.Time = new TimeSpan(net.Map.Time.Days, t, net.Map.Time.Minutes, net.Map.Time.Seconds);
                //            net.Map.Time = new TimeSpan(net.Map.Time.Days, t, 0, 0);
                //            foreach (var ch in net.Map.GetActiveChunks())
                //                ch.Value.LightCache.Clear();
                //            if(net is Server)
                //                (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                //            return true;

                //            break;

                //        default:
                //            break;
                //    }
                //    break;

                case "resetlight":
                    foreach (var chunk in net.Map.GetActiveChunks().Values)
                    {
                        var items = chunk.ResetHeightMap();
                        new LightingEngine(net.Map).HandleBatchSync(items);
                    }
                    break;

                case "fog":
                    var client = net as Client;
                    if (client != null)
                    {
                        ScreenManager.CurrentScreen.Camera.FogLevel = int.Parse(p[1]);
                    }
                    if (net is Server)
                        (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                    break;

                case "teleport":
                    int x=  int.Parse(p[1]);
                    int y= int.Parse(p[2]);
                    int z = int.Parse(p[3]);
                    var pos = new Vector3(x,y,z);
                    if (!net.Map.PositionExists(pos))
                        break;
                    player.Character.ChangePosition(pos);
                    byte[] data = Network.Serialize(w =>
                    {
                        w.Write(player.Character.Network.ID);
                        w.Write(pos);
                    });
                    if (net is Server)
                        (net as Server).Enqueue(PacketType.ChangeEntityPosition, data, SendType.OrderedReliable);
                    break;


                default:
                    GameModes.GameMode.Current.ParseCommand(net, command);
                    break;
            }
            return false;
        }


        static public bool Execute(IObjectProvider net, string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                //case "set":
                //    switch (p[1])
                //    {
                //        case "time":
                //            int t = int.Parse(p[2]);
                //            //net.Map.Time = new TimeSpan(net.Map.Time.Days, t, net.Map.Time.Minutes, net.Map.Time.Seconds);
                //            net.Map.Time = new TimeSpan(net.Map.Time.Days, t, 0, 0);
                //            foreach (var ch in net.Map.GetActiveChunks())
                //                ch.Value.LightCache.Clear();
                //            if(net is Server)
                //                (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                //            return true;

                //            break;

                //        default:
                //            break;
                //    }
                //    break;

                case "resetlight":
                    foreach (var chunk in net.Map.GetActiveChunks().Values)
                    {
                        var items = chunk.ResetHeightMap();
                        new LightingEngine(net.Map).HandleBatchSync(items);
                    }
                    break;

                

                case "fog":
                    var client = net as Client;
                    if(client!=null)
                    {
                        ScreenManager.CurrentScreen.Camera.FogLevel = int.Parse(p[1]);
                    }
                    if (net is Server)
                        (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                    break;



                default:
                    GameModes.GameMode.Current.ParseCommand(net, command);
                    break;
            }
            return false;
        }

    }
}
