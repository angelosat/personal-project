using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components
{
    class ObjectType
    {
        static public readonly string None = "";
        static public readonly string Human = "Human";
        static public readonly string Undead = "Undead";
        static public readonly string Equipment = "Equipment";
        static public readonly string BodyPart = "Body part";
        static public readonly string Consumable = "Consumable";
        static public readonly string Material = "Material";
        static public readonly string Bars = "Bars";
        static public readonly string Block = "Block";
        static public readonly string Blueprint = "Blueprint";
        static public readonly string Plant = "Plant";
        static public readonly string Container = "Container";
        static public readonly string Construction = "Construction";
        static public readonly string WorkBench = "Workbench";
        static public readonly string Furniture = "Furniture";
        static public readonly string Lightsource = "Lightsource";
        static public readonly string Package = "Package";
        static public readonly string Ability = "Ability";
        static public readonly string Condition = "Condition";
        public static readonly string Job = "Job";
        public static readonly string Plan = "Plan";
        public static readonly string Project = "Project";
        public static readonly string BuildingPlan = "Building Plan";
        public static readonly string Entity = "Entity";
        public static readonly string Schematic = "Schematic";
        public static readonly string Weapon = "Weapon";
        public static readonly string Furnace = "Furnace";
        public static readonly string Fuel = "Fuel";
        public static readonly string Smeltery = "Smeltery";
        public static readonly string Shield = "Shield";
        public static readonly string Armor = "Armor";

        //static public string None { get { return ""; } }
        //static public string Human { get { return "Human"; } }
        //static public string Undead { get { return "Undead"; } }
        //static public string Equipment { get { return "Equipment"; } }
        //static public string BodyPart { get { return "Body part"; } }
        //static public string Consumable { get { return "Consumable"; } }
        //static public string Material { get { return "Material"; } }
        //static public string Block { get { return "Block"; } }
        //static public string Blueprint { get { return "Blueprint"; } }
        //static public string Plant { get { return "Plant"; } }
        //static public string Container { get { return "Container"; } }
        //static public string Construction { get { return "Construction"; } }
        //static public string WorkBench { get { return "Workbench"; } }
        //static public string Furniture { get { return "Furniture"; } }
        //static public string Lightsource { get { return "Lightsource"; } }
        //static public string Package { get { return "Package"; } }
        //static public string Ability { get { return "Ability"; } }
        //static public string Condition { get { return "Condition"; } }
        //public static string Job { get { return "Job"; } }
        //public static string Plan { get { return "Plan"; } }
        //public static string Project { get { return "Project"; } }
        //public static string BuildingPlan { get { return "Building Plan"; } }
        //public static string Entity { get { return "Entity"; } }
        //public static string Schematic { get { return "Schematic"; } }
        //public static string Weapon { get { return "Weapon"; } }

        
    }
    public class Quality
    {
        public enum Types { Trash, Common, Uncommon, Rare, Epic, Legendary, Unique, Cheating }
        public Types Type;
        public Color Color;
        Quality(Types type, Color color)
        {
            this.Type = type;
            this.Color = color;
        }

        static public Quality Trash { get { return new Quality(Types.Trash, Color.Gray); } }
        static public Quality Common { get { return new Quality(Types.Common, Color.White); } }
        static public Quality Uncommon { get { return new Quality(Types.Uncommon, Color.Lime); } }
        static public Quality Rare { get { return new Quality(Types.Rare, Color.DodgerBlue); } }// new Color(0, 111, 222, 255)); } }
        static public Quality Epic { get { return new Quality(Types.Epic, Color.BlueViolet); } }
        static public Quality Legendary { get { return new Quality(Types.Legendary, Color.DarkOrange); } }
        static public Quality Unique { get { return new Quality(Types.Unique, Color.Yellow); } }
        static public Quality Cheating { get { return new Quality(Types.Cheating, Color.LightSkyBlue); } }
    }
    public class GeneralComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Info";
            }
        }
        //protected int _ID { get { return (int)this.ID;}{set}
        //protected GameObject.Types ID { get { return (GameObject.Types)this["ID"]; } set { this["ID"] = value; } }
        public int ID { get { return (int)this["ID"]; } protected set { this["ID"] = value; } }
        public Quality Quality { get { return (Quality)this["Quality"]; } protected set { this["Quality"] = value; } }
        public string Name { get { return (string)this["Name"]; } set { this["Name"] = value; } }
        public string Type { get { return (string)this["Type"]; } set { this["Type"] = value; } }
        public string Description { get { return (string)this["Description"]; } set { this["Description"] = value; } }
        public bool SaveName { get { return (bool)this["SaveName"]; } set { this["SaveName"] = value; } }
        public bool SaveWithChunk { get { return (bool)this["SaveWithChunk"]; } set { this["SaveWithChunk"] = value; } }
        public int StackSize { get { return (int)this["StackSize"]; } set { 
            this["StackSize"] = value;
            if (value == 0)
                Console.WriteLine("WARNING: STACKSIZE SET TO 0");
                //throw (new Exception("WARNING: STACKSIZE SET TO 0")); 
            } }
        public int StackMax { get { return (int)this["StackMax"]; } set { this["StackMax"] = value; } }
        public ItemSubType ItemSubType { get { return (ItemSubType)this["ItemSubType"]; } set { this["ItemSubType"] = value; } }

        public GeneralComponent Initialize(int id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null, bool saveName = false)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Type = objType;
            this.Quality = quality ?? Quality.Common;
            this.SaveName = saveName;
            this.StackSize = this.StackMax = 1;
            this.ItemSubType = ItemSubType.Generic;
            return this;
        }
        public GeneralComponent Initialize(GameObject.Types id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null, bool saveName = false)//, int height = 1, int weight = -1)
        {
            return this.Initialize((int)id, objType, name, description, quality, saveName);
        }
        public GeneralComponent Initialize(ItemSubType subtype)
        {
            this.ItemSubType = subtype;
            return this;
        }
            //{
        //    this.ID = (int)id;
        //    this.Name = name;
        //    this.Description = description;
        //    this.Type = objType;
        //    this.Quality = quality ?? Quality.Common;
        //    this.SaveName = saveName;
        //    this.StackSize = 1;
        //    return this;
        //}

        public GeneralComponent()
            : base()
        {
            this.SaveWithChunk = true;
            this.SaveName = false;
            this.Name = "GameObject";
            this.ID = (int)GameObject.Types.Default;
            this.Description = "";
            this.Type = "";
            Quality = Components.Quality.Common;
            this.SaveName = false;
            this.StackSize = 1;
            this.StackMax = 1;// 256; //default items are unstackable
            this.ItemSubType = ItemSubType.Generic;
        }

        public GeneralComponent(GameObject.Types id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null)//, int height = 1, int weight = -1)
        {
            this["ID"] = (int)id;
            this["Name"] = name;
            this["Description"] = description;
            this["Type"] = objType;
            this["Quality"] = quality ?? Quality.Common;
            this.SaveName = false;
            this.StackSize = this.StackMax = 1;
            this.ItemSubType = ItemSubType.Generic;
        }
        public GeneralComponent(int id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null)//, int height = 1, int weight = -1)
        {
            this["ID"] = id;
            this["Name"] = name;
            this["Description"] = description;
            this["Type"] = objType;
            this["Quality"] = quality ?? Quality.Common;
            this.SaveName = false;
            this.StackSize = this.StackMax = 1;
            this.ItemSubType = ItemSubType.Generic;
        }
        public override object Clone()
        {
            //if (this.ItemSubType != ItemSubType.Generic)
            //    "asd".ToConsole();
            GeneralComponent phys = new GeneralComponent();
            foreach (KeyValuePair<string, object> property in Properties)
            {
                phys.Properties[property.Key] = property.Value;
            }
            return phys;
        }

        public string GetName()
        {
            return this.Name + (this.StackSize > 1 ? " (x" + this.StackSize + ")" : "");
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            //tooltip.Color = GetQualityColor();
            //Label labelName = new Label(Vector2.Zero, this.Name + " (x" + this.StackSize + ")", tooltip.Color, Color.Black, UIManager.FontBold) { TextColorFunc = () => tooltip.Color },
            //    labelType = new Label(labelName.BottomLeft, this.Quality.Type + " " + this.Type, fill: Color.Gold) { TextColorFunc = () => Color.Gold },
            //    labelDescription = new Label(labelType.BottomLeft, this.Description);
            //tooltip.Controls.Add(labelName, labelType, labelDescription);

            tooltip.Color = GetQualityColor();
            tooltip.Controls.Add(new Label(Vector2.Zero, this.GetName(), tooltip.Color, Color.Black, UIManager.FontBold) { TextColorFunc = () => tooltip.Color });
            //tooltip.Controls.Add(new Label(this.Quality.Type + " " + this.Type) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });
            //tooltip.Controls.Add(new Label(this.ItemSubType.ToString()) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });
            tooltip.Controls.Add(new Label(this.Quality.Type + " " + this.ItemSubType.ToString()) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });
            tooltip.Controls.Add(new Label(this.Description) { Location = tooltip.Controls.BottomLeft });
        }
        static public Color GetQualityColor(GameObject obj)
        {
            return ((Quality)obj["Info"]["Quality"]).Color;
            //switch ((Quality)obj["Info"]["Quality"])
            //{
            //    case Components.Quality.Uncommon:
            //        return Color.Lime;
            //    case Components.Quality.Rare:
            //        return new Color(0, 111, 222, 255);//Color.DeepSkyBlue;
            //    case Components.Quality.Epic:
            //        return Color.BlueViolet;//new Color(180, 0, 255, 255);
            //    case Components.Quality.Legendary:
            //        return Color.DarkOrange;//Red;
            //    default:
            //        return Color.White;
            //}
        }

        public Color GetQualityColor()
        {
            return Quality.Color;
            //switch (Quality)
            //{
            //    case Components.Quality.Uncommon:
            //        return Color.Lime;
            //    case Components.Quality.Rare:
            //        return new Color(0, 111, 222, 255);//Color.DeepSkyBlue;
            //    case Components.Quality.Epic:
            //        return Color.BlueViolet;//new Color(180, 0, 255, 255);
            //    case Components.Quality.Legendary:
            //        return Color.DarkOrange;//Red;
            //    default:
            //        return Color.White;
            //}
        }

        //public override string ToString()
        //{
        //    return this.ItemSubType.ToString();
        //}


        // TODO: maybe optimize this by a custom name flag so i don't have to send names of default named objects
        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.Name);
            writer.Write(SaveName);
            writer.Write(this.StackSize);
        }

        public override void Read(BinaryReader reader)
        {
            this.Name = reader.ReadString();
            this.SaveName = reader.ReadBoolean();
            this.StackSize = reader.ReadInt32();
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            // tag.Add(new SaveTag(SaveTag.Types.Bool, "CustomName", this.Name));
            if (SaveName)
                tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Stack", this.StackSize));
            return tag;
        }

        internal override void Load(SaveTag compTag)
        {
            this.StackSize = Math.Max(1, compTag.TagValueOrDefault<int>("Stack", 1));
            this.Name = compTag.TagOrDefault<string>("Name", tag =>
            {
                this.SaveName = true;
                return (string)tag.Value;
            }, this.Name);

            // this.Name = (string)compTag["Name"].Value;
        }
    }
}
