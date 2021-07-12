using System;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class PlantProperties : Def
    {
        public Sprite TextureGrowing, TextureGrown;
        public int GrowTicks;
        public int YieldThreshold;
        public ItemDef ProductCutDown;
        public int MaxYieldCutDown;
        public TreeProperties Tree;
        public ShrubProperties Shrub;
        public GrowthProperties Growth;

        public bool IsTree => this.Tree is not null;
        public bool IsShrub => this.Shrub is not null;
        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;

        public PlantProperties(string name) : base($"Plant_{name}")
        {

        }

        static public readonly PlantProperties Berry = new PlantProperties("Berry")
        {
            TextureGrowing = ItemContent.BerryBushGrowing,
            TextureGrown = ItemContent.BerryBushGrown,
            Shrub = new ShrubProperties(),
            Growth = new GrowthProperties(ItemDefOf.Fruit, MaterialDefOf.Berry, 5, 6)
        };

        static public readonly PlantProperties LightTree = new PlantProperties("LightTree")
        {
            TextureGrowing = ItemContent.TreeFull,
            TextureGrown = ItemContent.TreeFull,
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
            Register(Berry);
            Register(LightTree);
        }

        internal GameObject CreateSeeds()
        {
            var seeds = ItemDefOf.Seeds.Create();
            seeds.GetComponent<SeedComponent>().SetPlant(this);
            return seeds;
        }
    }
}
