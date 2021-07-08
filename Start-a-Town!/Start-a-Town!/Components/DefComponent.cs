using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ObjectType
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
    }
    public class DefComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Info";
            }
        }

        public ItemDef Def;
       
        public bool InCatalogue = true;
        public Quality Quality = Quality.Common;


        private int _ID;
        public int ID
        {
            get
            {
                return this.Def?.ID ?? this._ID;
            }
            set
            {
                this._ID = value;
            }
        }


        public string CustomName = "";
        public string Name
        {
            get { return string.IsNullOrEmpty(this.CustomName) ? this.Def.Label : this.CustomName; }
            set
            {
                this.CustomName = value;
            }
        }

        private string _Type;
        public string Type
        { get { return this.Def?.ObjType ?? this._Type; } set { this._Type = value; } }

        string _Description;
        public string Description
        { get { return this.Def?.Description ?? this._Description; } set { this._Description = value; } }

        int _StackCapacity;
        public int StackMax
        { get { return this.Def?.StackCapacity ?? this._StackCapacity; } set { this._StackCapacity = value; } }

        public string Prefix = "";
        public bool SaveWithChunk;
        int _StackSize;
        public int StackSize
        {
            get { return this._StackSize; }
            set
            {
                if (value < 0)
                    throw new Exception();
                this._StackSize = value;
            }
        }

        public DefComponent Initialize(int id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null, bool saveName = false)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Type = objType;
            this.Quality = quality ?? Quality.Common;
            this.StackSize = this.StackMax = 1;
            return this;
        }
        internal override void Initialize(Entity parent, Quality quality)
        {
            this.Quality = quality;
        }
       
        public DefComponent(ItemDef def)
        {
            this.Def = def;
        }
        
        public DefComponent()
            : base()
        {
            this.SaveWithChunk = true;
            this.Description = "";
            this.Type = "";
            Quality = Quality.Common;
            this.StackSize = 1;
            this.StackMax = 1;// 256; //default items are unstackable
        }

       
        public override object Clone()
        {
            DefComponent phys = new DefComponent();
            phys.Def = this.Def;
            phys.CustomName = this.CustomName;
            phys.ID = this.ID;
            phys.Quality = this.Quality;
            phys.Description = this.Description;
            phys.StackMax = this.StackMax;
            phys.StackSize = this.StackSize;
            phys.SaveWithChunk = this.SaveWithChunk;
            return phys;
        }

        public string GetName()
        {
            return $"{this.Name}{(this.StackSize > 1 ? $" (x{this.StackSize})" : "")}";
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            tooltip.Color = GetQualityColor();
            var namelabel = new Label(Vector2.Zero, this.GetName(), tooltip.Color, Color.Black, UIManager.FontBold) { TextColorFunc = () => tooltip.Color, TextFunc = this.GetName };
            tooltip.Controls.Add(namelabel);
            tooltip.Controls.Add(new Label(this.Quality.Label/* + " " + this.ItemSubType.ToString()*/) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });
            tooltip.Controls.Add(new Label(this.Description) { Location = tooltip.Controls.BottomLeft });
        }
        static public Color GetQualityColor(GameObject obj)
        {
            return (obj.GetInfo().Quality).Color;
        }

        public Color GetQualityColor()
        {
            return Quality.Color;
        }

        // TODO: maybe optimize this by a custom name flag so i don't have to send names of default named objects
        public override void Write(BinaryWriter w)
        {
            w.Write(this.CustomName);
            w.Write(this.StackSize);
            w.Write(this.Quality.Name);
        }

        public override void Read(BinaryReader r)
        {
            this.CustomName = r.ReadString();
            this.StackSize = r.ReadInt32();
            this.Quality = Start_a_Town_.Def.GetDef<Quality>(r.ReadString());
        }

        internal override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>
            {
                this.CustomName.Save("CustomName"),
                new SaveTag(SaveTag.Types.Int, "Stack", this.StackSize),
                this.Quality.Save("Quality")
            };
            return tag;
        }

        internal override void Load(SaveTag tag)
        {
            this.StackSize = Math.Max(1, tag.TagValueOrDefault<int>("Stack", 1));
            tag.TryGetTagValue<string>("CustomName", v => this.CustomName = v);
            tag.TryGetTagValue<string>("Quality", s => this.Quality = Start_a_Town_.Def.GetDef<Quality>(s));
        }
        internal override void GetInterface(GameObject gameObject, Control ui)
        {
            if (this.StackMax > 1)
                ui.AddControls(new Label("Stack Size: " + this.StackSize.ToString() + "/" + this.StackMax.ToString()) { Active = true, LeftClickAction = () => EditStackSize(gameObject) });
        }

        static private void EditStackSize(GameObject parent)
        {
            var win = new WindowSetStackSize((a) => SetStackSize(parent, a));
            win.ShowDialog();
        }

        static private void SetStackSize(GameObject parent, int amount)
        {
            Net.Client.PlayerRemoteCall(new TargetArgs(parent), Message.Types.DebugSetStackSize, w => w.Write(amount));
        }
        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.DebugSetStackSize:
                    e.Data.Translate(parent.Net, r =>
                    {
                        var amount = r.ReadInt32();
                        this.StackSize = amount;
                    });
                    break;

                default:
                    break;
            }
        }
        internal override void HandleRemoteCall(GameObject gameObject, Message.Types type, BinaryReader r)
        {
            switch(type)
            {
                case Message.Types.DebugSetStackSize:
                    var amount = r.ReadInt32();
                    this.StackSize = Math.Min(this.StackMax, Math.Max(0, amount));
                    break;

                default:
                    break;
            }
        }
      
        public override void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            plate.Controls.Add(new Label()
            {
                Font = UIManager.FontBold,
                TextFunc = () => parent.Name,
                TextColorFunc = parent.GetNameplateColor,
                MouseThrough = true,
                TextBackgroundFunc = () => parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f

            });

        }
    }
}
