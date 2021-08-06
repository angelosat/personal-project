﻿using System;
using System.Collections.Generic;
using Start_a_Town_.GameModes;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Sea : Terraformer
    {
        int _SeaLevel = MapBase.MaxHeight / 2;
        public int SeaLevel
        {
            get { return _SeaLevel; }
            set
            {
                _SeaLevel = Math.Max(0, Math.Min(MapBase.MaxHeight, value));
            }
        }

        public Sea()
        {
            this.ID = Terraformer.Types.Sea;
            this.Name = "Sea";
        }
       
        public override void Finally(Chunk chunk)
        {
            foreach(var c in chunk.CellGrid2)
            {
                var z = c.Z;
                if (z > this.SeaLevel)
                    continue;
                else if (z == this.SeaLevel)
                {
                    if (c.Block == BlockDefOf.Air)
                        c.Block = BlockDefOf.Water;
                    else
                        c.Block = BlockDefOf.Sand;
                }
                else
                {
                    if (c.Block == BlockDefOf.Air)
                    {
                        c.Block = BlockDefOf.Water;
                        c.BlockData = BlockWater.GetData(1);
                    }
                }
            }
        }
        public override Block Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            if (z > this.SeaLevel)
                return BlockDefOf.Air;

            return w.DefaultBlock;
        }
        public override List<MutatorProperty> GetAdjustableParameters()
        {
            var list = new List<MutatorProperty>();
            list.Add(new MutatorProperty("Sea Level", MapBase.MaxHeight / 2, 0, MapBase.MaxHeight, 1));
            return list;
        }

        public override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "Level", this.SeaLevel));
            return tag;
        }
        public override Terraformer Load(SaveTag save)
        {
            this.SeaLevel = save.TagValueOrDefault("Level", MapBase.MaxHeight / 2 - 1);
            return this;
        }

        public override object Clone()
        {
            return new Sea();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.SeaLevel);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.SeaLevel = r.ReadInt32();
        }
    }
}
