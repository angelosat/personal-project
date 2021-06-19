using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class SkillComponentOld : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Skill";
            }
        }

        public SkillComponentOld(SkillOld skill)
        {
            this.Skill = skill;
        }

        public SkillOld Skill { get { return (SkillOld)this["Skill"]; } set { this["Skill"] = value; } }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.Death:
                  //  sender.PostMessage(Message.Types.SkillAward, parent);
                    throw new NotImplementedException();
                    //e.Network.PostLocalEvent(sender, ObjectLocalEventArgs.Create(Message.Types.SkillAward, new TargetArgs(parent)));
                    break;
                default:
                    break;
            }
            return true;
        }

        public override object Clone()
        {
            SkillComponentOld comp = new SkillComponentOld(this.Skill);
            //foreach (KeyValuePair<string, object> parameter in Properties)
            //    comp[parameter.Key] = parameter.Value;
            return comp;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft,
                "Level: " + this.Skill.Level +
                "\nExperience: " + this.Skill.Experience));
            //this.Skill.Effects.ForEach(
            //    fx =>
            //    {
            //        tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, fx.ToString(Player.Actor)) { TextColorFunc = () => Color.CornflowerBlue });
            //    });
            foreach (var bonus in this.Skill.Bonuses)
            {
                Stat stat = Stat.GetStat(bonus.Key);
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft,
                    stat.ToString(bonus.Value(Skill.Level)) + "\n"// +
                    //"Next Level:\n" +
                    //stat.ToString(bonus.Value(Skill.Level + 1))
                    )
                    {
                        Font = UIManager.FontBold,
                        TextColorFunc = () => Color.CornflowerBlue
                    });
                tooltip.Controls.Add("Next Level:".ToLabel(tooltip.Controls.BottomLeft));
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft,
                                    stat.ToString(bonus.Value(Skill.Level + 1)))
                {
                    Font = UIManager.FontBold,
                    TextColorFunc = () => Color.CornflowerBlue
                });
            }
        }
    }
    
}
