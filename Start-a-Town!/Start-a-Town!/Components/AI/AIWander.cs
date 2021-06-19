using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class AIWander : Behavior
    {
        //class Properties : Behavior.BhavProperties
        //{
        //    Vector3 Leash;
        //}
        //Vector3 Leash;

        //Vector3 Direction { get { return (Vector3)this["Direction"]; } set { this["Direction"] = value; } }
        //TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
        //float Speed { get { return (float)this["SpeedNorm"]; } set { this["SpeedNorm"] = value; } }
        Vector3 Direction; 
        TimeSpan Time;     
        //float Speed;
        int Timer;
        //public override Behavior Initialize(AIState state)
        //{
        //    state.BehavProps.Add(Hash, new Properties());
        //    return base.Initialize(state);
        //}
        //internal override void OnSpawn(GameObject parent, AIState state)
        //{
        //    state.Leash = parent.Global;
        //}
        const float MaxRange = 2;
        int Seed;
        //Random Random;
        //bool InSync;
        static readonly int Hash = "wander".GetHashCode();

        //public override void Sync(int seed)
        //{
        //    this.Seed = seed + Hash;
        //    this.Random = new Random(this.Seed);
        //    base.Sync(this.Seed);
        //}

        public override string Name
        {
            get
            {
                return "Wander";
            }
        }

        public override object Clone()
        {
            return new AIWander();
        }

        public AIWander()
        {
            //double radians = this.Random.NextDouble() * 2 * Math.PI;
            //this.Direction = new Vector3((float)Math.Cos(radians), (float)Math.Sin(radians), 0);
            this.Direction = Vector3.UnitX;
            //this.Speed = 0.4f; //0.5f;
            this.Time = new TimeSpan(0, 0, 2);
            //this.Random = new Random();
        }

        //public Behavior Refresh(GameObject parent, AIState state)
        //{
        //    //double radians = Game1.Instance.Random.NextDouble() * 2 * Math.PI;
        //    ChooseDirection(parent, state);
        //    this.Speed = 0.3f; //0.5f;
        //    //this.Time = new TimeSpan(0, 0, 1);
        //    this.Time = TimeSpan.FromSeconds(1);//state.Personality.Composure.Normalized);

        //    //if (parent.Net is Client)
        //    //    this.InSync = false;
        //    return this;
        //}

        private void ChooseDirection(GameObject parent, AIState state)
        {
            var rand = parent.Map.Random;// (parent.Net as Server).GetRandom();
            double radians = rand.NextDouble() * 2 * Math.PI;
            var choice = new Vector3((float)Math.Cos(radians), (float)Math.Sin(radians), 0);
            var dist = Math.Min(Vector3.Distance(parent.Global, state.Leash) / (float)MaxRange, 1);
            var towardsLeash = (state.Leash - parent.Global);
            towardsLeash.Z = 0;
            if (towardsLeash != Vector3.Zero)
                towardsLeash.Normalize();
            //var dir = (1 - dist) * choice + dist * towardsLeash;
            var dir = choice + dist * (towardsLeash - choice);
            dir.Normalize();
            this.Direction = dir;
        }

        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    //if (server == null)
        //    //    return;
        //    var net = parent.Net;
        //    var server = (net as Server);

        //    if (this.Time.TotalMilliseconds <= 0)
        //    {
        //        Refresh(parent, state);
        //        if (net is Client)
        //            state.InSync = false;

        //        // don't send direction packet
        //        //server.AIHandler.AIChangeDirection(parent, this.Direction);
        //        parent.Direction = this.Direction;
        //        server.AIHandler.AIStartMove(parent);
        //        server.AIHandler.AIToggleWalk(parent, true);

        //        server.OutgoingStream.Write((int)PacketType.AIJobStarted);
        //        server.OutgoingStream.Write(parent.InstanceID);
        //        server.OutgoingStream.Write("Wandering");
        //    }

        //    TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(100 / 6f));
        //    Time = Time.Subtract(sub);
        //    if (Time.TotalMilliseconds > 0)
        //        return BehaviorState.Running;
        //    //parent.GetComponent<ControlComponent>().FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
        //    //parent.GetComponent<MobileComponent>().Stop(parent);
        //    server.AIHandler.AIStopMove(parent);
        //    server.OutgoingStream.Write((int)PacketType.AIJobComplete);
        //    server.OutgoingStream.Write(parent.InstanceID);
        //    return BehaviorState.Success;
        //}
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //if (server == null)
            //    return;
            var net = parent.Net;
            var server = (net as Server);


            if (this.Timer == 0)
            {
                Refresh(parent, state);
                if (net is Client)
                    state.InSync = false;

                // don't send direction packet
                //server.AIHandler.AIChangeDirection(parent, this.Direction);
                parent.Direction = this.Direction;
                parent.MoveToggle(true);
                parent.WalkToggle(true);
                //server.AIHandler.AIStartMove(parent);
                //server.AIHandler.AIToggleWalk(parent, true);
            }

            Timer--;
            if (Timer > 0)
                return BehaviorState.Running;
            parent.MoveToggle(false);
            //server.AIHandler.AIStopMove(parent);
            return BehaviorState.Success;
        }
        public Behavior Refresh(GameObject parent, AIState state)
        {
            ChooseDirection(parent, state);
            //this.Speed = 0.3f;
            this.Timer = Engine.TicksPerSecond;
            return this;
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Seed);
            w.Write(this.Timer);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Seed = r.ReadInt32();
            //this.Random = new Random(this.Seed);
            this.Timer = r.ReadInt32();
        }
        protected override void AddSaveData(SaveTag tag)
        {
            base.AddSaveData(tag);
            tag.Add(this.Timer.Save("Timer"));
        }
        internal override void Load(SaveTag tag)
        {
            base.Load(tag);
            tag.TryGetTagValue<int>("Timer", out this.Timer);
        }
    }
}
