using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    class BombComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Bomb"; }
        }
        public override object Clone()
        {
            return new BombComponent(this.Radius, this.Fuse);
        }

        public int Radius, Fuse, FuseMax;
        //public ParticleEmitterSphere Emitter;
        public bool Exploded;
        int FuseLengthIndex;
        //public List<ParticleEmitterSphere> Emitters = new List<ParticleEmitterSphere>();
        static int[] FuseLengths = new int[] { -1, 1, 2, 3 };

        public BombComponent(int radius, int fuse)
        {
            this.Radius = radius;
            this.Fuse = fuse;
            //this.FuseMax = -1;
        }

        public override void Tick(GameObject parent)
        {
            if (this.FuseMax == -1)
                return;
            if (!this.Exploded)
            {
                this.Fuse--;
                if (this.Fuse <= 0)
                    this.Explode(parent);
                return;
            }
            //if (this.Emitter == null)
            //{
            //    parent.Despawn();
            //    parent.Dispose();
            //    return;
            //}

            //this.Emitter.Update();
            //if (this.Emitter.Particles.Count == 0)
            //{
            //    parent.Despawn();
            //    parent.Dispose();
            //}
        }

        void Explode(GameObject parent)
        {
            this.Exploded = true;
            var origin = parent.Transform.Global.Round();
            var r = this.Radius;
            var map = parent.Map;
            var rsquared = this.Radius * this.Radius;
            for (int i = -r; i <= r; i++)
                for (int j = -r; j <= r; j++)
                    for (int k = -r; k <= r; k++)
                    {
                        var pos = origin + new Vector3(i, j, k);
                        var block = map.GetBlock(pos);
                        if (block == null || block == BlockDefOf.Air)
                            continue;
            
                        var d = Vector3.DistanceSquared(pos, origin);
                        if (d < rsquared)
                        {
                            block.Remove(map, pos);
                            //var e = block.GetEmitter();
                            //e.Source = pos + Vector3.UnitZ * 0.5f;
                            //e.SizeBegin = 1;
                            //e.SizeEnd = 1;
                            //e.ParticleWeight = 1;
                            //e.Radius = 1f;// .5f;
                            //e.Force = .1f;
                            //e.Friction = .5f;
                            //e.AlphaBegin = 1;
                            //e.AlphaEnd = 0;
                            //e.ColorBegin = block.Material.Color;// Color.White;
                            //e.ColorEnd = block.Material.Color;//Color.White;

                            //e.Lifetime = Engine.TargetFps * 2;
                            //var pieces = block.GetParticleRects(5);

                            //var direction = pos - parent.Global;// attacker.Velocity;
                            //direction.Normalize();
                            //direction *= .2f;

                            //e.Emit(Block.Atlas.Texture, pieces, direction);
                            //this.Emitters.Add(e);
                        }
                    }
            //parent.Despawn();
            //parent.Dispose();
            parent.GetComponent<SpriteComponent>().Hidden = true;
            var emitter = ParticleEmitter.Dust;
            emitter.ColorBegin = Color.White;
            emitter.ColorEnd = Color.Black;
            emitter.Source = parent.Global;
            emitter.Force = 3f;
            emitter.SizeBegin = 20;
            emitter.Lifetime = Engine.TicksPerSecond * 4;
            emitter.ParticleWeight = -.5f;
            emitter.Acceleration = new Vector3(.5f, .5f, .5f);
            emitter.HasPhysics = false;
            emitter.Emit(20);
            //parent.Map.EventOccured(Message.Types.ParticleEmitterAdd, emitter);
            parent.Map.ParticleManager.AddEmitter(emitter);
            this.PushEntities(parent);
        }

        void PushEntities(GameObject parent)
        {
            var rsquared = this.Radius * this.Radius;
            var global = parent.Transform.Global;
            var entities = parent.Map.GetObjectsAtChunk(global);
            foreach(var entity in entities)
            {
                var distance = entity.Transform.Global + entity.Physics.Height * Vector3.UnitZ * .5f - global;
                var d = distance.LengthSquared();
                if (d < rsquared)
                {
                    var t = 1 - d / (float)rsquared; //1 is ground zero , 0 is edge of radius
                    var force = t * distance.Normalized() *.5f; // TODO: factor entity weight
                    entity.Velocity += force;
                }
            }
        }

        static public GameObject GetEntity(int radius, int fuse)
        {
            var entity = new GameObject(GameObject.Types.Bomb, "Bomb", "Explodes and removes blocks", ObjectType.Weapon);
            entity.AddComponent(new SpriteComponent(new Bone(BoneDef.Torso, Sprite.Default)));
            entity.AddComponent(new BombComponent(radius, fuse));
            entity.AddComponent(new PhysicsComponent(size: 1, solid: false, height: 1, weight: 1));
            return entity;
        }

        //public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        //{
        //    if (this.Emitter == null)
        //        return;
        //    this.Emitter.Draw(camera, parent.Map, this.Emitter.Source);
        //    //foreach(var emitter in this.Emitters)
        //    //    emitter.Draw(camera, parent.Map, emitter.Source);
        //}

        internal override void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list)
        {
            list.Add(new InteractionSetFuse(this.CycleFuse));
        }

        int CycleFuse()
        {
            this.FuseLengthIndex++;
            this.FuseLengthIndex %= FuseLengths.Length;
            this.FuseMax = FuseLengths[this.FuseLengthIndex];
            this.Fuse = this.FuseMax * Engine.TicksPerSecond;
            return this.FuseMax;
        }

        class InteractionSetFuse : Interaction
        {
            Func<int> FuseSet;
            public override object Clone()
            {
                return new InteractionSetFuse(this.FuseSet);
            }
            public InteractionSetFuse(Func<int> callback)
            {
                this.Name = "Set fuse";
                this.FuseSet = callback;
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                var fuse = this.FuseSet();
                var client = a.Net as Net.Client;
                if (client != null)
                {
                    client.GetConsole().Write(Color.Red, "Fuse set to: " + (fuse == -1 ? "none" : TimeSpan.FromSeconds(fuse).TotalSeconds.ToString("#s")));
                }
            }
        }
    }
}
