using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Graphics;
using Start_a_Town_.Graphics.Animations;
using Start_a_Town_.Components.Stats;
using Start_a_Town_.Components.Particles;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class MobileComponent : Component
    {
        public class State
        {
            public string Name { get; set; }
            public float Speed { get; set; }
            public float SprintSpeed { get; set; }
            public float AnimationWeight { get; set; }
            public float AnimationSpeed { get; set; }
            public bool AllowJump { get; set; }
            public State(string name, float speed, float sprintSpeed, float animationWeight, float animationSpeed, bool allowJump)// :this()
            {
                this.Name = name;
                this.Speed = speed;
                this.SprintSpeed = sprintSpeed;
                this.AnimationWeight = animationWeight;
                this.AnimationSpeed = animationSpeed;
                this.AllowJump = allowJump;
            }
            public void Apply(MobileComponent component)
            {
                component.Speed = this.Speed;
                foreach (var item in component.AnimationWalk.Inner.Values)
                {
                    item.Weight = this.AnimationWeight;
                    item.Speed = this.AnimationSpeed;
                }
            }
            public override string ToString()
            {
                return this.Name;
            }
            //static public readonly State Stopped = new State(0);
            static public readonly State Walking = new State("Walking", speed: 0.66f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);
            //static public readonly State Running = new State("Running", speed: 1f, sprintSpeed: 0, animationWeight: 1f, animationSpeed: 1f, allowJump: true);// 1f, 1f, 1);
            static public readonly State Running = new State("Running", speed: 1f, sprintSpeed: 0, animationWeight: 0.75f, animationSpeed: 1f, allowJump: true);// 1f, 1f, 1);
            static public readonly State Sprinting = new State("Sprinting", speed: 1f, sprintSpeed: 0.5f, animationWeight: 1f, animationSpeed: 1.5f, allowJump: true); //1.5f
            static public readonly State Blocking = new State("Blocking", speed: 0.5f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false); //1.5f
        }
       

        public override string ComponentName
        {
            get { return "Mobile"; }
        }
        public override object Clone()
        {
            return new MobileComponent();
        }
        //float Acceleration = 0f;
        //public float Speed = 1f;
        float Acceleration { get { return (float)this["Acceleration"]; } set { this["Acceleration"] = value; } }
        public float Speed { get { return (float)this["Speed"]; } set { this["Speed"] = value; } }
        AnimationWalk AnimationWalk { get { return (AnimationWalk)this["Animation"]; } set { this["Animation"] = value; } }
        AnimationCollection AnimationJump { get { return (AnimationCollection)this["AnimationJump"]; } set { this["AnimationJump"] = value; } }
        public bool Moving { get { return (bool)this["Moving"]; } set { this["Moving"] = value; } }
        //Vector3 LastGroundPos { get { return (Vector3)this["LastGroundPos"]; } set { this["PreJumpPos"] = value; } }

        public State CurrentState
        {
            get { return (State)this["State"]; }
            set
            {
                this["State"] = value;
                value.Apply(this);
            }
        }

        public MobileComponent()
        {
            //this.AnimationWalk = AnimationCollection.Walking;
            this.AnimationWalk = new AnimationWalk();
            this.Acceleration = 0f;
            this.Speed = 1f;
            this.Moving = false;
            this.CurrentState = State.Running;
        }

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            //this.AnimationWalk = new AnimationWalking() { OnFootDown = () => EmitDust(parent) };

            this.AnimationJump = new AnimationJump();// AnimationCollection.Jumping;
            this.AnimationJump.Weight = 0;
            //parent.Body.Start(this.AnimationJump);
            parent.Body.AddAnimation(this.AnimationJump);

        }
        public override void ObjectLoaded(GameObject parent)
        {

            //this.AnimationWalk = new AnimationWalking() { OnFootDown = () => EmitDust(parent) };

            //this.AnimationJump = AnimationCollection.Jumping;
            this.AnimationJump = new AnimationJump();// AnimationCollection.Jumping;

            this.AnimationJump.Weight = 0;
            //parent.Body.Start(this.AnimationJump);
            parent.Body.AddAnimation(this.AnimationJump);

        }

        public void Start(GameObject parent)
        {
            //this.Start(parent, State.Running);
            this.Start(parent, this.CurrentState);
        }
        public void Start(GameObject parent, State state)
        {
            //if (this.CurrentState == state)
            //    return;
            this.CurrentState = state;
            this.Acceleration = 0;
            // TODO: don't create new animation instance every time
            this.AnimationWalk = new AnimationWalk();
            if (parent.Net is Net.Client)
                this.AnimationWalk.OnFootDown = () => EmitDust(parent);

            //foreach (var child in this.AnimationWalk)
            //{
            //    child.Value.State = Animation.States.Running;
            //    child.Value.Frame = 0;
            //    child.Value.Weight = 0;
            //}
            this.CurrentState.Apply(this);
            //parent.Body.Start(this.AnimationWalk);
            parent.Body.AddAnimation(this.AnimationWalk);
            //this.AnimationWalk.Speed = 0.1f;
            parent.TryGetComponent<ControlComponent>(c => c.Interrupt(parent));
            parent.Net.PostLocalEvent(parent, Message.Types.Walk);
            this.Moving = true;
        }
        public void Stop(GameObject parent)
        {
            //foreach (var item in this.Animation.Values)
            //    item.WeightFunc = () => item.WeightValue;
            this.Acceleration = 0;
            //parent.Body.FadeOut(this.AnimationWalk);
            parent.Body.FadeOutAnimation(this.AnimationWalk);

            this.Moving = false;
        }

        public void Jump(GameObject parent)
        {
            if(parent.Net is Net.Client)
            {
                parent.Net.PostLocalEvent(parent, Message.Types.Jumped);
                return;
            }
            Vector3 force = Vector3.Zero;
            //parent.Global.ToConsole();
            var feetposition = parent.Global + Vector3.UnitZ * 0.1f;
            var cell = parent.Net.Map.GetCell(feetposition);
            var block = cell.Block;// parent.Net.Map.GetBlock(parent.Global + Vector3.UnitZ * 0.1f); // to check if entity is in water
            if (!this.CurrentState.AllowJump)// != State.Walking)
                return;
            if (parent.Velocity.Z != 0)
                return;
            if (block == Block.Water)
            {
                force = Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
                //if (cell.BlockData == 1)
                var density = Block.Water.GetDensity(cell.BlockData, feetposition);
                    force *= (1 + 3 * density);
                    //force *= (1 + 20 * Block.Water.Density);
            }
            else
                if (parent.Velocity.Z == 0 && parent.Net.Map.IsSolid(parent.Global - Vector3.UnitZ * 0.1f))
                {
                    ////Resource stamina = parent.GetComponent<ResourcesComponent>().Resources[Resource.Types.Stamina];
                    //parent.Velocity += Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f)); // * stamina.Percentage;
                    //parent.Net.PostLocalEvent(parent, Message.Types.Jumped);

                    force = Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
                }

            if (force == Vector3.Zero)
                return;
            parent.Velocity += force;//Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f)); // * stamina.Percentage;
            parent.Net.PostLocalEvent(parent, Message.Types.Jumped);
        }
        public void JumpOld(GameObject parent)
        {
            if (!this.CurrentState.AllowJump)// != State.Walking)
                return;
            if (parent.Velocity.Z == 0 &&
                //(parent.Global - Vector3.UnitZ * 0.1f).IsSolid(parent.Net.Map))
                parent.Net.Map.IsSolid(parent.Global - Vector3.UnitZ * 0.1f))
            {
                ////Resource stamina = parent.GetComponent<ResourcesComponent>().Resources[Resource.Types.Stamina];
                parent.Velocity += Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f)); // * stamina.Percentage;
                parent.Net.PostLocalEvent(parent, Message.Types.Jumped);
            }

        }
        public void ToggleWalk(bool toggle)
        {
            if(this.CurrentState!= State.Blocking)
                this.CurrentState = toggle ? State.Walking : State.Running;
        }
        public void ToggleSprint(bool toggle)
        {
            if (this.CurrentState != State.Blocking)
                this.CurrentState = toggle ? State.Sprinting : State.Running;
        }
        public void ToggleBlock(bool toggle)
        {
            this.CurrentState = toggle ? State.Blocking : State.Running;
        }

        float DistanceFromGround(GameObject parent)
        {
            int n = (int)parent.Global.RoundXY().Z;
            while (n >= 0)
            {
                if (parent.Map.IsSolid(new Vector3(parent.Global.X, parent.Global.Y, n)))
                {
                    //return parent.Global.Z - (n + 1);

                    var blockglobal = new Vector3(parent.Global.X, parent.Global.Y, n);
                    //var block = parent.Map.GetBlock(blockglobal);
                    //var offset = blockglobal + new Vector3(0.5f, 0.5f, 0);
                    //var within = offset - offset.Round();
                    //var blockheight = block.GetHeight(within.X, within.Y);
                    var blockheight = Block.GetBlockHeight(parent.Map, blockglobal);
                    return parent.Global.Z - (n + blockheight);
                }
                n--;
            }
            return parent.Global.Z;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch(e.Type)
            {
                case Message.Types.HitGround:
                    this.AnimationWalk.Foreach(a => a.Frame = 0);
                    break;

                default:
                    break;
            }
            return false;
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            UpdateParticles(parent);

            bool midair = parent.Velocity.Z != 0;

            this.AnimationJump.Weight = midair ? 1:0;//this.DistanceFromGround(parent) : 0;
            if (!this.Moving)
                return;

            //don't change direction midair, or change it by a smaller factor?
            //if (parent.Velocity.Z != 0)
            //    return; 

            Vector2 direction = parent.GetComponent<PositionComponent>().Direction;
            Acceleration = Math.Min(1, Acceleration + 0.1f);

            //if (this.Speed > 1)
            //{
            //    Resource stamina = parent.GetComponent<ResourcesComponent>().Resources[Resource.Types.Stamina];
            //    //decimal sprintSpeed = (decimal)((this.Speed - 1) * stamina.Percentage);
            //    float sprintSpeed = (this.Speed - 1) * stamina.Percentage;
            //    // sprintSpeed = (float)Math.Ceiling(sprintSpeed * 1000000) / 1000000;
            //    //sprintSpeed = (float)Math.Round(sprintSpeed, 2);
            //    this.Speed = 1.01f + sprintSpeed;
            //    if (this.Speed == 1)
            //        "ASD".ToConsole();
            //    //stamina.Value -= 0.1f;
            //    stamina.Add(-0.1f);
            //}
            Resource stamina = parent.GetComponent<ResourcesComponent>().Resources[Resource.Types.Stamina];
            //Stat walkStat = StatsComponentNew.GetStat(parent, Stat.Types.WalkSpeed);
            //var walkValue = walkStat.GetFinalValue(parent);
            var walkValue = StatsComponentNew.GetStatValueOrDefault(parent, Stat.Types.WalkSpeed, 1);
            float walkSpeed = (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.WalkSpeed, 0f)) * (Speed + this.CurrentState.SprintSpeed * stamina.Percentage) * Acceleration * PhysicsComponent.Walk * walkValue;
            if (this.CurrentState == State.Sprinting)
                stamina.Add(-0.01f);
            //apply stamina
            // TODO: make stamina resource change walkspeed instead of fetching stamina from here
            

            if (walkSpeed == 0)
                Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");

            // if in mid-air, move at half speed
            if (midair)
            {
                walkSpeed *= 0.5f;
                this.AnimationJump.Weight = 1;
            }

            float walkX = direction.X * walkSpeed;
            float walkY = direction.Y * walkSpeed;
            walkX = Math.Abs(parent.Velocity.X) > Math.Abs(walkX) ? parent.Velocity.X : walkX;
            walkY = Math.Abs(parent.Velocity.Y) > Math.Abs(walkY) ? parent.Velocity.Y : walkY;

            PreventFall(parent, ref walkX, ref walkY);
            var nextvelocity = new Vector3(walkX, walkY, parent.Velocity.Z);
            //if(nextvelocity == Vector3.Zero)
            //    "asdasd".ToConsole();
            parent.Velocity = nextvelocity;
        }

        private void UpdateParticles(GameObject parent)
        {
            foreach (var e in this.DustEmitters.ToList())
            {
                e.Update(parent.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.DustEmitters.Remove(e);
            }
        }

        /// <summary>
        /// Prevents falling off edges when walking.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="walkX"></param>
        /// <param name="walkY"></param>
        private void PreventFall(GameObject parent, ref float walkX, ref float walkY)
        {
            //if (this.Speed == 0.5f)
            //if(this.CurrentState.Equals(State.Walking))
            if (this.CurrentState == State.Walking)
            {
                Vector3 global = parent.Global;

                // chunk cell underneath current position, if not solid it means we're midair 
                if (!parent.Map.IsSolid(new Vector3(global.X + walkX, global.Y, global.Z + PhysicsComponent.Gravity)))
                    walkX = 0;
                if (!parent.Map.IsSolid(new Vector3(global.X, global.Y + walkY, global.Z + PhysicsComponent.Gravity)))
                    walkY = 0;

                //is this necessary?
                if (!parent.Map.IsSolid(new Vector3(global.X + walkX, global.Y + walkY, global.Z + PhysicsComponent.Gravity)))
                    walkY = walkX = 0;

                //// chunk cell underneath current position, if not solid it means we're midair 
                //if (!parent.Map.IsSolid(new Vector3(global.X + walkX, global.Y, global.Z - 1)))
                //    walkX = 0;
                //if (!parent.Map.IsSolid(new Vector3(global.X, global.Y + walkY, global.Z - 1)))
                //    walkY = 0;

                ////is this necessary?
                //if (!parent.Map.IsSolid(new Vector3(global.X + walkX, global.Y + walkY, global.Z - 1)))
                //    walkY = walkX = 0;
            }
        }

        List<ParticleEmitter> DustEmitters = new List<ParticleEmitter>();
        public void EmitDust(GameObject parent)
        {
            if (parent.Velocity.Z != 0)
                return;
            //var emitter = CreateDust(parent); //CreateDirt(parent);// 

            var block = parent.Map.GetBlock(parent.Global - .01f * Vector3.UnitZ);
            var emitter = block.GetEmitter();//.GetDustEmitter();
            emitter.Source = parent.Global;
            emitter.Emit(10, -parent.Velocity * .1f);
            this.DustEmitters.Add(emitter);
        }

        private static ParticleEmitterSphere CreateDust(GameObject parent)
        {
            //var block = parent.Map.GetBlock(parent.Global - Vector3.UnitZ * 0.1f);
            //var dustcolor = block.DustColor;
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
                Force = .01f
            };
            return emitter;
        }
        private static ParticleEmitterSphere CreateDirt(GameObject parent)
        {
            var block = parent.Map.GetBlock(parent.Global - Vector3.UnitZ * 0.1f);
            var dustcolor = block.DirtColor;
            var emitter = new ParticleEmitterSphere() //DustEmitter.Clone() as ParticleEmitterSphere;
            {
                Lifetime = Engine.TargetFps / 2f,
                Offset = Vector3.Zero,
                Rate = 0,
                ParticleWeight = 1f,//1f,
                ColorEnd = dustcolor * .5f,//Color.SaddleBrown * .5f,
                ColorBegin = dustcolor,// Color.SaddleBrown,
                SizeEnd = 1,
                SizeBegin = 1,
                Force = .05f
            };
            return emitter;
        }
        static readonly ParticleEmitterSphere DustEmitter = new ParticleEmitterSphere()
        {
            Lifetime = Engine.TargetFps,
            Offset = Vector3.Zero,
            Rate = 0,
            ParticleWeight = 1f,
            ColorEnd = Color.SaddleBrown,
            ColorBegin = Color.SaddleBrown,
            SizeEnd = 1,
            SizeBegin = 1,
            Force = .05f
        };
        //public override void Update(GameObject parent)
        //{
        //    foreach (var e in this.DustEmitters.ToList())
        //    {
        //        e.Update(parent.Map, e.Source);
        //        if (e.Particles.Count == 0)
        //            this.DustEmitters.Remove(e);
        //    }
        //}
        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            foreach (var e in this.DustEmitters)
                e.Draw(camera, parent.Map, e.Source);
        }
    }
}
