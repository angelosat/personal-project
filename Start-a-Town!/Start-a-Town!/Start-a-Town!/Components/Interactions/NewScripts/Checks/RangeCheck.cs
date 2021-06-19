using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Components.Interactions
{
    public class RangeCheck : ScriptTaskCondition
    {
        Func<TargetArgs, Vector3> Position { get; set; }
        public float Min, Max;

        public static readonly float DefaultRange = (float)Math.Sqrt(2);
        public static readonly RangeCheck One = new RangeCheck();
        public static readonly RangeCheck Sqrt2 = new RangeCheck(DefaultRange);

        public RangeCheck(float max = InteractionOld.DefaultRange)// 2)
            : this(t => t.Global, max, 0)
        {

        }
        public RangeCheck(Func<TargetArgs, Vector3> target, float max = float.MaxValue, float min = 0)
            : base("Range")
        {
            //this.Name = "Range";
            this.Position = target;
            this.Min = min;
            this.Max = max;
            this.ErrorEvent = Message.Types.OutOfRange;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            //return base.Condition(actor);
            Vector3 globalTarget = this.Position(target);
            //float distancemid = Vector3.Distance(actor.Global + actor.Physics.Height * .5f * Vector3.UnitZ, global);
            float distancetop = Vector3.Distance(actor.Global + actor.Physics.Height * Vector3.UnitZ, globalTarget);
            float distancebottom = Vector3.Distance(actor.Global, globalTarget);
            var distance = Math.Min(distancetop, distancebottom);
            //float distance = Vector3.Distance(actor.Global + actor.Physics.Height * .5f * Vector3.UnitZ, global);

            var inrange = (distance >= this.Min && distance <= this.Max);
            //if(!inrange)
            //    if(actor.Net is Net.Client)
            //    Console.WriteLine("entity global: " + actor.Global.ToString());
            return inrange;
        }
        //public bool Condition(Vector3 global1, Vector3 global2)
        //{
        //    var distance = Vector3.Distance(global1, global2);
        //    var inrange = (distance >= this.Min && distance <= this.Max);
        //    return inrange;
        //}

        public override ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            return this.Condition(actor, target) ? null : this;
        }
        //public override AIInstruction AIGetPreviousStep(GameObject agent, TargetArgs target, AI.AIState state)
        //{
        //    //return new AIInstruction(new TargetArgs(this.Position(target)), new MoveTo(this.Min, this.Max));
        //    return new AIInstruction(new TargetArgs(this.Position(target)), new MoveTo(this.Min, this.Max));

        //}

        //public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, out AIInstruction instruction)
        //{
        //    //state.MoveTarget = new AITarget(target, this.Min, this.Max); // THIS IS NOT REQUIRED (?)
        //    instruction = null; //rangecheck is solvable (by the ai's pathfinding) but no instruction required, so set to null and return true
        //    return true;
        //}
    }
}
