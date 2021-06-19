using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptChopping : Script
    {
        //public ScriptArgs Args { get; set; }

        public ScriptChopping()
        {
            this.ID = Script.Types.Chopping;
            this.Name = "Chopping";
            this.BaseTimeInSeconds = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.ChoppingSpeed);
            this.RangeCheck = DefaultRangeCheck;
            //this.Conditions =
            //    new ConditionCollection(
            //        new Condition((actor, target) => UseComponent.HasAbility(actor.GetComponent<PersonalInventoryComponent>().Holding.Object, Script.Types.Chopping), "Requires appropriate tool", new AI.AIPrecondition[]{
            //            new AI.AIPrecondition(
            //                obj => UseComponent.HasAbility(obj, Script.Types.Chopping),
            //                Types.Equipping)})
            //              //  script => script == Types.Equipping)})
            //        );
            //this.AddComponent(new ScriptMatch(this));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Chopping", 3));//, this.Success));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptEvaluations((a) => this.ScriptState = Components.ScriptState.Finished,
                new ScriptEvaluation(
                //a => a.Target.Object.ID == GameObject.Types.Cobblestone,
                    a =>
                    {
                        Block block = a.Net.Map.GetCell(a.Target.Object.Global).Block;// Block.Registry[a.Target.Object.Global.GetCell(a.Net.Map).Type];
                        if (block.Material.IsNull())
                            return false;
                        return block.Material.Density <= 0.5f;
                    },
                    a => a.Net.EventOccured(Message.Types.TooDense, a.Actor)),
                new ScriptEvaluation(
                    a => DefaultRangeCheck(a.Actor, a.Target, this.RangeValue),
                    a => a.Net.EventOccured(Message.Types.OutOfRange, a.Actor))
                )
            );
        }

        //public override void Start(ScriptArgs args)
        //{
        //    base.Start(args);

        //    this.AddComponent(ScriptTimer2.StartNew(GetTimeInMs(args.Actor), () => Success(this.ArgsSnapshot)));
        //    ControlComponent control = args.Actor.GetComponent<ControlComponent>();
        //    this.AddComponent(new ScriptCondition(() => control.RunningScripts.ContainsKey(Script.Types.Walk), () => Stop(args)));

        //    this.ArgsSnapshot = args;

        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        //}

        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Chop, new object[] { args.Actor }));
        }

        //public override void Finish(ScriptArgs args)
        //{
        //    base.Finish(args);
        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        //}

        //public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        //{
        //    ScriptTimer timer = this.ScriptComponents.FirstOrDefault(c => c is ScriptTimer) as ScriptTimer;
        //    if (timer.IsNull())
        //        return;
        //    timer.DrawUI(sb, camera, parent);
        //}

        public override object Clone()
        {
            return new ScriptChopping();
        }
    }
}
