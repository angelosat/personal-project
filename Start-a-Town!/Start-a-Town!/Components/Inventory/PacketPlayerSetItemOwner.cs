using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class PacketPlayerSetItemOwner
    {
        static readonly int PacketIDPlayerSetItemOwner;
        static PacketPlayerSetItemOwner()
        {
            PacketIDPlayerSetItemOwner = Network.RegisterPacketHandler(Receive);
        }
        static public void Init()
        {
        }
        static public void Send(IObjectProvider net, int itemID, int ownerID)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(PacketIDPlayerSetItemOwner);
            stream.Write(itemID);
            stream.Write(ownerID);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var itemID = r.ReadInt32();
            var ownerID = r.ReadInt32();
            var item = net.GetNetworkObject(itemID);
            var previousOwner = item.GetOwner();// item.GetComponent<OwnershipComponent>().Owner;
            var owner = ownerID == -1 ? null : net.GetNetworkObject(ownerID);
            //item.GetComponent<OwnershipComponent>().Owner = owner;
            //if(owner!=null)
            //    NpcComponent.AddPossesion(owner, item);
            //if (previousOwner != null)
            //    if (owner != previousOwner)
            //        NpcComponent.RemovePossesion(previousOwner, item);


            // i moved this in the npccomponent add/removepossesion method// 
            item.SetOwner(ownerID);// item.GetComponent<OwnershipComponent>().Owner = ownerID;
            
            //if (owner != null)
            //    NpcComponent.AddPossesion(owner, item);
            //if(previousOwner != -1)
            //if (ownerID != previousOwner)
            //    NpcComponent.RemovePossesion(net.GetNetworkObject(previousOwner), item);
            //net.EventOccured(Message.Types.ItemOwnerChanged, itemID);
            if (net is Server)
                Send(net, itemID, ownerID);
        }
    }
}
