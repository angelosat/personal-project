using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class TreeComponent : Component// : ScriptComponent
    {
        //public TreeComponent(float maxHealth = 1, float initialPercentage = 1)
        //{
        //    Properties[Stat.Value.Name] = maxHealth * initialPercentage;
        //    Properties[Stat.Max.Name] = maxHealth;
        //  //  Properties[Stat.Skill.Name] = GameObjectDb.SkillLumberjacking.ID;
        //}

        public override string GetTooltipText()
        {
            return "Left click: Chop down";
        }

        public bool Break(Net.IObjectProvider net, GameObject self, Position pos)
        {
           // Map.RemoveObject(self);
            self.Despawn(net);
            return true;
        }

        public override object Clone()
        {
            TreeComponent comp = new TreeComponent();
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            return comp;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e) //GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (e.Type)
            {
                //case Message.Types.Attack:
                //    InventoryComponent inv = sender.GetComponent<InventoryComponent>("Inventory");
                //    //   GameObject weapon = inv.GetProperty<Dictionary<string, GameObjectSlot>>("Equipment")["Mainhand"].Object;

                //    StatsComponent stats = sender.GetComponent<StatsComponent>("Stats");
                //    SkillsComponent skills = sender.GetComponent<SkillsComponent>("Skills");
                //    float chop = stats.GetPropertyOrDefault<float>(Stat.Chop.Name) * 1f;
                //    float slash = stats.GetPropertyOrDefault<float>(Stat.Slash.Name) * 0.2f;
                //    float lumber = skills.GetPropertyOrDefault<float>(Stat.Lumberjacking.Name);
                //    float totalDamage = chop + slash;// +lumber;



                //    this[Stat.Value.Name] = GetProperty<float>(Stat.Value.Name) - totalDamage;
                //    //Log.Write(Log.EntryTypes.Default, totalDamage.ToString() + " chopping powah");
                //    StatsComponent dmgComp = new StatsComponent();
                //    dmgComp[Stat.Chop.Name] = chop;
                //    dmgComp[Stat.Slash.Name] = slash;
                //    //dmgComp[Stat.Lumberjacking.Name] = lumber;
                //    Log.Enqueue(Log.EntryTypes.Damage, sender, parent, dmgComp);

                //    float skillMod = 1 - 0.8f * lumber / 100f;
                //    sender["Cooldown"]["Cooldown"] = skillMod * 60; //*stats.GetProperty<float>(Stat.AtkSpeed.Name) * 


                //    if (GetProperty<float>(Stat.Value.Name) <= 0)
                //    {
                //        Log.Enqueue(Log.EntryTypes.Death, sender, parent);
                //        Break(parent, parent.GetComponent<MovementComponent>("Position").GetProperty<Position>("Position"));
                //        SkillsComponent senderSkills;
                //        if (sender.TryGetComponent<SkillsComponent>("Skills", out senderSkills))
                //        {
                //            GameObject skillObj = GameObjectDb.SkillLumberjacking;
                //            skillObj["Skill"][Stat.Value.Name] = 1f;
                //            senderSkills.Give(parent, sender, new GameObjectSlot(skillObj));
                //            //sender.HandleMessage(skillObj, Message.Types.SkillAward);
                //        }
                //        parent.HandleMessage(sender, Message.Types.Death);
                //        return false;
                //    }

                //    return true;

                //case Message.Types.Death:
                //    e.Sender.PostMessage(Message.Types.ModifyNeed, parent, "Wood", 50);
                //    return true;

                //case Message.Types.Query:
                //    (e.Parameters[0] as List<Interaction>).Add(new Interaction(TimeSpan.Zero, Message.Types.Attack, parent, "Attack"));
                //    return true;
                default:
                    return false;
            }
        }

        //public override void GetTooltip(GameObject parent, UI.Control tooltip)
        //{
        //    GroupBox box = new GroupBox();
        //    Bar bar = new Bar(new Vector2(0, tooltip.Controls[tooltip.Controls.Count - 1].Bottom));
        //    bar.Percentage = GetProperty<float>(Stat.Value.Name) / GetProperty<float>(Stat.Max.Name);
        //    box.Controls.Add(bar);
        //    tooltip.Controls.Add(box);
        //}

        //internal override List<Tag> Save()
        //{
        //    List<Tag> data = new List<Tag>();
        //    //foreach (KeyValuePair<string, object> parameter in Properties)
        //    //    data.Add(new Tag(Tag.Types.Float, parameter.Key, (float)parameter.Value));
        //    data.Add(new Tag(Tag.Types.Float, Stat.Value.Name, (float)this[Stat.Value.Name]));
        //    return data;
        //}

        //internal override void Load(Tag data)
        //{
        //    //List<Tag> stats = data.Value as List<Tag>;
        //   // for (int i = 0; i < stats.Count - 1; i++)
        //   //     this[stats[i].Name] = (float)stats[i].Value;
        //    this[data[Stat.Value.Name].Name] = (float)data[Stat.Value.Name].Value;
        //}
    }
}
