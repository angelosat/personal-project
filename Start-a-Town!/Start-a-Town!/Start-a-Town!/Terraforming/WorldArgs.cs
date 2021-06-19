using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public class WorldArgs
    {
        //public Map Map;
        public int Seed;
        public bool Trees, Lighting; //Caves,
        public Block.Types DefaultTile;// = Tile.Types.Soil;

        //public List<Terraformer> Mutators = new List<Terraformer>();
        public SortedSet<Terraformer> Mutators = new SortedSet<Terraformer>();
        public WorldArgs()
        {
            //this.Mutators = new List<Terraformer>();
            this.Mutators = new SortedSet<Terraformer>();
            this.Seed = 0;
            this.Trees = false;
            this.Lighting = true;
            this.DefaultTile = Block.Types.Soil;
            this.Name = "<unnamed>";
        }

        public WorldArgs(string name, bool trees, int seed, IEnumerable<Terraformer> terraformers, bool lighting = true, Block.Types defaultTile = Block.Types.Soil)
           // : this()
        {
            //Caves = caves;
            Trees = trees;
            Seed = seed;
            //this.Flat = flat;
            Name = name;//.Length == 0 ? "World_0" : name;
            this.Lighting = lighting;
            this.DefaultTile = defaultTile;

            //this.Mutators = new List<Terraformer>(terraformers);
            this.Mutators = new SortedSet<Terraformer>(terraformers);
        }

        public string Name;// { get; set; }
    }
}
