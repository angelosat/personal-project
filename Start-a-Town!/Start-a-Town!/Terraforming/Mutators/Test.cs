using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Test : Terraformer
    {
        public Test()
        {
            this.ID = Terraformer.Types.Test;
            this.Name = "Test";
        }
        public override Block.Types Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            double gradient, gradientRock, turbulence, zNormal;
            byte[] seedArray = w.GetSeedArray();
            int octaves = 7;
            Block.Types type = Block.Types.Air;

            zNormal = z / (float)Map.MaxHeight;

            turbulence = 0;

            for (int k = 0; k < octaves; k++)
            {
                double intensity = (1 - (k / (float)octaves));
                intensity = intensity * Generator.Perlin3D(x, y, z, 512, seedArray);
                turbulence += Generator.Perlin3D(x, y, z, 256 >> k, seedArray) * intensity;
            }
            
            gradient = zNormal + zNormal - 1;
            gradient += turbulence;

            if (z == 0)
                type = Block.Types.Stone;
            else
            {
                if (gradient < Map.GroundDensity)
                {
                    if (gradient > 0)
                    {
                        type = w.DefaultTile;
                    }
                    else
                    {
                        type = Block.Types.Stone;
                    }
                }
            }
            return type;
        }
        public override object Clone()
        {
            return new Test();
        }
    }
}
