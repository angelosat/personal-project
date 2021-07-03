using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_
{
    //public class Chopping : InteractionPerpetual
    //{
    //    public Chopping()
    //        : base("Chop")
    //    {
    //        this.Verb = "Chopping";
    //        this.Skill = Skill.Chopping;
    //    }
    //    static readonly TaskConditions conds = new TaskConditions(
    //                new AllCheck(
    //                    new RangeCheck(),
    //                    new SkillCheck(Skill.Chopping),
    //            new AnyCheck(
    //                        new AllCheck(
    //                            new TargetTypeCheck(TargetType.Position),
    //                            new ScriptTaskCondition("Material", (a, t) =>
    //                            {
    //                                //var block = a.Map.GetBlock(t.Global);
    //                                //return block.MaterialType == MaterialType.Wood;
    //                                return Block.GetBlockMaterial(a.Map, t.Global).Type == MaterialType.Wood;
    //                            })
    //                            ),
    //                        new AllCheck(
    //                            new TargetTypeCheck(TargetType.Entity),
    //                            new ScriptTaskCondition("Material", (a, t) =>
    //                            {
    //                                return t.Object.Body.Material != null && t.Object.Body.Material.Type == MaterialType.Wood;
    //                            }
    //                                )
    //                            )
    //            )));
    //    public override TaskConditions Conditions
    //    {
    //        get
    //        {
    //            return conds;
    //        }
    //    }
    //    public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
    //    {
    //        return this.Conditions.GetFailedCondition(actor, target) == null;
    //    }

    //    public override void Perform(GameObject a, TargetArgs t)
    //    {
    //        var server = a.Net as Server;
    //        //switch (t.Type)
    //        //{
    //        //    case TargetType.Position:
    //        var cell = a.Map.GetCell(t.Global);
    //        var block = cell.Block;// a.Map.GetBlock(t.Global);
    //        block.Break(a, t.Global);
    //        return;
    //        //if (block.MaterialType != MaterialType.Mineral)
    //        //    return;
    //        var material = block.GetMaterial(cell.BlockData);
    //        if (server != null)
    //        {
    //            var factory = Materials.MaterialType.Planks;// new Materials.RawMaterials.Rocks();
    //            //var entity = factory.Create(material);
    //            var entity = Materials.MaterialType.RawMaterial.Create(material);

    //            server.PopLoot(entity, t.Global, Vector3.Zero);
    //        }

    //        block.Remove(a.Map, t.Global);//.Break(a.Net, t.Global);

    //        //var tool = a.GetComponent<GearComponent>().Holding.Object;
    //        var tool = a.GetComponent<HaulComponent>().Holding.Object;

    //        tool.GetComponent<EquipComponent>().Durability.Add(-1);


    //        var emitters = WorkComponent.GetEmitters(a);
    //        if (emitters == null)
    //            return;
    //        if (t.Type != TargetType.Position)
    //            return;
    //        var e = block.GetEmitter();
    //        e.Source = t.Global + Vector3.UnitZ * 0.5f;
    //        e.SizeBegin = 1;
    //        e.SizeEnd = 1;
    //        e.ParticleWeight = 1;
    //        e.Radius = 1f;// .5f;
    //        e.Force = .1f;
    //        e.Friction = .5f;
    //        e.AlphaBegin = 1;
    //        e.AlphaEnd = 0;
    //        e.ColorBegin = material.Color;// Color.White;
    //        e.ColorEnd = material.Color;//Color.White;

    //        e.Lifetime = Engine.TargetFps * 2;
    //        var pieces = block.GetParticleRects(25);
    //        e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
    //        emitters.Add(e);
    //        // break;

    //        //    case TargetType.Entity:
    //        //        if (server != null)
    //        //        {
    //        //            var factory = Materials.MaterialType.Planks;// new Materials.RawMaterials.Rocks();
    //        //            material = t.Object.Body.Material;
    //        //            var entity = factory.Create(material);
    //        //            server.PopLoot(entity, t.Global, Vector3.Zero);
    //        //        }
    //        //        break;

    //        //    default:
    //        //        break;
    //        //}
    //    }

    //    protected override void Contact(GameObject actor, TargetArgs target)
    //    {
    //        // TODO: add an emitter at the equipped tool's tip, in a defined position in the mainhand bone (but that can't accept a vector3 as parameter)

    //        //var emitters = WorkComponent.GetEmtters(actor);
    //        //if (emitters == null)
    //        //    return;
    //        //if (target.Type != TargetType.Position)
    //        //    return;
    //        //var e = actor.Map.GetBlock(target.Global).GetEmitter();
    //        //var offset = target.FaceGlobal - target.Global;
    //        //offset.Normalize();
    //        //e.Source = target.FaceGlobal + offset * .01f;
    //        //e.Emit(10);
    //        //emitters.Add(e);
    //    }

    //    public override object Clone()
    //    {
    //        return new Chopping();
    //    }
    //}

    public class InteractionChopping : Interaction
    {
        public InteractionChopping()
            : base(
                "Chop",
                1
                )
        {
            this.Verb = "Chopping";
            this.Skill = ToolAbilityDef.Chopping;
        }
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new RangeCheck(),
                new AnyCheck(
                            new AllCheck(
                                new TargetTypeCheck(TargetType.Position),
                                new ScriptTaskCondition("Material", (a, t) =>
                                {
                                    //var block = a.Map.GetBlock(t.Global);
                                    //return block.MaterialType == MaterialType.Wood;
                                    return Block.GetBlockMaterial(a.Map, t.Global).Type == MaterialType.Wood;
                                })
                                ),
                            new AllCheck(
                                new TargetTypeCheck(TargetType.Entity),
                                new ScriptTaskCondition("Material", (a, t) =>
                                {
                                    return t.Object.Body.Material != null && t.Object.Body.Material.Type == MaterialType.Wood;
                                }
                                    )
                                )
                )));
        //public override TaskConditions Conditions
        //{
        //    get
        //    {
        //        return conds;
        //    }
        //}
        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;
        }

        public override void Perform(GameObject a, TargetArgs t)
        {


            var server = a.Net as Net.Server;
            //switch (t.Type)
            //{
            //    case TargetType.Position:
            var cell = a.Map.GetCell(t.Global);
            var block = cell.Block;// a.Map.GetBlock(t.Global);
            block.Break(a, t.Global);
            //return;
            ////if (block.MaterialType != MaterialType.Mineral)
            ////    return;
            //var material = block.GetMaterial(cell.BlockData);
            //if (server != null)
            //{
            //    var factory = Materials.MaterialType.Planks;// new Materials.RawMaterials.Rocks();
            //    //var entity = factory.Create(material);
            //    var entity = Materials.MaterialType.RawMaterial.Create(material);

            //    server.PopLoot(entity, t.Global, Vector3.Zero);
            //}

            //block.Remove(a.Map, t.Global);//.Break(a.Net, t.Global);

            ////var tool = a.GetComponent<GearComponent>().Holding.Object;
            //var tool = a.GetComponent<HaulComponent>().Holding.Object;

            //tool.GetComponent<EquipComponent>().Durability.Add(-1);


            ////var emitters = WorkComponent.GetEmitters(a);
            ////if (emitters == null)
            ////    return;
            //if (t.Type != TargetType.Position)
            //    return;
            //var e = block.GetEmitter();
            //e.Source = t.Global + Vector3.UnitZ * 0.5f;
            //e.SizeBegin = 1;
            //e.SizeEnd = 1;
            //e.ParticleWeight = 1;
            //e.Radius = 1f;// .5f;
            //e.Force = .1f;
            //e.Friction = .5f;
            //e.AlphaBegin = 1;
            //e.AlphaEnd = 0;
            //e.ColorBegin = material.Color;// Color.White;
            //e.ColorEnd = material.Color;//Color.White;
            //e.Rate = 0;

            //e.Lifetime = Engine.TicksPerSecond * 2;
            //var pieces = block.GetParticleRects(25);
            //e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
            ////emitters.Add(e);
            //a.Map.EventOccured(Message.Types.ParticleEmitterAdd, e);

            //// break;

            ////    case TargetType.Entity:
            ////        if (server != null)
            ////        {
            ////            var factory = Materials.MaterialType.Planks;// new Materials.RawMaterials.Rocks();
            ////            material = t.Object.Body.Material;
            ////            var entity = factory.Create(material);
            ////            server.PopLoot(entity, t.Global, Vector3.Zero);
            ////        }
            ////        break;

            ////    default:
            ////        break;
            ////}
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
            return new InteractionChopping();
        }
    }
}
