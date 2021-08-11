using System.Collections.Generic;

namespace Start_a_Town_
{
    public sealed class MaterialType : Def
    {
        //static int _idSequence = 0;
        //public static int IdSequence { get { return _idSequence++; } }
        public static Dictionary<int, MaterialType> Dictionary = new();
        public static Dictionary<int, MaterialType> DictionaryByHash = new();

        public ReactionClass ReactionClass;
        public readonly MaterialCategory Category;
        //public int ID;
        public string Label;
        public readonly int Hash;
        public HashSet<MaterialDef> SubTypes = new();
        public float Shininess;
        public ToolUseDef SkillToExtract;

        static public MaterialType GetMaterialType(int id)
        {
            return Dictionary[id];
        }
        public override string ToString()
        {
            return "Material:Type:" + this.Name;
        }
       
        public MaterialType(string name) : base($"MaterialType:{name}")
        {
            //int id = IdSequence;
            //this.ID = id;
            //Dictionary[id] = this;
            //this.Name = name;
            this.Label = name;
            this.Hash = name.GetHashCode();
            DictionaryByHash[this.Hash] = this;
            this.SubTypes = new HashSet<MaterialDef>();
        }
        public MaterialType(string name, MaterialCategory category)
            : this(name)
        {
            this.Category = category;
        }

        static public readonly MaterialType Soil = new("Soil", MaterialCategory.Inorganic) { SkillToExtract = ToolUseDef.Digging };
        static public readonly MaterialType Stone = new("Stone", MaterialCategory.Inorganic) { SkillToExtract = ToolUseDef.Mining };
        static public readonly MaterialType Metal = new("Metal", MaterialCategory.Inorganic) { ReactionClass = ReactionClass.Tools,  SkillToExtract = ToolUseDef.Mining };
        static public readonly MaterialType Gas = new("Gas", MaterialCategory.Inorganic);
        static public readonly MaterialType Water = new("Water", MaterialCategory.Inorganic);
        static public readonly MaterialType Glass = new("Glass", MaterialCategory.Inorganic);

        static public readonly MaterialType Meat = new("Meat", MaterialCategory.Creature) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialType Blood = new("Blood", MaterialCategory.Creature);
        static public readonly MaterialType Bone = new("Bone", MaterialCategory.Creature);

        static public readonly MaterialType Fruit = new("Fruit", MaterialCategory.Plant) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialType Dye = new("Dye", MaterialCategory.Plant);
        static public readonly MaterialType Wood = new("Wood", MaterialCategory.Plant) { ReactionClass = ReactionClass.Tools, SkillToExtract = ToolUseDef.Chopping, Shininess = .8f };
        static public readonly MaterialType PlantStem = new("PlantStem", MaterialCategory.Plant);
        static public readonly MaterialType Seed = new("Seed", MaterialCategory.Plant);

        public void AddMaterial(MaterialDef mat)
        {
            mat.Type = this;
            this.SubTypes.Add(mat);
        }
    }
}
