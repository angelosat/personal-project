using System;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class CommandParser
    {
        public void Execute(INetwork net, string command)
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
                        if (replace == BlockDefOf.Air || old == BlockDefOf.Air)
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
                                        cell.BlockData = replace.ParseData(data);
                                }
                        if (net is Server)
                            (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        break;

                    case "remove":
                        Block toremove = Block.Registry[(Block.Types)Enum.Parse(typeof(Block.Types), p[1], true)];
                        if (toremove == BlockDefOf.Air)
                            break;
                        foreach (var ch in net.Map.GetActiveChunks())
                            foreach (var cell in ch.Value.CellGrid2)
                                if (cell.Block == toremove)
                                    cell.Block.Remove(net.Map, cell.LocalCoords.ToGlobal(ch.Value));
                        if (net is Server)
                            (net as Server).Enqueue(PacketType.PlayerServerCommand, Network.Serialize(w => w.WriteASCII(command)), SendType.OrderedReliable);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception) { net.Log.Write("Invalid command"); }
        }
    }
}
