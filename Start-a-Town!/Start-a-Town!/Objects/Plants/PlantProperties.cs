using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class PlantProperties : Def// ItemDef// EntityDef
    {
        public Sprite TextureGrowing, TextureGrown;
        public int GrowTicks;
        //public ItemDef ProductHarvest;
        public int YieldThreshold;
        public ItemDef ProductCutDown;
        //public Material PlantMaterial;
        //public int MaxYieldHarvest;
        public int MaxYieldCutDown;
        //public ItemDef PlantEntity;
        public TreeProperties Tree;
        public ShrubProperties Shrub;
        public GrowthProperties Growth;

        //public bool IsTree => this.ProductCutDown == RawMaterialDef.Logs;
        public bool IsTree => this.Tree is not null;
        public bool IsShrub => this.Shrub is not null;
        //public bool ProducesFruit => this.ProductHarvest != null;
        public bool ProducesFruit => this.Growth?.GrowthItemDef == ItemDefOf.Fruit;

        public PlantProperties(string name) : base($"Plant_{name}")
        {

        }

        static public readonly PlantProperties Berry = new PlantProperties("Berry")//"BerryBush")
        {
            //PlantEntity = PlantDefOf.Bush,
            TextureGrowing = ItemContent.BerryBushGrowing,
            TextureGrown = ItemContent.BerryBushGrown,
            Shrub = new ShrubProperties(),
            //FruitGrowTicks = 6 * Engine.TicksPerSecond,
            //ProductHarvest = ItemDefOf.Fruit,
            //PlantMaterial = MaterialDefOf.Berry,
            //MaxYieldHarvest = 5,

            Growth = new GrowthProperties(ItemDefOf.Fruit, MaterialDefOf.Berry, 5, 6)
        };
        static public readonly PlantProperties LightTree = new PlantProperties("LightTree")//"Tree")
        {
            //PlantEntity = PlantDefOf.Tree,
            //ProductHarvest = ItemDefOf.Fruit,
            //TextureGrowing = ItemContent.BerryBushGrowing,
            //TextureGrown = ItemContent.BerryBushGrown,
            Tree = new TreeProperties(MaterialDefOf.LightWood, 5),
            //ProductCutDown = RawMaterialDef.Logs,
            //MaxYieldCutDown = 5,
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
        //static Dictionary<Material, PlantProperties> All = new();
        //static void Register(PlantProperties props)
        //{
        //    All.Add(props.PlantMaterial, props);
        //}
        //static PlantProperties()
        //{
        //    Register(BerryBush);
        //    Register(Tree);
        //}
        //static public PlantProperties GetProperties(Material plantMaterial)
        //{
        //    return All[plantMaterial];
        //}
        static public void Init()
        {
            Register(Berry);
            Register(LightTree);
        }

        internal GameObject CreateSeeds()
        {
            var seeds = ItemDefOf.Seeds.Create();
            seeds.GetComp<SeedComponent>().SetPlant(this);
            return seeds;
        }
    }
}
