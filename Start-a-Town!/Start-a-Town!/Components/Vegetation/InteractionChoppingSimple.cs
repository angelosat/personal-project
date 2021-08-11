using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Components
{
    public class InteractionChoppingSimple : InteractionPerpetual
    {
        int PlantHitPoints = 1;
        float CutDownProgress = 0;
        ParticleEmitterSphere EmitterStrike;
        List<Rectangle> ParticleRects;
        public InteractionChoppingSimple()
            : base("Chopping")
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        float Progress => this.CutDownProgress / (float)this.PlantHitPoints;
        protected override void Start()
        {
            var a = this.Actor;
            var t = this.Target; 
            var plant = t.Object as Plant;
            var plantProps = plant.PlantComponent.PlantProperties;
            this.Skill = plantProps.ToolToCut;
            this.Animation.Speed = StatDefOf.WorkSpeed.GetValue(a);

            this.PlantHitPoints = plantProps.StemMaterial.Density;// plantProps.GetCutDownHitPonts(plant);
            $"plant hitpoints: {this.PlantHitPoints}".ToConsole();
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
            this.CutDownProgress += amount;
            if (this.CutDownProgress < PlantHitPoints)
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
        protected override void AddSaveData(SaveTag tag)
        {
            this.CutDownProgress.Save(tag, "Current");
            this.PlantHitPoints.Save(tag, "Max");
        }
        public override void LoadData(SaveTag tag)
        {
            this.CutDownProgress = tag.LoadInt("Current");
            this.PlantHitPoints = tag.LoadInt("Max");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.CutDownProgress);
            w.Write(this.PlantHitPoints);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.CutDownProgress = r.ReadInt32();
            this.PlantHitPoints = r.ReadInt32();
        }
    }
}
