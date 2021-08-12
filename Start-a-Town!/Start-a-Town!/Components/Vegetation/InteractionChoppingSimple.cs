using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Components
{
    [Obsolete]
    public class InteractionChoppingSimple : InteractionPerpetual
    {
        ParticleEmitterSphere EmitterStrike;
        Resource HitPoints;
        List<Rectangle> ParticleRects;
        public InteractionChoppingSimple()
            : base("Chopping")
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        float Progress => 1 - this.HitPoints.Percentage;// this.CutDownProgress / (float)this.PlantHitPoints;
        protected override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            var plant = t.Object as Plant;
            var plantProps = plant.PlantComponent.PlantProperties;
            this.ToolUse = plantProps.ToolToCut;
            this.Animation.Speed = StatDefOf.WorkSpeed.GetValue(a);
            this.HitPoints = plant.GetResource(ResourceDefOf.HitPoints);
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
                Lifetime = Ticks.TicksPerSecond * 2,
                Rate = 0
            };
            this.ParticleRects = ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        public override void OnUpdate()
        {
            var a = this.Actor;
            var t = this.Target;
            if (a.Net is Net.Client)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            var nextspeed = StatDefOf.WorkSpeed.GetValue(a);
            this.Animation.Speed = nextspeed;
            float amount = getCutDownAmount(a, t);
            this.HitPoints.Value -= amount;
            if (this.Progress < 1)
                return;
            this.Done();
            this.Finish();

            static float getCutDownAmount(Actor a, TargetArgs t)
            {
                var plant = t.Object as Plant;
                var plantProps = plant.PlantComponent.PlantProperties;
                var trunkHardness = plantProps.StemMaterial.Density;
                var toolEffect = (int)StatDefOf.WorkEffectiveness.GetValue(a);
                var amount = toolEffect / (float)trunkHardness;
                return amount;
            }
        }
        public void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            CutDownPlant(a, t.Object as Plant);
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
    }
}
