using System;
using System.Collections.Generic;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class MineralsSlow : Terraformer
    {
        struct Template
        {
            public string Name;
            public double Threshold;
            public int Frequency;
            byte[] Seed;
            public MaterialDef Material;
            public int Hash;

            public Template(string name, MaterialDef material, int frequency, double threshold) :this()
            {
                this.Name = name;
                this.Frequency = frequency;
                this.Threshold = threshold;
                this.Material = material;
                this.Hash = name.GetHashCode();
            }
            
            public double GetGradient(IWorld w, int x, int y, int z)
            {
                return Generator.Perlin3D(x, y, z, this.Frequency, this.Seed);
            }
            public Template Clone()
            {
                return new Template(this.Name, this.Material, this.Frequency, this.Threshold);
            }
            public Template SetWorld(IWorld w)
            {
                this.Seed = BitConverter.GetBytes(w.Seed + this.Name.GetHashCode());
                return this;
            }
        }
        static readonly Template Coal = new Template("Coal", MaterialDefOf.Coal, 8, 0.5f);
        static readonly Template Iron = new Template("Iron", MaterialDefOf.Iron, 8, 0.5f);
        static readonly Template Gold = new Template("Gold", MaterialDefOf.Gold, 8, 0.5f);

        static new Dictionary<string, Template> Dictionary = new Dictionary<string, Template>(){
            {Coal.Name, Coal},
            {Iron.Name, Iron},
            {Gold.Name, Gold}
        };
        List<Template> Templates = new List<Template>();
        public override Terraformer SetWorld(IWorld w)
        {
            foreach (var item in Dictionary)
                this.Templates.Add(item.Value.Clone().SetWorld(w));
            return this;
        }
        public MineralsSlow()
        {
            this.ID = Terraformer.Types.MineralsSlow;
            this.Name = "MineralsSlow";
        }
        public override void Initialize(IWorld w, Cell c, int x, int y, int z, double g)
        {
            foreach (var mineral in this.Templates)
            {
                Block.Types mineralType = Block.Types.Mineral;
                if (c.Block.Type != Block.Types.Cobblestone)
                    return;

                double mineralGradient = mineral.GetGradient(w, x, y, z);

                if (mineralGradient > mineral.Threshold)
                {
                    Random random = new Random(g.GetHashCode() + mineral.Hash);
                    double p = random.NextDouble() * 2 - 1;
                    double chance = Math.Pow((mineralGradient - mineral.Threshold) / (1 - mineral.Threshold), 2);
                    if (p < chance)
                    {
                        c.SetBlockType(mineralType);
                        c.BlockData = (byte)mineral.Material.ID;
                    }
                }
            }
        }

        public override object Clone()
        {
            return new MineralsSlow();
        }
    }
}
