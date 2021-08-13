using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_
{
    class InteractionDeconstructNew : InteractionToolUse
    {
        Progress _progress;
        Cell Cell => this.Actor.Map.GetCell(this.Target.Global);
        Block Block => this.Cell.Block;
        MaterialDef Material => this.Cell.Material;

        protected override float WorkDifficulty => this.Block.WorkAmount;
        protected override float Progress => this._progress.Percentage;

        public InteractionDeconstructNew() : base("Deconstructing")
        {

        }
        protected override void Init()
        {
            var maxWork = this.Block.WorkAmount;
            this._progress = new Progress(0, maxWork, 0);

        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        protected override void ApplyWork(float workAmount)
        {
            this._progress.Value += workAmount;
        }

        protected override void Done()
        {
            this.Block.Deconstruct(this.Actor, this.Target.Global);
        }

        protected override Color GetParticleColor()
        {
            return Color.White;
            return this.Material.Color;
        }

        protected override List<Rectangle> GetParticleRects()
        {
            return this.Block.GetParticleRects(25);
        }

        protected override SkillDef GetSkill()
        {
            return SkillDefOf.Construction;
        }

        protected override ToolUseDef GetToolUse()
        {
            return ToolUseDefOf.Building;
        }
    }
}
