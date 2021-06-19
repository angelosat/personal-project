using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class CommandParser
    {
        public void Execute(IObjectProvider net, string command)
        {
            try
            {
                var p = command.Split(' ');
                var type = p[0];
                switch (type)
                {
                    case "set":
                        switch (p[1])
                        {
                            case "time":
                            case "hour":
                                int t = int.Parse(p[2]);
                                (net.Map as StaticMap).SetHour(t);
                                ////net.Map.Time = new TimeSpan(net.Map.Time.Days, t, net.Map.Time.Minutes, net.Map.Time.Seconds);
                                //net.Map.Time = new TimeSpan(net.Map.Time.Days, t, 0, 0);
                                //foreach (var ch in net.Map.GetActiveChunks())
                                //    ch.Value.LightCache.Clear();
                                if (net is Server)
                                    (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                                break;

                            default:
                                break;
                        }
                        break;

                    case "replace":
                        Block old = Block.Registry[(Block.Types)Enum.Parse(typeof(Block.Types), p[1], true)];
                        Block replace = Block.Registry[(Block.Types)Enum.Parse(typeof(Block.Types), p[2], true)];
                        if (replace == Block.Air || old == Block.Air)
                            break;
                        foreach (var ch in net.Map.GetActiveChunks())
                            foreach (var cell in ch.Value.CellGrid2)
                                if (cell.Block == old)
                                {
                                    cell.Block = replace;
                                    var rest = p.Skip(3);
                                    string data = "";
                                    foreach(var s in rest)
                                    {
                                        data += s + " ";
                                    }
                                    data = data.TrimEnd(' ');
                                    if (p.Length > 3)
                                        cell.BlockData = replace.ParseData(data);//p[3]);
                                }
                        if (net is Server)
                            (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        break;

                    case "remove":
                        Block toremove = Block.Registry[(Block.Types)Enum.Parse(typeof(Block.Types), p[1], true)];
                        if (toremove == Block.Air)
                            break;
                        foreach (var ch in net.Map.GetActiveChunks())
                            foreach (var cell in ch.Value.CellGrid2)
                                if (cell.Block == toremove)
                                    cell.Block.Remove(net.Map, cell.LocalCoords.ToGlobal(ch.Value));
                        if (net is Server)
                            (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        break;

                    case "plant":
                        //var global = new Vector3(int.Parse(p[1]),int.Parse(p[2]),int.Parse(p[3]));
                        var x = int.Parse(p[1]);
                        var y = int.Parse(p[2]);
                        var z = int.Parse(p[3]);

                        //var planttype = (GameObject.Types)int.Parse(p[4]);
                        var planttype = (GameObject.Types)Enum.Parse(typeof(GameObject.Types), p[4], true);
                        Terraforming.Mutators.Trees.GeneratePlants(net.Map, x, y, z, planttype);
                        //if (net is Server)
                        //    (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        break;

                   

                    default:
                        break;
                }
            }
            catch (Exception e) { net.GetConsole().Write("Invalid command"); }
        }
    }
}
