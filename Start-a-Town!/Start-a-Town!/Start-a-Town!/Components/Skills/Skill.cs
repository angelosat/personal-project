using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components.Skills
{
    public class Skill
    {
        //static int _IDSequence = 20000;
        static int _IDSequence = "skills".GetHashCode() >> 2;// 20000;
        public static int IDSequence { get { return _IDSequence++; } }

        static Dictionary<int, Skill> _Dictionary;
        public static Dictionary<int, Skill> Dictionary
        {
            get
            {
                if (_Dictionary.IsNull())
                    _Dictionary = new Dictionary<int, Skill>();
                return _Dictionary;
            }
        }

        public int ID { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        //public Work Work { get; private set; }
        Func<TargetArgs, Interaction> WorkFactory = (t) => null;

        public Skill()
        {

        }

        protected Skill(string name, string description) : this(name, description, null) { }
        //Skill(string name, string description, Work work)
        protected Skill(string name, string description, Func<TargetArgs, Interaction> workFactory)
        {
            this.ID = IDSequence;
            Dictionary[this.ID] = this;
            this.Name = name;
            this.Description = description;
            //this.Work = work;
            this.WorkFactory = workFactory;
        }

        public virtual Interaction GetWork(GameObject actor, TargetArgs target)
        {
            if (this.WorkFactory == null)
                return null;
            return this.WorkFactory(target);
        }
        public virtual Interaction GetWork()
        {
            if (this.WorkFactory == null)
                return null;
            return this.WorkFactory(TargetArgs.Empty);
        }
        static public readonly Skill Digging = new Skill("Digging", "Dig up soil and dirt blocks", (t) => new InteractionDigging());
        //static public readonly Skill Building = new Skill("Building", "Build blocks and other structures");//, () => new BuildStructure());
        static public readonly Skill Building = new SkillBuilding();
            //new Skill("Building", "Build blocks and other structures", (t) =>
            //{
            //    if (t.Object.HasComponent<StructureComponent>())
            //        return new BuildStructure();
            //    else if (t.Object.HasComponent<ConstructionComponent>())
            //        return new BuildBlock();
            //    return null;
            //});
        static public readonly Skill Mining = new Skill("Mining", "Dig up stone blocks", (t) => new Mining());
        //static public readonly Skill Chopping = new Skill("Chopping", "Chop down trees and enemies with axes", (t) => new ProcessMaterial("Chop", Chopping) { Verb = "Chopping" });
        static public readonly Skill Chopping = new Skill("Chopping", "Chop down trees and enemies with axes", (t) => new Chopping());

        static public readonly Skill Argiculture = new SkillArgiculture();// new Skill("Argiculture", "Helps determine type and growth time of plants", (t) => new Tilling());
        static public readonly Skill Planting = new Skill("Planting", "Planting plants", (t) => new Planting());
        static public readonly Skill Carpentry = new Skill("Carpentry", "The craft of converting wood to useful equipment", (t) => new ProcessMaterial("Saw", Carpentry) { Verb = "Sawing" });
        //static public readonly Skill Eating = new Skill("Eating", "The art of consuming food without choking yourself", (t) => new InteractionConsumeEquipped());
        //static public readonly Skill Fertilizing = new Skill("Fertilizing", "Speed of growth of plants by careful use of fertilizer", (t) => new InteractionFertilizing());
    }
}
