using System;

namespace Start_a_Town_
{
    class InteractionDepart : Interaction
    {
        public override void Perform(Actor a, TargetArgs t)
        {
            a.Despawn();
            if(a.GetVisitorProperties() is VisitorProperties props)
                props.OffsiteArea = OffsiteAreaDefOf.Forest;
            a.Net.Report($"{a.Name} has departed!");
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
