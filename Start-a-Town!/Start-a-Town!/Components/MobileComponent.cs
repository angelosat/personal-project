﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;
using Start_a_Town_.Components.Stats;
using Start_a_Town_.Particles;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class MobileComponent : EntityComponent
    {
        public class State
        {
            public enum Types { Walking, Running, Sprinting, Blocking };
            public Types Type;
            public string Name;// { get; set; }
            public float Speed;// { get; set; }
            public float SprintSpeed;// { get; set; }
            public float AnimationWeight;// { get; set; }
            public float AnimationSpeed;// { get; set; }
            public bool AllowJump;// { get; set; }
            public State(Types type, float speed, float sprintSpeed, float animationWeight, float animationSpeed, bool allowJump)// :this()
            {
                this.Type = type;
                this.Name = type.ToString();
                this.Speed = speed;
                this.SprintSpeed = sprintSpeed;
                this.AnimationWeight = animationWeight;
                this.AnimationSpeed = animationSpeed;
                this.AllowJump = allowJump;
            }
            public void Apply(MobileComponent component)
            {
                //component.AllowJump = this.AllowJump;
                //component.Speed = this.Speed;

                component.AnimationWalk.Weight = this.AnimationWeight;
                component.AnimationWalk.Speed = this.AnimationSpeed;

                //foreach (var item in component.AnimationWalk.Inner.Values)
                //{
                //    item.Weight = this.AnimationWeight;
                //    item.Speed = this.AnimationSpeed;
                //}
            }
            public override string ToString()
            {
                return this.Name;
            }
            //static public readonly State Stopped = new State(0);
            static readonly State Walking = new(Types.Walking, speed: 0.66f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false);
            //static public readonly State Running = new State("Running", speed: 1f, sprintSpeed: 0, animationWeight: 1f, animationSpeed: 1f, allowJump: true);// 1f, 1f, 1);
            static readonly State Running = new(Types.Running, speed: 1f, sprintSpeed: 0, animationWeight: 0.75f, animationSpeed: 1f, allowJump: true);// 1f, 1f, 1);
            static readonly State Sprinting = new(Types.Sprinting, speed: 1f, sprintSpeed: 0.5f, animationWeight: 1f, animationSpeed: 1.5f, allowJump: true); //1.5f
            static readonly State Blocking = new(Types.Blocking, speed: 0.5f, sprintSpeed: 0, animationWeight: 0.5f, animationSpeed: 1, allowJump: false); //1.5f

            static public Dictionary<Types, State> States = new()
            {
                {Types.Walking, Walking },
                {Types.Running, Running },
                {Types.Sprinting, Sprinting },
                {Types.Blocking, Blocking }
            };
        }

        public const float NormalWalkSpeed = .1f;// 0.08f; when i used friction wrongly

        public override string ComponentName
        {
            get { return "Mobile"; }
        }
        public override object Clone()
        {
            return new MobileComponent();
        }

        public float Acceleration;

        //AnimationWalk AnimationWalk;
        readonly Animation AnimationWalk;
        readonly Animation AnimationJump;
        readonly Animation AnimationCrouch;

        public bool Moving;


        State CurrentState;
        internal bool Crouching => this.AnimationCrouch.Weight > 0;
        const float AccelerationStep = .1f;
        public MobileComponent()
        {
            this.AnimationWalk = new(AnimationDef.Walk);
            this.AnimationJump = new(AnimationDef.Jump);
            this.AnimationCrouch = new(AnimationDef.Crouch);

            this.AnimationJump.Weight = 0;
            this.AnimationWalk.Weight = 0;
            this.AnimationCrouch.Weight = 0;

            this.Acceleration = 0f;
            this.Moving = false;
            this.CurrentState = State.States[State.Types.Running];
            this.CurrentState.Apply(this);
        }

        
        static public void OnFootDown(GameObject parent)
        {
            EmitDust(parent);
        }

        public override void OnObjectCreated(GameObject parent)
        {
            parent.AddAnimation(this.AnimationJump);
            parent.AddAnimation(this.AnimationWalk); // why not add it on creation?
            parent.AddAnimation(this.AnimationCrouch);
        }

        public void Toggle(GameObject parent, bool toggle)
        {
            if (toggle)
                this.Start(parent);
            else
                this.Stop(parent);
        }
        public void Start(GameObject parent)
        {
            this.Start(parent, this.CurrentState);
        }
        void Start(GameObject parent, State state)
        {
            if (this.Moving)
                return;
                //throw new Exception();
            this.CurrentState = state;
            this.Acceleration = AccelerationStep;
            // TODO: don't create new animation instance every time
            //this.AnimationWalk = new AnimationWalk();

            //if (parent.Net is Client)
            //    this.AnimationWalk.OnFootDown = () => EmitDust(parent);

            this.AnimationWalk.Weight = 1;
            this.AnimationWalk.WeightChange = 0;
            this.AnimationWalk.Restart();
            this.AnimationWalk.Enabled = true;

            this.CurrentState.Apply(this);
            //parent.TryGetComponent<ControlComponent>(c => c.Interrupt(parent));
            parent.Net.PostLocalEvent(parent, Message.Types.Walk);
            this.Moving = true;
            //this.AnimationCrouch.Weight = 1;//.FadeIn(false, 10, Interpolation.Lerp);
        }
        public void Stop(GameObject parent)
        {
            if (this.Acceleration == 0)
                return;
            this.Acceleration = 0;
            //parent.Body.FadeOutAnimation(this.AnimationWalk);
            this.AnimationWalk.FadeOut();


            //this.AnimationWalk.Enabled = false;
            //parent.Body.FadeOutAnimationAndRemove(this.AnimationWalk);

            this.Moving = false;

            //this.AnimationCrouch.Weight = 0;//.FadeOut();
        }

        public void Jump(GameObject parent)
        {
            if (parent.Net is Net.Server)
            {
                Vector3 force = Vector3.Zero;
                var feetposition = parent.Global + Vector3.UnitZ * 0.1f;
                var cell = parent.Net.Map.GetCell(feetposition);
                var block = cell.Block;// parent.Net.Map.GetBlock(parent.Global + Vector3.UnitZ * 0.1f); // to check if entity is in water
                //if (!this.CurrentState.AllowJump)// != State.Walking)
                //    return;
                var isStanding = PhysicsComponent.IsStanding(parent);
                if (!isStanding)
                    return;
                if (block == BlockDefOf.Water)
                {
                    if (parent.Velocity.Z <= 0)// only allow jumping in water when sinking
                    {
                        force = Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
                        var density = BlockDefOf.Water.GetDensity(cell.BlockData, feetposition);
                        force *= (1 + 3 * density);
                    }
                }
                else
                    if (parent.Velocity.Z == 0 && isStanding)// parent.Net.Map.IsSolid(parent.Global - Vector3.UnitZ * 0.1f)) // TODO: FIX: doesnt jump if on block edge
                    {
                        force = Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f));
                    }

                if (force == Vector3.Zero)
                    return;
                parent.Velocity += force;//Vector3.UnitZ * PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.JumpHeight, 0f)); // * stamina.Percentage;
            }
            parent.Net.PostLocalEvent(parent, Message.Types.Jumped);
        }
       
        public void ToggleWalk(bool toggle)
        {
            if (this.CurrentState.Type != State.Types.Blocking)
                this.CurrentState = toggle ? State.States[State.Types.Walking] : State.States[State.Types.Running];
            this.CurrentState.Apply(this);
            
        }
        public void ToggleSprint(bool toggle)
        {
            if (this.CurrentState.Type != State.Types.Blocking)
                this.CurrentState = toggle ? State.States[State.Types.Sprinting] : State.States[State.Types.Running];
            this.CurrentState.Apply(this);
        }
        public void ToggleBlock(bool toggle)
        {
            this.CurrentState = toggle ? State.States[State.Types.Blocking] : State.States[State.Types.Running];
            this.CurrentState.Apply(this);

        }
        public MobileComponent ToggleCrouch(bool enabled)
        {
            //this.ToggleWalk(enabled);
            this.AnimationCrouch.Weight = enabled ? 1 : 0;//.FadeIn(false, 10, Interpolation.Lerp);
            return this;
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
                    var blockheight = Block.GetBlockHeight(parent.Map, blockglobal);
                    return parent.Global.Z - (n + blockheight);
                }
                n--;
            }
            return parent.Global.Z;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.HitGround:
                    //this.AnimationWalk.Foreach(a => a.Frame = 0);
                    this.AnimationWalk.Frame = 0;
                    break;

                default:
                    break;
            }
            return false;
        }

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            bool midair = parent.Velocity.Z != 0;

            this.AnimationJump.Weight = midair ? 1 : 0;//this.DistanceFromGround(parent) : 0;
            if (!this.Moving)
                return;

            //don't change direction midair, or change it by a smaller factor?
            //if (parent.Velocity.Z != 0)
            //    return; 

            Vector2 direction = parent.Transform.Direction;//.GetComponent<PositionComponent>().Direction;
            this.Acceleration = Math.Min(1, this.Acceleration + AccelerationStep);

            /// OLD WALKSPEED CODE
            //var walkValue = StatsComponentNew.GetStatValueOrDefault(parent, Stat.Types.WalkSpeed, 1);
            //var oldwalk = StatsComponent.GetStatOrDefault(parent, Stat.Types.WalkSpeed, 0f);
            //float walkSpeed = 
            //    (1 + oldwalk) * 
            //    (this.CurrentState.Speed + this.CurrentState.SprintSpeed * stamina.Percentage) * 
            //    Acceleration * NormalWalkSpeed * walkValue;
            /// OLD WALKSPEED CODE

            var stamina = parent.GetResource(ResourceDef.Stamina);

            var newwalk = StatDefOf.WalkSpeed.GetValue(parent);
            var walkSpeed = newwalk * Acceleration * NormalWalkSpeed * (this.CurrentState.Speed + this.CurrentState.SprintSpeed * stamina.Percentage);

            if (this.CurrentState.Type == State.Types.Sprinting)
                stamina.Adjust(-0.01f);


            //apply stamina
            // TODO: make stamina resource change walkspeed instead of fetching stamina from here

            if (walkSpeed == 0)
                Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");

            // if in mid-air, move at half speed
            if (midair)
            {
                //walkSpeed *= 0.5f; // commenting this out because moving while mid-air should affect acceleration, not actual speed
                this.AnimationJump.Weight = 1;
            }

            float walkX = direction.X * walkSpeed;
            float walkY = direction.Y * walkSpeed;
            //walkX = Math.Abs(parent.Velocity.X) > Math.Abs(walkX) ? parent.Velocity.X : walkX;
            //walkY = Math.Abs(parent.Velocity.Y) > Math.Abs(walkY) ? parent.Velocity.Y : walkY;

            PreventFall(parent, ref walkX, ref walkY);
            var nextvelocity = new Vector3(walkX, walkY, parent.Velocity.Z);
            parent.Velocity = nextvelocity;
        }


        /// <summary>
        /// Prevents falling off edges when walking.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="walkX"></param>
        /// <param name="walkY"></param>
        private void PreventFall(GameObject parent, ref float walkX, ref float walkY)
        {
            if (this.CurrentState.Type != State.Types.Walking)
                return;
            // WE DONT WANT TO STOP MOVING WHEN JUMPING
            if (parent.Velocity.Z != 0) 
                return;
            if (parent.GetPath() != null)
                return;
            Vector3 global = parent.Global;
            if (!parent.Map.IsSolid(new Vector3(global.X + walkX, global.Y, global.Z + PhysicsComponent.Gravity)))
                walkX = 0;
            if (!parent.Map.IsSolid(new Vector3(global.X, global.Y + walkY, global.Z + PhysicsComponent.Gravity)))
                walkY = 0;
            if (!parent.Map.IsSolid(new Vector3(global.X + walkX, global.Y + walkY, global.Z + PhysicsComponent.Gravity)))
                walkY = walkX = 0;
        }
        static public void EmitDust(GameObject parent)
        {
            if (parent.Net is Net.Client)
            {
                if (parent.Velocity.Z != 0)
                    return;
                parent.Map.EventOccured(Message.Types.EntityFootStep, parent);
            }
        }

        private static ParticleEmitterSphere CreateDust(GameObject parent)
        {
            var emitter = new ParticleEmitterSphere() //DustEmitter.Clone() as ParticleEmitterSphere;
            {
                Lifetime = Engine.TicksPerSecond / 2f,
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
                Lifetime = Engine.TicksPerSecond / 2f,
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
            Lifetime = Engine.TicksPerSecond,
            Offset = Vector3.Zero,
            Rate = 0,
            ParticleWeight = 1f,
            ColorEnd = Color.SaddleBrown,
            ColorBegin = Color.SaddleBrown,
            SizeEnd = 1,
            SizeBegin = 1,
            Force = .05f
        };

        public override void Write(System.IO.BinaryWriter w)
        {
            base.Write(w);
            w.Write(this.Moving);
            w.Write(this.Acceleration);
            w.Write((int)this.CurrentState.Type);
            this.AnimationWalk.Write(w);
            this.AnimationJump.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            base.Read(r);
            this.Moving = r.ReadBoolean();
            this.Acceleration = r.ReadSingle();
            this.CurrentState = State.States[(State.Types)r.ReadInt32()];
            this.AnimationWalk.Read(r);
            this.AnimationJump.Read(r);
        }
        
        //internal override SaveTag SaveAs(string name = "")
        //{
        //    base.SaveAs(name);
        //    var tag = new SaveTag(SaveTag.Types.Compound, name);
        //    tag.Add(this.Moving.Save("Moving"));
        //    tag.Add(this.Acceleration.Save("Acceleration"));
        //    tag.Add(((int)this.CurrentState.Type).Save("State"));
        //    return tag;
        //}
        internal override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.Moving.Save("Moving"));
            tag.Add(this.Acceleration.Save("Acceleration"));
            tag.Add(((int)this.CurrentState.Type).Save("State"));

            tag.Add(this.AnimationWalk.Save("Walking"));
            tag.Add(this.AnimationJump.Save("Jumping"));

        }
        internal override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<bool>("Moving", out this.Moving);
            tag.TryGetTagValue<float>("Acceleration", out this.Acceleration);
            tag.TryGetTagValue<int>("State", v =>
            {
                this.CurrentState = State.States[(State.Types)v];
                this.CurrentState.Apply(this);
            });

            tag.TryGetTag("Walking", this.AnimationWalk.Load);
            tag.TryGetTag("Jumping", this.AnimationJump.Load);

        }
    }
}
