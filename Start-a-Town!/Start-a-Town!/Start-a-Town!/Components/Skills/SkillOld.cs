using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class SkillOld
    {
        public enum Types { Lumberjacking, Mining, Crafting, Carpentry, Digging, Farming, Construction, Argiculture };

        static List<SkillOld> _SkillRegistry;
        static public List<SkillOld> SkillRegistry
        {
            get
            {
                if (_SkillRegistry.IsNull())
                    Initialize();
                return _SkillRegistry;
            }
        }
        static void Initialize()
        {
            _SkillRegistry = new List<SkillOld>()
            {
                new SkillOld(Types.Lumberjacking, "Lumberjacking", 2),
                new SkillOld(Types.Mining,"Mining", 1),
                new SkillOld(Types.Digging,"Digging", 21),
                new SkillOld(Types.Crafting,"Crafting", 3),
                new SkillOld(Types.Construction,"Construction", 3),
                new SkillOld(Types.Carpentry,"Carpentry", 14),
                new SkillOld(Types.Farming,"Farming", 4),
                new Skills.SkillArgicultureOld()
               // new Skill(Types.Argiculture,"Argiculture", 4)
            };
            //AddEffect(Types.Digging, Formula.GetFormula(Formula.Types.DiggingSpeed));
            //AddEffect(Types.Construction, Formula.GetFormula(Formula.Types.MaterialsRecovery));

            AddBonus(Types.Digging, Stat.Types.Digging, lvl => lvl / 200f);// Formula.GetFormula(Formula.Types.DiggingSpeed));
            AddBonus(Types.Construction, Stat.Types.MatRecover, lvl => lvl / 200f);
        }

        static public SkillOld GetSkill(string name)
        {
            return SkillRegistry.ToDictionary(foo => foo.Name, foo => foo)[name];
        }
        static public SkillOld GetSkill(Types id)
        {
            return SkillRegistry.ToDictionary(foo => foo.ID, foo => foo)[id];
        }
        static public SkillOld Create(Types id, Action<SkillOld> initializer)
        {
            SkillOld sk = GetSkill(id).MemberwiseClone() as SkillOld;
            initializer(sk);
            return sk;
        }
        static public SkillOld Create(Types id)
        {
            SkillOld sk = GetSkill(id).MemberwiseClone() as SkillOld;
            return sk;
        }
        protected SkillOld()
        {
            this.Effects = new List<Formula>();
            this.Bonuses = new Dictionary<Stat.Types, Func<int, float>>();
        }
        public SkillOld(Types id, string name = "<untitled skill>", int iconID = 0)//, StatCollection stats = null)
        {
            this.ID = id;
            this.Name = name;
            this.IconID = iconID;
            this.Level = 0;
            this.Experience = 0;
            this.Description = "<description>";
            this.Effects = new List<Formula>();
            this.Bonuses = new Dictionary<Stat.Types, Func<int, float>>();// stats ?? new StatCollection();
        }
        public string Name { get; set; }
        public virtual string Description { get; set; }
        public int IconID { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public Types ID { get; set; }
        public List<Formula> Effects { get; set; }
        public Dictionary<Stat.Types, Func<int, float>> Bonuses { get; set; }

        public GameObject ToObject()
        {
            GameObject obj = GameObjectDb.SkillObj.Clone();
            obj.Name = this.Name;
            obj.Description = this.Description;
            obj.AddComponent<GuiComponent>().Initialize(this.IconID);
            obj["Skill"] = new SkillComponentOld(this);
           // obj["Equip"] = new EquipComponent();
            return obj;
        }

        static public void Award(Net.IObjectProvider net, GameObject actor, GameObject source, Types ID, int value)
        {
            SkillsComponent.AwardSkill(net, actor, ID, value);
            //net.PostLocalEvent(actor, ObjectEventArgs.Create(Message.Types.SkillAward, new object[] { ID, value }));
        }

        static public void AddEffect(Types skillID, Formula effect)
        {
            GetSkill(skillID).Effects.Add(effect);
        }
        //static public void AddBonus(Types skillID, Stat.Types stat, Formula formula)
        //{
        //    GetSkill(skillID).Bonuses[stat] = formula;
        //}
        static public void AddBonus(Types skillID, Stat.Types stat, Func<int, float> formula)
        {
            GetSkill(skillID).Bonuses[stat] = formula;
        }
    }

}
