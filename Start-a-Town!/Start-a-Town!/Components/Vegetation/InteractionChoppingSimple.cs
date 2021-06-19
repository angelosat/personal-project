using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;


namespace Start_a_Town_.Components
{
    public class InteractionChoppingSimple : InteractionPerpetual
    {
        static int MaxStrikes = 3;
        int StrikeCount = 0;
        ParticleEmitterSphere EmitterStrike;
        List<Rectangle> ParticleRects;
        GameObject Logs;
        public InteractionChoppingSimple()
            : base("Chopping")
        { }
        static readonly ScriptTaskCondition cancel = new Exists();
        static readonly TaskConditions conds = new TaskConditions(
                    new AllCheck(
                        new Exists(),
                        new RangeCheck()
            //,
            //new SkillCheck(Skills.Skill.Chopping)
                ));
        public override ScriptTaskCondition CancelState
        {
            get
            {
                return cancel;
            }
            set
            {
                base.CancelState = value;
            }
        }
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            base.Start(a, t);
            // cache variables
            //this.EmittersList = WorkComponent.GetEmitters(a);

            this.EmitterStrike = new ParticleEmitterSphere();
            this.EmitterStrike.Source = t.Global + Vector3.UnitZ;
            this.EmitterStrike.SizeBegin = 1;
            this.EmitterStrike.SizeEnd = 1;
            this.EmitterStrike.ParticleWeight = 1;
            this.EmitterStrike.Radius = 1f;// .5f;
            this.EmitterStrike.Force = .1f;
            this.EmitterStrike.Friction = .5f;
            this.EmitterStrike.AlphaBegin = 1;
            this.EmitterStrike.AlphaEnd = 0;
            this.EmitterStrike.ColorBegin = MaterialDefOf.LightWood.Color;
            this.EmitterStrike.ColorEnd = MaterialDefOf.LightWood.Color;
            this.EmitterStrike.Lifetime = Engine.TicksPerSecond * 2;
            this.EmitterStrike.Rate = 0;

            //this.Logs = MaterialType.RawMaterial.Create(t.Object.Body.Material);
            //this.ParticleRects = GameObject.Objects[Logs.IDType].Body.Sprite.AtlasToken.Rectangle.Divide(25);

            //this.Logs = ItemFactory.CreateFrom(RawMaterialDef.Logs, t.Object.Body.Material);// MaterialType.RawMaterial.Create(t.Object.Body.Material);
            this.ParticleRects = ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            if (a.Net is Net.Client)
            {
                //this.EmitterStrike.Emit(this.Logs.Body.Sprite.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            this.StrikeCount++;
            if (this.StrikeCount < MaxStrikes)
            {
                return;
            }
            this.Done(a, t);
            //this.State = States.Finished;
            this.Finish(a, t);
        }
        public void Done(GameObject a, TargetArgs t)
        {
            //TreeComponent.ChopDown(a, t.Object);
            CutDownPlant(a as Actor, t.Object as Plant);
        }
        public override object Clone()
        {
            return new InteractionChoppingSimple();
        }

        static public void CutDownPlant(Actor actor, Plant plant)
        {
            //var comp = parent.GetComponent<TreeComponent>();
            var comp = plant.PlantComponent;/// parent.GetComponent<PlantComponent>();
            comp.Harvest(plant, actor);
            comp.CutDown(plant, actor);
            //var plantdef = plant.Def.PlantProperties;
            ////var logs = MaterialType.RawMaterial.Create(parent.Body.Material);
            ////var logs = ItemFactory.CreateFrom(RawMaterialDef.Logs, parent.Body.Material);
            //if (plantdef.ProductCutDown != null)
            //{
            //    var table = comp.Growth.Value >= plantdef.YieldThreshold ?
            //        new LootTable(
            //            //new Loot(() => MaterialType.RawMaterial.Create(parent.Body.Material), 1, 1, 5, 10), //1, 3),
            //            //new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Logs, parent.Body.Material ?? Material.LightWood), 1, 1, 5, 10) //1, 3),
            //            new Loot(() => ItemFactory.CreateFrom(plantdef.ProductCutDown, plant.Body.Material ?? MaterialDefOf.LightWood), comp.GrowthBody.Percentage, 5, 1, 3) //1, 3),
            //                                                                                                                                                                 //,new Loot(ItemTemplate.Sapling.Factory.Create, 1, 1, 1, 3)
            //            )
            //            :
            //            new LootTable();

            //    //actor.Net.PopLoot(logs, parent);
            //    //actor.Net.PopLoot(sapling, parent);
            //    actor.Net.PopLoot(table, plant.Global, Vector3.Zero);
            //}
            //actor.Net.Despawn(plant);
            //actor.Net.DisposeObject(plant);
        }
    }
}
