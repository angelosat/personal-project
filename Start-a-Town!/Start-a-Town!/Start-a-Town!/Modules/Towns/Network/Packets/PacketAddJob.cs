using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class PacketAddJob : Packet
    {
        public int CreatorID;
        public TargetArgs Target;
        //public int InteractionID;
        public string InteractionName;

        //public PacketAddJob(int creatorID, TargetArgs target, int interactionID)
        //{
        //    this.CreatorID = creatorID;
        //    this.Target = target;
        //    this.InteractionID = interactionID;
        //}
        public PacketAddJob(int creatorID, TargetArgs target, string interactionName)
        {
            this.CreatorID = creatorID;
            this.Target = target;
            this.InteractionName = interactionName;
        }
        public PacketAddJob()
        {

        }
        //public PacketAddJob(BinaryReader r)
        //{
        //    this.CreatorID = r.ReadInt32();
        //    //this.Target = new TargetArgs(r);
        //    this.InteractionID = r.ReadInt32();
        //}

        public override byte[] Write()
        {
            return Network.Serialize(w =>
            {
                w.Write((int)TownsPacketHandler.Channels.AddJob);
                w.Write(this.CreatorID);
                this.Target.Write(w);
                //w.Write(this.InteractionID);
                w.Write(this.InteractionName);
            });
        }

        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.CreatorID = r.ReadInt32();
            this.Target = TargetArgs.Read(net, r);
            //this.InteractionID = r.ReadInt32();
            this.InteractionName = r.ReadString();
        }
    }
}
