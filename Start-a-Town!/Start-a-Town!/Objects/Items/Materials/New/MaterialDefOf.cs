using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class MaterialDefOf
    {
        internal static void Init()
        {
        }

        static public readonly Material Iron = new Material("Iron", MaterialTemplates.Metal)
            .SetPrefix("Iron")
            .SetColor(Color.LightSteelBlue)
            .SetValue(20);
        static public readonly Material Gold = new Material("Gold", MaterialTemplates.Metal)
            .SetPrefix("Golden")
            .SetColor(Color.Gold)
            .SetValue(100);


        static public readonly Material Coal = new(MaterialType.Stone, "Coal", "Coal", Color.DimGray, 100) { ValueBase = 1, Fuel = new Fuel(FuelDef.Organic, 20f), WorkToBreak = 25 };
        static public readonly Material Stone = new(MaterialType.Stone, "Stone", "Stone", Color.White, 80) { ValueBase = 5, WorkToBreak = 20 };//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite

        static public readonly Material ShrubStem = new("Twig", MaterialTemplates.PlantStem);

        static public readonly Material LightWood = new Material(MaterialTemplates.Wood)
            .SetName("Light Wood")
            .SetPrefix("Light Wood")
            .SetColor(Color.SandyBrown)
            .SetValue(5);
        static public readonly Material DarkWood = new Material(MaterialTemplates.Wood)
            .SetName("Dark Wood")
            .SetPrefix("Dark Wood")
            .SetColor(Color.SaddleBrown)
            .SetValue(10);
        static public readonly Material RedWood = new Material(MaterialTemplates.Wood)
            .SetName("Red Wood")
            .SetPrefix("Red Wood")
            .SetColor(Color.Brown)
            .SetValue(20);


        static public readonly Material Soil = new(MaterialType.Soil, "Soil", "Dirt", Color.SandyBrown, 20) { ValueBase = 2, WorkToBreak = 20 };
        static public readonly Material Sand = new(MaterialType.Soil, "Sand", "Sand", Color.BlanchedAlmond, 10) { ValueBase = 2 };


        static public readonly Material Air = new(MaterialType.Gas, "Air", "Air", 0);
        // basalt? new Color(120, 109, 95, 255)
        static public readonly Material Water = new(MaterialType.Water, "Water", "Water", Color.SeaGreen, 5);
        static public readonly Material Glass = new(MaterialType.Glass, "Glass", "Glass", Color.White, 40);

        static public readonly Material Human = 
            new Material(MaterialTemplates.Meat)
            .SetName("Human")
            .SetPrefix("Human")
            .SetValue(20);
        static public readonly Material Animal = 
            new Material(MaterialTemplates.Meat)
            .SetName("Animal")
            .SetPrefix("Animal")
            .SetValue(20);
        static public readonly Material Insect = 
            new Material(MaterialTemplates.Meat)
            .SetName("Insect")
            .SetPrefix("Insect")
            .SetValue(20);

        static public readonly Material Berry = 
            new Material(MaterialTemplates.Fruit)
            .SetName("Berry")
            .SetPrefix("Berry")
            .SetColor(new Color(141, 78, 133));
        static public readonly Material Seed = new("Seed", MaterialTemplates.Seed);

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
