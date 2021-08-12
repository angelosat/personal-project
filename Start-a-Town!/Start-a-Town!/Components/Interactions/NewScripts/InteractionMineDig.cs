﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class InteractionMineDig : InteractionToolUse
    {
        Progress _progress;
        protected override float Progress => this._progress.Percentage;
        ParticleEmitterSphere EmitterBreak;
        Cell Cell => this.Actor.Map.GetCell(this.Target.Global);
        Block Block => this.Cell.Block;
        MaterialDef Material => this.Cell.Material;

        public InteractionMineDig() : base("MineDig")
        {

        }
        protected override void Init()
        {
            var matType = this.Material.Type;
            if (matType == MaterialType.Soil)
                this.Name = "Digging";
            else if (matType == MaterialType.Stone || matType == MaterialType.Metal)
                this.Name = "Mining";

            var global = this.Target.Global;
            var maxWork = this.Block.GetWorkToBreak(this.Actor.Map, global);
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
            this.EmitterBreak.Lifetime = Ticks.TicksPerSecond * 2;
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
            var a = this.Actor;
            var t = this.Target;
            var cell = this.Cell;
            if (!isMetalOrMineral())
                return;
            //var material = block.GetMaterial(cell.BlockData);
            var material = cell.Material;
            var server = a.Net as Server;
            if (server != null)
            {
                if (material != MaterialDefOf.Stone)
                {
                    var resource = ItemFactory.CreateFrom(RawMaterialDef.Ore, material);
                    server.PopLoot(resource, t.Global, Vector3.Zero);
                }

                var byproduct = ItemFactory.CreateFrom(RawMaterialDef.Boulders, MaterialDefOf.Stone);
                server.PopLoot(byproduct, t.Global, Vector3.Zero);
            }

            a.Map.RemoveBlock(t.Global);

            emitBreak();

            bool isMetalOrMineral()
            {
                var mat = Block.GetBlockMaterial(a.Map, t.Global);
                return mat.Type == MaterialType.Stone || mat.Type == MaterialType.Metal;
            }
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
            if (matType == MaterialType.Soil)
                return SkillDefOf.Digging;
            else if (matType == MaterialType.Stone || matType == MaterialType.Metal)
                return SkillDefOf.Mining;
            throw new Exception();
        }

        protected override ToolUseDef GetToolUse()
        {
            var matType = this.Material.Type;
            if (matType == MaterialType.Soil)
                return ToolUseDefOf.Digging;
            else if (matType == MaterialType.Stone || matType == MaterialType.Metal)
                return ToolUseDefOf.Mining;
            throw new Exception();
        }

        protected override float GetWorkAmount()
        {
            var material = this.Material;
            var toolEffect = (int)StatDefOf.WorkEffectiveness.GetValue(this.Actor);
            var amount = toolEffect / (float)material.Density;
            return amount;
        }
    }
}
