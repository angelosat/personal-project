using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Scripts
{
    class ScriptJump : Script
    {
        //public static GameObjectSlot AbilityJump
        //{
        //    get
        //    {
        //        GameObjectSlot scriptSlot = Ability.Create(Script.Types.Jumping, 0, "Jump", "Jumping",
        //           (net, actor, target, args) =>
        //           {
        //               if (actor.Velocity.Z == 0)
        //                   actor.Velocity += new Vector3(0, 0, PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(actor, Stat.Types.JumpHeight, 0f)));// (float)a.Actor["Stats"]["Jump Force"]);
        //           },
        //           range: (a1, t) => true);
        //        Script script = scriptSlot.Object["Ability"] as Script;
        //        script.TargetSelector = (actor, target) => new TargetArgs(actor);
        //        return scriptSlot;
        //    }
        //}

        public ScriptJump()
        {
            this.ID = Types.Jumping;
            this.Name = "Jump";
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (parent.Velocity.Z == 0)
                this.ScriptState = Components.ScriptState.Finished;
        }

        public override void OnStart(ScriptArgs args)
        {
            //if(args.Actor.Velocity.Z == 0 &&)
            if (!args.Actor.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Jumping) &&
                args.Actor.Velocity.Z == 0 &&
                (args.Actor.Global - Vector3.UnitZ * 0.1f).IsSolid(args.Net.Map))
            {
                args.Actor.Velocity += Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(args.Actor, Stat.Types.JumpHeight, 0f));
            }
            args.Actor.GetComponent<ControlComponent>().FinishScript(this.ID, args);
        }

        public override object Clone()
        {
            return new ScriptJump();
        }
    }
}