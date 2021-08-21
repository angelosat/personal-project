using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Terraforming.Mutators;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public abstract class Terraformer : ISaveable, ISerializable
    {
        public TerraformerDef Def;
       
        public Action<RandomThreaded, WorldBase, Cell, int, int, int>
            Finalize = (r, w, c, x, y, z) => { };

        public virtual void Initialize(WorldBase w, Cell c, int x, int y, int z, double g) { }
        public virtual void Finally(Chunk chunk) { }
        internal virtual void Finally(Chunk newChunk, Dictionary<IntVec3, double> gradients)
        {
            this.Finally(newChunk);
        }

        public override string ToString()
        {
            return this.Def.Label;
        }
        public virtual void Generate(MapBase map) { }

        public static List<TerraformerDef> Defaults = new() { TerraformerDefOf.Normal, TerraformerDefOf.PerlinWorms, TerraformerDefOf.Minerals, TerraformerDefOf.Grass, TerraformerDefOf.Flowers, TerraformerDefOf.Trees };

        protected static int GetRandom(byte[] seedArray, int x, int y, int z, int min, int max)
        {
            var seed = Generator.Perlin3D(x, y, z, 16, seedArray);
            var random = new Random(seed.GetHashCode());
            var r = random.NextDouble();
            var val = min + (int)Math.Floor((max - min) * r);
            return val;
        }
        protected static int GetRandom(int seed, int min, int max)
        {
            var random = new Random(seed);
            var r = random.NextDouble();
            var val = min + (int)Math.Floor((max - min) * r);
            return val;
        }


        public virtual List<MutatorProperty> GetAdjustableParameters() { return new List<MutatorProperty>(); }

        public GroupBox GetUI()
        {
            var box = new GroupBox();
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

        public virtual Terraformer SetWorld(WorldBase w) { return this; }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag) { }

        public ISaveable Load(SaveTag tag)
        {
            this.Def = tag.LoadDef<TerraformerDef>("Def");
            this.LoadExtra(tag);
            return this;
        }
        protected virtual void LoadExtra(SaveTag tag) { }

        public void Write(BinaryWriter w)
        {
            this.Def.Write(w);
            this.WriteExtra(w);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }

        public ISerializable Read(BinaryReader r)
        {
            this.Def = r.ReadDef<TerraformerDef>();
            this.ReadExtra(r);
            return this;
        }
        protected virtual void ReadExtra(BinaryReader r) { }

    }
}
