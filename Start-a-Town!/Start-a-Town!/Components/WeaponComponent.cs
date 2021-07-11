using System;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class WeaponComponent : EntityComponent
    {
        public override string ComponentName => "Weapon";

        public StatCollection Damage;
        public float Speed;

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

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, "Speed: " + this.Speed + "\n" + this.Damage) { Font = UIManager.FontBold });
        }

        public override object Clone()
        {
            return new WeaponComponent(this.Speed, this.Damage.Select(foo => Tuple.Create(foo.Key, foo.Value)).ToArray());
        }

        static public StatCollection GetDamage(GameObject obj)
        {
            if (obj == null)
                return null;
            WeaponComponent w;
            if (!obj.TryGetComponent<WeaponComponent>("Weapon", out w))
                return null;
            return w.Damage;
        }

        public float GetTotalDamage()
        {
            float dmg = 0;
            foreach(var stat in this.Damage)
            {
                dmg += stat.Value;
            }
            return dmg;
        }
        static public float GetTotalDamage(GameObject obj)
        {
            return obj.GetComponent<WeaponComponent>().GetTotalDamage();
        }
    }
}
