using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Blocks.Bed
{
    class InteractionStartSleep : Interaction
    {
        public InteractionStartSleep()
            : base("Sleep in bed")
        {

        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            //var bedentity = a.Map.GetBlock(t.Global).GetBlockEntity(a.Map, t.Global) as Blocks.BlockBed.Entity;
            //bedentity.ToggleSleep(a, t.Global);

            var body = a.Body;
            //var headBone = a.Body.Joints[Graphics.BoneDef.Head].Bone;
            var headBone = a.Body.FindBone(BoneDef.Head);

            body.RestingFrame = new Keyframe(0, a.Body[BoneDef.Head].GetTotalOffset(), 0);
            body.SetEnabled(false, true);
            headBone.SetEnabled(true, false);
            headBone.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
            var bedPos = BlockBed.GetPartsDic(a.Map, t.Global)[BlockBed.Part.Top];
            a.ChangePosition(bedPos + new Vector3(0, 0, BlockBed.GetBlockHeight(a.Map, bedPos)));
            a.GetNeed(NeedDef.Energy).Mod += 1;
        }
        //public override void Interrupt(GameObject parent)
        //{
        //    base.Interrupt(parent);
        //}
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
        public override void Perform(GameObject a, TargetArgs t)
        {
            //var bedentity = a.Map.GetBlock(t.Global).GetBlockEntity(a.Map, t.Global) as Blocks.BlockBed.Entity;
            //bedentity.ToggleSleep(a, t.Global);

            var spriteComp = a.GetComponent<SpriteComponent>();
            var body = a.Body;
            var head = body[BoneDef.Head];
            body.SetEnabled(true, true);
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            head.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            a.GetNeed(NeedDef.Energy).Mod -= 1;
        }
        //public override void Interrupt(GameObject parent)
        //{
        //    base.Interrupt(parent);
        //}
        public override object Clone()
        {
            return new InteractionStopSleep();
        }
    }
}
