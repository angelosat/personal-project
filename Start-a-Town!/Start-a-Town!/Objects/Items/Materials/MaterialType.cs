using System.Collections.Generic;

namespace Start_a_Town_
{
    public partial class MaterialType
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        static Dictionary<int, MaterialType> _Dictionary;
        public static Dictionary<int, MaterialType> Dictionary => _Dictionary ??= new Dictionary<int, MaterialType>();
        public ReactionClass ReactionClass;
        public readonly MaterialCategory Category;
        public int ID;
        public string Name;
        public HashSet<Material> SubTypes = new();
        public float Shininess;
        public bool EdibleRaw, EdibleCooked;
        public ToolAbilityDef SkillToExtract;
        public JobDef Labor;
        public RawMaterialDef ExtractedItemDef;

        static public MaterialType GetMaterialType(int id)
        {
            return Dictionary[id];
        }
        public override string ToString()
        {
            return "Material:Type:" + this.Name;
        }
        static public void Initialize()
        {
            foreach (var item in Dictionary.Values)
            {
                var current = item.ExtractedItemDef;
                while (current != null)
                {
                    foreach (var mat in item.SubTypes)
                    {
                        var prototype = current.CreateFrom(mat);
                        GameObject.AddTemplate(prototype);
                    }
                    current = current.CanBeProcessedInto;
                }
            }
            return;
        }
       
        public MaterialType(string name)
        {
            int id = IDSequence;
            this.ID = id;
            Dictionary[id] = this;
            this.Name = name;

            this.SubTypes = new HashSet<Material>();
        }
        public MaterialType(string name, MaterialCategory category)
            : this(name)
        {
            this.Category = category;
        }

        static public readonly MaterialType Soil = new("Soil", MaterialCategory.Inorganic) { SkillToExtract = ToolAbilityDef.Digging, Labor = JobDefOf.Digger };
        static public readonly MaterialType Stone = new("Stone", MaterialCategory.Inorganic) { SkillToExtract = ToolAbilityDef.Mining, Labor = JobDefOf.Miner };
        static public readonly MaterialType Metal = new("Metal", MaterialCategory.Inorganic) { ReactionClass = ReactionClass.Tools,  SkillToExtract = ToolAbilityDef.Mining, Labor = JobDefOf.Miner, Shininess = 1 }; //ExtractedItemDef = RawMaterialDef.Ore,
        static public readonly MaterialType Gas = new("Gas", MaterialCategory.Inorganic);
        static public readonly MaterialType Water = new("Water", MaterialCategory.Inorganic);
        static public readonly MaterialType Glass = new("Glass", MaterialCategory.Inorganic);

        static public readonly MaterialType Meat = new("Meat", MaterialCategory.Creature) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialType Blood = new("Blood", MaterialCategory.Creature);
        static public readonly MaterialType Bone = new("Bone", MaterialCategory.Creature);

        static public readonly MaterialType Fruit = new("Fruit", MaterialCategory.Plant) { ReactionClass = ReactionClass.Protein };
        static public readonly MaterialType Dye = new("Dye", MaterialCategory.Plant);
        static public readonly MaterialType Wood = new("Wood", MaterialCategory.Plant) { ReactionClass = ReactionClass.Tools, SkillToExtract = ToolAbilityDef.Chopping, Shininess = .8f, Labor = JobDefOf.Lumberjack }; //ExtractedItemDef = RawMaterialDef.Logs,
        static public readonly MaterialType PlantStem = new("PlantStem", MaterialCategory.Plant);
        static public readonly MaterialType Seed = new("Seed", MaterialCategory.Plant);

        public void AddMaterial(Material mat)
        {
            mat.Type = this;
            this.SubTypes.Add(mat);
        }
    }
}
