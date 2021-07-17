using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Blocks.Bed
{
    class InteractionStartSleep : Interaction
    {
        public InteractionStartSleep()
            : base("Sleep in bed")
        {

        }
        public override void Perform(Actor a, TargetArgs t)
        {
            var body = a.Body;
            var headBone = a.Body.FindBone(BoneDef.Head);

            body.RestingFrame = new Keyframe(0, a.Body[BoneDef.Head].GetTotalOffset(), 0);
            body.SetEnabled(false, true);
            headBone.SetEnabled(true, false);
            headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
            var bedPos = BlockBed.GetPartsDic(a.Map, t.Global)[BlockBed.Part.Top];
            a.SetPosition(bedPos + new Vector3(0, 0, BlockBed.GetBlockHeight(a.Map, bedPos)));
            a.GetNeed(NeedDef.Energy).Mod += 1;
        }
       
        public override object Clone()
        {
            return new InteractionStartSleep();
        }
    }
    class InteractionStopSleep : Interaction
    {
        public InteractionStopSleep()
            : base("Stop sleep in bed")
        {

        }
        public override void Perform(Actor a, TargetArgs t)
        {
            var body = a.Body;
            var head = body[BoneDef.Head];
            body.SetEnabled(true, true);
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            a.GetNeed(NeedDef.Energy).Mod -= 1;
        }
        public override object Clone()
        {
            return new InteractionStopSleep();
        }
    }
}
