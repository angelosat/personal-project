using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.AI
{
    public class AIPlanStep
    {
        public TargetArgs Target { get; set; }
        public Script Script { get; set; }
        public AIPlanStep(TargetArgs target, Script script)
        {
            this.Target = target;
            this.Script = script;
        }
        //public Script.Types Script { get; set; }
        //public AIPlanStep(GameObject target, Script.Types script)
        //{
        //    this.Target = target;
        //    this.Script = script;
        //}
    }
}
