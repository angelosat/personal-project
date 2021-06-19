using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptThrow : Script
    {
        public ScriptThrow()
        {
            this.ID = Script.Types.Throw;
            this.Execute = (net, actor, target, args) => { Success(new ScriptArgs(net, actor, target, args));};
            this.Name = "Throw";
            this.BaseTimeInSeconds = 0;
            this.RangeCheck = (actor, target, r) => true;
            this.Conditions =
                new ConditionCollection(
                    new Condition(
                        //(actor, target) => actor.GetComponent<GearComponent>().Holding.HasValue, "Nothing to throw!"));
                        (actor, target) => actor.GetComponent<HaulComponent>().Holding.HasValue, "Nothing to throw!"));

        }

        public override void OnStart(ScriptArgs args)
        {
            //GameObjectSlot Holding = args.Actor.GetComponent<PersonalInventoryComponent>().Holding;
            //if (!Holding.HasValue)
            //    return;

            Vector3 dir = args.Args.Translate<DirectionEventArgs>(args.Net).Direction;
            Vector3 speed = dir * 0.1f + args.Actor.Velocity;

            //GameObject newobj = Holding.Take();
            ////args.Net.Spawn(newobj, new Position(args.Net.Map, args.Actor.Global + new Vector3(0, 0, args.Actor.GetComponent<PhysicsComponent>().Height), speed));
            //newobj.Global = args.Actor.Global + new Vector3(0, 0, args.Actor.GetComponent<PhysicsComponent>().Height);
            //newobj.Velocity = speed;
            //args.Net.Spawn(newobj);
            //args.Actor.GetComponent<GearComponent>().Throw(speed, args.Actor);
            args.Actor.GetComponent<HaulComponent>().Throw(speed, args.Actor);

            //args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);

            Finish(args);
            //if (!Holding.HasValue)
            //    args.Actor.GetComponent<ActorSpriteComponent>().UpdateHeldObject(null);
        }

        public override object Clone()
        {
            return new ScriptThrow();
        }
    }
}
