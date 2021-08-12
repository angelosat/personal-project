using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    abstract class InteractionToolUse : InteractionPerpetual
    {
        ParticleEmitterSphere EmitterStrike;
        List<Rectangle> ParticleRects;
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        protected sealed override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            this.ToolUse = this.GetToolUse();
            this.Animation.Speed = StatDefOf.WorkSpeed.GetValue(a);
            this.OnInit();
            var particleColor = this.GetParticleColor();
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
            this.ParticleRects = GetParticleRects();
        }

        public sealed override void OnUpdate()
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
            float amount = GetWorkAmount();
            this.ApplyWorkAmount(amount);
            var skill = this.GetSkill();
            a.AwardSkillXP(skill, amount);
            if (this.Progress < 1)
                return;
            this.Done();
            this.Finish();
        }
        protected virtual void OnInit() { }
        protected abstract void ApplyWorkAmount(float workAmount);
        protected abstract float Progress { get; }
        protected abstract void Done();
        protected abstract ToolUseDef GetToolUse();
        protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract float GetWorkAmount();
        protected abstract Color GetParticleColor();
    }

    class InteractionChop : InteractionToolUse
    {
        protected override float Progress => 1 - this.HitPoints.Percentage;
        Resource HitPoints => this.Target.Object.GetResource(ResourceDefOf.HitPoints);
        Plant Plant => this.Target.Object as Plant;

        public InteractionChop() : base("Chopping")
        {

        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        protected override void ApplyWorkAmount(float workAmount)
        {
            this.HitPoints.Value -= workAmount;
            this.Plant.PlantComponent.Wiggle((float)Math.PI / 32f, 20);
        }

        protected override void Done()
        {
            var plant = this.Plant;
            var comp = plant.PlantComponent;
            comp.Harvest(plant, this.Actor);
            comp.CutDown(plant, this.Actor);
        }

        protected override Color GetParticleColor()
        {
            return this.Plant.PlantComponent.PlantProperties.StemMaterial.Color;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return ItemContent.LogsGrayscale.AtlasToken.Rectangle.Divide(25);
        }

        protected override SkillDef GetSkill()
        {
            return SkillDef.Plantcutting;
        }

        protected override ToolUseDef GetToolUse()
        {
            return ToolUseDefOf.Chopping;
        }

        protected override float GetWorkAmount()
        {
            var plant = this.Plant;
            var plantProps = plant.PlantComponent.PlantProperties;
            var trunkHardness = plantProps.StemMaterial.Density;
            var toolEffect = (int)StatDefOf.WorkEffectiveness.GetValue(this.Actor);
            var amount = toolEffect / (float)trunkHardness;
            return amount;
        }
    }
}
