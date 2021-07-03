using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Net;
using Start_a_Town_.Particles;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionMining : InteractionPerpetual
    {
        static float SpeedFormula(GameObject actor)
        {
            var fromSkill = actor.GetSkill(SkillDef.Digging).Level * .1f + 1; //+.5f 
            var fromTool = StatDefOf.WorkSpeed.GetValue(actor);
            return fromSkill * fromTool;
            return fromSkill;
        }
        Progress Progress;
        //static int MaxStrikes = 3;
        //float MaxStrikes;

        //int StrikeCount = 0;
        int StrikeCount
        {
            get { return (int)this.Progress.Value; }
            set
            {
                this.Progress.Value = value;
            }
        }
        public InteractionMining()
            : base("Mine")
        {
            this.Verb = "Mining";
            this.Skill = ToolAbilityDef.Digging;
            //this.Progress = new Progress(0, MaxStrikes, 0);
        }

        static public int ID = "Mine".GetHashCode();
       
        Block Block;
        ParticleEmitterSphere EmitterStrike;
        ParticleEmitterSphere EmitterBreak;
        List<Rectangle> ParticleTextures;

        public override void Start(GameObject a, TargetArgs t)
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
        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            var actor = a as Actor;
            this.EmitStrike(actor);
            //this.StrikeCount++;
            //if (this.StrikeCount < MaxStrikes)
            //{
            //    return;
            //}
            //var skill = this.Block.GetMaterial()
            var material = actor.Map.GetBlockMaterial(t.Global);
            var skill = material.GetSkillToExtract();
            //var workAmount = actor.GetToolWorkAmount(Components.GearType.Types.Mainhand, skill.ID);
            var workAmount = actor.GetToolWorkAmount(skill.ID);
            actor.AwardSkillXP(SkillDef.Mining, (int)workAmount);

            this.Progress.Value += workAmount;
            this.Animation.Speed = SpeedFormula(actor);
            if (this.Progress.Percentage == 1)
            {
                this.Done(actor, t);
                //this.State = States.Finished;
                this.Finish(actor, t);
            }
        }
        public void Done(GameObject a, TargetArgs t)
        {
            //this.Block.Break(a.Map, t.Global);
            //var tool = GearComponent.GetSlot(a, GearType.Mainhand).Object;

            var cell = a.Map.GetCell(t.Global);
            var block = cell.Block;// a.Map.GetBlock(t.Global);

            //if (block.MaterialType != MaterialType.Mineral)
            if (!IsMetalOrMineral(a, t))// block.GetMaterial(cell.BlockData).Type != MaterialType.Mineral)
                return;
            var material = block.GetMaterial(cell.BlockData);
            var server = a.Net as Server;
            if (server != null)
            {
                //var factory = Materials.MaterialType.RawMaterial.Rocks;// new Materials.RawMaterials.Rocks();
                //var entity = Materials.MaterialType.RawMaterial.Create(material);
                //var entity = ItemFactory.CreateFrom(RawMaterialDef.Rock, material);

                if (material != MaterialDefOf.Stone)
                {
                    var resource = ItemFactory.CreateFrom(RawMaterialDef.Ore, material);
                    server.PopLoot(resource, t.Global, Vector3.Zero);
                }

                var byproduct = ItemFactory.CreateFrom(RawMaterialDef.Boulders, MaterialDefOf.Stone);
                server.PopLoot(byproduct, t.Global, Vector3.Zero);
            }

            block.Remove(a.Map, t.Global);//.Break(a.Net, t.Global);

            this.EmitBreak(a);
        }
        static bool IsMetalOrMineral(GameObject a, TargetArgs t)
        {
            var mat = Block.GetBlockMaterial(a.Map, t.Global);
            return mat.Type == MaterialType.Stone || mat.Type == MaterialType.Metal;
        }
        private void EmitStrike(GameObject a)
        {
            this.EmitterStrike.Emit(Block.Atlas.Texture, this.ParticleTextures, Vector3.Zero);
            //a.Map.EventOccured(Message.Types.ParticleEmitterAdd, this.EmitterStrike);
            a.Map.ParticleManager.AddEmitter(this.EmitterStrike);
        }
        private void EmitBreak(GameObject a)
        {
            this.EmitterBreak.Emit(Block.Atlas.Texture, this.ParticleTextures, Vector3.Zero);
            //a.Map.EventOccured(Message.Types.ParticleEmitterAdd, this.EmitterBreak);
            a.Map.ParticleManager.AddEmitter(this.EmitterBreak);
        }

        BarSmooth BarSmooth;
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent, TargetArgs target)
        {
            Vector3 global = parent.Global;
            //Rectangle bounds = camera.GetScreenBounds(global, Block.Bounds);
            //Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            //Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 barLoc = camera.GetScreenPositionFloat(global);
            if (this.BarSmooth == null)
                this.BarSmooth = new BarSmooth(this.Progress);
            this.BarSmooth.Draw(sb, UIManager.Bounds, barLoc, InteractionBar.DefaultWidth, camera.Zoom * .2f);
            //Bar.Draw(sb, camera, global, "", this.StrikeCount / (float)MaxStrikes, camera.Zoom * .2f);
        }
        public override object Clone()
        {
            return new InteractionDigging();
        }
    }
    
}
