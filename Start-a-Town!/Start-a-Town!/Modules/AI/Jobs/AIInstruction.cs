using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI
{
    [Obsolete]
    public class AIInstruction
    {
        public override string ToString()
        {
            ////return this.Interaction.Name + ": " + this.Target.ToString();
            return this.Interaction.ToString() + ": " + this.Target.ToString();
        }

        public string InteractionName;
        //public Interaction Interaction;
        Interaction _Interaction;
        public Interaction Interaction
        {
            get
            {
                //return this.Target.GetInteraction(this.InteractionName);
                if(this._Interaction == null)
                    this._Interaction = this.Target.GetInteraction(this.InteractionName);
                return this._Interaction;
            }
        }
        public TargetArgs Target;
        public bool Completed;

        //public AIInstruction(TargetArgs target, string work)
        public AIInstruction(TargetArgs target, Interaction work)
        {
            this.Target = target;
            //if (this.Target.Type == TargetType.Entity)
            //    if (this.Target.Object == null)
            //        throw new Exception();
            this.InteractionName = work.Name;
        }

        public void Write(BinaryWriter w)
        {
            this.Target.Write(w);
            //w.Write(this.Interaction.Name);
            w.Write(this.InteractionName);
        }
        public void Read(INetwork net, BinaryReader r)
        {
            this.Target = TargetArgs.Read(net, r);
            //this.Interaction = this.Target.GetInteraction(net, r.ReadString());
            this.InteractionName = r.ReadString();
        }

        public SaveTag Save()
        {
            var tag = new SaveTag(SaveTag.Types.Compound);
            tag.Add(new SaveTag(SaveTag.Types.Compound, "Target", this.Target.SaveAsList()));
            //tag.Add(new SaveTag(SaveTag.Types.String, "InteractionName", this.Interaction.Name));
            //tag.Add(new SaveTag(SaveTag.Types.Compound, "InteractionArgs", this.Interaction.Save()));
            tag.Add(new SaveTag(SaveTag.Types.String, "InteractionName", this.InteractionName));
            //tag.Add(new SaveTag(SaveTag.Types.Compound, "InteractionArgs", this.Interaction.Save()));
            return tag;
        }
        public void Load(INetwork net, SaveTag tag)
        {
            throw new Exception();
            //this.Target = new TargetArgs(net, tag);
            //this.InteractionName = tag.GetValue<string>("InteractionName");
        }
        public AIInstruction(INetwork net, SaveTag tag)
        {
            this.Load(net, tag);
        }
        public AIInstruction(INetwork net, BinaryReader r)
        {
            this.Read(net, r);
        }
    }
}
