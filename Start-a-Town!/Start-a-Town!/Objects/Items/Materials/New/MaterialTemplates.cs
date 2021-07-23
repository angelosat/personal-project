﻿using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class MaterialTemplates
    {
        static public readonly MaterialDef Fruit = new MaterialDef()
            { 
                EdibleRaw = true,
                EdibleCooked = true,
                State = MaterialState.Solid,
                Density = 2
            }.SetType(MaterialType.Fruit)
            ;

        static public readonly MaterialDef Wood = new MaterialDef()
            { Fuel = new Fuel(FuelDef.Organic, 20f) }
            .SetState(MaterialState.Solid)
            .SetDensity(50)
            .SetType(MaterialType.Wood)
            ;

        static public readonly MaterialDef Metal = new MaterialDef()
            .SetState(MaterialState.Solid)
            .SetDensity(200)
            .SetReflectiveness(1)
            .SetType(MaterialType.Metal)
            ;

        static public readonly MaterialDef Stone = new MaterialDef()
            .SetState(MaterialState.Solid)
            .SetDensity(100)
            .SetType(MaterialType.Stone)
            ;

        static public readonly MaterialDef Soil = new MaterialDef()
          .SetState(MaterialState.Solid)
          .SetDensity(100)
          .SetType(MaterialType.Soil)
          ;

        static public readonly MaterialDef Meat = new MaterialDef()
          {
              EdibleRaw = true,
              EdibleCooked = true
          }
          .SetState(MaterialState.Solid)
          .SetDensity(10)
          .SetColor(Color.LightPink)
          .SetType(MaterialType.Meat)
          ;

        static public readonly MaterialDef PlantStem = new MaterialDef()
            .SetDensity(40)
            .SetState(MaterialState.Solid)
            .SetType(MaterialType.PlantStem);

        static public readonly MaterialDef Seed = new MaterialDef()
            .SetDensity(40)
            .SetState(MaterialState.Solid)
            .SetType(MaterialType.Seed);

    }
}
