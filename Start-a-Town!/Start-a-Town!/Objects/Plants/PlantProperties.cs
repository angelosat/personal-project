using System;
using System.Collections.Generic;
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

        [Obsolete]
        ///instead of using cutdowndifficulty, determine cutdown hitpoints by stem material density
        public int CutDownDifficulty = 1;

        [XmlIgnore]
        public MaterialDef StemMaterial;
        [XmlIgnore]
        public ItemDef PlantEntity;
        [XmlIgnore]
        public ItemDef ProductCutDown;
     
        public GrowthProperties Growth;
        internal ToolUseDef ToolToCut;

        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;


        public PlantProperties()
        {

        }
        public PlantProperties(string name) : base($"Plant_{name}")
        {

        }
        public int GetCutDownHitPonts(GameObject plant) => (int)(this.StemMaterial.Density * plant.TotalWeight / 5f);

        static public readonly PlantProperties Berry = new("Berry")
        {
            TextureGrowing = ItemContent.BerryBushGrowing.AssetPath,
            TextureGrown = ItemContent.BerryBushGrown.AssetPath,
            StemMaterial = MaterialDefOf.ShrubStem,
            Growth = new GrowthProperties(ItemDefOf.Fruit, MaterialDefOf.Berry, 5, 6),
            CutDownDifficulty = 3,
            PlantEntity = PlantDefOf.Bush,
        };

        static public readonly PlantProperties LightTree = new("LightTree")
        {
            TextureGrowing = ItemContent.TreeFull.AssetPath,
            TextureGrown = ItemContent.TreeFull.AssetPath,
            StemMaterial = MaterialDefOf.LightWood,
            ProductCutDown = RawMaterialDef.Logs,
            MaxYieldCutDown = 5,
            CutDownDifficulty = 10,
            GrowTicks = 6 * Engine.TicksPerSecond,
            PlantEntity = PlantDefOf.Tree,
            ToolToCut = ToolUseDef.Chopping
        };

        public Plant CreatePlant()
        {
            var entity = this.PlantEntity.Create() as Plant;
            entity.PlantComponent.PlantProperties = this;
            if (this.PlantEntity == PlantDefOf.Tree)
                entity.SetMaterial(this.StemMaterial);
            return entity;
        }

        static public void Init()
        {
            var ser = new XmlSerializer(typeof(List<PlantProperties>));

            var path = $"{GlobalVars.SaveDir}/{Berry.Name}.xml";

            Register(Berry);
            Register(LightTree);


            System.IO.FileStream file = System.IO.File.Create(path);
            var list = new List<PlantProperties>(GetDefs<PlantProperties>());
            ser.Serialize(file, list);
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
