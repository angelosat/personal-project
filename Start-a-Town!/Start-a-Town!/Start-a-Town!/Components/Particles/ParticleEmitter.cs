using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Physics;

namespace Start_a_Town_.Components.Particles
{
    public abstract class ParticleEmitter : ICloneable
    {
        

        public List<Particle> Particles = new List<Particle>();
        public Random Random = new Random();
        public float Radius;// = 0.2f;
        public Vector3 Offset = Vector3.Zero;
        public float ParticleWeight;// = 0;
        public float Lifetime = Engine.TargetFps;
        public float Friction = 0.5f;
        public int Rate = 1;
        public Color ColorBegin = Color.White, ColorEnd = Color.White;
        public float AlphaBegin = 1, AlphaEnd = 0;
        public float SizeBegin = 1, SizeEnd = 1;
        public float SizeVariance = 0;
        public bool HasPhysics = true;
        public Vector3 Acceleration = Vector3.One;

        //public Texture2D Texture = UI.UIManager.Highlight;
        //public Rectangle SourceRectangle = new Rectangle(0, 0, 1, 1);
        public Texture2D Texture = Block.ParticlePixel.Atlas.Texture;// UI.UIManager.Highlight;
        public Rectangle SourceRectangle = Block.ParticlePixel.Rectangle;//new Rectangle(0, 0, 1, 1);

        public Vector3 Source;

        public ParticleEmitter(Vector3 offset, float lifetime, float radius = 0, float particleWeight = 0)
        {
            this.Lifetime = lifetime;
            this.Radius = radius;
            this.Offset = offset;
            this.ParticleWeight = particleWeight;
        }
        public ParticleEmitter(float radius = 0)
        {
            this.Radius = radius;
        }

        /// <summary>
        /// Update particles with physics
        /// </summary>
        /// <param name="map"></param>
        /// <param name="global"></param>
        public void Update(IMap map, Vector3 global)
        {
            if (map.Net is Net.Server)
                throw new Exception();
            if (!HasPhysics)
            {
                this.Update();
                return;
            }
            //var global = this.Global;
            foreach (var p in this.Particles.ToList())
            {
                if (p.Lifetime <= 0)
                    this.Particles.Remove(p);
                //var adjustment = new Vector3(0, 0, PhysicsComponent.Gravity * this.ParticleWeight);
                //p.Update(adjustment);

                //if (HasPhysics)
                //{
                    var pos = global + this.Offset + p.Offset;
                    Vector3 nextpos, nextvel;
                    Physics.Physics.Update(this.ParticleWeight, this.Friction, map, pos, p.Velocity, out nextpos, out nextvel);
                    p.Offset = nextpos - global - this.Offset;
                    p.Velocity = nextvel;
                //}
                //else
                //{
                //    var adjustment = new Vector3(0, 0, PhysicsComponent.Gravity * this.ParticleWeight);
                //    p.Velocity += adjustment;
                //    p.Offset += p.Velocity;
                //}


                p.Update();
            }

            //for (int i = 0; i < this.Rate; i++)
            //{
                Emit(this.Rate);
            //}
        }
        /// <summary>
        /// Update particles without physics
        /// </summary>
        public void Update()
        {
            Vector3 adjustment;
            foreach (var p in this.Particles.ToList())
            {
                if (p.Lifetime <= 0)
                    this.Particles.Remove(p);
                adjustment = new Vector3(0, 0, PhysicsComponent.Gravity * this.ParticleWeight);
                p.Velocity += adjustment;
                p.Velocity *= this.Acceleration;
                p.Offset += p.Velocity;
                p.Update();
            }
            Emit(this.Rate);
        }
        public void Emit(int count)
        {
            this.Emit(count, Vector3.Zero);
        }
        public void Emit(int count, Vector3 force)
        {
            count = (int)(count * ParticleDensityLevel.Current.Factor);
            for (int i = 0; i < count; i++)
            {
                var startVelocity = this.GetStartVelocity();
                var startOffset = this.GetStartOffset();
                var scalevariance = (float)this.Random.NextDouble() * this.SizeVariance * 2;
                scalevariance -= (this.SizeVariance / 2f);

                //var particle = new Particle(startOffset, startVelocity, this.Lifetime) { ScaleFunc = p => p.LifePercentage * 3, ColorFunc = p => Color.Lerp(Color.Red, Color.Yellow, p.LifePercentage) };
                var particle = new Particle(startOffset, startVelocity + force, this.Lifetime)
                {
                    //ScaleFunc = p => p.LifePercentage * 3,
                    ScaleFunc = p =>
                    {
                        return (this.SizeBegin + scalevariance) * p.LifePercentage + (this.SizeEnd + scalevariance) * (1 - p.LifePercentage);
                    },// Size p.LifePercentage * 3,
                    ColorFunc = p => Color.Lerp(this.ColorEnd, this.ColorBegin, p.LifePercentage),
                    Texture = this.Texture,
                    SourceRectangle = this.SourceRectangle,
                    AlphaFunc = p => AlphaBegin * p.LifePercentage + AlphaEnd * (1 - p.LifePercentage)
                };

                this.Particles.Add(particle);
                if (float.IsNaN(particle.Velocity.X) || float.IsNaN(particle.Velocity.Y))
                    throw new Exception();
            }
        }
        public void Emit(Texture2D texture, List<Rectangle> pieces, Vector3 force)
        {
            foreach(var rect in pieces)
            {
                var startVelocity = this.GetStartVelocity();
                var startOffset = this.GetStartOffset();
                var scalevariance = (float)this.Random.NextDouble() * this.SizeVariance * 2;
                scalevariance -= (this.SizeVariance / 2f);

                //var particle = new Particle(startOffset, startVelocity, this.Lifetime) { ScaleFunc = p => p.LifePercentage * 3, ColorFunc = p => Color.Lerp(Color.Red, Color.Yellow, p.LifePercentage) };
                var particle = new Particle(startOffset, startVelocity + force, this.Lifetime)
                {
                    //ScaleFunc = p => p.LifePercentage * 3,
                    ScaleFunc = p =>
                    {
                        return (this.SizeBegin + scalevariance) * p.LifePercentage + (this.SizeEnd + scalevariance) * (1 - p.LifePercentage);
                    },// Size p.LifePercentage * 3,
                    ColorFunc = p => Color.Lerp(this.ColorEnd, this.ColorBegin, p.LifePercentage),
                    Texture = texture,
                    SourceRectangle = rect,
                    AlphaFunc = p => AlphaBegin * p.LifePercentage + AlphaEnd * (1 - p.LifePercentage)
                };

                this.Particles.Add(particle);
            }
        }
        protected abstract Vector3 GetStartVelocity();
        //{
        //    double θ = this.Random.NextDouble() * (Math.PI + Math.PI);
        //    double φ = this.Random.NextDouble() * (Math.PI);
        //    float x = (float)(Math.Sin(φ) * Math.Cos(θ));
        //    float y = (float)(Math.Sin(φ) * Math.Sin(θ));
        //    float z = (float)Math.Cos(φ);
        //    Vector3 direction = new Vector3(x, y, z);
        //    direction.Normalize();
        //    var startVelocity = direction * 0.01f;
        //    return startVelocity;
        //}
        Vector3 GetStartOffset()
        {
            double θ = this.Random.NextDouble() * (Math.PI + Math.PI);
            var r = (float)this.Random.NextDouble() * this.Radius;
            float x = (float)Math.Cos(θ);
            float y = (float)Math.Sin(θ);
            var startOffset = new Vector3(x, y, 0) * r;
            return startOffset;
        }

        //public void Draw(Camera cam, IMap map, Vector3 global)
        //{
        //    SortParticlesByDepth(cam, map);
        //    foreach (var particle in this.Particles)
        //        particle.Draw(cam, map, global + this.Offset);
        //}
        public void Draw(Camera cam, IMap map, Vector3 global)
        {
            SortParticlesByDepth(cam, map);
            foreach (var particle in this.Particles)
                particle.Draw(cam, map, global + this.Offset);
        }
        private void SortParticlesByDepth(Camera cam, IMap map)
        {
            this.Particles.Sort((p1, p2) =>
            {
                var d1 = p1.Offset.GetDrawDepth(map, cam);
                var d2 = p2.Offset.GetDrawDepth(map, cam);
                if (d1 < d2)
                    return -1;
                if (d1 > d2)
                    return 1;
                return 0;
            });
        }

        public abstract object Clone();
        //{
        //    return new ParticleEmitter(this.Offset, this.Lifetime, this.Radius, this.ParticleWeight);
        //}

        static public ParticleEmitterSphere Fire
        {
            get
            {
                return new ParticleEmitterSphere()
                {
                    Offset = new Vector3(0, 0, .5f),
                    Lifetime = Engine.TargetFps,
                    Radius = .2f,
                    ParticleWeight = -.1f,
                    Force = .01f,
                    Friction = .5f,
                    ColorBegin = Color.Yellow,
                    ColorEnd = Color.Red,
                    SizeBegin = 3,
                    SizeEnd = 1
                };
            }
        }
        static public ParticleEmitterSphere Dust
        {
            get
            {
                var emitter = new ParticleEmitterSphere() //DustEmitter.Clone() as ParticleEmitterSphere;
                {
                    Lifetime = Engine.TargetFps / 2f,
                    Offset = Vector3.Zero,
                    Rate = 0,
                    ParticleWeight = 0f,//1f,
                    ColorEnd = Color.White * .5f,
                    ColorBegin = Color.White,
                    SizeEnd = 1,
                    SizeBegin = 3,
                    Force = .01f,
                    Friction = 0f
                };
                return emitter;
            }
        }
    }
}
