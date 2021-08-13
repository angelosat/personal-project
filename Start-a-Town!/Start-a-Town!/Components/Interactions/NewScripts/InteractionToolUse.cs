﻿using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    abstract class InteractionToolUse : InteractionPerpetual
    {
        protected ParticleEmitterSphere EmitterStrike;
        protected List<Rectangle> ParticleRects;
        protected InteractionToolUse(string name) : base(name)
        {
            this.DrawProgressBar(() => this.Actor.Global, () => this.Progress, () => this.Name);
        }
        protected sealed override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            //this.ToolUse = this.GetToolUse();
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
            if (a.Net is Net.Client)
            {
                this.EmitterStrike.Emit(ItemContent.LogsGrayscale.AtlasToken.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
            }
            var nextspeed = StatDefOf.WorkSpeed.GetValue(a);
            this.Animation.Speed = nextspeed;
            var toolEffect = this.GetToolEffectiveness();
            //float amount = GetWorkAmount(toolEffect);
            var amount = toolEffect / WorkDifficulty;
            this.ApplyWork(amount);
            var skill = this.GetSkill();
            //if((this.Actor.Gear.GetGear(GearType.Mainhand) as Tool)?.ToolComponent.Props.Skill is SkillDef skill)
                a.AwardSkillXP(skill, amount);
            if (this.Progress < 1)
                return;
            this.Done();
            this.Finish();
        }

        protected float GetToolEffectiveness()
        {
            if (this.Actor.Gear.GetGear(GearType.Mainhand) is Tool tool && tool.ToolComponent.Props.Ability.Def == this.GetToolUse())
                return tool.GetStat(StatDefOf.ToolEffectiveness);
            else
                return this.Actor.GetMaterial(BoneDefOf.RightHand).Density;
        }

        protected abstract float Progress { get; }
        protected abstract float WorkDifficulty { get; }

        protected virtual void Init() { }
        protected abstract void ApplyWork(float workAmount);
        protected abstract void Done();
        protected abstract ToolUseDef GetToolUse();
        protected abstract SkillDef GetSkill();
        protected abstract List<Rectangle> GetParticleRects();
        protected abstract Color GetParticleColor();
    }
}
