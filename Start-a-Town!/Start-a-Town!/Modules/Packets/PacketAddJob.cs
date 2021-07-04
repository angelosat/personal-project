using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    [Obsolete]
    class PacketAddJob : Packet
    {
        public int CreatorID;
        public TargetArgs Target;
        public string InteractionName;

        public PacketAddJob(int creatorID, TargetArgs target, string interactionName)
        {
            this.CreatorID = creatorID;
            this.Target = target;
            this.InteractionName = interactionName;
        }
        public PacketAddJob()
        {

        }

        public override byte[] Write()
        {
            return Network.Serialize(w =>
            {
                w.Write((int)TownsPacketHandler.Channels.AddJob);
                w.Write(this.CreatorID);
                this.Target.Write(w);
                w.Write(this.InteractionName);
            });
        }

        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.CreatorID = r.ReadInt32();
            this.Target = TargetArgs.Read(net, r);
            this.InteractionName = r.ReadString();
        }
    }
}
