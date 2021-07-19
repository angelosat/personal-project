﻿using System.Collections.Generic;

namespace Start_a_Town_
{
    public class WorldArgs
    {
        public int Seed;
        public bool Trees, Lighting;
        public Block.Types DefaultTile;

        public SortedSet<Terraformer> Mutators = new SortedSet<Terraformer>();
        public WorldArgs()
        {
            this.Mutators = new SortedSet<Terraformer>();
            this.Seed = 0;
            this.Trees = false;
            this.Lighting = true;
            this.DefaultTile = Block.Types.Soil;
            this.Name = "untitled world";
        }

        public WorldArgs(string name, bool trees, int seed, IEnumerable<Terraformer> terraformers, bool lighting = true, Block.Types defaultTile = Block.Types.Soil)
        {
            Trees = trees;
            Seed = seed;
            Name = name;
            this.Lighting = lighting;
            this.DefaultTile = defaultTile;
            this.Mutators = new SortedSet<Terraformer>(terraformers);
        }

        public string Name;
    }
}