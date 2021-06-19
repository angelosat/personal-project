using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketCommandNpc
    {
        static public void Send(IObjectProvider net, List<int> npcIDs, TargetArgs target, bool enqueue)
        {
            var w = net.GetOutgoingStream();
            w.Write((int)PacketType.NpcCommand);
            w.Write(npcIDs);
            target.Write(w);
            w.Write(enqueue);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var npcids = r.ReadListInt();
            var target = TargetArgs.Read(net, r);
            var enqueue = r.ReadBoolean();
            foreach(var npc in net.GetNetworkObjects(npcids))
                npc.MoveOrder(target, enqueue);
            //if (net is Server)
            //    Send(net, npcid, target);
        }
        //static public void SendSingle(IObjectProvider net, int npcID, TargetArgs target)
        //{
        //    var w = net.GetOutgoingStream();
        //    w.Write((int)PacketType.NpcCommand);
        //    w.Write(npcID);
        //    target.Write(w);
        //}
        //static public void ReceiveSingle(IObjectProvider net, BinaryReader r)
        //{
        //    var npcid = r.ReadInt32();
        //    var npc = net.GetNetworkObject(npcid);
        //    var target = TargetArgs.Create(net, r);
        //    npc.MoveOrder(target);
        //    //if (net is Server)
        //    //    Send(net, npcid, target);
        //}
        static public void Init()
        {
            Server.RegisterPacketHandler(PacketType.NpcCommand, Receive);
            Client.RegisterPacketHandler(PacketType.NpcCommand, Receive);
        }
    }
}
