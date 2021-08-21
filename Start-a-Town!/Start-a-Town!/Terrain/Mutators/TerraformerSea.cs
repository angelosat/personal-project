using System;
using System.Collections.Generic;

namespace Start_a_Town_.Terraforming.Mutators
{
    class TerraformerSea : Terraformer
    {
        int _seaLevel = MapBase.MaxHeight / 2 - 1;
        public int SeaLevel
        {
            get => this._seaLevel; 
            set => this._seaLevel = Math.Max(0, Math.Min(MapBase.MaxHeight - 1, value));
        }

        public override void Finally(Chunk chunk)
        {
            foreach(var c in chunk.Cells)
            {
                var z = c.Z;
                if (z > this.SeaLevel)
                    continue;
                else if (z == this.SeaLevel)
                {
                    if (c.Block == BlockDefOf.Air)
                    {
                        c.Block = BlockDefOf.Fluid;
                        c.Material = MaterialDefOf.Water;
                    }
                    //else /// sand is applied by the default terraformer (currently normal.cs)
                    //{
                    //    c.Block = BlockDefOf.Sand;
                    //    c.Material = MaterialDefOf.Sand;
                    //}
                }
                else
                {
                    if (c.Block == BlockDefOf.Air)
                    {
                        c.Block = BlockDefOf.Fluid;
                        c.Material = MaterialDefOf.Water;
                        c.BlockData = BlockFluid.GetData(1);
                    }
                }
            }
        }
      
        public override IEnumerable<TerraformerProperty> GetAdjustableParameters()
        {
            yield return new TerraformerProperty("Sea Level", MapBase.MaxHeight / 2 - 1, 0, MapBase.MaxHeight - 1, 1);
        }
       
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, "Level", this.SeaLevel));
        }
        protected override void LoadExtra(SaveTag save)
        {
            this.SeaLevel = save.TagValueOrDefault("Level", MapBase.MaxHeight / 2 - 1);
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.SeaLevel);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.SeaLevel = r.ReadInt32();
        }
    }
}
