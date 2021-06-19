using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptTilling : Script
    {
        //public ScriptArgs Args { get; set; }
        public double Time { get; set; }
        public double Length { get; set; }
        public ScriptTilling()
        {
            this.ID = Script.Types.Tilling;
            this.Name = "Tilling";
            //this.BaseTimeInSeconds = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.TillingSpeed);
            this.Conditions =
                new ConditionCollection(
                    new Condition((actor, target) =>
                    {
                        //return UseComponentOld.HasScript(actor.GetComponent<GearComponent>().Holding.Object, Script.Types.Tilling);
                        return UseComponentOld.HasScript(actor.GetComponent<HaulComponent>().Holding.Object, Script.Types.Tilling);

                    }, "Requires appropriate tool")
                //new InteractionCondition((actor, target) => UseComponent.HasAbility(actor.GetComponent<InventoryComponent>().Holding.Object, Script.Types.Tilling), "Requires appropriate tool")
                    );
            //this.AddComponent(new ScriptTimer(5000, this.Success));
            //this.AddComponent(new ScriptTimer(a=>GetTimeInMs(a.Actor), "Tilling", 3, this.Success));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Tilling", 3));//, this.Success));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptModifyDurability(-1));
            //this.AddComponent(new ScriptTargetFilter(o => o.ID == GameObject.Types.Grass));
            this.AddComponent(new ScriptEvaluations((a) => this.ScriptState = Components.ScriptState.Finished,
                new ScriptEvaluation(
                    a => a.Target.Object.ID == GameObject.Types.Grass,
                    a => a.Net.EventOccured(Message.Types.InvalidTarget, a.Actor)),
                new ScriptEvaluation(
                    a => DefaultRangeCheck(a.Actor, a.Target, this.RangeValue),
                    a => a.Net.EventOccured(Message.Types.OutOfRange, a.Actor))
                )
            );


            //this.AddComponent(new ScriptEvaluation(a =>
            //{ 
            //    return a.Target.Object.ID == GameObject.Types.Grass; 
            //}, a =>
            //{
            //    a.Net.EventOccured(Message.Types.InvalidTarget, a.Actor);
            //    this.ScriptState = Components.ScriptState.Finished;
            //}));
            //this.AddComponent(new ScriptEvaluation(a => DefaultRangeCheck(a.Actor, a.Target, this.RangeValue), a =>
            //{
            //    a.Net.EventOccured(Message.Types.OutOfRange, a.Actor);
            //    this.ScriptState = Components.ScriptState.Finished;
            //}));
        }

        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Till, new object[] { args.Actor }));
        }

        public override object Clone()
        {
            return new ScriptTilling();
        }
    }
}
