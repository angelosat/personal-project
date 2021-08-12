﻿using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEditAppearance
    {
        static readonly int p;
        static PacketEditAppearance()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        public static void Send(Actor actor, CharacterColors colors)
        {
            var w = actor.Net.GetOutgoingStream();
            w.Write(p);
            w.Write(actor.RefID);
            colors.Write(w);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var actorID = r.ReadInt32();
            var actor = net.GetNetworkObject(actorID) as Actor;
            var colors = new CharacterColors(r);
            actor.Sprite.Customization = colors;
            if (net is Server)
                Send(actor, colors);
        }
    }
}
