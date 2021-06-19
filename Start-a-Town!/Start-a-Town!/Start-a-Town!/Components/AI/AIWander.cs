using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIWander : Behavior
    {
        //class Properties : Behavior.BhavProperties
        //{
        //    Vector3 Leash;
        //}
        //Vector3 Leash;
        Vector3 Direction { get { return (Vector3)this["Direction"]; } set { this["Direction"] = value; } }
        TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
        float Speed { get { return (float)this["SpeedNorm"]; } set { this["SpeedNorm"] = value; } }

        //public override Behavior Initialize(AIState state)
        //{
        //    state.BehavProps.Add(Hash, new Properties());
        //    return base.Initialize(state);
        //}
        internal override void OnSpawn(GameObject parent, AIState state)
        {
            state.Leash = parent.Global;
        }
        const float MaxRange = 2;
        int Seed;
        Random Random;
        //bool InSync;
        static readonly int Hash = "wander".GetHashCode();

        public override void Sync(int seed)
        {
            this.Seed = seed + Hash;
            this.Random = new Random(this.Seed);
            //this.InSync = true;
            base.Sync(this.Seed);
        }

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
            this.Speed = 0.4f; //0.5f;
            this.Time = new TimeSpan(0, 0, 2);
        }

        public Behavior Refresh(GameObject parent, AIState state)
        {
            //double radians = Game1.Instance.Random.NextDouble() * 2 * Math.PI;
            ChooseDirection(parent, state);
            this.Speed = 0.3f; //0.5f;
            //this.Time = new TimeSpan(0, 0, 1);
            this.Time = TimeSpan.FromSeconds(1);//state.Personality.Composure.Normalized);

            //if (parent.Net is Net.Client)
            //    this.InSync = false;
            return this;
        }

        private void ChooseDirection(GameObject parent, AIState state)
        {
            double radians = this.Random.NextDouble() * 2 * Math.PI;
            var choice = new Vector3((float)Math.Cos(radians), (float)Math.Sin(radians), 0);
            var dist = Math.Min(Vector3.Distance(parent.Global, state.Leash) / (float)MaxRange, 1);
            var towardsLeash = (state.Leash - parent.Global);
            if (towardsLeash != Vector3.Zero)
                towardsLeash.Normalize();
            //var dir = (1 - dist) * choice + dist * towardsLeash;
            var dir = choice + dist * (towardsLeash - choice);
            dir.Normalize();
            this.Direction = dir;
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            //if (server == null)
            //    return;
            var net = parent.Net;
            var server = (net as Net.Server);

            if (this.Time.TotalMilliseconds <= 0)
            {
                Refresh(parent, state);
                if (net is Net.Client)
                    state.InSync = false;

                // don't send direction packet
                //server.AIHandler.AIChangeDirection(parent, this.Direction);
                parent.Direction = this.Direction;
                server.AIHandler.AIStartMove(parent);
                server.AIHandler.AIToggleWalk(parent, true);
            }

            TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(100 / 6f));
            Time = Time.Subtract(sub);
            if (Time.TotalMilliseconds > 0)
                return BehaviorState.Running;
            //parent.GetComponent<ControlComponent>().FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
            //parent.GetComponent<MobileComponent>().Stop(parent);
            server.AIHandler.AIStopMove(parent);

            return BehaviorState.Success;
        }
        //public override BehaviorState Execute(Net.IObjectProvider net, GameObject parent, AIState state)
        //{
        //    parent.Direction = this.Direction;
        //        parent.GetComponent<ControlComponent>().TryStartScript(Script.Types.Walk, new ScriptArgs(net, parent));
        //    TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(100 / 6f));
        //    Time = Time.Subtract(sub);
        //    if (Time.TotalMilliseconds > 0)
        //        return BehaviorState.Running;
        //    parent.GetComponent<ControlComponent>().FinishScript(Script.Types.Walk, new ScriptArgs(net, parent));
        //    Initialize(parent);
        //    if (net is Net.Server)
        //        (net as Net.Server).SyncAI(parent);
        //    return BehaviorState.Success;
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Detect:
                    Personality personality = parent["AI"]["Personality"] as Personality;
                    if (personality.Reaction != ReactionType.Hostile)
                        return true;
                    var map = parent.Map;
                    //Chunk chunk = Position.GetChunk(map, parent.Global);
                    Chunk chunk = map.GetChunk(parent.Global);

                    List<GameObject> objects = (e.Parameters[0] as List<GameObject>).FindAll(foo => foo.Type == ObjectType.Human);
                    //foreach (Chunk ch in Position.GetChunks(map, chunk.MapCoords))
                    foreach (Chunk ch in map.GetChunks(chunk.MapCoords))
                        objects.AddRange(ch.GetObjects().FindAll(foo => foo.Type == ObjectType.Human));

                    GameObject closest = null;
                    float closestDist = 5;
                    foreach (GameObject obj in objects)
                    {
                        if (obj == parent)
                            continue;
                        float dist = Vector3.Distance(obj.Global, parent.Global);
                        if (dist < closestDist)
                        {
                            closest = obj;
                            closestDist = dist;
                        }
                    }
                    if (closest == null)
                        return true;
                    throw new NotImplementedException();
                    parent["AI"]["Current"] = new AIAttack(closest);
                    return true;
                default:
                    return false;
            }
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Seed);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Seed = r.ReadInt32();
            this.Random = new Random(this.Seed);
        }
    }
}
