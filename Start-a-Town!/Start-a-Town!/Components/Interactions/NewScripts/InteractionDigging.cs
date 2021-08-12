using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Particles;
using System;

namespace Start_a_Town_
{
    [Obsolete]
    class InteractionDigging : InteractionPerpetual
    {
        static float SpeedFormula(GameObject actor)
        {
            var fromSkill = actor.GetSkill(SkillDef.Digging).Level * .1f + 1; 
            var fromTool = StatDefOf.WorkSpeed.GetValue(actor);
            return fromSkill * fromTool;
        }
        Progress Progress;
        
        public InteractionDigging()
            : base("Dig")
        {
            this.Verb = "Digging";
            this.ToolUse = ToolUseDefOf.Digging;
        }
        Block Block;
        ParticleEmitterSphere EmitterStrike;
        ParticleEmitterSphere EmitterBreak;
        List<Rectangle> ParticleTextures;

        protected override void Start()
        {
            var a = this.Actor;
            var t = this.Target;
            this.Animation.Speed = SpeedFormula(a);
            // cache variables
            this.Block = a.Map.GetBlock(t.Global);
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
            var a = this.Actor;
            var t = this.Target;
            var actor = a as Actor;
            this.EmitStrike(actor);
           
            var material = actor.Map.GetBlockMaterial(t.Global);
            var skill = material.Type.SkillToExtract;
            var workAmount = actor.GetToolWorkAmount(skill);
            actor.AwardSkillXP(SkillDef.Digging, (int)workAmount);

            this.Progress.Value += workAmount;
            this.Animation.Speed = SpeedFormula(actor);
            if (this.Progress.Percentage == 1)
            {
                this.Done();
                this.Finish();
            }
        }
        public void Done()
        {
            var a = this.Actor;
            var t = this.Target;
            this.Block.Break(a.Map, t.Global);
            var tool = a.Gear.GetSlot(GearType.Mainhand).Object;
            this.EmitBreak(a);
        }
        private void EmitStrike(Actor a)
        {
            this.EmitterStrike.Emit(Block.Atlas.Texture, this.ParticleTextures, Vector3.Zero);
            a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
        }
        private void EmitBreak(Actor a)
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
