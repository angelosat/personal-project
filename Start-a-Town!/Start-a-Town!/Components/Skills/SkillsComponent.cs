using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class SkillsComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Skills";
            }
        }

        public List<SkillOld> Skills { get { return (List<SkillOld>)this["Skills"]; } set { this["Skills"] = new List<SkillOld>(); } }
        public List<GameObjectSlot> SkillsSlots { get { return (List<GameObjectSlot>)this["SkillsSlots"]; } set { this["SkillsSlots"] = new List<GameObjectSlot>(); } }
        public int MaxSkills { get { return (int)this["MaxSkills"]; } set { this["MaxSkills"] = value; } }
        public SkillsComponent()
        {
            this.SkillsSlots = new List<GameObjectSlot>();
            this.Skills = new List<SkillOld>();
        }

        public SkillsComponent Initialize(int maxSkills, params GameObjectSlot[] skills)
        {
            skills.ToList().ForEach(sk => this.SkillsSlots.Add(sk));
            this.MaxSkills = maxSkills;
            return this;
        }

        public SkillsComponent Initialize(int maxSkills, params SkillOld[] skills)
        {
            skills.ToList().ForEach(sk => this.Skills.Add(sk));
            this.MaxSkills = maxSkills;
            return this;
        }

        public SkillsComponent(params GameObjectSlot[] skills)
            : this()
        {
            skills.ToList().ForEach(sk=>this.SkillsSlots.Add(sk));
        }

        public SkillsComponent(float initialValue)
        {
            //foreach (KeyValuePair<GameObject.Types, GameObject> stat in Skill.SkillTypes)
            //{
            //    Properties.Add(stat.Value.Name, initialValue);
            //}
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.SkillAward:



                 //   "skill awarded".ToConsole();
                  //  Skill sk = this.Skills.FirstOrDefault(foo=>foo.id
                    GameObjectSlot skSlot = this.SkillsSlots
                        .FindAll(foo => foo.HasValue)
                        .FindAll(foo => foo.Object.Components.ContainsKey("Skill"))
                        .FindAll(foo => foo.Object["Skill"].GetProperty<SkillOld>("Skill").ID == (SkillOld.Types)e.Parameters[0])
                        .FirstOrDefault();
                    if (skSlot.IsNull())
                        return true;
                    // TODO: send message to skill to increase its xp
                    var value = (int)e.Parameters[1];
                    skSlot.Object["Skill"].GetProperty<SkillOld>("Skill").Level += value;

                    return true;
                default:
                    return false;
            }
        }

        static public void AwardSkill(IObjectProvider net, GameObject entity, SkillOld.Types type, int value)
        {
            entity.TryGetComponent<SkillsComponent>(sk =>
            {
                SkillOld skill = sk.Skills.FirstOrDefault(s => s.ID == type);
                if (skill.IsNull())
                    return;
                skill.Level += value;
                net.EventOccured(Message.Types.SkillAward, entity, skill, value);
            });
        }

        public override object Clone()
        {
            SkillsComponent comp = new SkillsComponent();
            foreach (KeyValuePair<string, object> parameter in Properties)
                comp[parameter.Key] = parameter.Value;
            return comp;
        }

        static public int GetSkillLevel(GameObject actor, SkillOld.Types skillID)
        {
            //SkillsComponent skills;
            //if (!actor.TryGetComponent<SkillsComponent>("Skills", out skills))
            //    return 0;
            //GameObjectSlot skillSlot = skills.SkillsSlots.FirstOrDefault(foo => foo.Object["Skill"].GetProperty<Skill>("Skill").ID == skillID);
            //if (skillSlot.IsNull())
            //    return 0;
            //return skillSlot.Object["Skill"].GetProperty<Skill>("Skill").Level;
            SkillOld skill;
            if (TryGetSkill(actor, skillID, out skill))
                return skill.Level;
            return 0;
        }

        static public bool TryGetSkill(GameObject actor, SkillOld.Types skillID, out SkillOld skill)
        {
            skill = null;
            SkillsComponent skills;
            if (!actor.TryGetComponent<SkillsComponent>("Skills", out skills))
                return false;
            skill = skills.Skills.FirstOrDefault(s => s.ID == skillID);
            return !skill.IsNull();
        }

        public override string GetStats()
        {
            return "Max Skills: " + this.MaxSkills;
        }

        static public void PollStats(GameObject obj, StatCollection stats)
        {
            SkillsComponent skills;
            if (!obj.TryGetComponent<SkillsComponent>("Skills", out skills))
                return;
            //foreach(var skillObj in skills.Skills)
            foreach (var skill in skills.SkillsSlots.Select(foo => foo.Object["Skill"]["Skill"] as SkillOld))
                foreach(var bonus in skill.Bonuses)
                    stats[bonus.Key] = stats.GetValueOrDefault(bonus.Key) + bonus.Value(skill.Level);
        }
    }
    
}
