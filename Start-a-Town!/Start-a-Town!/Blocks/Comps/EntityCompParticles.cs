using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class EntityCompParticles : BlockEntityComp
    {
        readonly HashSet<ParticleEmitter> Emitters = new HashSet<ParticleEmitter>();
        //Func<bool> Condition { get; set; } = () => true;
        public EntityCompParticles(params ParticleEmitter[] emitters)
        {
            for (int i = 0; i < emitters.Length; i++)
            {
                this.Emitters.Add(emitters[i]);
            }
        }
        //public EntityCompParticles SetCondition(Func<bool> condition)
        //{
        //    this.Condition = condition;
        //    return this;
        //}
        public void AddEmitter(ParticleEmitter emitter)
        {
            this.Emitters.Add(emitter);
        }
        //public virtual void Tick(IObjectProvider net, IEntityCompContainer entity, Vector3 global)
        //{
        //    foreach (var e in this.Emitters)
        //        e.Update();
        //}
        public override void Tick(IObjectProvider net, BlockEntity entity, Vector3 global)
        {
            foreach (var e in this.Emitters)
                e.Update();
        }
        public override void Draw(Camera camera, IMap map, Vector3 global)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, map, global);
        }
    }
}
