using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Particles
{
    class VelocityGetterSphere : IVelocityGetter
    {
        double θmin, θmax, φmin, φmax;
        float Factor;
        Random Random = new Random();
        public VelocityGetterSphere(float factor)
        {
            this.Factor = factor;
        }
        public Vector3 Get()
        {
            double θ = θmin + this.Random.NextDouble() * (θmax - θmin); //(Math.PI + Math.PI);
            double φ = φmin + this.Random.NextDouble() * φmax;
            float x = (float)(Math.Sin(φ) * Math.Cos(θ));
            float y = (float)(Math.Sin(φ) * Math.Sin(θ));
            float z = (float)Math.Cos(φ);
            Vector3 direction = new Vector3(x, y, z);
            direction.Normalize();
            var startVelocity = direction * this.Factor;
            return startVelocity;
        }
        public object Clone()
        {
            return new VelocityGetterSphere(this.Factor);
        }
    }
}
