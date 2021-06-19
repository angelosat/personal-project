using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptDropOnTarget : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Types.DropOnTarget;
            }
        }
        public override string Name
        {
            get
            {
                return "DropOnTarget";
            }
        }

        public ScriptDropOnTarget()
        {
            this.AddComponent(new ScriptTargetFilter(t => t.IsBlock()));
            //this.AddComponent(new ScriptMatch(this));
        }
        public override void OnStart(ScriptArgs args)
        {
            var ani = AnimationCollection.ManipulateItem;
            args.Actor.Body.CrossFade(ani, true, 10);
            ani[Bone.Types.RightHand].OnFadeIn = () =>
            {
                //args.Net.Spawn(args.Actor.GetComponent<GearComponent>().Holding.Take(), args.Target.FinalGlobal);
                args.Net.Spawn(args.Actor.GetComponent<HaulComponent>().Holding.Take(), args.Target.FinalGlobal);

                args.Actor.Body.FadeOut(AnimationCollection.ManipulateItem);
                Finish(args);
            };
        }

        //public override void OnStart(ScriptArgs args)
        //{
        //   // args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);
        //    //args.Actor.Avatar.Find(AnimationCollection.Hauling.Name, "Right Hand").OnFaded = () =>

        //    //Animation.Start(args.Actor, AnimationCollection.ManipulateItem, 1f, onFinish: new Dictionary<string, Action>() { { "Right Hand", () =>
        //    Animation.CrossFade(args.Actor, AnimationCollection.ManipulateItem, 0.1f, 1f, onFinish: new Dictionary<string, Action>() { { "Right Hand", () =>
        //    {
        //        args.Net.Spawn(args.Actor.GetComponent<PersonalInventoryComponent>().Holding.Take(), args.Target.FinalGlobal);
        //        //args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);
        //        args.Actor.Avatar.FadeOut(AnimationCollection.ManipulateItem);
        //        Finish(args);
        //    }}});
        //  //  args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);


        //    //Animation.Start(args.Actor, AnimationCollection.DropHands, 1f, onFinish: new Dictionary<string, Action>() { { "Right Hand", () =>
        //    //{
        //    //    args.Net.Spawn(args.Actor.GetComponent<PersonalInventoryComponent>().Holding.Take(), args.Target.FinalGlobal);
        //    //    //this.ScriptState = ScriptState.Finished;
        //    //    this.Finish(args);
        //    //    args.Actor.Avatar.FadeOut(AnimationCollection.DropHands);
        //    //    args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);
        //    //} } });

        //    //Animation.Start(args.Actor, AnimationCollection.Idle, 1f, onFinish: new Dictionary<string, Action>() { { "Right Hand", () =>
        //    //{
        //    //    args.Net.Spawn(args.Actor.GetComponent<PersonalInventoryComponent>().Holding.Take(), args.Target.FinalGlobal);
        //    //    //this.ScriptState = ScriptState.Finished;
        //    //    args.Actor.Avatar.FadeOut(AnimationCollection.Idle);
        //    //    args.Actor.GetComponent<ControlComponent>().FinishScript(Types.Hauling, args);
        //    //    Finish(args);
        //    //} } });
        //}
        public override object Clone()
        {
            return new ScriptDropOnTarget();
        }
    }
}
