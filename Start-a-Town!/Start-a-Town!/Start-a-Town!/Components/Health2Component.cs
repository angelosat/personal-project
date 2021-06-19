using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class PeriodicLoot
    {
        public Loot Loot;
        public float Rate;
        public PeriodicLoot(GameObject.Types objID, float chance = 1, int count = 1, float rate = 1)
        {
            this.Loot = new Loot(objID, chance, count);
            this.Rate = rate;
        }
    }

    public class Health2Component : Component
    {
        public override string GetTooltipText()
        {
            return "Left click: Attack";
        }

        public bool Break(Net.IObjectProvider net, GameObject self, Position pos)
        {
            //Map.RemoveObject(self);
            self.Despawn(net);
            return true;
        }

        public float Value { get { return (float)this["Value"]; } set { this["Value"] = value; } }
        public float Max { get { return (float)this["Max"]; } set { this["Max"] = value; } }
        public List<Loot> Loot { get { return this["Loot"] as List<Loot>; } set { this["Loot"] = value; } }
        public float Percentage { get { return this.Value / this.Max; } set { this.Value = value * this.Max; } }

        public Health2Component(float maxHealth = 1, float initialPercentage = 1, float resistances = 0, params Loot[] loot)
        {
            Properties.Add(Stat.Value.Name, maxHealth * initialPercentage);
            Properties.Add(Stat.Max.Name, maxHealth);

            foreach (var dmgType in Damage.DamageTypes)
                Properties.Add(Stat.GetStat(dmgType.ID).Name, resistances);

            this.Loot = new List<Loot>(loot);
        }


        public override object Clone()
        {
            Health2Component comp = new Health2Component((float)this["Value"]);
            return comp;
        }


        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Attack:
                    StatsComponent stats = e.Sender.GetComponent<StatsComponent>("Stats");

                    StatsComponent totalDamage = new StatsComponent();
                    foreach (KeyValuePair<string, object> dmgStat in StatsComponent.Damage.Properties)
                    {
                        float prop;
                        if (stats.TryGetProperty<float>(dmgStat.Key, out prop))
                            totalDamage.Properties[dmgStat.Key] = prop * (1 - this.GetProperty<float>(dmgStat.Key));
                    }

                    this[Stat.Value.Name] = GetProperty<float>(Stat.Value.Name) - Damage.GetTotal(totalDamage);

                    foreach (Loot loot in Loot)
                    {

                    }

                    Log.Enqueue(Log.EntryTypes.Damage, e.Sender, parent, totalDamage);

                    e.Sender["Control"]["Cooldown"] = 60f;

                    if (GetProperty<float>(Stat.Value.Name) <= 0)
                    {
                        Log.Enqueue(Log.EntryTypes.Death, e.Sender, parent);
                        Break(e.Network, parent, parent.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position"));
                        throw new NotImplementedException();
                        //parent.PostMessage(Message.Types.Death, e.Sender);
                        return true;
                    }

                    return false;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                case Message.Types.RestoreHealth:
                    Value = Math.Min(Max, Value + (float)e.Parameters[1]);
                    return true;
                default:
                    return false;
            }
        }

        public override void Query(GameObject parent, List<Interaction> list)//GameObjectEventArgs e)
        {
            if (this.Value <= 0) //if it's dead, return no interaction
                return;
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Attack, parent, "Attack"));//, need: new Need("Hunger", 100)));
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            GroupBox box = new GroupBox();
            InteractionBar bar = new InteractionBar(new Vector2(0, tooltip.Controls.Count > 0 ? tooltip.Controls.Last().Bottom : 0));

            bar.Percentage = GetProperty<float>(Stat.Value.Name) / GetProperty<float>(Stat.Max.Name);
            box.Controls.Add(bar);
            tooltip.Controls.Add(box);
        }

    }
}
