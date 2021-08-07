using System;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Grass : Terraformer
    {
        Random Randomizer;

        public Grass()
        {
            this.ID = Terraformer.Types.Grass;
            this.Name = "Grass";
            this.Finalize = (RandomThreaded random, IWorld w, Cell c, int x, int y, int z) =>
            {

            };
        }

        public override Terraformer SetWorld(IWorld w)
        {
            this.Randomizer = new Random(w.Seed + "Grass".GetHashCode());
            return this;
        }

        public override void Finally(Chunk chunk)
        {
            int varCount = BlockDefOf.Grass.Variations.Count;
            var count = Chunk.Size;
            for (int lx = 0; lx < count; lx++)
            {
                for (int ly = 0; ly < count; ly++)
                {
                    int z = chunk.HeightMap[lx][ly];
                    var c = chunk.GetLocalCell(lx, ly, z);
                    if (c.Block != BlockDefOf.Soil)
                        continue;
                    var variation = (byte)this.Randomizer.Next(varCount);
                    c.Block = BlockDefOf.Grass;
                    c.Material = MaterialDefOf.Soil;
                    c.Variation = variation;
                }
            }
        }

        public override object Clone()
        {
            return new Grass();
        }
    }
}
