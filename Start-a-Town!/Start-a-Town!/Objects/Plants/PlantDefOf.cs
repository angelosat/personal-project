﻿using Start_a_Town_.Components;
using System.Linq;

namespace Start_a_Town_
{
    public static class PlantDefOf
    {
        static public ItemDef Tree = new("Tree")
        {
            ItemClass = typeof(Plant),
            Description = "A lovely tree",
            Height = 4,
            Weight = 100,
            Haulable = false,
            DefaultMaterial = MaterialDefOf.LightWood,
            Body = new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull),
            //Body = new Bone(BoneDefOf.TreeTrunk, ItemContent.TreeFull).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true })
            Size = ObjectSize.Haulable
        };

        static public ItemDef Bush = new("Bush")
        {
            ItemClass = typeof(Plant),
            Description = "A lovely fluffy bush.",
            Height = 1,
            Weight = 5,
            Haulable = false,
            DefaultMaterial = MaterialDefOf.ShrubStem,
            Body = new Bone(BoneDefOf.PlantStem, ItemContent.BerryBushGrowing).AddJoint(new Bone(BoneDefOf.PlantFruit) { DrawMaterialColor = true }),
            Size = ObjectSize.Haulable
        };
        static PlantDefOf()
        {
        }
        static public void Init() 
        {
            Def.Register(Tree);
            Def.Register(Bush);

            var bush = PlantProperties.Berry.CreatePlant();
            bush.GetComponent<PlantComponent>().SetProperties(PlantProperties.Berry);
            bush.GrowthBody = 1;
            bush.GrowthFruit = 1;
            GameObject.AddTemplate(bush);

            var tree = PlantProperties.LightTree.CreatePlant();
            tree.GrowthBody = 1;
            GameObject.AddTemplate(tree);

            //GameObject.AddTemplate(PlantProperties.Berry.CreateSeeds());
            var allPlants = Def.GetDefs<PlantProperties>();
            GameObject.AddTemplates(allPlants.Select(p => p.CreateSeeds()));


            Def.Register(new Reaction("Extract Seeds", SkillDefOf.Argiculture)
                .AddBuildSite(IsWorkstation.Types.PlantProcessing)
                .AddIngredient("a", new Ingredient()
                    .SetAllow(ItemDefOf.Fruit, true))
                //.AddProduct(new Reaction.Product(ItemDefOf.Seeds, 4)
                //    .GetMaterialFromIngredient("a"))
                .AddProduct(new Reaction.Product(i => Def.GetDefs<PlantProperties>().First(d => d.FruitMaterial == i["a"].PrimaryMaterial).CreateSeeds() as Entity, 4))
                );
        }
    }
}
