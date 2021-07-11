using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class EntityCompParticles : BlockEntityComp
    {
        readonly HashSet<ParticleEmitter> Emitters = new HashSet<ParticleEmitter>();
        public EntityCompParticles(params ParticleEmitter[] emitters)
        {
            for (int i = 0; i < emitters.Length; i++)
            {
                this.Emitters.Add(emitters[i]);
            }
        }
        
        public void AddEmitter(ParticleEmitter emitter)
        {
            this.Emitters.Add(emitter);
        }
        
        public override void Tick(MapBase map, BlockEntity entity, IntVec3 global)
        {
            foreach (var e in this.Emitters)
                e.Update(map);
        }
        public override void Draw(Camera camera, MapBase map, Vector3 global)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, map, global);
        }
    }
}
