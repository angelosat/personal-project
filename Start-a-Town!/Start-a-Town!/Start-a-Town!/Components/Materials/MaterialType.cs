using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Materials.RawMaterials;
using Start_a_Town_.Components.Items;
using Start_a_Town_.AI;

namespace Start_a_Town_.Components.Materials
{
    public class MaterialType
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        static Dictionary<int, MaterialType> _Dictionary;
        public static Dictionary<int, MaterialType> Dictionary
        {
            get
            {
                if (_Dictionary.IsNull())
                    _Dictionary = new Dictionary<int, MaterialType>();
                return _Dictionary;
            }
        }
        public override string ToString()
        {
            return "Material:Type:" + this.Name;
        }

        static public void Initialize()
        {
            foreach (var item in Dictionary.Values)
            {
                var entities = item.Templates.GetEntities(item);
                foreach (var obj in entities)
                    GameObject.Objects.Add(obj);
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public HashSet<Material> SubTypes { get; set; }
        //public HashSet<GameObject> EntityTemplates { get; set; }
        public float Shininess;
        public CanBeMadeInto Templates { get; set; }
        public Skill SkillToExtract { get; set; }
        public AILabor Labor { get; set; }

        public MaterialType(string name, CanBeMadeInto templates)
        {
            int id = IDSequence;
            this.ID = id;
            Dictionary[id] = this;
            this.Name = name;

            this.SubTypes = new HashSet<Material>();
            this.Templates = templates;
        }
        public MaterialType(string name)
            : this(name, new CanBeMadeInto())
        {
        }

        static CanBeMadeInto CanBeMadeInto(params RawMaterial[] templates)
        {
            return new CanBeMadeInto(templates);
        }


        public abstract class RawMaterial : IItemFactory// : GameObject
        {
            const int IDRange = 40000;
            static int _IDSequence = IDRange;
            public static int IDSequence { get { return _IDSequence++; } }
            //protected int ID = IDSequence;
            public int ID { get; protected set; }
            public HashSet<CanBeMadeInto> Children = new HashSet<CanBeMadeInto>();
            protected Dictionary<Material, GameObject> Templates = new Dictionary<Material, GameObject>();
            //protected abstract GameObject Initialize(Material mat);

            public RawMaterial()
            {

            }
            protected abstract GameObject Template();

            public GameObject Initialize(Material mat)
            {
                this.ID = IDSequence;
                GameObject obj = this.Template();
                obj.Name = mat.Name + " " + obj.Name;
                Sprite sprite = new Sprite(obj.GetSprite()) { Material = mat };
                obj.GetComponent<SpriteComponent>().Sprite = sprite;
                obj.Body.Sprite = sprite;
                obj.Body.Material = mat;
                this.Templates[mat] = obj;
                return obj;
            }
            public GameObject CreateFrom(Material mat)
            {
                return this.Templates[mat].Clone();
            }
            static public GameObject Create(Material mat)
            {
                //return mat.ProcessingChain.First(obj => obj.GetComponent<MaterialsComponent>().Parts["Body"].Material == mat).Clone();
                return mat.ProcessingChain.First(obj => obj.Body.Material == mat).Clone();

            }
            
            public RawMaterial CanBeMadeInto(params RawMaterial[] children)
            {
                //this.Children = new HashSet<CanBeMadeInto>(children);
                foreach (var item in children)
                    this.Children.Add(new CanBeMadeInto(item));
                return this;
            }
            public virtual GameObject Create(List<GameObjectSlot> materials) { return GameObject.Create(this.ID); }

            public class Factory
            {
                string ReagentName;
                public Factory(string reagentName)
                {
                    this.ReagentName = reagentName;
                }
                public static GameObject GetNextInChain(GameObject obj)
                {
                    Material mat = obj.Body.Material;// obj.GetComponent<MaterialsComponent>().Parts["Body"].Material;
                    MaterialType matType = mat.Type;

                    var chain = mat.ProcessingChain;
                    var currentStep = chain.FindIndex(item => item.ID == obj.ID);
                    if (currentStep == chain.Count - 1)
                        return null;
                    var product = chain[currentStep + 1].Clone();
                    return product;
                }

                public GameObject Create(List<GameObjectSlot> materials)
                {
                    var obj = materials.First(n => n.Name == ReagentName).Object;
                    var tool = materials.FirstOrDefault(s => s.Name == "Tool");
                    var final = GetNextInChain(obj);
                    return final;
                }
                //public static GameObject Create(Material mat)
                //{
                //    MaterialType matType = mat.Type;

                //    var chain = mat.ProcessingChain;
                //    var currentStep = chain.FindIndex(item => item.Body.Material == mat);
                //    if (currentStep == chain.Count - 1)
                //        return null;
                //    var product = chain[currentStep].Clone();
                //    return product;
                //}
                //public GameObject Get(MaterialType mat)
                //{
                //    var obj = materials.First(n => n.Name == ReagentName).Object;
                //    var tool = materials.FirstOrDefault(s => s.Name == "Tool");
                //    var final = GetNextInChain(obj);
                //    return final;
                //}
            }
        }

        static public readonly RawMaterial Bag = new Bags();
        static public readonly RawMaterial Logs = new Logs();
        static public readonly RawMaterial Planks = new Planks();
        static public readonly RawMaterial Ore = new Ore();
        static public readonly RawMaterial Rocks = new Rocks();
        static public readonly RawMaterial Bars = new Bars();
     
        //static public readonly MaterialType Wood = new MaterialType("Wood", CanBeMadeInto(Logs, Planks)) { SkillToExtract = Skill.Chopping };
        static public readonly MaterialType Twig = new MaterialType("Twig"){ Labor = AILabor.Lumberjack };
        static public readonly MaterialType Wood = new MaterialType("Wood", CanBeMadeInto(Logs.CanBeMadeInto(Planks))) { SkillToExtract = Skill.Chopping, Shininess = .8f, Labor = AILabor.Lumberjack };
        static public readonly MaterialType Soil = new MaterialType("Soil", CanBeMadeInto(Bag)) { SkillToExtract = Skill.Digging, Labor = AILabor.Digger };
        static public readonly MaterialType Sand = new MaterialType("Sand", CanBeMadeInto(Bag)) { SkillToExtract = Skill.Digging, Labor = AILabor.Digger };
        static public readonly MaterialType Mineral = new MaterialType("Mineral", CanBeMadeInto(Rocks)) { SkillToExtract = Skill.Mining, Labor = AILabor.Miner };
        //static public readonly MaterialType Stone = new MaterialType("Stone", CanBeMadeInto(Rock)) { SkillToExtract = Skill.Mining };
        //static public readonly MaterialType Metal = new MaterialType("Metal", CanBeMadeInto(Ore, Bars)) { SkillToExtract = Skill.Mining };
        static public readonly MaterialType Metal = new MaterialType("Metal", CanBeMadeInto(Ore.CanBeMadeInto(Bars))) { SkillToExtract = Skill.Mining, Labor = AILabor.Miner, Shininess = 1 };
        static public readonly MaterialType Gas = new MaterialType("Gas");
        static public readonly MaterialType Liquid = new MaterialType("Liquid");
        static public readonly MaterialType Dye = new MaterialType("Dye");
        static public readonly MaterialType Glass = new MaterialType("Glass");
    }
}
