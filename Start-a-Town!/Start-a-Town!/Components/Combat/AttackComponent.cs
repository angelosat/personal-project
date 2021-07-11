using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    class AttackComponent : EntityComponent
    {
        public override string ComponentName => "Attack";

        public static float DefaultRange = Interaction.DefaultRange;
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
        public Animation AttackAnimation;

        public GameObject Target;
        const float NearestEnemyIntervalSeconds = 0.5f;// in seconds TODO: store nearest entities with gameobject so individual components can access them without calculating them by themselves each time
        int NearestEnemyUpdateTimer = 0;
        public int NearestEnemyUpdateTimerMax
        {
            get
            {
                return (int)(NearestEnemyIntervalSeconds * Engine.TicksPerSecond);
            }
        }

        public int DamageValue
        {
            get { return (int)Math.Round(ChargeFunc() * Damage.Values.Aggregate((a, b) => a + b)); }
        }

        public override void Tick()
        {
            var parent = this.Parent;

            if (this.State == Attack.States.Charging)
                this.Charge = Math.Min(this.Charge + 1, this.ChargeMax);
            this.NearestEnemyUpdateTimer--;
            if (this.NearestEnemyUpdateTimer <= 0)
            {
                this.NearestEnemyUpdateTimer = this.NearestEnemyUpdateTimerMax;
                var nextTarget = FindClosestEnemy(parent);
                if(this.Target != nextTarget)
                    parent.Net.EventOccured(Message.Types.AttackTargetChanged, parent, this.Target);

                this.Target = nextTarget;
            }
        }

        public void Start(GameObject parent)
        {
            if (this.State != Attack.States.Ready)
                return;
            this.State = Attack.States.Charging;
            this.AttackAnimation = Animation.RaiseRHand(parent);
            throw new NotImplementedException();
            this.Charge = 0;
            this.ChargeMax = 80;
            parent.CrossFade(AttackAnimation, true, this.ChargeMax,
                (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)));
            // TODO: maybe pass a startattack event to the parent object and let components handle it?
            PersonalInventoryComponent.DropHauled(parent);
        }
        public void Cancel(GameObject parent)
        {
            if (this.State != Attack.States.Charging)
                return;
            this.AttackAnimation.FadeOut();

            this.State = Attack.States.Ready;
        }
        public void Finish(GameObject parent)
        {
            var dir = (this.Target == null) ? parent.Direction : (Vector3.Normalize(this.Target.Global - parent.Global) * new Vector3(1, 1, 0));
            this.Finish(parent, dir);
            return;

        }
        public void Finish(GameObject parent, Vector3 dir)
        {
            if (this.State != Attack.States.Charging)
                return;
            this.State = Attack.States.Delivering;
            throw new NotImplementedException();

            Attack attack = Attack.Create(parent, dir, Attack.Ray, this.ChargeFunc);
            float lunge = parent.Velocity.Z == 0 ? this.Lunge(this.ChargeFunc()) : 0;

            dir = (this.Target == null) ? dir : (Vector3.Normalize(this.Target.Global - parent.Global) * new Vector3(1, 1, 0));
            parent.Direction = dir;
            // temporarily disable lunging
            // TODO: find better way to do lunging to prevent lunging through the target and missing
            //parent.Velocity += dir * lunge;


            //var coll = AnimationCollection.DeliverAttack;
            //if (attack.Value < 0)
            //    attack.Value.ToConsole();
            //parent.Body.AddAnimation(coll);
            //Animation.Start(parent, coll, 1f, onFinish: new Dictionary<BoneDef, Action>() { { BoneDef.Torso, () =>
            //{
            //    FinishDelivering(parent, attack);
            //} } });
            //coll.Speed = .1f;

            this.AniAttack = AnimationDeliverAttack.Create(parent, () => Perform(parent, parent.Direction, attack), () => FinishDelivering(parent, attack));//() => FinishDelivering(parent, attack)); //
            
            if (attack.Value < 0)
                attack.Value.ToConsole();
            parent.AddAnimation(this.AniAttack);

            // telegraph attack to potential targets
            // TODO: all target in range and in direction of attack?
            if (this.Target != null)
                this.Target.PostMessage(new ObjectEventArgs(Message.Types.AttackTelegraph, parent, parent));
        }
        Animation AniAttack;

        private void FinishDelivering(GameObject parent, Attack attack)
        {
            this.AttackAnimation.FadeOut();
            this.AttackAnimation.Stop();
            this.State = Attack.States.Ready;
        }
        float Lunge(float charge)
        {
            return charge < 0.5f ? 0 : LungeMax * ((charge - 0.5f) / 0.5f);
        }

        void Perform(GameObject actor, Vector3 direction, Attack attack)
        {
            float range = Attack.DefaultRange;
            double halfArc = Attack.DefaultArc;

            var attackOrigin = actor.Global + actor.Physics.Height * .5f * Vector3.UnitZ;
            var attackEndPoint = attackOrigin + actor.Direction * range;
            var nearbies = actor.GetNearbyObjects(range: (r) => r <= range);
            foreach (var obj in nearbies)
                if (attack.CollisionType(attackOrigin, direction, obj))//actor, obj, direction))
                //if (LineOfSight(actor.Map, actor.Global, obj.Global))
                // check line of sight between actor attack origin (half his height), and the END POINT OF HIS ATTACK!!! NOT WORLD POSITION OF COLLIDING ENTITY!!!!
                    // BUT WHY NOT?
                {
                    attackEndPoint = obj.Global + obj.Physics.Height * .5f * Vector3.UnitZ;
                    // TODO: modify the checks so it stops checking for line of sight beyond the target entity if the entity is closer than the endpoint
                    if (LineOfSight(actor.Map, attackOrigin, attackEndPoint))
                        actor.Net.PostLocalEvent(obj, ObjectEventArgs.Create(Message.Types.Attacked, new object[] { actor, attack }));
                }
        }
        static public bool LineOfSight(MapBase map, Vector3 start, Vector3 end)
        {
            Vector3 difference = end - start;
            float maxlength = difference.LengthSquared();
            var direction = difference;
            direction.Normalize();
            var precision = .5f;
            var step = difference * precision;

            var stepLength = step.LengthSquared();
            var currentCheck = start + step;
            var currentlength = stepLength;
            while(currentlength < maxlength)
            {
                if (map.IsSolid(currentCheck))
                    return false;
                currentCheck += step;
                currentlength += stepLength;
            }
            return true;
        }
        [Obsolete]
        static public bool LineOfSightOld(MapBase map, Vector3 start, Vector3 end)
        {
            Vector3 difference = end - start;
            float length = difference.Length();
            difference.Normalize();
            if (Math.Abs(difference.X) > Math.Abs(difference.Y))
            {
                if (difference.X > 0)
                    for (int i = 1; i < difference.X + 1; i++)
                    {
                        float t = i / difference.X;
                        Vector3 check = start + new Vector3(i, t * difference.Y, t * difference.Z);
                        if (map.IsSolid(check))
                            return false;
                    }
                else
                    for (int i = -1; i > difference.X - 1; i--)
                    {
                        float t = i / Math.Abs(difference.X);
                        Vector3 check = start + new Vector3(i, t * difference.Y, t * difference.Z);
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
                        if (map.IsSolid(check))
                            return false;
                    }
                else
                    for (int i = -1; i > difference.Y - 1; i--)
                    {
                        float t = i / Math.Abs(difference.Y);
                        Vector3 check = start + new Vector3(t * difference.X, i, t * difference.Z);
                        if (map.IsSolid(check))
                            return false;
                    }
            }
            return true;
        }
        static GameObject FindClosestEnemy(GameObject parent)
        {
            var list = parent.GetNearbyObjects(r => r <= 5, foo => foo != parent);
            list = list.Where(foo => foo.HasResource(ResourceDef.Health)).ToList();
            list.Sort((foo1, foo2) => Vector3.DistanceSquared(parent.Global, foo1.Global) <= Vector3.DistanceSquared(parent.Global, foo2.Global) ? -1 :1);
            return list.FirstOrDefault();
        }

        public override object Clone()
        {
            return new AttackComponent();
        }
    }
}
