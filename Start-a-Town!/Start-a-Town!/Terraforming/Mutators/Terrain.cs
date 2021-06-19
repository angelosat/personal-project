using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Terrain
    {
        float SoilThickness = 0.1f;
        float GroundRatio = 0;

        public void Initialize(World w, Cell c, int x, int y, int z, double gradient)
        {
            float zNormal = z / (float)Map.MaxHeight - 0.5f; 
            double gradientSoil = zNormal + gradient;
            double gradientRock = zNormal + gradient * 5 + SoilThickness;

            if (gradientRock < this.GroundRatio)
            {
                Random random = new Random(gradient.GetHashCode() + "rock".GetHashCode());
                c.Variation = (byte)random.Next(Block.Registry[Block.Types.Cobblestone].Variations.Count);
                //c.Type = Block.Types.Cobblestone;
                c.SetBlockType(Block.Types.Cobblestone);
                return;
            }

            if (z == 0)
            {
                //c.Type = Block.Types.Stone;
                c.SetBlockType(Block.Types.Stone);
                return;
            }
            if (gradientSoil <= this.GroundRatio)
            {
                Random random = new Random(gradient.GetHashCode() + "soil".GetHashCode());
                c.Variation = (byte)random.Next(Block.Registry[Block.Types.Soil].Variations.Count);
                //c.Type = w.DefaultTile;
                c.SetBlockType(w.DefaultTile);
                return;
            }
            //c.Type = Block.Types.Air;
            c.SetBlockType(Block.Types.Air);
        }
    }
}
