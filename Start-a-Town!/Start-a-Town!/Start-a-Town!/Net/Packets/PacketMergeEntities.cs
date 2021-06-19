using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_.Net.Packets
{
    class PacketMergeEntities : Packet
    {
        public GameObject Master, Slave;
        PacketMergeEntities()
        {
            this.PacketType = Net.PacketType.MergeEntities;
        }
        //public PacketMergeEntities(byte[] data)
        //    : this()
        //{

        //}
        //public PacketMergeEntities(int entityID)
        //{
        //    this.EntityID = entityID;
        //}
        public PacketMergeEntities(GameObject master, GameObject slave)
        {
            this.Master = master;
            this.Slave = slave;
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.Master.InstanceID);
            w.Write(this.Slave.InstanceID);
        }
        static public void Write(BinaryWriter w, GameObject master, GameObject slave)
        {
            w.Write(master.InstanceID);
            w.Write(slave.InstanceID);
        }
        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.Master = net.GetNetworkObject(r.ReadInt32());
            this.Slave = net.GetNetworkObject(r.ReadInt32());
        }
        public override void Handle(IObjectProvider net)
        {
            if (this.Master.ID != this.Slave.ID)
                throw new Exception();
            if (this.Slave.StackSize + this.Master.StackSize >= this.Slave.StackMax)
                return;
            this.Master.StackSize += this.Slave.StackSize;
            this.Slave.Despawn();
            this.Slave.Dispose();
        }
        static public void Handle(IObjectProvider net, BinaryReader r)
        {
            var master = net.GetNetworkObject(r.ReadInt32());
            var slave = net.GetNetworkObject(r.ReadInt32());
            if (master.ID != slave.ID)
                throw new Exception();
            if (slave.StackSize + master.StackSize >= slave.StackMax)
                return;
            master.StackSize += slave.StackSize;
            slave.Despawn();
            slave.Dispose();
        }
        static public void Handle(Client client, Packet packet)
        {
            packet.Payload.Deserialize(r =>
            {
                Handle(client, r);
            });
        }
    }
}
