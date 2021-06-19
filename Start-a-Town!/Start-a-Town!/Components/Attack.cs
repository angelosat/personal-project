using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class Attack
    {
        public enum States { Ready, Charging, Charged, Delivering }
        public const float DefaultRange = Interaction.DefaultRange;
        public static double DefaultArc = Math.PI / 6d;

      //  public Dictionary<string, float> Values = new Dictionary<string, float>();
      //  public StatsComponent Values;
        public StatCollection Damage;
        public Vector3 Direction, Momentum;
        public GameObject Attacker;
        public bool Critical;
        public float Charge;
        public Func<Vector3, Vector3, GameObject, bool> CollisionType;

        public int Value
        {
            //get { return Values.Values.Aggregate((a, b) => a + b); }
            get
            {
                var charge = Math.Max(.1f, this.Charge); // maybe change range of charge to never be 0?
                return (int)Math.Ceiling(charge * Damage.Values.Aggregate((a, b) => a + b)); 
            }
            //get { return Charge * (float)Values.Properties.Values.Aggregate((a, b) => (float)a + (float)b); }
        }

        static public Attack Create(GameObject source, Vector3 direction, Func<Vector3, Vector3, GameObject, bool> collisionType, Func<float> chargeFunc)
        {
            return new Attack(source, direction, collisionType, chargeFunc());
        }

        Attack(GameObject attacker, Vector3 direction, Func<Vector3, Vector3, GameObject, bool> collisionType, float charge)
        {
            this.Attacker = attacker;
            this.Direction = direction;
            this.Momentum = attacker.Velocity;
            this.CollisionType = collisionType;

            if(attacker.TryGetComponent<InventoryComponent>(c=>{
            GameObjectSlot holdSlot = attacker["Inventory"]["Holding"] as GameObjectSlot;

            //this.Damage = WeaponComponent.GetDamage(holdSlot.Object ?? (attacker.GetComponent<BodyComponent>().BodyParts[Stat.Mainhand.Name]).Base.Object);
            this.Damage = WeaponComponent.GetDamage(holdSlot.Object) ?? WeaponComponent.GetDamage(attacker.GetComponent<BodyComponent>().BodyParts[Stat.Mainhand.Name].Base.Object);
            })) { }
            else
            {
                this.Damage = WeaponComponent.GetDamage(attacker.GetComponent<BodyComponent>().BodyParts[Stat.Mainhand.Name].Base.Object);
            }
            Charge = charge;
        }

        static public void Perform(GameObject actor, Vector3 direction, Attack attack)
        {
            //float range = Components.Attack.DefaultRange;
            //double halfArc = Components.Attack.DefaultArc;
            //foreach (var obj in actor.GetNearbyObjects(range: (r) => r <= range))
            //    if(attack.CollisionType(actor, obj, direction))
                    throw new NotImplementedException();
                    //obj.PostMessage(Message.Types.Attacked, actor, attack);          
        }


        static public Func<Vector3, Vector3, GameObject, bool> Ray
        {
            get
            {
                return (origin, direction, target) =>
                {
                    //var origin = actor.Global + Vector3.UnitZ * actor.GetPhysics().Height / 2;
                    //var dir = new Vector3(actor.Transform.Direction, origin.Z);
                    Ray ray = new Ray(origin, direction);

                    //Ray ray = new Ray(actor.Global, direction);
                    var cylinder = BoundingCylinder.Create(target);
                    var success = cylinder.Intersects(ray);
                    return success;

                    //Ray ray = new Ray(actor.Global, direction);
                    //return (BoundingCylinder.Create(target).Intersects(ray));
                };
            }
        }
        static public Func<GameObject, GameObject, Vector3, bool> Arc
        {
            get
            {
                return (actor, target, direction) =>
                {
                    throw new NotImplementedException();
                    Vector3 distance = target.Global - actor.Global;
                    distance.Normalize();
                    float angle = (float)Math.Acos(Vector3.Dot(direction, distance));
                    return (Math.Abs(angle) < Components.Attack.DefaultArc);
                };
            }
        }

        internal Vector3 GetMomentum()
        {
            float coef =  this.Charge * (1 + StatsComponent.GetStatOrDefault(this.Attacker, Stat.Types.Knockback, 0));
            return this.Momentum * coef;
        }
    }
}
