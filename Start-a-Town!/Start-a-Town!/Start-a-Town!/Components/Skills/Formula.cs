using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    public class Formula
    {
        public enum Types { Default, TillingSpeed, DiggingSpeed, MiningSpeed, ConstructionSpeed, SawingSpeed, ChoppingSpeed, MaterialsRecovery }

        static List<Formula> _Registry;
        static public List<Formula> Registry
        {
            get
            {
                if (_Registry.IsNull())
                    Initialize();
                return _Registry;
            }
        }
        static void Initialize()
        {
            _Registry = new List<Formula>()
            {
                new Formula(Types.Default, "Default")
                {
                    Function = (a)=>0
                },
                new Formula(Types.TillingSpeed, "Tilling Speed")
                {
                    Function = (actor)=>
                    {
                        int skill = SkillsComponent.GetSkillLevel(actor, SkillOld.Types.Farming);
                        return (skill / 200f);
                    }
                },
                new Formula(Types.DiggingSpeed, "Digging Speed")
                {
                    Function = (actor)=>
                    {
                        //int skill = SkillsComponent.GetSkillLevel(actor, Skill.Types.Digging);
                        //return (skill / 200f);
                        return StatsComponent.GetStatOrDefault(actor, Stat.Types.Digging, 0);
                    }
                },
                new Formula(Types.ChoppingSpeed, "Chopping Speed")
                {
                    Function = (actor)=>
                    {
                        return StatsComponent.GetStatOrDefault(actor, Stat.Types.Lumberjacking, 0);
                    }
                },
                new Formula(Types.MiningSpeed, "Mining Speed")
                {
                    Function = (actor)=>
                    {
                        return StatsComponent.GetStatOrDefault(actor, Stat.Types.Mining, 0);
                    }
                },
                new Formula(Types.MaterialsRecovery, "change to recover materials")
                {
                    Function = (actor)=>
                    {
                        int skill = SkillsComponent.GetSkillLevel(actor, SkillOld.Types.Construction);
                        return (skill / 200f);
                    }
                }
            };
        }

        static public Formula GetFormula(Types id)
        {
            return Registry.ToDictionary(foo => foo.ID, foo => foo)[id];
        }
        static public float GetValue(GameObject actor, Types id)
        {
            return Registry.ToDictionary(foo => foo.ID, foo => foo)[id].GetValue(actor);
        }
        public Types ID { get; set; }
        public string Name { get; set; }
      //  public float Value { get; set; }
        public Func<GameObject, float> Function { get; set; }
        public string Format;

        public Formula(Types id, string name)
        {
            this.ID = id;
            this.Name = name;
            this.Function = (a) =>
            {
                return 0;
            };
            this.Format = "+#0.##% " + Name;
        }

        public string ToString(GameObject actor)
        {
            return GetValue(actor).ToString(this.Format);
        }

        public float GetValue(GameObject actor)
        {
            return this.Function(actor);
        }
    }
}
