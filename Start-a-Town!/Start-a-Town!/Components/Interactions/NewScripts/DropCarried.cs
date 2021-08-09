using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    [Obsolete]
    class DropCarried : Interaction
    {
        bool All;

        public DropCarried(bool all = false)
            : base(
            "Drop",
            0
            )
        {
            this.All = all;
        }
       
        public override void Perform()
        {
            this.Actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, this.Actor, this.All);
        }

        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.All ? " All" : "");
        }

        public override object Clone()
        {
            return new DropCarried(this.All);
        }
    }
}
