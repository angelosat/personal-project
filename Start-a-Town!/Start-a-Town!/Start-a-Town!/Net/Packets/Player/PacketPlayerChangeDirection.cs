using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Net.Packets.Player
{
    class PacketPlayerChangeDirection : Packet, IPacketHandler
    {
        public int EntityID;
        public GameObject Entity;
        public Vector3 Direction;

        public void Handle(IObjectProvider net, Packet packet)
        {
            packet.Player.Character.GetComponent<MobileComponent>().Jump(packet.Player.Character);
            net.Enqueue(PacketType.PlayerJump, packet.Payload, SendType.Ordered | SendType.Reliable);
        }


        public PacketPlayerChangeDirection(GameObject entity, Vector3 direction)
        {
            this.PacketType = Net.PacketType.PlayerChangeDirection;
            this.Entity = entity;
            this.EntityID = entity.Network.ID;
            this.Direction = direction;
        }
        public PacketPlayerChangeDirection(IObjectProvider net, byte[] data)
        {
            this.PacketType = Net.PacketType.PlayerChangeDirection;
            this.Read(net, data);
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Entity.Network.ID);
            w.Write(this.Direction);
        }


        //public override void Read(IObjectProvider net, byte[] data)
        //{
        //    data.Deserialize(r =>
        //    {
        //        this.EntityID = r.ReadInt32();
        //        this.Direction = r.ReadVector3();
        //        this.Entity = net.GetNetworkObject(this.EntityID);
        //    });
        //}
        public override void Read(IObjectProvider net, byte[] data)
        {
            data.Translate(r =>
            {
                this.EntityID = r.ReadInt32();
                this.Direction = r.ReadVector3();
                this.Entity = net.GetNetworkObject(this.EntityID);
            });
        }
    }
}
