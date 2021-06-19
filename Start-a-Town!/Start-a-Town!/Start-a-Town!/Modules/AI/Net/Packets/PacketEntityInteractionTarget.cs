using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.Net
{
    class PacketEntityInteractionTarget : Packet
    {
        public int EntityID;
        public string InteractionName;
        public TargetArgs Target;
        public Vector3 EntityGlobal, EntityVelocity;

        public PacketEntityInteractionTarget(IObjectProvider net, BinaryReader r)
        {
            this.Read(net, r);
        }
        public PacketEntityInteractionTarget(GameObject agent, string interactionName, TargetArgs target)
        {
            this.EntityID = agent.InstanceID;
            this.InteractionName = interactionName;
            this.Target = target;
            this.EntityGlobal = agent.Global;
            this.EntityVelocity = agent.Velocity;
        }

        //public override byte[] Write()
        //{
        //    return Network.Serialize(w =>
        //    {
        //        w.Write(this.EntityID);
        //        w.Write(this.InteractionName);
        //        this.Target.Write(w);
        //        w.Write(this.EntityGlobal);
        //        w.Write(this.EntityVelocity);
        //    });
        //}
        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.InteractionName);
            this.Target.Write(w);
            w.Write(this.EntityGlobal);
            w.Write(this.EntityVelocity);
        }
        public override void Read(IObjectProvider net, BinaryReader r)
        {
            this.EntityID = r.ReadInt32();
            this.InteractionName = r.ReadString();
            this.Target = TargetArgs.Read(net, r);
            this.EntityGlobal = r.ReadVector3();
            this.EntityVelocity = r.ReadVector3();
        }

        public override void Handle(IObjectProvider net)
        {
            //Network.Deserialize(this.Payload, r =>
            //{
                //PacketEntityInteractionTarget pack = new PacketEntityInteractionTarget(Instance, r);
                var entity = net.GetNetworkObject(this.EntityID);
                var interaction = this.Target.GetInteraction(net, this.InteractionName);
                if (interaction == null)
                {
                    Client.Console.Write("Interaction " + this.InteractionName + " not found at " + this.Target.ToString());
                    return;
                    //throw new Exception("Interaction " + interaction.Name + " doesn't exist on " + this.Target.ToString());
                }
                //entity.ChangePosition(pack.EntityGlobal); // WORKAROUND until tickstamped packets
                //entity.Velocity = pack.EntityVelocity;
                //System.Console.WriteLine("entity global changed to: " + entity.Global.ToString());
                var range = Vector3.Distance(entity.Global, Target.Global);
                if (range > InteractionOld.DefaultRange)
                    "ti sto putso".ToConsole();
                entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, interaction, this.Target));
            //});
        }
    }
}
