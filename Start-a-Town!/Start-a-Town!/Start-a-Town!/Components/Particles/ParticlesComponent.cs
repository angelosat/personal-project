using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Particles
{
    class ParticlesComponent : Component
    {
        public override string ComponentName
        {
            get { return "ParticleSystem"; }
        }
        //public Vector3 Offset = Vector3.Zero;

        List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        public ParticlesComponent()
        {
            //this.Emitters.Add(new ParticleEmitter());
        }
        //public ParticlesComponent(Vector3 offset, float radius)//:this()
        //{
        //    this.Offset = offset;
        //    this.Emitters.Add(new ParticleEmitter(radius));
        //}
        public ParticlesComponent(params ParticleEmitter[] emitters)
        {
            this.Emitters.AddRange(emitters);
        }
        public override void Update(GameObject parent)
        {
            foreach (var emitter in this.Emitters)
                emitter.Update(parent.Map, parent.Global);
        }
        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            foreach (var emitter in this.Emitters)
                emitter.Draw(camera, parent.Map, parent.Global);// + this.Offset);
        }
        public override object Clone()
        {
            return new ParticlesComponent(this.Emitters.Select(f => f.Clone() as ParticleEmitter).ToArray());
        }
    }
}
