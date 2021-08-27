using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class InteractionBreakBlock : InteractionToolUse
    {
        private Progress _progress;
        ParticleEmitterSphere EmitterBreak;
        Cell _cellCached;
        Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
        Block Block => this.Cell.Block;
        MaterialDef Material => this.Cell.Material;
        float WorkAppliedThisBreakStage;

        protected override float WorkDifficulty => this.Material.Density;
        protected override float Progress => this._progress.Percentage;// 1 - this.Cell.HitPointsPercentage;
        protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnFinish;

        public InteractionBreakBlock() : base("MineDig")
        {
        }
        protected override void Init()
        {
            var matType = this.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
                this.Name = "Digging";
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
                this.Name = "Mining";

            var global = this.Target.Global;
            var maxWork = this.Cell.Material.BreakResistance * this.Cell.HitPoints;// this.Block.GetBreakResistance(this.Actor.Map, global);
            this._progress = new Progress(0, maxWork, 0);

            this.EmitterBreak = this.Block.GetEmitter();
            this.EmitterBreak.Source = global + Vector3.UnitZ * 0.5f;
            this.EmitterBreak.SizeBegin = 1;
            this.EmitterBreak.SizeEnd = 1;
            this.EmitterBreak.ParticleWeight = 1;
            this.EmitterBreak.Radius = 1f;// .5f;
            this.EmitterBreak.Force = .1f;
            this.EmitterBreak.Friction = .5f;
            this.EmitterBreak.AlphaBegin = 1;
            this.EmitterBreak.AlphaEnd = 0;
            this.EmitterBreak.ColorBegin = Color.White;
            this.EmitterBreak.ColorEnd = Color.White;
            this.EmitterBreak.Lifetime = Ticks.PerSecond * 2;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }

        protected override void ApplyWork(float workAmount)
        {
            this.WorkAppliedThisBreakStage += workAmount;
            var resistance = this.Cell.Material.BreakResistance;
            this._progress.Value += workAmount;
            if (this.WorkAppliedThisBreakStage >= resistance)
            {
                this.Cell.Damage++;
                this.WorkAppliedThisBreakStage -= resistance;
            }
        }

        protected override void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            var cell = this.Cell;

            if (a.Net is Server server && cell.Block.BreakProduct is ItemDef productDef)
                server.PopLoot(ItemFactory.CreateFrom(productDef, cell.Material), t.Global, Vector3.Zero);

            a.Map.RemoveBlock(t.Global);

            emitBreak();

            void emitBreak()
            {
                this.EmitterBreak.Emit(Block.Atlas.Texture, this.ParticleRects, Vector3.Zero);
                a.Map.ParticleManager.AddEmitter(this.EmitterBreak);
            }
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
            var matType = this.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
                return SkillDefOf.Digging;
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
                return SkillDefOf.Mining;
            throw new Exception();
        }

        protected override ToolUseDef GetToolUse()
        {
            var matType = this.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
                return ToolUseDefOf.Digging;
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
                return ToolUseDefOf.Mining;
            throw new Exception();
        }

        protected override void AddSaveData(SaveTag tag)
        {
            this._progress.Save(tag, "Progress");
            this.WorkAppliedThisBreakStage.Save(tag, "WorkApplied");
        }
        public override void LoadData(SaveTag tag)
        {
            this._progress = new Progress(tag["Progress"]);
            this.WorkAppliedThisBreakStage = (float)tag["WorkApplied"].Value;
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            this._progress.Write(w);
            w.Write(this.WorkAppliedThisBreakStage);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this._progress = new Progress(r);
            this.WorkAppliedThisBreakStage = r.ReadSingle();
        }
    }

    //class InteractionBreakBlock : InteractionToolUse
    //{
    //    Progress _progress;
    //    ParticleEmitterSphere EmitterBreak;
    //    Cell _cellCached;
    //    Cell Cell => _cellCached ??= this.Actor.Map.GetCell(this.Target.Global);
    //    Block Block => this.Cell.Block;
    //    MaterialDef Material => this.Cell.Material;

    //    protected override float WorkDifficulty => this.Material.Density;
    //    protected override float Progress => this._progress.Percentage;
    //    protected override SkillAwardTypes SkillAwardType { get; } = SkillAwardTypes.OnFinish;

    //    public InteractionBreakBlock() : base("MineDig")
    //    {
    //    }
    //    protected override void Init()
    //    {
    //        var matType = this.Material.Type;
    //        if (matType == MaterialTypeDefOf.Soil)
    //            this.Name = "Digging";
    //        else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
    //            this.Name = "Mining";

    //        var global = this.Target.Global;
    //        var maxWork = this.Block.GetBreakResistance(this.Actor.Map, global);
    //        this._progress = new Progress(0, maxWork, 0);

    //        this.EmitterBreak = this.Block.GetEmitter();
    //        this.EmitterBreak.Source = global + Vector3.UnitZ * 0.5f;
    //        this.EmitterBreak.SizeBegin = 1;
    //        this.EmitterBreak.SizeEnd = 1;
    //        this.EmitterBreak.ParticleWeight = 1;
    //        this.EmitterBreak.Radius = 1f;// .5f;
    //        this.EmitterBreak.Force = .1f;
    //        this.EmitterBreak.Friction = .5f;
    //        this.EmitterBreak.AlphaBegin = 1;
    //        this.EmitterBreak.AlphaEnd = 0;
    //        this.EmitterBreak.ColorBegin = Color.White;
    //        this.EmitterBreak.ColorEnd = Color.White;
    //        this.EmitterBreak.Lifetime = Ticks.TicksPerSecond * 2;
    //    }
    //    public override object Clone()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override void ApplyWork(float workAmount)
    //    {
    //        this._progress.Value += workAmount;
    //    }

    //    protected override void Done()
    //    {
    //        var a = this.Actor;
    //        var t = this.Target;
    //        var cell = this.Cell;

    //        if (a.Net is Server server && cell.Block.BreakProduct is ItemDef productDef)
    //            server.PopLoot(ItemFactory.CreateFrom(productDef, cell.Material), t.Global, Vector3.Zero);

    //        a.Map.RemoveBlock(t.Global);

    //        emitBreak();

    //        void emitBreak()
    //        {
    //            this.EmitterBreak.Emit(Block.Atlas.Texture, this.ParticleRects, Vector3.Zero);
    //            a.Map.ParticleManager.AddEmitter(this.EmitterBreak);
    //        }
    //    }

    //    protected override Color GetParticleColor()
    //    {
    //        return Color.White;
    //        return this.Material.Color;
    //    }

    //    protected override List<Rectangle> GetParticleRects()
    //    {
    //        return this.Block.GetParticleRects(25);
    //    }

    //    protected override SkillDef GetSkill()
    //    {
    //        var matType = this.Material.Type;
    //        if (matType == MaterialTypeDefOf.Soil)
    //            return SkillDefOf.Digging;
    //        else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
    //            return SkillDefOf.Mining;
    //        throw new Exception();
    //    }

    //    protected override ToolUseDef GetToolUse()
    //    {
    //        var matType = this.Material.Type;
    //        if (matType == MaterialTypeDefOf.Soil)
    //            return ToolUseDefOf.Digging;
    //        else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
    //            return ToolUseDefOf.Mining;
    //        throw new Exception();
    //    }
    //}
}
