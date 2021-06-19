using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Materials
{
    public class Material
    {
        //public enum Types { Air, Wood, Metal, Soil, Mineral }

        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }

        static Dictionary<int, Material> _Templates;
        public static Dictionary<int, Material> Templates
        {
            get
            {
                if (_Templates.IsNull())
                    _Templates = new Dictionary<int, Material>();
                return _Templates;
            }
        }

        static public void Initialize()
        {

        }
        

        public override string ToString()
        {
            //return "Material:" + this.Name.ToString();
            return "Material:" + this.Type.Name + ":" + this.Name;
        }
        static public Material Parse(string name)
        {
            return Templates.Values.ToDictionary(f => f.Name, f => f)[name];
        }

        //public Types Type { get; set; }
        public MaterialType Type { get; set; }
        public readonly int ID;
        public Color Color { get; set; }
        public Vector4 ColorVector;
        public string Name { get; set; }
        public string Prefix { get; set; }
        public float Density { get; set; }
        public float Fuel { get; set; }
        public List<GameObject> ProcessingChain { get; set; }

        Material(MaterialType type, string name, string prefix, float density)
            : this(type, name, prefix, Color.White, density) { }

        Material(MaterialType type, string name, string prefix, Color textcolor, float density)
        {
            this.Type = type;
            type.SubTypes.Add(this);
            this.ID = IDSequence;
            Templates[this.ID] = this;
            this.Name = name;
            this.Prefix = prefix;
            this.Density = density;
            this.Color = textcolor;
            this.ColorVector = new Vector4(textcolor.R / 255.0f, textcolor.G / 255.0f, textcolor.B / 255.0f, type.Shininess);// textcolor.A / 255.0f);

            this.Fuel = 0;
        }

        static public Material CreateColor(Color color)
        {
            var mat = new Material(MaterialType.Dye, "Color", "Color", color, .05f);
            return mat;
        }

        //GameObject CreateEntities(Material mat)
        //{
        //    GameObject obj = MaterialTemplate;
        //    obj["Info"] = new GeneralComponent(GameObject.Types.WoodenPlank, ObjectType.Material, "Wood Plank", "Made out of wood", Quality.Common);
        //    obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 24), new Vector2(16, 24)) { Tint = Material.LightWood.TextColor });
        //    obj.AddComponent<GuiComponent>().Initialize(10, 8);//64);
        //    obj["Physics"] = new PhysicsComponent(size: 1);
        //    obj.AddComponent<UseComponent>().Initialize(Script.Types.Framing);
        //    obj.AddComponent<MaterialComponent>().Initialize(Material.LightWood, Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
        //    return obj;
        //}

        //static public readonly Material Metal = new Material("Metal", "Metal", Color.SteelBlue, 1);
        //static public readonly Material Iron = new Material(MaterialType.Metal, "Iron", "Iron", Color.SteelBlue, 1);// Color.LightSteelBlue, 1);

        static public readonly Material Iron = new Material(MaterialType.Metal, "Iron", "Iron", Color.LightSteelBlue, 1);// Color.LightSteelBlue, 1);
        static public readonly Material Gold = new Material(MaterialType.Metal, "Gold", "Gold", Color.Gold, 1);//Color.PaleGoldenrod, 1);

        static public readonly Material Coal = new Material(MaterialType.Mineral, "Coal", "Coal", Color.DimGray, 1) { Fuel = 5f };
        static public readonly Material Stone = new Material(MaterialType.Mineral, "Stone", "Stone", Color.White, 0.8f);//LightSlateGray, 0.8f); new Color(213, 209, 201, 255) //Color.AntiqueWhite
        //static public readonly Material Wood = new Material(Types.Wood, "Wood", "Wooden", Color.SaddleBrown, 0.5f);
        //static public readonly Material DarkWood = new Material(Types.Wood, "Dark Wood", "Dark Wood", Color.Brown, 0.5f);
        static public readonly Material Twig = new Material(MaterialType.Twig, "Twig", "Twig", new Color(139, 136, 95, 255), 0.3f);// Color.DarkOliveGreen, 0.5f);
        static public readonly Material LightWood = new Material(MaterialType.Wood, "Light Wood", "Light Wood", Color.SandyBrown, 0.5f);
        static public readonly Material DarkWood = new Material(MaterialType.Wood, "Dark Wood", "Dark Wood", Color.SaddleBrown, 0.5f);
        static public readonly Material RedWood = new Material(MaterialType.Wood, "Red Wood", "Red Wood", Color.Brown, 0.5f);
        static public readonly Material Soil = new Material(MaterialType.Soil, "Soil", "Dirt", Color.SandyBrown, 0.2f);
        static public readonly Material Sand = new Material(MaterialType.Sand, "Sand", "Sand", Color.BlanchedAlmond, 0.1f);
        static public readonly Material Air = new Material(MaterialType.Gas, "Air", "Air", 0);
        // basalt? new Color(120, 109, 95, 255)
        static public readonly Material Water = new Material(MaterialType.Liquid, "Water", "Water", Color.SeaGreen, .05f);
        //static public readonly Material Dye = new Material(MaterialType.Dye, "Color", "Color", Color.Transparent, .05f);
        static public readonly Material Glass = new Material(MaterialType.Glass, "Glass", "Glass", Color.White, .05f);
    }
}
