using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class MaterialDefOf
    {
        internal static void Init()
        {
        }

        static public readonly MaterialDef Iron = new MaterialDef("Iron", MaterialTemplates.Metal)
            .SetPrefix("Iron")
            .SetColor(Color.LightSteelBlue)
            .SetValue(20);
        static public readonly MaterialDef Gold = new MaterialDef("Gold", MaterialTemplates.Metal)
            .SetPrefix("Golden")
            .SetColor(Color.Gold)
            .SetValue(100);


        static public readonly MaterialDef Coal = new(MaterialType.Stone, "Coal", "Coal", Color.DimGray, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), WorkToBreak = 25 };
        static public readonly MaterialDef Stone = new(MaterialType.Stone, "Stone", "Stone", Color.White, 80) { ValueBase = 5, WorkToBreak = 20 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite

        static public readonly MaterialDef ShrubStem = new("Twig", MaterialTemplates.PlantStem);

        static public readonly MaterialDef LightWood = new MaterialDef(MaterialTemplates.Wood)
            .SetName("Light Wood")
            .SetPrefix("Light Wood")
            .SetColor(Color.SandyBrown)
            .SetValue(5);
        static public readonly MaterialDef DarkWood = new MaterialDef(MaterialTemplates.Wood)
            .SetName("Dark Wood")
            .SetPrefix("Dark Wood")
            .SetColor(Color.SaddleBrown)
            .SetValue(10);
        static public readonly MaterialDef RedWood = new MaterialDef(MaterialTemplates.Wood)
            .SetName("Red Wood")
            .SetPrefix("Red Wood")
            .SetColor(Color.Brown)
            .SetValue(20);


        static public readonly MaterialDef Soil = new(MaterialType.Soil, "Soil", "Dirt", Color.SandyBrown, 20) { ValueBase = 2, WorkToBreak = 20 };
        static public readonly MaterialDef Sand = new(MaterialType.Soil, "Sand", "Sand", Color.BlanchedAlmond, 10) { ValueBase = 2 };


        static public readonly MaterialDef Air = new(MaterialType.Gas, "Air", "Air", 0);
        // basalt? new Color(120, 109, 95, 255)
        static public readonly MaterialDef Water = new(MaterialType.Water, "Water", "Water", Color.SeaGreen, 5);
        static public readonly MaterialDef Glass = new(MaterialType.Glass, "Glass", "Glass", Color.White, 40);

        static public readonly MaterialDef Human = 
            new MaterialDef(MaterialTemplates.Meat)
            .SetName("Human")
            .SetPrefix("Human")
            .SetValue(20);
        static public readonly MaterialDef Animal = 
            new MaterialDef(MaterialTemplates.Meat)
            .SetName("Animal")
            .SetPrefix("Animal")
            .SetValue(20);
        static public readonly MaterialDef Insect = 
            new MaterialDef(MaterialTemplates.Meat)
            .SetName("Insect")
            .SetPrefix("Insect")
            .SetValue(20);

        static public readonly MaterialDef Berry = 
            new MaterialDef(MaterialTemplates.Fruit)
            .SetName("Berry")
            .SetPrefix("Berry")
            .SetColor(new Color(141, 78, 133));
        static public readonly MaterialDef Seed = new("Seed", MaterialTemplates.Seed);

        static MaterialDefOf()
        {
            Def.Register(Iron);
            Def.Register(Gold);

            Def.Register(LightWood);
            Def.Register(DarkWood);
            Def.Register(RedWood);

            Def.Register(Coal);
            Def.Register(Stone);

            Def.Register(ShrubStem);

            Def.Register(Soil);
            Def.Register(Sand);

            Def.Register(Air);
            Def.Register(Water);
            Def.Register(Glass);

            Def.Register(Human);
            Def.Register(Animal);
            Def.Register(Insect);

            Def.Register(Berry);
            Def.Register(Seed);
        }
    }
}
