using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ToolAbilityDef : Def
    {
        //static int _IDSequence = 20000;
        static int _IDSequence = "skills".GetHashCode() >> 2;// 20000;
        public static int IDSequence { get { return _IDSequence++; } }

        static Dictionary<int, ToolAbilityDef> _Dictionary;
        public static Dictionary<int, ToolAbilityDef> Dictionary
        {
            get
            {
                if (_Dictionary == null)
                    _Dictionary = new Dictionary<int, ToolAbilityDef>();
                return _Dictionary;
            }
        }

        public int ID { get; protected set; }
        //public string Name { get; protected set; }
        public string Description { get; protected set; }
        //public Work Work { get; private set; }
        Func<TargetArgs, Interaction> WorkFactory = (t) => null;

        public ToolAbilityDef(string name) : base(name)
        {
            
        }
        
        protected ToolAbilityDef(string name, string description) : this(name, description, null) { }
        //Skill(string name, string description, Work work)
        protected ToolAbilityDef(string name, string description, Func<TargetArgs, Interaction> workFactory) : base(name)
        {
            this.ID = IDSequence;
            Dictionary[this.ID] = this;
           
            this.Description = description;
            //this.Work = work;
            this.WorkFactory = workFactory;
        }

        public virtual Interaction GetInteraction(GameObject actor, TargetArgs target)
        {
            if (this.WorkFactory == null)
                return null;
            return this.WorkFactory(target);
        }
        public virtual Interaction GetInteraction()
        {
            if (this.WorkFactory == null)
                return null;
            return this.WorkFactory(TargetArgs.Null);
        }
        static public readonly ToolAbilityDef Digging = new ToolAbilityDef("Digging", "Dig up soil and dirt blocks", (t) => new InteractionDigging());
        //static public readonly Skill Building = new Skill("Building", "Build blocks and other structures");//, () => new BuildStructure());
        static public readonly ToolAbilityDef Building = new SkillBuilding();
            //new Skill("Building", "Build blocks and other structures", (t) =>
            //{
            //    if (t.Object.HasComponent<StructureComponent>())
            //        return new BuildStructure();
            //    else if (t.Object.HasComponent<ConstructionComponent>())
            //        return new BuildBlock();
            //    return null;
            //});
        static public readonly ToolAbilityDef Mining = new ToolAbilityDef("Mining", "Dig up stone blocks", (t) => new InteractionMining());
        //static public readonly Skill Chopping = new Skill("Chopping", "Chop down trees and enemies with axes", (t) => new ProcessMaterial("Chop", Chopping) { Verb = "Chopping" });
        static public readonly ToolAbilityDef Chopping = new ToolAbilityDef("Chopping", "Chop down trees and enemies with axes", (t) => new Start_a_Town_.InteractionChopping());

        static public readonly ToolAbilityDef Argiculture = new SkillArgiculture();// new Skill("Argiculture", "Helps determine type and growth time of plants", (t) => new Tilling());
        static public readonly ToolAbilityDef Planting = new ToolAbilityDef("Planting", "Planting plants", (t) => new Planting());
        static public readonly ToolAbilityDef Carpentry = new ToolAbilityDef("Carpentry", "The craft of converting wood to useful equipment", (t) => new ProcessMaterial("Saw", Carpentry) { Verb = "Sawing" });
        //static public readonly Skill Eating = new Skill("Eating", "The art of consuming food without choking yourself", (t) => new InteractionConsumeEquipped());
        //static public readonly Skill Fertilizing = new Skill("Fertilizing", "Speed of growth of plants by careful use of fertilizer", (t) => new InteractionFertilizing());

        internal static ToolAbilityDef GetSkill(int skillID)
        {
            return Dictionary[skillID];
        }
        internal static Control GetUI(int skillID, float value)
        {
            var skill = GetSkill(skillID);
            var label = new Label(skill.Name + ": " + value.ToString());
            return label;
        }

        static ToolAbilityDef()
        {
            Register(Digging);
            Register(Building);
            Register(Mining);
            Register(Chopping);
            Register(Argiculture);
            Register(Planting);
            Register(Carpentry);
        }
    }
}
