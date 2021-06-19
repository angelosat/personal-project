using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    class BloodComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Blood"; }
        }
        public override object Clone()
        {
            return new BloodComponent();
        }

        static readonly ParticleEmitterSphere BloodEmitter = new ParticleEmitterSphere()
        {
            Lifetime = Engine.TicksPerSecond * 5,
            Offset = Vector3.UnitZ,
            Rate = 0,
            ParticleWeight = 1f,
            ColorEnd = Color.Red,
            ColorBegin = Color.Red,
            SizeEnd = 3,
            SizeBegin = 3,
            SizeVariance = 2,
            Force = .1f
        };
        List<ParticleEmitterSphere> Emitters = new List<ParticleEmitterSphere>();

        public BloodComponent()
        {

        }
        public override void Tick(GameObject parent)
        {
            //this.BloodEmitter.Update(parent.Map, parent.Global);
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(parent.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.Attacked:
                    //this.BloodEmitter.Emit(10);
                    if (parent.Net is Server)
                        break;
                    GameObject attacker = e.Parameters[0] as GameObject;
                    var direction = parent.Global - attacker.Global;// attacker.Velocity;
                    direction.Normalize();
                    direction *= .05f;
                    direction += attacker.Velocity;

                    var emitter = BloodEmitter.Clone() as ParticleEmitterSphere;
                    emitter.Source = parent.Global;
                    emitter.Emit(10, direction);
                    this.Emitters.Add(emitter);
                    break;

                default:
                    break;
            }
            return true;
        }

        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            //this.BloodEmitter.Draw(camera, parent.Map, parent.Global);
            foreach (var e in this.Emitters)
                e.Draw(camera, parent.Map, e.Source);
        }
    }
}
