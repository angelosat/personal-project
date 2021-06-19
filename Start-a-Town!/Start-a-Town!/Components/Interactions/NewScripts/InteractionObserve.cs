using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionObserve : Interaction
    {
        public InteractionObserve():base("Observe", 4)
        {
            this.Animation = null;
            //this.NeedSatisfaction = new HashSet<Need.Types>() { Need.Types.Curiosity };
            //this.NeedSatisfaction.Add(Need.Types.Curiosity, 50);
        }

        static readonly Dictionary<Need.Types, float> satisfyneed = new Dictionary<Need.Types, float>() { { Need.Types.Curiosity, 50 } };
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    RangeCheck.Sqrt2,
                    new Exists()
                    ));

        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override Dictionary<Need.Types, float> NeedSatisfaction
        {
            get
            {
                return satisfyneed;
            }
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            throw new Exception();//
            //NeedsComponent.ModifyNeed(a, Need.Types.Curiosity, 50);
            //SpeechComponent.Say(a, "What a nice " + t.Object.Name);
        }

        public override object Clone()
        {
            return new InteractionObserve();
        }
    }
}
