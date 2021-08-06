using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Terraforming.Mutators;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public abstract class Terraformer : IComparable<Terraformer>, ICloneable
    {
        public int CompareTo(Terraformer other)
        {
            if (this.ID < other.ID)
                return -1;
            else if (this.ID > other.ID)
                return 1;
            return 0;
        }

        public enum Types { None, Land, Sea, Normal, Caves, Minerals, Test, Grass, Empty, Flowers, Trees, MineralsSlow, PerlinWorms }
        public static readonly Terraformer Sea = new Sea();
        public static readonly Terraformer Land = new Land();
        public static readonly Terraformer Normal = new Normal();
        public static readonly Terraformer Grass = new Grass();
        public static readonly Terraformer Flowers = new Flowers();
        public static readonly Terraformer Trees = new GeneratorPlants();
        public static readonly Terraformer Caves = new Caves();
        public static readonly Terraformer Minerals = new Minerals();
        public static readonly Terraformer Empty = new Empty();
        public static readonly Terraformer PerlinWorms = new PerlinWormGenerator();

        static public Dictionary<Types, Terraformer> Dictionary { get { return _Dictionary; } }
        static readonly Dictionary<Types, Terraformer> _Dictionary = new Dictionary<Types, Terraformer>()
        {
            {Types.Land, Land},
            {Types.Sea, Sea},
            {Types.Normal, Normal},
            {Types.Caves, Caves},
            {Types.PerlinWorms, PerlinWorms},
            {Types.Grass, Grass},
            {Types.Flowers, Flowers},
            {Types.Trees, Trees},
            {Types.Minerals, Minerals},
            {Types.Empty, Empty},
        };
        public string Name { get; protected set; }
        public Types ID { get; protected set; }

        public Action<RandomThreaded, IWorld, Cell, int, int, int>
            Finalize = (r, w, c, x, y, z) => { };

        public virtual Block Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r) { return c.Block; }
        public virtual void Initialize(IWorld w, Cell c, int x, int y, int z, double g) { }
        public virtual void Finally(Chunk chunk) { }
        internal virtual void Finally(Chunk newChunk, Dictionary<IntVec3, double> gradients)
        {
            this.Finally(newChunk);
        }

        public override string ToString()
        {
            return Name;
        }
        public virtual void Generate(MapBase map) { }

        static public List<Terraformer> All
        {
            get
            {
                return Dictionary.Values.ToList();
            }
        }
        static public List<Terraformer> Defaults
        {
            get
            {
                return new List<Terraformer>() { Normal, PerlinWorms, Minerals, Grass, Flowers, Trees };
            }
        }

        static protected int GetRandom(byte[] seedArray, int x, int y, int z, int min, int max)
        {
            double seed = Generator.Perlin3D(x, y, z, 16, seedArray);
            Random random = new Random(seed.GetHashCode());
            double r = random.NextDouble();
            int val = min + (int)Math.Floor((max - min) * r);
            return val;
        }
        static protected int GetRandom(int seed, int min, int max)
        {
            Random random = new Random(seed);
            double r = random.NextDouble();
            int val = min + (int)Math.Floor((max - min) * r);
            return val;
        }


        public virtual List<MutatorProperty> GetAdjustableParameters() { return new List<MutatorProperty>(); }

        public GroupBox GetUI()
        {
            GroupBox box = new GroupBox();
            var props = this.GetAdjustableParameters();
            foreach (var item in props)
            {
                var name = new Label(item.Name) { Location = box.Controls.BottomLeft };
                box.Controls.Add(name);
                TextBox input;
                var slider = new SliderNew(() => item.Value, v => item.Value = v, 100, item.Min, item.Max, item.Step);
                input = new TextBox(50) { Location = slider.TopRight, Text = item.Value.ToString() };
                input.TextChangedFunc = newtxt =>
                {
                    float newValue = int.Parse(newtxt);
                    newValue = (float)(Math.Round(newValue / item.Step) * item.Step);
                    newValue = MathHelper.Clamp(newValue, item.Min, item.Max);
                    input.Text = newValue.ToString();
                };
                /// THIS IS THE OLD CODE TO CORRECT INVALID VALUE ENTERED
                //input.InputFunc = (txt, ch) =>
                //{
                //    if (!char.IsDigit(ch))
                //        return txt;
                //    string newtxt = txt + ch;
                //    float newValue = int.Parse(newtxt);
                //    newValue = (float)(Math.Round(newValue / item.Step) * item.Step);
                //    newValue = MathHelper.Clamp(newValue, item.Min, item.Max);
                //    newtxt = newValue.ToString();
                //    return newtxt;
                //};
                input.TextChangedFunc = txt =>
                {
                    if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                        txt = "0";
                    float newValue = int.Parse(txt);
                    slider.Value = newValue;
                };
                box.Controls.Add(slider, input);
            }
            return box;
        }

        public virtual List<SaveTag> Save()
        {
            return null;
        }
        public virtual Terraformer Load(SaveTag save)
        { return this; }
        public virtual void Write(BinaryWriter w)
        {
        }
        public virtual void Read(BinaryReader r)
        {
        }

        public abstract object Clone();
        public virtual Terraformer SetWorld(IWorld w) { return this; }
        static public Terraformer Create(Terraformer.Types id, IWorld world)
        {
            return (Dictionary[id].Clone() as Terraformer).SetWorld(world);
        }
    }
}
