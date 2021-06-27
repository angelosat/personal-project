﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class Material : Def, ILabeled
    {
        static readonly public Random Randomizer = new();
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }

        public static Dictionary<int, Material> Database = new();
       

        public string Label => Name;

        static public Material GetMaterial(int materialID)
        {
            if (materialID < 0)
                return null;
            return Database[materialID];
        }

        internal static Material GetRandom()
        {
            var index = Randomizer.Next(0, _IDSequence);
            return Database[index];
        }

        static public void Initialize()
        {
            MaterialDefOf.Init();
        }


        public override string ToString()
        {
            return this.Name;
        }
        public string DebugName { get { return $"Material:{this.Name}"; } }

        public bool IsTemplate { get; private set; }

        static public Material Parse(string name)
        {
            return Database.Values.ToDictionary(f => f.Name, f => f)[name];
        }

        public MaterialType Type;
        public readonly int ID;
        public Color Color;
        public Vector4 ColorVector;
        public string Prefix;
        public float Shininess;
        public int Density;
        public bool EdibleRaw, EdibleCooked;
        public MaterialState State;
        public MaterialCategory Category;
        public HashSet<MaterialToken> Tokens = new();
        public Fuel Fuel;
        public List<GameObject> ProcessingChain;

        public Material()
        {
            this.IsTemplate = true;
        }
        public Material(MaterialType type, string name, string prefix, int density)
            : this(type, name, prefix, Color.White, density) { }
        public Material(string name, Material template) : base(name)
        {
            this.ID = IDSequence;
            Database[this.ID] = this;

            this.State = template.State;
            this.Category = template.Category;
            this.Density = template.Density;
            this.Tokens = new(template.Tokens);
            this.SetColor(template.Color);
            this.Type = template.Type;
            this.Type.AddMaterial(this);
            this.Fuel = template.Fuel;

        }

        public Material(Material template)
        {
            this.ID = IDSequence;
            Database[this.ID] = this;

            this.State = template.State;
            this.Category = template.Category;
            this.Density = template.Density;
            this.Tokens = new(template.Tokens);
            this.SetColor(template.Color);
            this.Type = template.Type;
            this.Type.AddMaterial(this);
            this.Fuel = template.Fuel;
        }
       
        internal void Apply(Entity entity)
        {
            //entity.Name = $"{this.Prefix} {entity.Name}";
        }

        public Material(MaterialType type, string name, string prefix, Color color, int density) : base(name)
        {
            this.Type = type;
            type.SubTypes.Add(this);
            this.ID = IDSequence;
            Database[this.ID] = this;
            this.Prefix = prefix;
            this.Density = density;
            this.Color = color;
            this.ColorVector = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, type.Shininess);// textcolor.A / 255.0f);
        }

        internal static IEnumerable<Material> GetMaterials(MaterialToken madeFrom)
        {
            return Database.Values.Where(m => m.Tokens.Contains(madeFrom));
        }
        internal static IEnumerable<Material> GetMaterials(Func<Material, bool> condition)
        {
            return Database.Values.Where(condition);
        }
        internal static IEnumerable<Material> GetMaterialsAny(params MaterialToken[] tokens)
        {
            return Material.GetMaterials(m => tokens.Any(m.Tokens.Contains));
        }
        internal static IEnumerable<Material> GetMaterialsAll(params MaterialToken[] tokens)
        {
            return Material.GetMaterials(m => tokens.All(m.Tokens.Contains));
        }
        static public Material CreateColor(Color color)
        {
            var mat = new Material(MaterialType.Dye, "Color", "Color", color, 1);
            return mat;
        }
        public float WorkToBreak = 1;
        public int ValueBase = 1;
        public float ValueMultiplier = 1;
        public int Value => (int)(this.ValueBase * this.ValueMultiplier);
       
        public Material AddTokens(params MaterialToken[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
                this.Tokens.Add(tokens[i]);
            return this;
        }
        public Material SetColor(Color color)
        {
            this.Color = color;
            this.ColorVector = new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, this.Shininess);// textcolor.A / 255.0f);
            return this;
        }
        public Material SetDensity(int density)
        {
            this.Density = density;
            return this;
        }
        public Material SetReflectiveness(float reflectiveness)
        {
            this.Shininess = reflectiveness;
            this.ColorVector = new Vector4(this.Color.R / 255.0f, this.Color.G / 255.0f, this.Color.B / 255.0f, reflectiveness);// textcolor.A / 255.0f);
            return this;
        }
        public Material SetState(MaterialState state)
        {
            this.State = state;
            return this;
        }
        public Material SetCategory(MaterialCategory category)
        {
            this.Category = category;
            return this;
        }
        public Material SetName(string name)
        {
            this.Name = name;
            return this;
        }
        public Material SetPrefix(string prefix)
        {
            if (this.IsTemplate)
                throw new Exception();
            this.Prefix = prefix;
            return this;
        }
        public Material SetValue(int value)
        {
            this.ValueBase = value;
            return this;
        }
        public Material SetValueMultiplier(float value)
        {
            if (this.IsTemplate)
                throw new Exception();
            this.ValueMultiplier = value;
            return this;
        }
        public Material SetType(MaterialType type)
        {
            this.Type = type;
            return this;
        }

        internal ToolAbilityDef GetSkillToExtract()
        {
            return this.Type.SkillToExtract;
        }
    }
}
