using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Blocks;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_
{
    class PacketDesignateConstruction
    {
        internal static void Send(IObjectProvider net, ToolDrawing.Args a)
        {
            Send(net, null, a);
        }
        //internal static void Send(IObjectProvider net, Vector3 position, bool remove, bool replace, int orientation, bool cheat)
        //{
        //    ToolDrawing.Args a = new ToolDrawing.Args(ToolDrawing.Modes.Box, position, position, remove, replace, cheat, orientation);
        //    Send(net, a);
        //}
        static public void Send(IObjectProvider net, BlockRecipe.ProductMaterialPair item, ToolDrawing.Args a)
        {
            var stream = net.GetOutgoingStream();
            stream.Write((int)PacketType.PlaceWallConstruction);
            a.Write(stream);
            if(!a.Removing)
                item.Write(stream);
            //var data = Network.Serialize(w =>
            //{
            //    w.Write(Player.Actor.InstanceID);
            //    item.Write(w);
            //    a.Write(w);
            //});
            //Client.Instance.Send(PacketType.PlaceWallConstruction, data);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var args = new ToolDrawing.Args(r);
            var product = args.Removing ? null : new BlockRecipe.ProductMaterialPair(r);
            var positions = ToolDrawing.GetPositions(args);
            net.Map.Town.ConstructionsManager.Handle(args, product, positions);
            //var cheat = false;// true;// args.Cheat;
            //var map = net.Map;
            //if (cheat)
            //{
            //    if (!args.Removing)
            //    {
            //        product.Block.Place(
            //            map,
            //            positions.Where(vec => args.Replacing ? map.GetBlock(vec) != Block.Air : map.GetBlock(vec) == Block.Air).ToList(),
            //            product.Data,
            //            args.Orientation
            //            , true);
            //        //map.Town.ConstructionsManager.Add(positions.Where(vec => args.Replacing ? map.GetBlock(vec) != Block.Air : map.GetBlock(vec) == Block.Air).ToList(), product, args.Orientation);
            //    }
            //    else
            //    {
            //        map.RemoveBlocks(positions);
            //    }
            //}
            //else
            //{
            //    if (args.Removing)
            //    {
            //        map.RemoveBlocks(positions.Where(vec => map.GetBlock(vec) == Block.Designation).ToList(), false);
            //        //BlockDesignation.Remove(map, positions.Where(vec => map.GetBlock(vec) == Block.Designation).ToList());
            //    }
            //    else
            //        foreach (var pos in positions)
            //        {
            //            if (map.GetBlock(pos) != Block.Air)
            //                continue;
            //            BlockDesignation.Place(map, pos, 0, 0, args.Orientation, product);
            //        }
            //}

            if (net is Server)
                Send(net, product, args);
            return;
        }

        static public void Init()
        {
            Server.RegisterPacketHandler(PacketType.PlaceWallConstruction, PacketDesignateConstruction.Receive);
            Client.RegisterPacketHandler(PacketType.PlaceWallConstruction, PacketDesignateConstruction.Receive);
        }

        
    }
}
