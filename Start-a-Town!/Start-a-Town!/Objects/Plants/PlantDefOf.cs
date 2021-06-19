using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public static class PlantDefOf
    {
        //static public PlantDef Tree = new PlantDef("Tree")
        //{
        //    ItemClass = typeof(Plant),
        //    Description = "A lovely tree",
        //    Height = 4,
        //    Weight = 100,
        //    Factory = Plant.CreateTree,
        //    Body = new Bone(BoneDef.Item, ItemContent.TreeFull) { ScaleFunc = o=>o.GetComponent<TreeComponent>().Growth.Percentage }
        //};
        //static public PlantDef BerryBush = new PlantDef("BerryBush")
        //{
        //    ItemClass = typeof(Plant),
        //    Description = "A lovely berry bush",
        //    Height = 1,
        //    Weight = 5,
        //    Factory = Plant.CreateBush,
        //    TextureGrowing = ItemContent.BerryBushGrowing,
        //    TextureGrown = ItemContent.BerryBushGrown,
        //    FruitGrowTicks = 6 * Engine.TicksPerSecond,
        //    Body = new Bone(BoneDef.Item, ItemContent.BerryBushGrowing)// { ScaleFunc = o => o.GetComponent<PlantComponent().Growth.Percentage }//{ ScaleFunc = o => o.GetComponent<PlantComponentNew>().Growth.Percentage }
        //};
        static public ItemDef Tree = new("Tree")
        {
            ItemClass = typeof(Plant),
            Description = "A lovely tree",
            Height = 4,
            Weight = 100,
            Haulable = false,
            //PlantProperties = PlantProperties.Tree,
            DefaultMaterial = MaterialDefOf.LightWood,
            Body = new Bone(BoneDef.TreeTrunk, ItemContent.TreeFull),// { Material = Material.LightWood }// { ScaleFunc = o => o.GetComponent<TreeComponent>().Growth.Percentage }
            //CompProps = new List<ComponentProps>() { new TreeComponent.Props() }
            CompProps = new List<ComponentProps>() { new PlantComponent.Props() }
        };
        static public ItemDef Bush = new("BerryBush")
        {
            ItemClass = typeof(Plant),
            Description = "A lovely berry bush",
            Height = 1,
            Weight = 5,
            Haulable = false,
            //Factory = Plant.CreateBush,
            //PlantProperties = PlantProperties.BerryBush,
            DefaultMaterial = MaterialDefOf.ShrubStem,// MaterialDefOf.Twig,
            Body = new Bone(BoneDef.PlantStem, ItemContent.BerryBushGrowing),// { ScaleFunc = o => o.GetComponent<PlantComponent().Growth.Percentage }//{ ScaleFunc = o => o.GetComponent<PlantComponentNew>().Growth.Percentage }
            CompProps = new List<ComponentProps>() { new PlantComponent.Props() }
        };
        static PlantDefOf()
        {
            //Def.Register(Tree);
            //Def.Register(BerryBush);

            //var bush = BerryBush.Create();
            //bush.GetComponent<PlantComponent>().GrowthBody.Percentage = 1;
            //GameObject.AddTemplate(bush);
            
            //var tree = Tree.Create();
            ////tree.GetComponent<TreeComponent>().GrowthNew.Percentage = 1;
            //tree.GetComponent<PlantComponent>().GrowthBody.Percentage = 1;
            //GameObject.AddTemplate(tree);

            ////GameObject.AddTemplate(ItemFactory.CreateSeeds(PlantDefOf.BerryBush));
            //GameObject.AddTemplate(ItemFactory.CreateSeeds(PlantProperties.BerryBush));

            ////GameObject.AddTemplate(ItemDefOf.Seeds.CreateFrom(MaterialDefOf.Berry));


        }
        static public void Init() 
        {
            Def.Register(Tree);
            Def.Register(Bush);

            //var bush = Bush.Create();
            var bush = PlantProperties.Berry.CreatePlant();
            bush.GetComponent<PlantComponent>().SetProperties(PlantProperties.Berry);
            bush.GetComponent<PlantComponent>().GrowthBody.Percentage = 1;
            GameObject.AddTemplate(bush);

            //var seed = ItemDefOf.Seeds.Create();
            //seed.GetComp<SeedComponent>().Plant = PlantProperties.Berry;
            //GameObject.AddTemplate(seed);

            //var tree = Tree.Create();
            var tree = PlantProperties.LightTree.CreatePlant();
            //tree.GetComponent<TreeComponent>().GrowthNew.Percentage = 1;
            tree.GetComponent<PlantComponent>().GrowthBody.Percentage = 1;
            GameObject.AddTemplate(tree);

            //GameObject.AddTemplate(ItemFactory.CreateSeeds(PlantDefOf.BerryBush));
            //GameObject.AddTemplate(ItemFactory.CreateSeeds(PlantProperties.Berry));
            GameObject.AddTemplate(PlantProperties.Berry.CreateSeeds());

            //GameObject.AddTemplate(ItemDefOf.Seeds.CreateFrom(MaterialDefOf.Berry));
        }
    }
}
