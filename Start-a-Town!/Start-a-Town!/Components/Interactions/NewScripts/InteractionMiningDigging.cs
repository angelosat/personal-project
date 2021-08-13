using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    [Obsolete]
    class InteractionMiningDigging : InteractionPerpetual
    {
        static float SpeedFormula(Actor actor)
        {
            var fromSkill = actor.GetSkill(SkillDefOf.Digging).Level * .1f + 1; //+.5f 
            var fromTool = StatDefOf.WorkSpeed.GetValue(actor);
            return fromSkill * fromTool;
        }
        Progress Progress;

        public InteractionMiningDigging()
            : base("Mine")
        {
            this.Verb = "Mining";
        }

        Block Block;
        ParticleEmitterSphere EmitterStrike;
        ParticleEmitterSphere EmitterBreak;
        List<Rectangle> ParticleTextures;
        SkillDef SkillDef;

        protected override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            this.Animation.Speed = SpeedFormula(a);
            // cache variables
            var cell = a.Map.GetCell(t.Global);
            this.Block = cell.Block;
            var matType = cell.Material.Type;
            if (matType == MaterialTypeDefOf.Soil)
            {
                this.ToolUse = ToolUseDefOf.Digging;
                this.SkillDef = SkillDefOf.Digging;
            }
            else if (matType == MaterialTypeDefOf.Stone || matType == MaterialTypeDefOf.Metal)
            {
                this.ToolUse = ToolUseDefOf.Mining;
                this.SkillDef = SkillDefOf.Mining;
            }
            var maxWork = this.Block.GetWorkToBreak(a.Map, t.Global);
            this.Progress = new Progress(0, maxWork, 0);

            this.EmitterStrike = this.Block.GetEmitter();
            this.EmitterStrike.Source = t.FaceGlobal;
            this.EmitterStrike.SizeBegin = 1;
            this.EmitterStrike.SizeEnd = 1;
            this.EmitterStrike.ParticleWeight = 1;
            this.EmitterStrike.Radius = 1f;// .5f;
            this.EmitterStrike.Force = .1f;
            this.EmitterStrike.Friction = .5f;
            this.EmitterStrike.AlphaBegin = 1;
            this.EmitterStrike.AlphaEnd = 0;
            this.EmitterStrike.ColorBegin = Color.White;
            this.EmitterStrike.ColorEnd = Color.White;
            this.EmitterStrike.Lifetime = Ticks.TicksPerSecond * 2;

            this.EmitterBreak = this.Block.GetEmitter();
            this.EmitterBreak.Source = t.Global + Vector3.UnitZ * 0.5f;
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

            this.ParticleTextures = this.Block.GetParticleRects(25);
        }
        public override void OnUpdate()
        {
            var actor = this.Actor;
            var t = this.Target;
            this.EmitStrike(actor);

            var workAmount = getWorkAmount();
            actor.AwardSkillXP(this.SkillDef, workAmount);

            this.Progress.Value += workAmount;
            this.Animation.Speed = SpeedFormula(actor);
            if (this.Progress.Percentage == 1)
            {
                this.Done();
                this.Finish();
            }

            float getWorkAmount()
            {
                var material = actor.Map.GetCell(t.Global).Material;
                var toolEffect = (int)StatDefOf.WorkEffectiveness.GetValue(actor);
                var amount = toolEffect / (float)material.Density;
                return amount;
            }
        }
        public void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            var cell = a.Map.GetCell(t.Global);
            var block = cell.Block;
            if (!IsMetalOrMineral(a, t))
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

            this.EmitBreak(a);
        }
        static bool IsMetalOrMineral(GameObject a, TargetArgs t)
        {
            var mat = Block.GetBlockMaterial(a.Map, t.Global);
            return mat.Type == MaterialTypeDefOf.Stone || mat.Type == MaterialTypeDefOf.Metal;
        }
        private void EmitStrike(GameObject a)
        {
            this.EmitterStrike.Emit(Block.Atlas.Texture, this.ParticleTextures, Vector3.Zero);
            a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
        }
        private void EmitBreak(GameObject a)
        {
            this.EmitterBreak.Emit(Block.Atlas.Texture, this.ParticleTextures, Vector3.Zero);
            a.Map.ParticleManager.AddEmitter(this.EmitterBreak);
        }

        BarSmooth BarSmooth;
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            var parent = this.Actor;
            Vector3 global = parent.Global;
            Vector2 barLoc = camera.GetScreenPositionFloat(global);
            if (this.BarSmooth == null)
                this.BarSmooth = new BarSmooth(this.Progress);
            this.BarSmooth.Draw(sb, UIManager.Bounds, barLoc, InteractionBar.DefaultWidth, camera.Zoom * .2f);
        }
        public override object Clone()
        {
            return new InteractionDigging();
        }
    }
}
