using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Interactions
{
    public class Mining : Interaction
    {
        public Mining()
            : base(
                "Mine",
                1
                )
        {
            this.Verb = "Mining";
            this.Skill = Skill.Mining;
        }
        static readonly TaskConditions conds = new TaskConditions(
            new AllCheck(
                new AllCheck(
                    new TargetTypeCheck(TargetType.Position),
                    new ScriptTaskCondition("Material", IsMetalOrMineral)),
                new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                new SkillCheck(Skill.Mining)
                ));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        static bool IsMetalOrMineral(GameObject a, TargetArgs t)
        {
            var mat = Block.GetBlockMaterial(a.Map, t.Global);
            return mat.Type == MaterialType.Mineral || mat.Type == MaterialType.Metal;
        }

        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return new SkillCheck(Skill.Mining).Condition(actor, target);
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            var cell = a.Map.GetCell(t.Global);
            var block = cell.Block;// a.Map.GetBlock(t.Global);

            //if (block.MaterialType != MaterialType.Mineral)
            if (!IsMetalOrMineral(a, t))// block.GetMaterial(cell.BlockData).Type != MaterialType.Mineral)
                return;
            var material = block.GetMaterial(cell.BlockData);
            var server = a.Net as Net.Server;
            if(server!=null)
            {
                var factory = Materials.MaterialType.Rocks;// new Materials.RawMaterials.Rocks();
                //var entity = factory.Create(material);
                var entity = Materials.MaterialType.RawMaterial.Create(material);

                server.PopLoot(entity, t.Global, Vector3.Zero);
            }

            block.Remove(a.Map, t.Global);//.Break(a.Net, t.Global);

            //var tool = a.GetComponent<GearComponent>().Holding.Object;
            //var tool = a.GetComponent<HaulComponent>().Holding.Object;
            var tool = GearComponent.GetSlot(a, GearType.Mainhand).Object;
            //tool.GetComponent<EquipComponent>().Durability.Add(-1);


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
            e.ColorBegin = material.Color;// Color.White;
            e.ColorEnd = material.Color;//Color.White;
            
            e.Lifetime = Engine.TargetFps * 2;
            var pieces = block.GetParticleRects(25);
            e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
            emitters.Add(e);
        }

        protected override void Contact(GameObject actor, TargetArgs target)
        {
            // TODO: add an emitter at the equipped tool's tip, in a defined position in the mainhand bone (but that can't accept a vector3 as parameter)

            //var emitters = WorkComponent.GetEmtters(actor);
            //if (emitters == null)
            //    return;
            //if (target.Type != TargetType.Position)
            //    return;
            //var e = actor.Map.GetBlock(target.Global).GetEmitter();
            //var offset = target.FaceGlobal - target.Global;
            //offset.Normalize();
            //e.Source = target.FaceGlobal + offset * .01f;
            //e.Emit(10);
            //emitters.Add(e);
        }

        public override object Clone()
        {
            return new Mining();
        }
    }
}
