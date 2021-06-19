using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptBuild : Script
    {
        public double Time { get; set; }
        public double Length { get; set; }
        public ScriptBuild()
        {
            this.ID = Script.Types.Build;
            this.Name = "Building";
            this.BaseTimeInSeconds = 1;
            this.RangeCheck = DefaultRangeCheck;
            this.Conditions = 
                new ConditionCollection(
                    //new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<GearComponent>().Holding.Object, Script.Types.Build), "Requires appropriate tool")
                    new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<HaulComponent>().Holding.Object, Script.Types.Build), "Requires appropriate tool")

                    );
            this.AddComponent(new ScriptTargetTypeFilter(t => t.Type == TargetType.Entity));
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(
                    a =>
                    {
                        //return a.Target.Object.GetComponent<ConstructionFootprint>().Ready;
                        //return a.Target.Object.GetComponent<StructureComponent>().DetectMaterials(a.Target.Object, a.Actor);
                        return a.Target.Object.GetComponent<ConstructionComponent>().DetectMaterials(a.Target.Object, a.Actor);
                    }, Message.Types.InvalidTarget
                    )));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Building", 1));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
        }

        public override void OnSuccess(ScriptArgs args)
        {
            //args.Target.Object.GetComponent<ConstructionFootprint>().Finish(args.Net, args.Target.Object, args.Actor);
            //args.Target.Object.GetComponent<StructureComponent>().Finish(args.Target.Object, args.Actor);
            args.Target.Object.GetComponent<ConstructionComponent>().Finish(args.Target.Object, args.Actor);
        }

        public override object Clone()
        {
            return new ScriptBuild();
        }
    }
}
