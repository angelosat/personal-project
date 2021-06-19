using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptAttack : Script
    {
        Bone Hand { get; set; }
        Bone Body { get; set; }
        public static float DefaultRange = InteractionOld.DefaultRange;
        public static double DefaultArc = Math.PI / 6d;
        public static float LungeMax = 0.2f;

        public StatCollection Damage;
        public Vector3 Direction, Momentum;
        public GameObject Source;
        public bool Critical;
        public int Charge { get; set; }
        public int ChargeMax { get; set; }
        public Func<float> ChargeFunc { get; set; }
        public Attack.States State;
        public Func<GameObject, GameObject, Vector3, bool> CollisionType;
        public AnimationCollection AttackAnimation;
        public int DamageValue
        {
            //get { return (int)Math.Round(Charge * Damage.Values.Aggregate((a, b) => a + b)); }
            get { return (int)Math.Round(ChargeFunc() * Damage.Values.Aggregate((a, b) => a + b)); }
        }

        public ScriptAttack()
        {
            this.ID = Types.Attack;
            this.Flow = ScriptFlow.Channeled;
            this.Execute = (net, actor, target, args) => { Success(new ScriptArgs(net, actor, target, args)); };
            this.Name = "Attack";
            this.BaseTimeInSeconds = 0;
            this.RangeCheck = (actor, target, r) => true;
        }

        public override void Update(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //this.Hand.Step();
            this.Charge = Math.Min(this.Charge + 1, this.ChargeMax);
        }

        public override void OnStart(ScriptArgs args)
        {
            if (args.Actor.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Attack))
            {
                this.ScriptState = ScriptState.Finished;
                return;
            }
            //this.Charge = 0;
            
            
            this.State = Attack.States.Charging;
            this.Body = args.Actor.GetComponent<SpriteComponent>().Body;
            this.AttackAnimation = AnimationCollection.RaiseRHand;
            this.ChargeFunc = //() => this.Charge / (float)this.ChargeMax;// this.AttackAnimation["Right Hand"].Percentage;
                () => this.AttackAnimation[Bone.Types.RightHand].Fade;
            this.Charge = 0;
            this.ChargeMax = 80;// this.AttackAnimation["Right Hand"].FrameCount;
            //this.Body.Start(AttackAnimation);
            this.Body.CrossFade(AttackAnimation, true, this.ChargeMax,
                // (a,b,c)=>Interpolation.Exp(b,a,1-c));// Interpolation.Exp);
                (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)));
        }
        public override void Interrupt(ScriptArgs args)
        {
            //base.Interrupt(args);
        }
        public override void Finish(ScriptArgs args)
        {
            if (this.ScriptState == ScriptState.Finished)
                return;
            if (this.State == Attack.States.Delivering)
                return;
            this.State = Attack.States.Delivering;

            Vector3 dir;
            DirectionEventArgs.Translate(args.Args, out dir);
            Animation hand = this.AttackAnimation[Bone.Types.RightHand];
            
            //this.Body.Stop(this.AttackAnimation);
            //this.Body.FadeOut(this.AttackAnimation);
            Attack attack = Attack.Create(args.Actor, dir, Attack.Ray, this.ChargeFunc);// () => charge);
            float lunge = Lunge(this.ChargeFunc());
            args.Actor.Direction = dir;
            args.Actor.Velocity += dir * lunge;// *this.ChargeFunc();//* 0;//
            //Animation.Start(args.Actor, AnimationCollection.DeliverAttack, 1f, onFinish: new Dictionary<string, Action>() { { "Right Hand", () =>
            //{
            //    Perform(args.Net, args.Actor, dir, attack);
            //    this.ScriptState = ScriptState.Finished;
            //    this.Body.FadeOut(AnimationCollection.DeliverAttack);
            //} } });

            var coll = AnimationCollection.DeliverAttack;
            //foreach(var a in coll)
            //    a.Value.FadeIn(false, a.Value.FrameCount, Interpolation.Lerp);
            Animation.Start(args.Actor, coll, 1f, onFinish: new Dictionary<Bone.Types, Action>() { { Bone.Types.Torso, () =>
            {
                //Perform(args.Net, args.Actor, dir, attack);
                Perform(args.Actor, args.Actor.Direction, attack);
                //Stop(args);
                this.ScriptState = ScriptState.Finished;
                this.Body.FadeOut(AnimationCollection.DeliverAttack);
                this.Body.Stop(this.AttackAnimation);
            } } });
        }

        public override void Stop(ScriptArgs args)
        {
            this.ScriptState = ScriptState.Finished;
            this.Body.FadeOut(AnimationCollection.DeliverAttack);
            this.Body.FadeOut(AnimationCollection.RaiseRHand);
            this.Body.Stop(this.AttackAnimation);
        }

        void Perform(GameObject actor, Vector3 direction, Attack attack)
        {
            throw new NotImplementedException();
            //float range = Components.Attack.DefaultRange;
            //double halfArc = Components.Attack.DefaultArc;
            //foreach (var obj in actor.GetNearbyObjects(range: (r) => r <= range))
            //    if (attack.CollisionType(actor, obj, direction))
            //        if (LineOfSight(actor.Map, actor.Global, obj.Global))
            //            //obj.PostMessage(Message.Types.Attacked, actor, attack);    
            //            actor.Net.PostLocalEvent(obj, ObjectEventArgs.Create(Message.Types.Attacked, new object[] { actor, attack }));
        }

        static public bool LineOfSight(IMap map, Vector3 start, Vector3 end)
        {
            Vector3 difference = end - start;
            float length = difference.Length();
            difference.Normalize();
            //Func<float, float> f = x=>
            //if (difference.X > difference.Y)
            if (Math.Abs(difference.X) > Math.Abs(difference.Y))
            {
                if (difference.X > 0)
                    for (int i = 1; i < difference.X + 1; i++)
                    {
                        float t = i / difference.X;
                        Vector3 check = start + new Vector3(i, t * difference.Y, t * difference.Z);
                        //if (check.IsSolid(map))
                        if (map.IsSolid(check))
                            return false;
                    }
                else
                    for (int i = -1; i > difference.X - 1; i--)
                    {
                        float t = i / Math.Abs(difference.X);
                        Vector3 check = start + new Vector3(i, t * difference.Y, t * difference.Z);
                        //if (check.IsSolid(map))
                        if (map.IsSolid(check))
                            return false;
                    }
            }
            else
            {
                if (difference.Y > 0)
                    for (int i = 1; i < difference.Y + 1; i++)
                    {
                        float t = i / difference.Y;
                        Vector3 check = start + new Vector3(t * difference.X, i, t * difference.Z);
                        //if (check.IsSolid(map))
                        if (map.IsSolid(check))
                            return false;
                    }
                else
                    for (int i = -1; i > difference.Y - 1; i--)
                    {
                        float t = i / Math.Abs(difference.Y);
                        Vector3 check = start + new Vector3(t * difference.X, i, t * difference.Z);
                        //if (check.IsSolid(map))
                        if (map.IsSolid(check))
                            return false;
                    }
            }
            return true;
        }

        static public Func<GameObject, GameObject, Vector3, bool> Ray
        {
            get
            {
                return (actor, target, direction) =>
                {
                    Ray ray = new Ray(actor.Global, direction);
                    return (BoundingCylinder.Create(target).Intersects(ray));
                };
            }
        }
        static public Func<GameObject, GameObject, Vector3, bool> Arc
        {
            get
            {
                return (actor, target, direction) =>
                {
                    Vector3 distance = target.Global - actor.Global;
                    distance.Normalize();
                    float angle = (float)Math.Acos(Vector3.Dot(direction, distance));
                    return (Math.Abs(angle) < Components.Attack.DefaultArc);
                };
            }
        }

        internal Vector3 GetMomentum()
        {
          //  float coef = this.Charge * (1 + StatsComponent.GetStatOrDefault(this.Source, Stat.Types.Knockback, 0));
            float coef = this.ChargeFunc() * (1 + StatsComponent.GetStatOrDefault(this.Source, Stat.Types.Knockback, 0));
            return this.Momentum * coef;
        }

        float Lunge(float charge)
        {
            return charge < 0.5f ? 0 : LungeMax * ((charge - 0.5f) / 0.5f);
        }

        public override void AIControl(ScriptArgs args)
        {
            if (args.Target.Object.IsNull())
                return;
            float ch = this.ChargeFunc();
            float speedLength = args.Actor.Velocity.Length();
           // float ch = 1 - this.Charge / this.ChargeMax;
            //if (ch >= 1)//0.5f)// &&
                //Vector3.Distance(args.Actor.Global, args.Target.Object.Global) <= 1)
           // if(Vector3.Distance(args.Actor.Global, args.Target.Object.Global) <= 1)
            float s = 10 * (LungeMax + speedLength) - (1 / 2f) * PhysicsComponent.Friction * PhysicsComponent.Friction;
            float dist = Vector3.Distance(args.Actor.Global, args.Target.Object.Global);

            if ((dist <= s + Components.Attack.DefaultRange) && ch >= 1 || 
                (dist <= 1 && ch >= 0.5f))
            {
                Finish(new ScriptArgs(args.Net, args.Actor, args.Target, w => w.Write(args.Actor.Direction)));    //w.Write(new Vector3(args.Actor.GetPosition().Direction, 0))));
                return;
            }

        }

        public override object Clone()
        {
            return new ScriptAttack();
        }
    }
}
