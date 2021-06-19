using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class WeaponComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Weapon";
            }
        }

        public StatCollection Damage { get { return (StatCollection)this["Damage"]; } set { this["Damage"] = value; } }
        public float Speed { get { return (float)this["Speed"]; } set { this["Speed"] = value; } }

        public WeaponComponent()
        {
            this.Damage = new StatCollection();
            this.Speed = 1;
        }
        public WeaponComponent Initialize(float speed, params Tuple<Stat.Types, float>[] damage)
        {
            this.Damage = new StatCollection();
            this.Speed = speed;
            foreach (var dmg in damage)
                this.Damage[dmg.Item1] = this.Damage.GetValueOrDefault(dmg.Item1) + dmg.Item2;
            return this;
        }
        public WeaponComponent(float speed, params Tuple<Stat.Types, float>[] damage)
        {
            this.Damage = new StatCollection();
            this.Speed = speed;
            foreach (var dmg in damage)
                this.Damage[dmg.Item1] = this.Damage.GetValueOrDefault(dmg.Item1) + dmg.Item2;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Speed: " + this.Speed + "\n" + this.Damage) { Font = UIManager.FontBold });
        }

        public override object Clone()
        {
            return new WeaponComponent(this.Speed, this.Damage.Select(foo => Tuple.Create(foo.Key, foo.Value)).ToArray());
        }

        //static public StatCollection GetDamage(GameObject obj)
        //{
        //    WeaponComponent w;
        //    if(!obj.TryGetComponent<WeaponComponent>("Weapon", out w))
        //        return new StatCollection();
        //    return w.Damage;
        //}
        static public StatCollection GetDamage(GameObject obj)
        {
            if (obj.IsNull())
                return null;
            WeaponComponent w;
            if (!obj.TryGetComponent<WeaponComponent>("Weapon", out w))
                return null;
            return w.Damage;
        }
    }
}
