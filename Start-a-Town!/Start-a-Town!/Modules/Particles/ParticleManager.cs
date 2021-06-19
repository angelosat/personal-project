using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.GameEvents;
using Start_a_Town_.Net;

namespace Start_a_Town_.Particles
{
    public class ParticleManager
    {
        // HashSet cause we dont want to add the same emitter twice
        readonly HashSet<ParticleEmitter> Emitters = new();
        readonly IMap Map;
        public ParticleManager(IMap map)
        {
            this.Map = map;
        }
        public void AddEmitter(ParticleEmitter emitter)
        {
            if (this.Map.Net is Net.Server)
                return;
            this.Emitters.Add(emitter);
        }
        public void Update()
        {
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(this.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }
        public void Draw(Camera camera)
        {
            foreach (var e in this.Emitters)
            {
                e.Draw(camera, this.Map, e.Source);
            }
        }

        public void OnGameEvent(GameEvent e)
        {
            if (e.Net is Server)
                return;
            switch (e.Type)
            {
                case Components.Message.Types.EntityHitCeiling:
                    this.EntityHitCeiling(e);
                    break;

                case Components.Message.Types.EntityHitGround:
                    this.EntityHitGround(e);
                    break;

                case Components.Message.Types.EntityFootStep:
                    this.EntityFootStep(e);
                    break;

                //case Components.Message.Types.ParticleEmitterAdd:
                //    //this.AddEmitter(e.Parameters[0] as ParticleEmitter);
                //    break;

                default:
                    break;
            }
        }

        void EntityHitGround(GameEvent e)
        {
            var entity = e.Parameters[0] as GameObject;
            var vector3 = (Vector3)e.Parameters[1];
            var block = entity.Map.GetBlock(vector3);
            var emitter = block.GetEmitter();//.GetDustEmitter();
            emitter.Source = entity.Global;// +Vector3.UnitZ;
            //emitter.Lifetime = Engine.TargetFps * .1f;
            emitter.Emit(10);//, -parent.Velocity * .1f);
            this.Emitters.Add(emitter);
        }

        void EntityHitCeiling(GameEvent e)
        {
            var entity = e.Parameters[0] as GameObject;
            var vector3 = (Vector3)e.Parameters[1];
            var block = entity.Map.GetBlock(vector3);
            var emitter = block.GetEmitter();//.GetDustEmitter();
            emitter.Source = new Vector3(vector3.XY(), (float)Math.Floor(vector3.Z) - .1f);// +Vector3.UnitZ;
            //emitter.Lifetime = Engine.TargetFps * .1f;
            emitter.Emit(10);//, -parent.Velocity * .1f);
            this.Emitters.Add(emitter);
        }

        void EntityFootStep(GameEvent e)
        {
            var entity = e.Parameters[0] as GameObject;
            //var block = entity.Map.GetBlock(entity.Global - .01f * Vector3.UnitZ);
            var vec = new Vector3(entity.Global.X, entity.Global.Y, (int)Math.Ceiling(entity.Global.Z) - 1);
            var block = entity.Map.GetBlock(vec);
            var emitter = block.GetEmitter();//.GetDustEmitter();
            emitter.Source = entity.Global;
            emitter.Emit(10, -entity.Velocity * .1f);
            this.Emitters.Add(emitter);
        }
    }
}
