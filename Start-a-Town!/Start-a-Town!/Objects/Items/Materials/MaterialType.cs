using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    public partial class MaterialType
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        static Dictionary<int, MaterialType> _Dictionary;
        public static Dictionary<int, MaterialType> Dictionary
        {
            get
            {
                if (_Dictionary == null)
                    _Dictionary = new Dictionary<int, MaterialType>();
                return _Dictionary;
            }
        }
        static public MaterialType GetMaterialType(int id)
        {
            return Dictionary[id];
        }
        public override string ToString()
        {
            return "Material:Type:" + this.Name;
        }
        public ReactionClass ReactionClass;
        public readonly MaterialCategory Category;
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

        public int ID;
        public string Name;
        public HashSet<Material> SubTypes = new();
        //public HashSet<GameObject> EntityTemplates { get; set; }
        public float Shininess;
        public bool EdibleRaw, EdibleCooked;
        public CanBeMadeInto Templates;
        public ToolAbilityDef SkillToExtract;
        public JobDef Labor;
        public RawMaterialDef ExtractedItemDef;
        public MaterialType(string name, CanBeMadeInto templates)
        {
            int id = IDSequence;
            this.ID = id;
            Dictionary[id] = this;
            this.Name = name;

            this.SubTypes = new HashSet<Material>();
            this.Templates = templates;
        }
        public MaterialType(string name, MaterialCategory category)
            : this(name, new CanBeMadeInto())
        {
            this.Category = category;
        }

        static CanBeMadeInto CanBeMadeInto(params RawMaterial[] templates)
        {
            return new CanBeMadeInto(templates);
        }



        static public readonly MaterialType Soil = new("Soil", MaterialCategory.Inorganic) { SkillToExtract = ToolAbilityDef.Digging, Labor = JobDefOf.Digger }; //ExtractedItemDef = RawMaterialDef.Bags, 
        //static public readonly MaterialType Sand = new MaterialType("Sand") { SkillToExtract = ToolAbilityDef.Digging, Labor = AILabor.Digger }; //ExtractedItemDef = RawMaterialDef.Bags, 
        static public readonly MaterialType Stone = new("Stone", MaterialCategory.Inorganic) { SkillToExtract = ToolAbilityDef.Mining, Labor = JobDefOf.Miner }; // ExtractedItemDef = RawMaterialDef.Rock,
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
        static public readonly MaterialType PlantStem = new("PlantStem", MaterialCategory.Plant);// { Labor = JobDefOf.Lumberjack };
        static public readonly MaterialType Seed = new("Seed", MaterialCategory.Plant);// { Labor = JobDefOf.Lumberjack };

        //public Material CreateMaterial()
        //{
        //    var mat = new Material();
        //    this.AddMaterial(mat);
        //    return mat;
        //}
        public void AddMaterial(Material mat)
        {
            mat.Type = this;
            this.SubTypes.Add(mat);
        }
    }
}
