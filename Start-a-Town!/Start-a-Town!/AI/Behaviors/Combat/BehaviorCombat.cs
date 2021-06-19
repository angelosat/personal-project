using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorCombat : BehaviorSequence//Queue
    {
        public BehaviorCombat()
        {
            this.Children = new List<Behavior>()
            {
                //new BehaviorInverter(
                    new BehaviorThreatsExist("lastKnownPosition"),
                    //),
                new BehaviorSelector(

                    // flee node
                    new BehaviorSequence(
                        new BehaviorResourceCheck(ResourceDef.Health, BehaviorResourceCheck.Comparison.Less, .5f),
                        new BehaviorFlee()),

                    // fight node
                    new BehaviorSelector( 
                        // if attacked before updating known nearby entities, 
                        // the ai will start attacking before going to pick up nearby weapon
                        // do i want this?
                        // also do i want the ai to fight back while it's calculating path to weapon?
                        new BehaviorFindEquipment(),
                        new BehaviorInverter(new BehaviorChaseThreat()), //working
                        new BehaviorSelector(
                            new BehaviorAttackDeliver(),
                            new BehaviorAttackCharge()))),

                new BehaviorSucceeder()
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
        public override object Clone()
        {
            return new BehaviorCombat();
        }
    }

    class BehaviorCombatOld : BehaviorSelector//BehaviorQueue//
    {
        public BehaviorCombatOld()
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorInverter(new BehaviorThreatsExist("lastKnownPosition")),
                //new BehaviorThreatsExist("lastKnownPosition"),
                new BehaviorSequence(
                    new BehaviorResourceCheck(ResourceDef.Health, BehaviorResourceCheck.Comparison.Less, .5f),
                    new BehaviorFlee()),

                new BehaviorFindEquipment(),

                new BehaviorInverter(new BehaviorChaseThreat()), //working
                //new BehaviorPathTo("lastKnownPosition"), // working but pauses each time a path is calculated

                ///notworking
                //new BehaviorGetAt("lastKnownPosition", Components.Attack.DefaultRange),

                //new BehaviorInverter(
                //    new BehaviorSelector(
                //        new BehaviorQueue(
                //            new BehaviorInverter(new BehaviorLineOfSight("lastKnownPosition")),
                //            new BehaviorChaseTarget("lastKnownPosition")),
                //        new BehaviorPathTo("lastKnownPosition"))),

                //new BehaviorSelector(
                //    new BehaviorInverter(new BehaviorLineOfSight("lastKnownPosition")),
                //    new BehaviorPathTo("lastKnownPosition")),

                // working
                //new BehaviorInverter(
                //    new BehaviorSelector(
                //        new BehaviorQueue(
                //            new BehaviorInverter(new BehaviorLineOfSight("lastKnownPosition")),
                //            new BehaviorChaseTarget("lastKnownPosition")),
                //        new BehaviorChaseTarget("lastKnownPosition", .1f))),
                ///


                new BehaviorSelector(
                    new BehaviorAttackDeliver(),
                    new BehaviorAttackCharge()),

                new BehaviorSucceeder()
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
    }
}
