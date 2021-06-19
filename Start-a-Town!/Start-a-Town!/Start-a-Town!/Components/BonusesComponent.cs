using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public enum BonusType { Flat, Percentile }
    public struct Bonus
    {
        public BonusType Type;
        public float Value;

        public Bonus(float value, BonusType type = BonusType.Flat)
        {
            this.Value = value;
            this.Type = type;
        }

        public override string ToString()
        {
         //   return Value.ToString("f2") + (Type == BonusType.Percentile ? "%" : "");
            switch (Type)
            {
                case BonusType.Percentile:
                    return Value.ToString("#0.##%");//(Value * 100).ToString("##.##").   //ToString("f2") + "%";
                default:
                    return Value.ToString();
            }
        }

        public void Apply(ref float value)
        {
            switch (Type)
            {
                case BonusType.Percentile:
                    value += value * this.Value;
                    return;
                default:
                    value += this.Value;
                    return;
            }
        }
    }
    public class BonusCollection : Dictionary<string, Bonus> 
    {
        public override string ToString()
        {
            string text = "";
            foreach (var bonus in this)
                text += bonus.Key + ": " + bonus.Value.ToString() + '\n';
            return text.TrimEnd('\n');
        }
    };

    class BonusesComponent : Component
    {
        public override string ComponentName
        {
            get { return "Bonuses"; }
        }
        static public BonusesComponent Create(GameObject obj, string name)
        {
            BonusesComponent comp = new BonusesComponent();
            obj[name] = comp;
            return comp;
        }

        static public float GetStat(GameObject obj, string statName)
        {
            StatsComponent statsComp;
            if (!obj.TryGetComponent<StatsComponent>("Stats", out statsComp))
                return 0;
            Bonus stat;
            if (statsComp.TryGetProperty<Bonus>(statName, out stat))
                return stat.Value;
            return 0;
        }
        static public bool TryGetStat(GameObject obj, string statName, out float stat)
        {
            BonusesComponent statsComp;
            if (!obj.TryGetComponent<BonusesComponent>("Bonuses", out statsComp))
            {
                stat = 0;
                return false;
            }
            Bonus bonus;
            if (statsComp.TryGetProperty<Bonus>(statName, out bonus))
            {
                stat = bonus.Value;
                return true;
            }
            stat = 0;
            return false;
        }
        static public void GetBonuses(GameObjectSlot objSlot, BonusCollection list)
        {
            GameObject obj = objSlot.Object;
            if (obj == null)
                return;
            GetBonuses(obj, list);
        }
        static public void GetBonuses(BodyPart objSlot, BonusCollection list)
        {
            GameObject obj = objSlot.Object;
            if (obj == null)
                return;
            GetBonuses(obj, list);
        }
        static public void GetBonuses(GameObject obj, BonusCollection list)
        {
            BonusesComponent bc;
            if(!obj.TryGetComponent<BonusesComponent>("Bonuses", out bc))
                return;
          //  bc.Properties.Values.ToList().ForEach(foo => list.Add((Bonus)foo));
            foreach (var p in bc.Properties)
            {
                Bonus bonus = (Bonus)p.Value;
                if (!list.ContainsKey(p.Key))
                {
                    list[p.Key] = bonus;
                    continue;
                }
                list[p.Key] = new Bonus(list[p.Key].Value * (1 + bonus.Value), BonusType.Percentile);
            }
        }
        /// <summary>
        /// Returns a sum of all the bonuses from the items an actor is wearing and holding.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        static public BonusCollection PollBonuses(GameObject actor)
        {
            BonusCollection list = new BonusCollection();
            InventoryComponent.CollectBonuses(actor, list);
            BodyComponent.CollectBonuses(actor, list);
            return list;
        }
        /// <summary>
        /// Collects all the bonuses from the items an actor is wearing and holding and returns the value of a specific bonus, or a default value if there is no bonus.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="bonusName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        static public float GetBonusOrDefault(GameObject actor, string bonusName, float defaultValue)
        {
            Bonus bonus;
            BonusCollection bonuses = PollBonuses(actor);
            if (bonuses.TryGetValue(bonusName, out bonus))
                return bonus.Value;
            return defaultValue;
        }

        public override object Clone()
        {
            BonusesComponent comp = new BonusesComponent();
            foreach (KeyValuePair<string, Bonus> bonus in Properties.ToDictionary(foo => foo.Key, foo => (Bonus)foo.Value))
                comp[bonus.Key] = bonus.Value;
            return comp;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            foreach (KeyValuePair<string, Bonus> bonus in Properties.ToDictionary(foo => foo.Key, foo => (Bonus)foo.Value))
            {
                // float value = (float)property.Value;
                float value = bonus.Value.Value;
                bool good = value > 0;
                //tooltip.Controls.Add(new Label(new Vector2(0, tooltip.Controls.Count > 0 ? tooltip.Controls.Last().Bottom : 0), bonus.Key + ": " + (good ? "+" : "") + value.ToString() + (bonus.Value.Type == BonusType.Percentile ? "%" : ""), good ? Color.Lime : Color.Red));
                tooltip.Controls.Add(new Label(new Vector2(0, tooltip.Controls.Count > 0 ? tooltip.Controls.Last().Bottom : 0), bonus.Key + ": " + (good ? "+" : "") + bonus.Value.ToString()) { TextColorFunc = () => good ? Color.Lime : Color.Red });
            }
        }
    }
}
