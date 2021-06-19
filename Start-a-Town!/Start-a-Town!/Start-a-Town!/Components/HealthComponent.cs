using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class HealthComponent : Component// : ScriptComponent
    {
        //public Progress Health;
        //Dictionary<Damage.Types, float> Resistances;

        public override string GetTooltipText()
        {
            return "Left click: Attack";
        }

        public bool Break(Net.IObjectProvider net, GameObject self, Position pos)
        {

          //  Map.RemoveObject(self);
            self.Despawn(net);
           // self.HandleMessage(self, Message.Types.Death);
            return true;
        }

        //public float GetResistance(Damage.Types type)
        //{
        //    return Resistances[type];
        //}

        public float Value { get { return (float)this["Value"]; } set { this["Value"] = value; } }
        public float Max { get { return (float)this["Max"]; } set { this["Max"] = value; } }

        public float Percentage { get { return this.Value / this.Max; } set { this.Value = value * this.Max; } }

        public HealthComponent(float maxHealth = 1, float initialPercentage = 1, float resistances = 0) //float chopRes = 0, float bluntRes = 0, float slashRes = 0, float pierceRes = 0)
        {
            //Health = new Progress(0, maxHealth * initialHealth, 0);
            Properties.Add(Stat.Value.Name, maxHealth * initialPercentage);//new Progress(0, maxHealth, initialPercentage * maxHealth));
            Properties.Add(Stat.Max.Name, maxHealth);
            //Resistances = new Dictionary<Damage.Types, float>() { { Damage.Types.Chop, chopRes }, { Damage.Types.Blunt, bluntRes }, { Damage.Types.Slash, slashRes }, { Damage.Types.Pierce, pierceRes } };
            foreach (var dmgType in Damage.DamageTypes)
                Properties.Add(Stat.GetStat(dmgType.ID).Name, resistances);
        }

        public override void NameplateInit(Nameplate plate)
        {
            plate.AddControls(new Bar() { 
                Location = plate.Controls.Last().BottomLeft,
                Width = plate.Controls.Last().Width,
                Height = 3,
                MouseThrough = true,
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Percentage),
                Tag = this,
                PercFunc = () => this.Percentage,
            });
        }

        public override object Clone()
        {
            HealthComponent comp = new HealthComponent((float)this["Value"]);
            //foreach (KeyValuePair<string, object> parameter in Properties)
            //    comp[parameter.Key] = parameter.Value;
            return comp;
        }

        //public override bool Activate(GameObject actor, StaticObject self)
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            switch (e.Type)
            {
                case Message.Types.Attacked:
                    //StatsComponent stats = e.Sender.GetComponent<StatsComponent>("Stats");
                    //StatsComponent totalDamage = new StatsComponent();
                    //foreach (KeyValuePair<string, object> dmgStat in StatsComponent.Damage.Properties)
                    //{
                    //    float prop;
                    //    if (stats.TryGetProperty<float>(dmgStat.Key, out prop))
                    //        totalDamage.Properties[dmgStat.Key] = prop * (1 - this.GetProperty<float>(dmgStat.Key));
                    //}
                    //this[Stat.Value.Name] = GetProperty<float>(Stat.Value.Name) - Damage.GetTotal(totalDamage);

                    Attack attack = e.Parameters[0] as Attack;
                    this.Value -= attack.Value;
                    Log.Enqueue(Log.EntryTypes.Damage, e.Sender, parent, attack);// totalDamage);

                    e.Sender["Control"]["Cooldown"] = (float)Engine.TargetFps;// 60f;

                    if (this.Value <= 0)
                    {
                        Log.Enqueue(Log.EntryTypes.Death, e.Sender, parent);
                        Break(e.Network, parent, parent.GetComponent<PositionComponent>("Position").GetProperty<Position>("Position"));
                        throw new NotImplementedException();
                        //parent.PostMessage(Message.Types.Death, e.Sender);
                        return true;
                    }

                    //if (attack.Momentum == Vector3.Zero)
                    //    return true;

                    //parent.PostMessage(Message.Types.ApplyForce, parent, attack.Momentum);

                    return false;

                case Message.Types.RestoreHealth:
                    Value = Math.Min(Max, Value + (float)e.Parameters[1]);
                    return true;
                default:
                    return false;
            }
        }

        public override void Query(GameObject parent, List<Interaction> list)// GameObjectEventArgs e)
        {
            if (this.Value <= 0) //if it's dead, return no interaction
                return;
            //if (e.Sender == parent)
            //    return;
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Attack, parent, "Attack"));//, need: new Need("Hunger", 100)));
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            GroupBox box = new GroupBox();
            InteractionBar bar = new InteractionBar(new Vector2(0, tooltip.Controls.Count > 0 ? tooltip.Controls.Last().Bottom : 0));
           //bar.Percentage = GetProperty<Progress>("Health").Percentage;
            bar.Percentage = GetProperty<float>(Stat.Value.Name) / GetProperty<float>(Stat.Max.Name);
            box.Controls.Add(bar);
            tooltip.Controls.Add(box);
        }

        //Bar Bar;
        //public override void Focus(GameObject parent)
        //{
        //    Bar = new Bar();
        //    Bar.Track(Progress);
        //    Bar.Object = parent;
        //    Bar.Show();
        //}
        //public override void FocusLost(GameObject parent)
        //{
        //    Bar.Stop();
        //    Bar.Hide();
        //}
    }
}
