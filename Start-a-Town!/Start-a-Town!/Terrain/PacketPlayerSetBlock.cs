﻿using Start_a_Town_.Net;
using System;
using System.IO;

namespace Start_a_Town_
{
    static class PacketPlayerSetBlock
    {
        static readonly int p;
        static PacketPlayerSetBlock()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        public static void Init() { }
        public static void Send(INetwork net, PlayerData player, IntVec3 global, Block.Types type, byte data = 0, int variation = 0, int orientation = 0)
        {
            if (net is Server)
                Perform(net.Map, global, type, data, variation, orientation);

            net.GetOutgoingStream().Write(
                p,
                player.ID,
                global,
                (int)type,
                data,
                variation,
                orientation
                );
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var player = net.GetPlayer(r.ReadInt32());
            var global = r.ReadIntVec3();
            var type = (Block.Types)r.ReadInt32();
            var data = r.ReadByte();
            var variation = r.ReadInt32();
            var orientation = r.ReadInt32();

            if (net is Server)
                Send(net, player, global, type, data, variation, orientation);
            else
                Perform(net.Map, global, type, data, variation, orientation);
        }

        private static void Perform(MapBase map, IntVec3 global, Block.Types type, byte data, int variation, int orientation)
        {
            if (!map.IsInBounds(global))
                return;
            // DONT CALL PREVIOUS BLOCK'S REMOVE METHOD
            // when in block editing mode, we don't want to call block's remove method, so for example they don't pop out their contents or have any other effects to the world
            // HOWEVER we want to dispose their contents (gameobjects) if any! 
            // so 1) query their contents and dispose them here? 
            //    2) call something like dispose() on them and let them dispose them themselves?
            // TODO: DECIDE!
            map.RemoveBlock(global);

            if (type != Block.Types.Air)
            {
                var block = Block.Registry[type];
                block.Place(map, global, data, variation, orientation);
            }
        }
    }
}
