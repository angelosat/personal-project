using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class AINextCommand : Behavior
    {
        public override string Name
        {
            get
            {
                return "AINextCommand";
            }
        }
        public override object Clone()
        {
            return new AINextCommand();
        }

        public override BehaviorState Execute(Actor parent, AIState state)
        {           
            // TODO: get next command
            var commands = state.Commands;// state.Properties["Commands"] as Queue<AIInstruction>;
            if (commands.Count == 0)
                return BehaviorState.Fail;
            var command = commands.Peek();
            //command.Interaction.GetJob(parent, command.Target, state);
            //command.Interaction.AIInit(parent, command.Target, state);

            //check if target is within line of sight, if it's not, find a path
            //state.MoveTarget = command.GetTargetRange();


            var a = parent.Global.Round();
            //var b = state.MoveTarget.Target.Global.Round();
            var b = command.Target.Global.Round();

            var path = new Pathing().GetPath(parent.Map, a, b);
            if (path != null)
                path.ToConsole();
            Line.LineOfSight((int)a.X, (int)a.Y, (int)b.X, (int)b.Y, (int)a.Z, p => parent.Map.IsSolid(p)).ToConsole();
            //throw new Exception();
            state.Path = path;
            state.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), 0, 0.1f);


            return BehaviorState.Success;
        }


    }
}
