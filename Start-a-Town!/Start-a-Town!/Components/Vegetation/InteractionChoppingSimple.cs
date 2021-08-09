using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Components
{
    public class InteractionChoppingSimple : InteractionPerpetual
    {
        int MaxStrikes = 300;
        int StrikeCount = 0;
        ParticleEmitterSphere EmitterStrike;
        List<Rectangle> ParticleRects;
        public InteractionChoppingSimple()
            : base("Chopping")
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        float Progress => this.StrikeCount / (float)this.MaxStrikes;
        public override void Start(Actor a, TargetArgs t)
        {
            base.Start(a, t);
            var plant = t.Object as Plant;
            this.MaxStrikes = plant.PlantComponent.PlantProperties.CutDownDifficulty;
            var particleColor = plant.PrimaryMaterial.Color; //MaterialDefOf.LightWood.Color
            this.EmitterStrike = new ParticleEmitterSphere
            {
                Source = t.Global + Vector3.UnitZ,
                SizeBegin = 1,
                SizeEnd = 1,
                ParticleWeight = 1,
                Radius = 1f,// .5f;
                Force = .1f,
                Friction = .5f,
                AlphaBegin = 1,
                AlphaEnd = 0,
                ColorBegin = particleColor,
                ColorEnd = particleColor,
                Lifetime = Engine.TicksPerSecond * 2,
                Rate = 0
            };
            this.ParticleRects = ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        public override void OnUpdate(Actor a, TargetArgs t)
        {
            if (a.Net is Net.Client)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            this.StrikeCount++;
            if (this.StrikeCount < MaxStrikes)
            {
                return;
            }
            this.Done(a, t);
            this.Finish(a, t);
        }
        public void Done(GameObject a, TargetArgs t)
        {
            CutDownPlant(a as Actor, t.Object as Plant);
        }
        public override object Clone()
        {
            return new InteractionChoppingSimple();
        }

        static public void CutDownPlant(Actor actor, Plant plant)
        {
            var comp = plant.PlantComponent;
            comp.Harvest(plant, actor);
            comp.CutDown(plant, actor);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this.StrikeCount.Save(tag, "Current");
            this.MaxStrikes.Save(tag, "Max");
        }
        public override void LoadData(SaveTag tag)
        {
            this.StrikeCount = tag.LoadInt("Current");
            this.MaxStrikes = tag.LoadInt("Max");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.StrikeCount);
            w.Write(this.MaxStrikes);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.StrikeCount = r.ReadInt32();
            this.MaxStrikes = r.ReadInt32();
        }
    }
}
