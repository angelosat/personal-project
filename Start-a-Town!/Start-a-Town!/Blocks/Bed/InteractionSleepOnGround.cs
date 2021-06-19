using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class InteractionSleepOnGround : Interaction
    {
        public InteractionSleepOnGround()
            : base("Sleeping on ground")
        {
            this.RunningType = RunningTypes.Continuous;
            this.Animation = null;
        }
        internal override void InitAction(GameObject a, TargetArgs t)
        {
            //a.GetNeed(NeedDef.Energy).Mod += 1;
            a.GetNeed(NeedDef.Energy).AddMod(NeedLetDefOf.Sleeping, 0, 1);
            a.GetNeed(NeedDef.Comfort).AddMod(NeedLetDefOf.Sleeping, -20, 0);

            var body = a.Body;
            body.RestingFrame = new Keyframe(0, Vector2.Zero, (float)(Math.PI / 2f)); //(float)(Math.PI / 3f));
            body.OriginGroundOffset = Vector2.Zero;
        }
        internal override void FinishAction(GameObject a, TargetArgs t)
        {
            //a.GetNeed(NeedDef.Energy).Mod -= 1;
            a.GetNeed(NeedDef.Energy).RemoveMod(NeedLetDefOf.Sleeping);
            a.GetNeed(NeedDef.Comfort).RemoveMod(NeedLetDefOf.Sleeping);
            var body = a.Body;
            body.RestingFrame = new Keyframe(0, Vector2.Zero, 0);
            body.OriginGroundOffset = (a as Actor).Def.Body.OriginGroundOffset;
        }
        public override object Clone()
        {
            return new InteractionSleepOnGround();
        }
        //public override void Write(System.IO.BinaryWriter w)
        //{
        //    w.Write(this.PreviousStandingPosition);
        //}
        //public override void Read(System.IO.BinaryReader r)
        //{
        //    this.PreviousStandingPosition = r.ReadVector3();
        //}
        //protected override void AddSaveData(SaveTag tag)
        //{
        //    tag.Add(this.PreviousStandingPosition.SaveOld("PreviousStandingPosition"));
        //}
        //public override void LoadData(SaveTag tag)
        //{
        //    this.PreviousStandingPosition = tag["PreviousStandingPosition"].LoadVector3();
        //}
    }
}
