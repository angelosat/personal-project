using System;
using System.Xml;
using System.Xml.Serialization;

namespace Start_a_Town_
{
    public class PlantProperties : Def
    {
        public struct Graphics
        {
            public string Growing, Grown;

            public Graphics(string textureGrowing, string textureGrown)
            {
                this.Growing = textureGrowing;
                this.Grown = textureGrown;
            }
        }
        //public string TextureGrowing, TextureGrown;
        [XmlIgnore]
        public string TextureGrown
        {
            get => this.Textures.Growing;
            set => this.Textures.Growing = value;
        }
        [XmlIgnore]
        public string TextureGrowing
        {
            get => this.Textures.Grown;
            set => this.Textures.Grown = value;
        }
        public Graphics Textures;
        public int GrowTicks;
        public int YieldThreshold;
        public int MaxYieldCutDown;

        [XmlIgnore]
        public ItemDef ProductCutDown;

        [XmlIgnore]
        public TreeProperties Tree;

        [XmlIgnore]
        public ShrubProperties Shrub;

        public GrowthProperties Growth;

        public bool IsTree => this.Tree is not null;
        public bool IsShrub => this.Shrub is not null;
        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;
        public PlantProperties()
        {

        }
        public PlantProperties(string name) : base($"Plant_{name}")
        {

        }

        static public readonly PlantProperties Berry = new PlantProperties("Berry")
        {
            TextureGrowing = ItemContent.BerryBushGrowing.AssetPath,
            TextureGrown = ItemContent.BerryBushGrown.AssetPath,
            Shrub = new ShrubProperties(),
            Growth = new GrowthProperties(ItemDefOf.Fruit, MaterialDefOf.Berry, 5, 6)
        };

        static public readonly PlantProperties LightTree = new PlantProperties("LightTree")
        {
            TextureGrowing = ItemContent.TreeFull.AssetPath,
            TextureGrown = ItemContent.TreeFull.AssetPath,
            Tree = new TreeProperties(MaterialDefOf.LightWood, 5),
            ProductCutDown = RawMaterialDef.Logs,
            MaxYieldCutDown = 5,
            GrowTicks = 6 * Engine.TicksPerSecond,
        };

        public ItemDef PlantEntity
        {
            get
            {
                if (this.IsTree)
                    return PlantDefOf.Tree;
                else if (this.IsShrub)
                    return PlantDefOf.Bush;
                else 
                    throw new Exception();
            }
        }
        public Plant CreatePlant()
        {
            var entity = this.PlantEntity.Create() as Plant;
            entity.PlantComponent.PlantProperties = this;
            if (this.IsTree)
                entity.SetMaterial(this.Tree.Material);
            return entity;
        }
        
        static public void Init()
        {
            var ser = new XmlSerializer(typeof(PlantProperties));
            var path = $"{GlobalVars.SaveDir}/{Berry.Name}.xml";
            //var lightTreeXml = new XmlDocument();
            //lightTreeXml.Load(path);
            //var lightTree = FromXml<PlantProperties>(lightTreeXml.FirstChild);

            Register(Berry);
            Register(LightTree);

            
            System.IO.FileStream file = System.IO.File.Create(path);
            ser.Serialize(file, Berry);
            file.Close();
        }

        internal GameObject CreateSeeds()
        {
            var seeds = ItemDefOf.Seeds.Create();
            seeds.GetComponent<SeedComponent>().SetPlant(this);
            return seeds;
        }

        static T FromXml<T>(XmlNode xmlRoot) where T : new()
        {
            var obj = new T();

            return obj;
        }
    }
}
