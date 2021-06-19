using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Interactions
{
    public class InteractionDigging : Interaction
    {
        public InteractionDigging()
            : base(
                "Dig",
                1
                )
        {
            this.Verb = "Digging";
            this.Skill = Skill.Digging;
            //this.CancelState = new Start_a_Town_.AI.AIGoalState("IsSoil", (a, t) =>
            //{
            //    var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
            //    return blockmat.Type == MaterialType.Soil;
            //});
        }

        static readonly ScriptTaskCondition cancel = new ScriptTaskCondition("IsSoil", (a, t) =>
        {
            var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
            return blockmat.Type == MaterialType.Soil;
        });
        public override ScriptTaskCondition CancelState
        {
            get
            {
                return cancel;
            }
        }
        static readonly TaskConditions conds = 
            new TaskConditions(
                new AllCheck(
                    new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                    new ScriptTaskCondition("Material", (a, t) =>
                    {
                        var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
                        return blockmat.Type == MaterialType.Soil;
                    }),
                    new SkillCheck(Skill.Digging)
                    ));
        //static readonly TaskConditions conds = new TaskConditions(
        //    new AllCheck(
        //        new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
        //        new AllCheck(
        //            new TargetTypeCheck(TargetType.Position),
        //            new ScriptTaskCondition("Material", (a, t) =>
        //                {
        //                    var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
        //                    return blockmat.Type == MaterialType.Soil;
        //                })),
        //        new AnyCheck(
        //            new SkillCheck(Skill.Digging),
        //            new ScriptTaskCondition("EmptyHands", (a, t) => GearComponent.GetSlot(a, GearType.Mainhand).Object == null))
        //        ));

        public override TaskConditions Conditions
        {
	        get 
	        { 
		         return conds;
	        }
        }
        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;
            return new SkillCheck(Skill.Digging).Condition(actor, target);
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            //t.Global.GetBlock(a.Map).Break(a.Net, t.Global);
            
            var block = a.Map.GetBlock(t.Global);
            block.Break(a.Map, t.Global);
            //block.Break(a, t.Global);
            //block.Remove(a.Map, t.Global);
            //Block.Air.Place(a.Map, t.Global, 0, 0, 0);


            //var tool = a.GetComponent<GearComponent>().Holding.Object;
            //var tool = a.GetComponent<HaulComponent>().Holding.Object;
            var tool = GearComponent.GetSlot(a, GearType.Mainhand).Object;

            //if (tool != null)
            //    tool.GetComponent<EquipComponent>().Durability.Add(-1);


            var emitters = WorkComponent.GetEmitters(a);
            if (emitters == null)
                return;
            if (t.Type != TargetType.Position)
                return;
            var e = block.GetEmitter();
            e.Source = t.Global + Vector3.UnitZ * 0.5f;
            e.SizeBegin = 1;
            e.SizeEnd = 1;
            e.ParticleWeight = 1;
            e.Radius = 1f;// .5f;
            e.Force = .1f;
            e.Friction = .5f;
            e.AlphaBegin = 1;
            e.AlphaEnd = 0;

            e.ColorBegin = Color.White;
            e.ColorEnd = Color.White;

            e.Lifetime = Engine.TargetFps * 2;
            var pieces = block.GetParticleRects(25);
            e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
            emitters.Add(e);
        }

        public override object Clone()
        {
            return new InteractionDigging();
        }
    }
}
