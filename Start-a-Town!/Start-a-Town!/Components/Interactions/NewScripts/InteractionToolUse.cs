using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using System.Collections.Generic;

namespace Start_a_Town_
{
    abstract class InteractionToolUse : InteractionPerpetual
    {
        protected enum SkillAwardTypes { OnSwing, OnFinish }
        protected ParticleEmitterSphere EmitterStrike;
        protected List<Rectangle> ParticleRects;
        float TotalWorkAmount;
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        protected sealed override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            this.Animation.Speed = StatDefOf.WorkSpeed.GetValue(a);
            this.Init();
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
            if (a.Net is Net.Client && this.ParticleRects is not null)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            this.Animation.Speed = StatDefOf.WorkSpeed.GetValue(a);
            var toolEffect = GetToolEffectiveness();
            var amount = toolEffect / WorkDifficulty;
            this.ApplyWork(amount);
            this.TotalWorkAmount += amount;
            var skill = this.GetSkill();
            //if((this.Actor.Gear.GetGear(GearType.Mainhand) as Tool)?.ToolComponent.Props.Skill is SkillDef skill)
            if(this.SkillAwardType == SkillAwardTypes.OnSwing)
                a.AwardSkillXP(skill, amount);
            if (this.Progress < 1)
                return;
            if (this.SkillAwardType == SkillAwardTypes.OnFinish)
                a.AwardSkillXP(skill, this.TotalWorkAmount);
            this.Done();
            this.Finish();
        }
        protected virtual float GetToolEffectiveness()
        {
            if (this.Actor.Gear.GetGear(GearType.Mainhand) is Tool tool && tool.ToolComponent.Props.ToolUse == this.GetToolUse())
                return tool.GetStat(StatDefOf.ToolEffectiveness);
            else
                return this.Actor.GetMaterial(BoneDefOf.RightHand).Density;
        }

        protected abstract float Progress { get; }
        protected abstract float WorkDifficulty { get; }
        protected abstract SkillAwardTypes SkillAwardType { get; }
        protected virtual void Init() { }
        protected abstract void ApplyWork(float workAmount);
        protected abstract void Done();
        protected abstract ToolUseDef GetToolUse();
        protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
    }
}
