using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    static class PacketEditAppearance
    {
        static public void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.PacketEditAppearance, Receive);
            Client.RegisterPacketHandler(PacketType.PacketEditAppearance, Receive);
        }
        public static void Send(Actor actor, CharacterColors colors)
        {
            var w = actor.Net.GetOutgoingStream();
            w.Write(PacketType.PacketEditAppearance);
            w.Write(actor.RefID);
            colors.Write(w);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
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
