﻿using System;
using System.Collections.Generic;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Land : Terraformer
    {
        int _LandLevel;
        public int LandLevel
        {
            get => this._LandLevel;
            set => this._LandLevel = Math.Max(0, Math.Min(MapBase.MaxHeight, value));
        }

        public Land()
        {
            this.LandLevel = MapBase.MaxHeight / 2;
        }
        public override void Initialize(WorldBase w, Cell c, int x, int y, int z, double g)
        {
            if (z > this.LandLevel)
            {
                c.Block = BlockDefOf.Air;
                return;
            }
            c.Block = w.DefaultBlock;
            c.Material = c.Block.DefaultMaterial;
        }
        public override List<MutatorProperty> GetAdjustableParameters()
        {
            var list = new List<MutatorProperty>();
            list.Add(new MutatorProperty("Land Level", this.LandLevel, 0, MapBase.MaxHeight, 1));
            return list;
        }

        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(new SaveTag(SaveTag.Types.Int, "Level", this.LandLevel));
        }
        protected override void LoadExtra(SaveTag save)
        {
            this.LandLevel = save.TagValueOrDefault<int>("Level", MapBase.MaxHeight / 2);
        }

        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write(this.LandLevel);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.LandLevel = r.ReadInt32();
        }
    }
}
