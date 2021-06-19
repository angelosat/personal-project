using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components.Particles
{
    class ParticleSystem
    {
        List<ParticleEmitter> Emitters = new List<ParticleEmitter>();
        public void AddEmitter(ParticleEmitter emitter)
        {
            this.Emitters.Add(emitter);
        }
        private void Update(IMap map)
        {
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }
        public void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            //foreach (var e in this.Emitters)
            //    e.Draw(camera, parent.Map, e.Source);
        }
    }
}
