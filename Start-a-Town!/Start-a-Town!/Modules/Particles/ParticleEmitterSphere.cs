using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Particles
{
    public class ParticleEmitterSphere : ParticleEmitter
    {
        public float Force = .01f;

        public ParticleEmitterSphere()//AtlasWithDepth atlas)
            //: base(atlas)
        {

        }
        public ParticleEmitterSphere(
            //AtlasWithDepth atlas, 
            Vector3 offset, float lifetime, float radius, float particleWeight, float force)
            : base(//atlas,
            offset, lifetime, radius, particleWeight)
        {
            this.Force = force;
        }

        protected override Vector3 GetStartVelocity()
        {
            double θ = this.Random.NextDouble() * (Math.PI + Math.PI);
            double φ = this.Random.NextDouble() * (Math.PI);
            float x = (float)(Math.Sin(φ) * Math.Cos(θ));
            float y = (float)(Math.Sin(φ) * Math.Sin(θ));
            float z = (float)Math.Cos(φ);
            Vector3 direction = new Vector3(x, y, z);
            direction.Normalize();
            var startVelocity = direction * this.Force;// 0.01f;
            return startVelocity;
        }

        public override object Clone()
        {
            return new ParticleEmitterSphere(
                //this.Atlas, 
                this.Offset, this.Lifetime, this.Radius, this.ParticleWeight, this.Force)
            {
                Friction = this.Friction,
                Rate = this.Rate,
                ColorBegin = this.ColorBegin,
                ColorEnd = this.ColorEnd,
                SizeBegin = this.SizeBegin,
                SizeEnd = this.SizeEnd,
                SizeVariance = this.SizeVariance
            };
        }

        
    }
}
