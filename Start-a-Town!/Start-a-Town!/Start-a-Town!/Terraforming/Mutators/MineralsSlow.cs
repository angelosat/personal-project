using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class MineralsSlow : Terraformer
    {
        struct Template
        {
            public string Name;// { get; private set; }
            public double Threshold;// { get; private set; }
            public int Frequency;// { get; private set; }
            byte[] Seed;// { get; set; }
            public Material Material;// { get; set; }
            public int Hash;

            public Template(string name, Material material, int frequency, double threshold) :this()
            {
                this.Name = name;
                this.Frequency = frequency;
                this.Threshold = threshold;
                this.Material = material;
                this.Hash = name.GetHashCode();
                //Seed = BitConverter.GetBytes(this.GetHashCode());
            }
            //public byte[] GetSeed(World w)
            //{
            //    if(this.Seed.IsNull())
            //        this.Seed = BitConverter.GetBytes(w.Seed + this.Name.GetHashCode());
            //    return this.Seed;
            //}
            public double GetGradient(IWorld w, int x, int y, int z)
            {
                return Generator.Perlin3D(x, y, z, this.Frequency, this.Seed);// this.GetSeed(w));
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
        static readonly Template Coal = new Template("Coal", Material.Coal, 8, 0.5f);
        static readonly Template Iron = new Template("Iron", Material.Iron, 8, 0.5f);
        static readonly Template Gold = new Template("Gold", Material.Gold, 8, 0.5f);

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
            System.Diagnostics.Debug.Assert(false, "kakaka");
            foreach (var mineral in this.Templates)// Dictionary.Values)
            {
                Block.Types mineralType = Block.Types.Mineral;
                if (c.Block.Type != Block.Types.Cobblestone)
                    return;

                double mineralGradient = mineral.GetGradient(w, x, y, z);

                if (mineralGradient > mineral.Threshold)
                {
                    Random random = new Random(g.GetHashCode() + mineral.Hash);
                    double p = random.NextDouble() * 2 - 1;
                    //double chance = Math.Pow((mineralGradient - 0.5) * 2, 2);
                    double chance = Math.Pow((mineralGradient - mineral.Threshold) / (1 - mineral.Threshold), 2);
                    //double chance = (mineralGradient - 0.5) * 2;
                    if (p < chance)//mineralGradient)
                    {
                        //c.Type = mineralType;
                        c.SetBlockType(mineralType);
                        c.BlockData = (byte)mineral.Material.ID;
                    }
                }
            }
        }

        //public void Initialize(World world, Cell cell, int globalX, int globalY, int globalZ)
        //{
        //    if (cell.Type != Block.Types.Stone)
        //        return;

        //    double 
        //        zNormal = globalZ / (float)Map.MaxHeight, 
        //        coal;//, gold;

        //    byte[] coalSeed = BitConverter.GetBytes(world.Seed + "coal".GetHashCode());
        //    coal = Generator.Perlin3D(globalX, globalY, globalZ, 8, coalSeed);
        //    coal = (coal + 1) / 2f; //output of perlin is in (-1,1), so normalize within (0,1)

        //    if (coal > 0.5)
        //        cell.Type = Block.Types.Coal;
        //}

        public override object Clone()
        {
            return new MineralsSlow();
        }
    }
}
