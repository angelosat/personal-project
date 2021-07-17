using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
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
            this.Skill = ToolAbilityDef.Digging;
        }
        static public int ID = "Dig".GetHashCode();
        static readonly ScriptTaskCondition cancel = new ScriptTaskCondition("IsSoil", (a, t) =>
        {
            var blockmat = Block.GetBlockMaterial(a.Map, t.Global);
            return blockmat.Type == MaterialType.Soil;
        });
        public override ScriptTaskCondition CancelState
        {
            get
            {
                return cancel;
            }
        }
        
        Block Block;
        ParticleEmitterSphere EmitterStrike;
        ParticleEmitterSphere EmitterBreak;
        List<Rectangle> ParticleTextures;

        public override void Start(Actor a, TargetArgs t)
        {
            base.Start(a, t);
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
            this.EmitterStrike.Lifetime = Engine.TicksPerSecond * 2;

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
            this.EmitterBreak.Lifetime = Engine.TicksPerSecond * 2;

            this.ParticleTextures = this.Block.GetParticleRects(25);
        }
        public override void OnUpdate(Actor a, TargetArgs t)
        {
            var actor = a as Actor;
            this.EmitStrike(actor);
           
            var material = actor.Map.GetBlockMaterial(t.Global);
            var skill = material.Type.SkillToExtract;
            var workAmount = actor.GetToolWorkAmount(skill.ID);
            actor.AwardSkillXP(SkillDef.Digging, (int)workAmount);

            this.Progress.Value += workAmount;
            this.Animation.Speed = SpeedFormula(actor);
            if (this.Progress.Percentage == 1)
            {
                this.Done(actor, t);
                this.Finish(actor, t);
            }
        }
        public void Done(Actor a, TargetArgs t)
        {
            this.Block.Break(a.Map, t.Global);
            var tool = GearComponent.GetSlot(a, GearType.Mainhand).Object;
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
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent, TargetArgs target)
        {
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
