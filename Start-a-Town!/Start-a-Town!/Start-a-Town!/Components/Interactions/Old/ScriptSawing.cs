using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptSawing : Script
    {
        public ScriptSawing()
        {
            this.ID = Script.Types.Sawing;
            this.Name = "Sawing";
            //this.BaseTimeInSeconds = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.Default);
            this.RangeCheck = DefaultRangeCheck;
            this.Conditions = 
                new ConditionCollection(
                    new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<InventoryComponent>().Holding.Object, Script.Types.Sawing), "Requires appropriate tool")
                    );
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Sawing", 3));//, this.Success));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptModifyDurability(-1));
            this.AddComponent(new ScriptEvaluations((a) => this.ScriptState = Components.ScriptState.Finished,
                //new ScriptEvaluation(
                //    a => a.Target.Object.ID == GameObject.Types.Log,
                //    a => a.Net.EventOccured(Message.Types.InvalidTarget, a.Actor)),
                new ScriptEvaluation(
                    a => DefaultRangeCheck(a.Actor, a.Target, this.RangeValue),
                    a => a.Net.EventOccured(Message.Types.OutOfRange, a.Actor))
                )
            );
        }
        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Saw, new object[] { args.Actor }));
            //SkillsComponent.AwardSkill(args.Net, args.Actor, Skill.Types.Carpentry, 1);
            SkillOld.Award(args.Net, args.Actor, args.Target.Object, SkillOld.Types.Carpentry, 1);
        }

        public override object Clone()
        {
            return new ScriptSawing();
        }
    }
}
