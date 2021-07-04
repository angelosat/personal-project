using System;

namespace Start_a_Town_
{
    class InteractionDepart : Interaction
    {
        public override void Perform(GameObject a, TargetArgs t)
        {
            var actor = a as Actor;
            a.Despawn();
            if(actor.GetVisitorProperties() is VisitorProperties props)
                props.OffsiteArea = OffsiteAreaDefOf.Forest;
            a.Net.Report($"{a.Name} has departed!");
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
