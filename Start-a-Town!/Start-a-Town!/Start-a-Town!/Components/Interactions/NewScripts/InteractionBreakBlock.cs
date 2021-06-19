using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionBreakBlock : Interaction
    {
        public InteractionBreakBlock()
            : base("Break", 2)
        {
            this.Verb = "Breaking";
        }

        static readonly TaskConditions conds = 
            new TaskConditions(
                new AllCheck(
                    RangeCheck.Sqrt2
                    ));

        public override TaskConditions Conditions
        {
	        get 
	        { 
		         return conds;
	        }
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            var block = a.Map.GetBlock(t.Global);
            block.Break(a.Map, t.Global);
            EmitParticles(a, t, block);
        }

        public static void EmitParticles(GameObject a, TargetArgs t, Block block)
        {
            var emitters = WorkComponent.GetEmitters(a);
            if (emitters == null)
                return;
            if (t.Type != TargetType.Position)
                return;
            var e = block.GetEmitter();
            e.Source = t.Global + Vector3.UnitZ * 0.5f;
            e.SizeBegin = 1;
            e.SizeEnd = 1;
            e.ParticleWeight = 1;
            e.Radius = 1f;// .5f;
            e.Force = .1f;
            e.Friction = .5f;
            e.AlphaBegin = 1;
            e.AlphaEnd = 0;

            e.ColorBegin = Color.White;
            e.ColorEnd = Color.White;

            e.Lifetime = Engine.TargetFps * 2;
            var pieces = block.GetParticleRects(25);
            e.Emit(Block.Atlas.Texture, pieces, Vector3.Zero);
            emitters.Add(e);
        }

        public override object Clone()
        {
            return new InteractionBreakBlock();
        }
    }
}
