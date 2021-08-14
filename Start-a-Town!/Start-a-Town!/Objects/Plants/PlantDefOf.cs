using Start_a_Town_.Components;

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
        };

        static public ItemDef Bush = new("BerryBush")
        {
            ItemClass = typeof(Plant),
            Description = "A lovely berry bush",
            Height = 1,
            Weight = 5,
            Haulable = false,
            DefaultMaterial = MaterialDefOf.ShrubStem,
            Body = new Bone(BoneDefOf.PlantStem, ItemContent.BerryBushGrowing).AddJoint(new Bone(BoneDefOf.PlantFruit))
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
            bush.Growth = 1;
            GameObject.AddTemplate(bush);

            var tree = PlantProperties.LightTree.CreatePlant();
            tree.Growth = 1;
            GameObject.AddTemplate(tree);
            GameObject.AddTemplate(PlantProperties.Berry.CreateSeeds());
        }
    }
}
