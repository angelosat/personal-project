using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Scripts
{
    class ScriptAttack : Script
    {
        Bone Hand { get; set; }
        Bone Body { get; set; }
        public static float DefaultRange = Interaction.DefaultRange;
        public static double DefaultArc = Math.PI / 6d;

        public StatCollection Damage;
        public Vector3 Direction, Momentum;
        public GameObject Source;
        public bool Critical;
        public float Charge;
        public Attack.States State;
        public Func<GameObject, GameObject, Vector3, bool> CollisionType;
        public AnimationCollection AttackAnimation;
        public int Value
        {
            get { return (int)Math.Round(Charge * Damage.Values.Aggregate((a, b) => a + b)); }
        }

        public ScriptAttack()
        {
            this.ID = Types.Attack;
            this.Flow = ScriptFlow.Channeled;
            this.Execute = (net, actor, target, args) => { Success(new AbilityArgs(net, actor, target, args)); };
            this.Name = "Attack";
            this.BaseTime = 0;
            this.RangeCheck = (actor, target, r) => true;
        }

        public override void Update(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //this.Hand.Step();
        }

        public override void Start(AbilityArgs args)
        {
            if (args.Actor.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Attack))
            {
                this.ScriptState = ScriptState.Finished;
                return;
            }
            this.State = Attack.States.Charging;

            //this.Hand = args.Actor.GetComponent<ActorSpriteComponent>().Body["Right Hand"];
            //this.Hand.Start(AnimationCollection.RaiseRHand["Right Hand"]);

            this.Body = args.Actor.GetComponent<ActorSpriteComponent>().Body;
            this.AttackAnimation = AnimationCollection.RaiseRHand;
            this.Body.Start(AttackAnimation);
        }

        public override void Finish(AbilityArgs args)
        {
            if (this.ScriptState == ScriptState.Finished)
                return;
            if (this.State == Attack.States.Delivering)
                return;
            this.State = Attack.States.Delivering;

            Vector3 dir;
            DirectionEventArgs.Translate(args.Args, out dir);
          //  AnimationCollection raiseRHand = AnimationCollection.RaiseRHand;
         //   Animation anim = raiseRHand["Right Hand"];
            Animation hand = this.AttackAnimation["Right Hand"];
            float charge = hand.Percentage;
            float lunge = 0.2f;
            this.Body.Stop(this.AttackAnimation);
            Attack attack = Attack.Create(args.Actor, dir, Attack.Ray, () => charge);
            args.Actor.Velocity += dir * lunge * charge;
            Animation.Start(args.Actor, AnimationCollection.DeliverAttack, 1f, onFinish: new Dictionary<string, Action>() { { "Right Hand", () =>
                    {
                        Perform(args.Net, args.Actor, dir, attack);
                    //    this.State = Components.Attack.States.Ready;
                        this.ScriptState = ScriptState.Finished;
                        this.Body.FadeOut(AnimationCollection.DeliverAttack);
                    } } });
        }

        void Perform(IObjectProvider net, GameObject actor, Vector3 direction, Attack attack)
        {
            float range = Components.Attack.DefaultRange;
            double halfArc = Components.Attack.DefaultArc;
            foreach (var obj in actor.GetNearbyObjects(range: (r) => r <= range))
                if (attack.CollisionType(actor, obj, direction))
                    //obj.PostMessage(Message.Types.Attacked, actor, attack);    
                    net.PostLocalEvent(obj, ObjectEventArgs.Create(Message.Types.Attacked, new object[] { actor, attack }));
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
            float coef = this.Charge * (1 + StatsComponent.GetStatOrDefault(this.Source, Stat.Types.Knockback, 0));
            return this.Momentum * coef;
        }

        public override object Clone()
        {
            return new ScriptAttack();
        }
    }
}
