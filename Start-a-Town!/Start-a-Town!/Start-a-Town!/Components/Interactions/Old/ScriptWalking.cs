using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptWalking : Script
    {
        public enum States { Stopped, Walking, Running }

        Bone Body { get; set; }

        States State = States.Stopped;
        float Acceleration = 0f;
        public float Speed = 1f;

        AnimationCollection Walking;

        public ScriptWalking()
        {
            this.ID = Types.Walk;
            this.Name = "Walking";
        }

        public override void OnStart(ScriptArgs args)
        {
            this.ScriptState = ScriptState.Running;
            this.Body = args.Actor.GetComponent<SpriteComponent>().Body;
            this.Walking = AnimationCollection.Walking;
            this.Body.Start(Walking);
            args.Actor.TryGetComponent<ControlComponent>(c => c.Interrupt(args));
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //if (parent.Velocity.Z != 0)
            //    return; //don't walk midair
            Vector2 direction = parent.GetComponent<PositionComponent>().Direction;
            this.State = this.Speed > 0.5f ? States.Running : States.Walking;
            Acceleration = Math.Min(1, Acceleration + 0.1f);

            float walkSpeed = (1 + StatsComponent.GetStatOrDefault(parent, Stat.Types.WalkSpeed, 0f)) * Speed * Acceleration * PhysicsComponent.Walk;
            if (walkSpeed == 0)
                Log.Enqueue(Log.EntryTypes.System, "Warning! " + parent.Name + " is trying to move but their movement speed is zero!");
            //parent.Velocity = new Vector3(direction.X * walkSpeed, direction.Y * walkSpeed, parent.Velocity.Z);
            float walkX = direction.X * walkSpeed;
            float walkY = direction.Y * walkSpeed;
            walkX = Math.Abs(parent.Velocity.X) > Math.Abs(walkX) ? parent.Velocity.X : walkX;
            walkY = Math.Abs(parent.Velocity.Y) > Math.Abs(walkY) ? parent.Velocity.Y : walkY;
            parent.Velocity = new Vector3(walkX, walkY, parent.Velocity.Z);
            foreach (var item in this.Walking.Inner.Values)
                item.Weight = this.Speed;
            //this.Body.Step();
        }

        public override void Interrupt(ScriptArgs args)
        {
            //base.Interrupt(args);
        }

        //protected override void OnSuccess(AbilityArgs args)
        public override void Finish(ScriptArgs args)
        {
            this.State = States.Stopped;
            this.ScriptState = ScriptState.Finished;
            this.Body.FadeOut(this.Walking);
        }

        public override object Clone()
        {
            return new ScriptWalking();
        }
    }
}
