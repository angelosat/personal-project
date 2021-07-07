﻿using System;
using System.Collections.Generic;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Land : Terraformer
    {
        int _LandLevel;
        public int LandLevel
        {
            get { return _LandLevel; }
            set
            {
                _LandLevel = Math.Max(0, Math.Min(Map.MaxHeight, value));
            }
        }

        public Land()
        {
            this.ID = Terraformer.Types.Land;
            this.Name = "Land";
            this.LandLevel = Map.MaxHeight / 2;
        }
        public override void Initialize(IWorld w, Cell c, int x, int y, int z, double g)
        {
            if (z > this.LandLevel)
            {
                c.SetBlockType(Block.Types.Air);
                return;
            }
            c.SetBlockType(w.DefaultTile);
        }
        public override List<MutatorProperty> GetAdjustableParameters()
        {
            List<MutatorProperty> list = new List<MutatorProperty>();
            list.Add(new MutatorProperty("Land Level", this.LandLevel, 0, Map.MaxHeight, 1));
            return list;
        }

        public override List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "Level", this.LandLevel));
            return tag;
        }
        public override Terraformer Load(SaveTag save)
        {
            this.LandLevel = save.TagValueOrDefault<int>("Level", Map.MaxHeight / 2);
            return this;
        }

        public override object Clone()
        {
            return new Land();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.LandLevel);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.LandLevel = r.ReadInt32();
        }
    }
}
