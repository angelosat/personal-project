using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class MaterialTemplates
    {
        static public readonly Material Fruit = new Material()
            { 
                EdibleRaw = true,
                EdibleCooked = true,
                State = MaterialState.Solid,
                Density = 2
            }.SetType(MaterialType.Fruit)
            ;

        static public readonly Material Wood = new Material()
            { Fuel = new Fuel(FuelDef.Organic, 20f) }
            .SetState(MaterialState.Solid)
            .SetDensity(50)
            .SetType(MaterialType.Wood)
            ;

        static public readonly Material Metal = new Material()
            .SetState(MaterialState.Solid)
            .SetDensity(200)
            .SetReflectiveness(1)
            .SetType(MaterialType.Metal)
            ;

        static public readonly Material Stone = new Material()
            .SetState(MaterialState.Solid)
            .SetDensity(100)
            .SetType(MaterialType.Stone)
            ;

        static public readonly Material Soil = new Material()
          .SetState(MaterialState.Solid)
          .SetDensity(100)
          .SetType(MaterialType.Soil)
          ;

        static public readonly Material Meat = new Material()
          {
              EdibleRaw = true,
              EdibleCooked = true
          }
          .SetState(MaterialState.Solid)
          .SetDensity(10)
          .SetColor(Color.LightPink)
          .SetType(MaterialType.Meat)
          ;

        static public readonly Material PlantStem = new Material()
            .SetDensity(40)
            .SetState(MaterialState.Solid)
            .SetType(MaterialType.PlantStem);

        static public readonly Material Seed = new Material()
            .SetDensity(40)
            .SetState(MaterialState.Solid)
            .SetType(MaterialType.Seed);

    }
}
