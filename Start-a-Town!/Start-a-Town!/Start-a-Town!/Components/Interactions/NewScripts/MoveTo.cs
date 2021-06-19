using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class MoveTo : Interaction
    {
        public override object Clone()
        {
            return new MoveTo(this);
        }

        public float RangeMin, RangeMax;

        public MoveTo(float rangeMin = 0, float rangeMax = 0):base("Moving", 0)
        {
            //this.Range = 0;
        }
        public MoveTo(MoveTo toCopy):this(toCopy.RangeMin, toCopy.RangeMax)
        {
            
        }

        public override void Update(GameObject actor, TargetArgs target)
        {
            //base.Update(actor, target);
            Vector3 distanceVector = target.Global - actor.Global;
            if(distanceVector.Length() <= this.RangeMax)
            {
                this.State = States.Finished;
                return;
            }

            if (this.State == States.Unstarted)
            {
                actor.TryGetComponent<MobileComponent>(c => c.Start(actor));
                this.State = States.Running;
            }

            Vector2 directionNormal = distanceVector.XY();
            directionNormal.Normalize();
            actor.Transform.Direction = directionNormal;

        }
    }
}
