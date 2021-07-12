using System.Collections.Generic;
using Start_a_Town_.Blocks;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class EntityCompParticles : BlockEntityComp
    {
        readonly HashSet<ParticleEmitter> Emitters = new();
        public EntityCompParticles(params ParticleEmitter[] emitters)
        {
            for (int i = 0; i < emitters.Length; i++)
            {
                this.AddEmitter(emitters[i]);
            }
        }
        
        public void AddEmitter(ParticleEmitter emitter)
        {
            this.Emitters.Add(emitter);
        }
        
        public override void Tick(BlockEntity entity, MapBase map, IntVec3 global)
        {
            foreach (var e in this.Emitters)
                e.Update(map);
        }
        public override void Draw(Camera camera, MapBase map, IntVec3 global)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, map, global);
        }
    }
}
