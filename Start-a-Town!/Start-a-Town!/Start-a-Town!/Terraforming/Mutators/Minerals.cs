using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.GameModes;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Minerals : Terraformer
    {
        class Template
        {
            public string Name;
            public int Frequency;
            public Material Material;
            public int MaxVeinSize;
            public int MinZ, MaxZ;

            public Template(string name, Material material, int frequency, int maxVeinSize, int minZ, int maxZ)
            {
                this.Name = name;
                this.Frequency = frequency;
                this.Material = material;
                this.MaxVeinSize = maxVeinSize;
                this.MinZ = minZ;
                this.MaxZ = maxZ;
            }
            //public double GetGradient(World w, int x, int y, int z)
            //{
            //    return Generator.Perlin3D(x, y, z, this.Frequency, this.Seed);// this.GetSeed(w));
            //}
            public Template Clone()
            {
                return new Template(this.Name, this.Material, this.Frequency, this.MaxVeinSize, this.MinZ, this.MaxZ);
            }
            public Template SetWorld(IWorld w)
            {
                //this.Seed = BitConverter.GetBytes(w.Seed + this.Name.GetHashCode());
                return this;
            }
        }
        static readonly Template Coal = new Template("Coal", Material.Coal, 32, 32, 1, 128);
        static readonly Template Iron = new Template("Iron", Material.Iron, 16, 20, 1, 96);
        static readonly Template Gold = new Template("Gold", Material.Gold, 8, 10, 1, 64);

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
        public Minerals()
        {
            this.ID = Terraformer.Types.Minerals;
            this.Name = "Minerals";
        }
        //public override void Initialize(World w, Cell c, int x, int y, int z, double g)
        //{
        //    foreach (var ore in this.Templates)// Dictionary.Values)
        //    {
        //        Block.Types mineralType = Block.Types.Mineral;
        //        if (c.Block.Type != Block.Types.Cobblestone)
        //            return;

        //        for (int i = 0; i < ore.Frequency; i++)
        //        {
        //            Random r = new Random(g.GetHashCode());
        //            int startX = r.Next(Chunk.Size);
        //            int startY = r.Next(Chunk.Size);

        //        }
        //    }
        //}
        public override void Finally(Chunk chunk)
        {
            return;
            var chunkSeed = chunk.MapCoords.GetHashCode() + chunk.World.Seed;
       
            foreach (var ore in this.Templates)// Dictionary.Values)
            {
                var seed = ore.Name.GetHashCode() + chunkSeed;
                Random r = new Random(seed);
                for (int i = 0; i < ore.Frequency; i++)
                {
                    int x = r.Next(Chunk.Size);
                    int y = r.Next(Chunk.Size);
                    //Cell c = chunk[x, y, chunk.HeightMap[x][y]];

                    //c.SetBlock(Block.Types.Mineral);
                    //c.BlockData = (byte)ore.Material.ID;

                    int z = r.Next(ore.MaxZ);
                    Cell c = chunk[x, y, z];
                    if (c.Block.Type != Block.Types.Cobblestone)
                        continue;
                    c.SetBlockType(Block.Types.Mineral);
                    c.BlockData = (byte)ore.Material.ID;

               
                    for (int j = 0; j < ore.MaxVeinSize; j++)
                    {
                        x = x + r.Next(3) - 1;
                        y = y + r.Next(3) - 1;
                        z = z + r.Next(3) - 1;
                        if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > ore.MaxZ - 1)
                            continue;
                        //c = chunk[x, y, chunk.HeightMap[x][y]];
                        c = chunk[x, y, z];
                        if (c.Block.Type != Block.Types.Cobblestone)
                            continue;

                        c.SetBlockType(Block.Types.Mineral);
                        c.BlockData = (byte)ore.Material.ID;
                    }

                    //x = x + r.Next(3) - 1;
                    //y = y + r.Next(3) - 1;
                    //z = z + r.Next(3) - 1;
                    //for (int j = 0; j < ore.MaxVeinSize; j++)
                    //{
                    //    if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > ore.MaxZ - 1)
                    //        break;
                    //    //c = chunk[x, y, chunk.HeightMap[x][y]];
                    //    c = chunk[x, y, z];
                    //    if (c.Block.Type != Block.Types.Cobblestone)
                    //        break;

                    //    c.SetBlockType(Block.Types.Mineral);
                    //    c.BlockData = (byte)ore.Material.ID;

                    //    x = x + r.Next(3) - 1;
                    //    y = y + r.Next(3) - 1;
                    //    z = z + r.Next(3) - 1;
                    //}
                }
            }
        }

        //Vector3 GetNext(double g)
        //{
            
        //}
        //Vector3[] = new 

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
            return new Minerals();
        }
    }
}
