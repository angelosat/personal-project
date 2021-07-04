﻿using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components;

namespace Start_a_Town_.Particles
{
    class ParticlesComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "ParticleSystem"; }
        }

        List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        public ParticlesComponent()
        {
        }
       
        public ParticlesComponent(params ParticleEmitter[] emitters)
        {
            this.Emitters.AddRange(emitters);
        }
        public override void Tick(GameObject parent)
        {
            foreach (var emitter in this.Emitters)
                emitter.Update(parent.Map, parent.Global);
        }
        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            foreach (var emitter in this.Emitters)
                emitter.Draw(camera, parent.Map, parent.Global);
        }
        public override object Clone()
        {
            return new ParticlesComponent(this.Emitters.Select(f => f.Clone() as ParticleEmitter).ToArray());
        }
    }
}
