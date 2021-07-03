using System;
using System.Collections.Generic;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionObserve : Interaction
    {
        public InteractionObserve():base("Observe", 4)
        {
            this.Animation = null;
        }

        static readonly Dictionary<Need.Types, float> satisfyneed = new Dictionary<Need.Types, float>() { { Need.Types.Curiosity, 50 } };
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    RangeCheck.Sqrt2,
                    new Exists()
                    ));

        public override TaskConditions Conditions => conds;
        public override Dictionary<Need.Types, float> NeedSatisfaction => satisfyneed;
        public override void Perform(GameObject a, TargetArgs t)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            return new InteractionObserve();
        }
    }
}
