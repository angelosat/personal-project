using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class TownJobStep
    {
       // public GameObject Creator { get; set; }
        public TargetArgs Target { get; set; }
        public Script.Types Script { get; set; }
        //public TownJobStep(GameObject creator, TargetArgs target, Script.Types script)
        //{
        //    this.Creator = creator;
        //    this.Target = target;
        //    this.Script = script;
        //}
        TownJobStep()
        {

        }
        public TownJobStep(TargetArgs target, Script.Types script)
        {
            this.Target = target;
            this.Script = script;
        }

        public void Write(BinaryWriter w)
        {
            Target.Write(w);
            w.Write((int)this.Script);
        }
        static public TownJobStep Read(BinaryReader r, Net.IObjectProvider net)
        {
            TownJobStep step = new TownJobStep();
            step.Target = TargetArgs.Read(net, r);
            step.Script = (Script.Types)r.ReadInt32();
            return step;
        }
    }
}
