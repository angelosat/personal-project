using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.AI
{
    public class AIPrecondition
    {
        public Script.Types Solution { get; set; }
        public Func<Script.Types, bool> ScriptSelector { get; set; }
        public Func<GameObject, bool> TargetSelector { get; set; }
        public AIPrecondition(Func<GameObject, bool> targetSelector, Script.Types solution)
        {
            this.Solution = solution;
            this.TargetSelector = targetSelector;
        }
        //public AIPrecondition(Func<GameObject, bool> targetSelector, Func<Script.Types, bool> scriptSelector)
        //{
        //    this.ScriptSelector = scriptSelector;
        //    this.TargetSelector = targetSelector;
        //}
    }
}
