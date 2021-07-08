using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    [Obsolete]
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
}
