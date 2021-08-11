using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class MaterialDef : Def, ILabeled, IInspectable
    {
        public static readonly Random Randomizer = new();

        public static Dictionary<int, MaterialDef> RegistryByHash = new();

        public string Label => this.Name;

        public static MaterialDef GetMaterial(int materialHash)
        {
            return RegistryByHash[materialHash];
        }

        public static void Initialize()
        {
            MaterialDefOf.Init();
        }

        public override string ToString()
        {
            return this.Name;
        }
        public string DebugName => $"Material:{this.Name}";

        public bool IsTemplate { get; private set; }

        private readonly int HashCode;
        public MaterialType Type;
        public Color Color;
        public Vector4 ColorVector;
        public string Prefix;
        public float Shininess;
        public int Density;
        public bool EdibleRaw, EdibleCooked;
        public MaterialState State;
        public MaterialCategory Category;
        public Fuel Fuel;
        public float WorkToBreak = 1;
        public int ValueBase = 1;
        public float ValueMultiplier = 1;

        /// <summary>
        /// How many ticks it takes for the liquid to flow to nearby cells
        /// </summary>
        internal int Viscosity;

        public int Value => (int)(this.ValueBase * this.ValueMultiplier);

        public MaterialDef()
        {
            this.IsTemplate = true;
        }
        public MaterialDef(MaterialType type, string name, string prefix, int density)
            : this(type, name, prefix, Color.White, density) { }
        public MaterialDef(string name, MaterialDef template) : base(name)
        {
            this.State = template.State;
            this.Category = template.Category;
            this.Density = template.Density;
            this.SetColor(template.Color);
            this.Type = template.Type;
            this.Type.AddMaterial(this);
            this.Fuel = template.Fuel;
        }

        public MaterialDef(MaterialDef template)
        {
            this.State = template.State;
            this.Category = template.Category;
            this.Density = template.Density;
            this.SetColor(template.Color);
            this.Type = template.Type;
            this.Type.AddMaterial(this);
            this.Fuel = template.Fuel;
        }
        public MaterialDef(MaterialType type, string name, string prefix, Color color, int density) : base(name)
        {
            this.HashCode = name.GetHashCode();
            RegistryByHash[this.HashCode] = this;
            this.Type = type;
            type.SubTypes.Add(this);
            this.Prefix = prefix;
            this.Density = density;
            this.Color = color;
            this.ColorVector = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, type.Shininess);
        }
        internal void Apply(Entity entity)
        {
        }

        internal static IEnumerable<MaterialDef> GetMaterials(Func<MaterialDef, bool> condition)
        {
            return RegistryByHash.Values.Where(condition);
        }
        
        public static MaterialDef CreateColor(Color color)
        {
            var mat = new MaterialDef(MaterialType.Dye, "Color", "Color", color, 1);
            return mat;
        }

        public MaterialDef SetColor(Color color)
        {
            this.Color = color;
            this.ColorVector = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, this.Shininess);
            return this;
        }
        public MaterialDef SetDensity(int density)
        {
            this.Density = density;
            return this;
        }
        public MaterialDef SetReflectiveness(float reflectiveness)
        {
            this.Shininess = reflectiveness;
            this.ColorVector = new Vector4(this.Color.R / 255.0f, this.Color.G / 255.0f, this.Color.B / 255.0f, reflectiveness);
            return this;
        }
        public MaterialDef SetState(MaterialState state)
        {
            this.State = state;
            return this;
        }

        public MaterialDef SetName(string name)
        {
            this.Name = name;
            return this;
        }
        public MaterialDef SetPrefix(string prefix)
        {
            if (this.IsTemplate)
                throw new Exception();
            this.Prefix = prefix;
            return this;
        }
        public MaterialDef SetValue(int value)
        {
            this.ValueBase = value;
            return this;
        }

        public MaterialDef SetType(MaterialType type)
        {
            this.Type = type;
            return this;
        }

        public IEnumerable<(object item, object value)> Inspect()
        {
            yield return (nameof(this.Type), this.Type);
            yield return (nameof(this.Density), this.Density);
            yield return (nameof(this.Color), this.Color);
            yield return (nameof(this.Value), this.Value);
            yield return (nameof(this.Shininess), this.Shininess);
        }
    }
}
