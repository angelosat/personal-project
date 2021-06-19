using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Items;
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
        public Quality Quality = Quality.Common;// { get { return (Quality)this["Quality"]; } protected set { this["Quality"] = value; } }


        private int _ID;
        public int ID//;// { get { return (int)this["ID"]; } protected set { this["ID"] = value; } }
        {
            get
            {
                //throw new Exception();
                return this.Def?.ID ?? this._ID;
            }
            set
            {
                //throw new Exception();
                this._ID = value;
            }
        }


        public string CustomName = "";
        public string Name//;// { get { return (string)this["Name"]; } set { this["Name"] = value; } }
        {
            //get { return this.Def?.Name ?? this._Name; }
            get { return string.IsNullOrEmpty(this.CustomName) ? this.Def.Label : this.CustomName; } //this.Def?.Name ?? this._Name; }
            set
            {
                this.CustomName = value;
            }
        }
            //{ get { return this.Prefix + " " + this.Def?.Name ?? this._Name; } set { this._Name = value; } }
        //private string _Name;

        private string _Type;
        public string Type//;// { get { return (string)this["Type"]; } set { this["Type"] = value; } }
        { get { return this.Def?.ObjType ?? this._Type; } set { this._Type = value; } }

        string _Description;
        public string Description//;// { get { return (string)this["Description"]; } set { this["Description"] = value; } }
        { get { return this.Def?.Description ?? this._Description; } set { this._Description = value; } }

        int _StackCapacity;// { get { return (int)this["StackMax"]; } set { this["StackMax"] = value; } }
        public int StackMax//;// { get { return (string)this["Description"]; } set { this["Description"] = value; } }
        { get { return this.Def?.StackCapacity ?? this._StackCapacity; } set { this._StackCapacity = value; } }

        ItemSubType _ItemSubType;
        public ItemSubType ItemSubType// { get { return (ItemSubType)this["ItemSubType"]; } set { this["ItemSubType"] = value; } }
        { get { return this.Def?.SubType ?? this._ItemSubType; } set { this._ItemSubType = value; } }

        public string Prefix = "";
        //public bool CustomName;// { get { return (bool)this["SaveName"]; } set { this["SaveName"] = value; } }
        public bool SaveWithChunk;// { get { return (bool)this["SaveWithChunk"]; } set { this["SaveWithChunk"] = value; } }
        int _StackSize;
        public int StackSize
        {
            get { return this._StackSize; }
            set
            {
                if (value < 0)
                    throw new Exception();
                this._StackSize = value;
                //if (value == 0)
                    //Console.WriteLine("WARNING: STACKSIZE SET TO 0");
                //throw (new Exception("WARNING: STACKSIZE SET TO 0")); 
            }
        }

        public DefComponent Initialize(int id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null, bool saveName = false)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Type = objType;
            this.Quality = quality ?? Quality.Common;
            //this.CustomName = saveName;
            this.StackSize = this.StackMax = 1;
            this.ItemSubType = ItemSubType.Generic;
            return this;
        }
        internal override void Initialize(Entity parent, Quality quality)
        {
            this.Quality = quality;
        }
       
        public DefComponent Initialize(GameObject.Types id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null, bool saveName = false)//, int height = 1, int weight = -1)
        {
            return this.Initialize((int)id, objType, name, description, quality, saveName);
        }
        public DefComponent Initialize(ItemSubType subtype)
        {
            this.ItemSubType = subtype;
            return this;
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
            this.ItemSubType = ItemSubType.Generic;
        }

        public DefComponent(GameObject.Types id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null)//, int height = 1, int weight = -1)
        {
            this.ID = (int)id;
            this.Name = name;
            this.Description = description;
            this.Type = objType;
            this.Quality = quality ?? Quality.Common;
            this.StackSize = this.StackMax = 1;
            this.ItemSubType = ItemSubType.Generic;
        }
        public DefComponent(int id, string objType = "<undefined>", string name = "<undefined>", string description = "<undefined>", Quality quality = null)//, int height = 1, int weight = -1)
        {
            this.ID = id;
            this.Name = name;
            this.Description = description;
            this.Type = objType;
            this.Quality = quality ?? Quality.Common;
            //this.CustomName = false;
            this.StackSize = this.StackMax = 1;
            this.ItemSubType = ItemSubType.Generic;
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
            phys.ItemSubType = this.ItemSubType;
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
            tooltip.Controls.Add(new Label(this.Quality.Label + " " + this.ItemSubType.ToString()) { Fill = Color.Gold, Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.Gold });
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

            //if (this.CustomName)
            //    w.Write(this.Name);
            w.Write(this.CustomName);// ?? "");
            w.Write(this.StackSize);
            w.Write(this.Quality.Name);
        }

        public override void Read(BinaryReader r)
        {

            //if(this.CustomName)
            //    this.Name = r.ReadString();
            //this.CustomName = r.ReadBoolean();
            //this._Name = r.ReadString();

            //var customName = r.ReadString();
            //this.CustomName = string.IsNullOrEmpty(customName) ? null : customName;
            this.CustomName = r.ReadString();
            this.StackSize = r.ReadInt32();
            this.Quality = Start_a_Town_.Def.GetDef<Quality>(r.ReadString());
        }

        internal override List<SaveTag> Save()
        {
            var tag = new List<SaveTag>
            {
                // tag.Add(new SaveTag(SaveTag.Types.Bool, "CustomName", this.Name));
                //if(this.CustomName!=null)
                this.CustomName.Save("CustomName"),
                //tag.Add(this._Name.Save("CustomName"));

                //if (CustomName)
                //    tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
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
            //if(Game1)
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
        public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera, GameObject parent)
        {
            //if(camera.Zoom > 6f)
            //    UIManager.DrawStringOutlined(sb, this.Name, parent.GetScreenPosition(camera) - new Vector2(0, parent.Body.Sprite.AtlasToken.Rectangle.Height * camera.Zoom / 2 - Label.DefaultHeight), Color.Black, Color.Gray, Color.Black * .5f, HorizontalAlignment.Center);
            return;
            //if (this.StackSize == 1 || this.StackMax == 1 || camera.Zoom <= 2.5f)
            //    return;
            //UIManager.DrawStringOutlined(sb,
            //    GetNameplateText(camera), 
            //    //parent.GetScreenPosition(camera) - new Vector2(0, ( parent.Body.Sprite.OriginGround.Y / 2) * camera.Zoom / 2), 
            //    parent.GetScreenPosition(camera) - new Vector2(0, (parent.Body.Sprite.AtlasToken.Rectangle.Height) * camera.Zoom / 2), 
            //    Color.Black, Color.Gray, Color.Black *.5f, HorizontalAlignment.Center);//, VerticalAlignment.Center);
        }

        private string GetNameplateText(Camera camera)
        {
            //return (ScreenManager.CurrentScreen.Camera.Zoom > 6f ? string.Format("{0} ", this.Name) : string.Empty) + string.Format("({0})", this.StackSize);
            return (camera.Zoom > 6f ? string.Format("{0} ", this.Name) : string.Empty) + string.Format("({0})", this.StackSize);
        }

        public override void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            plate.Controls.Add(new Label()
            {
                //Width = 100,
                Font = UIManager.FontBold,
                //TextFunc = GetName,// this.GetNameplateText,
                TextFunc = () => parent.Name,
                TextColorFunc = parent.GetNameplateColor,// this.GetQualityColor,
                MouseThrough = true,
                TextBackgroundFunc = () => parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
                //TextBackgroundFunc = () => Color.Red,//parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f
                //BackgroundColorFunc = () => parent.HasFocus() ? this.Quality.Color * .5f : Color.Black * .5f

            });
            //plate.AddControlsBottomLeft(new Label("test") { TextBackgroundFunc = () => Color.Lime });
            //plate.BackgroundColorFunc = () => parent.HasFocus() ? Color.Black * .5f : this.Quality.Color * .5f;

        }

    }

    class WindowSetStackSize : Window
    {
        TextBox TextBox;
        Action<int> Callback;
        public WindowSetStackSize(int initial, Action<int> callback)
        {
            this.AutoSize = true;
            this.Title = "Set stack size";
            this.TextBox = new TextBox(100) { Text = initial.ToString() };
            var btnok = new Button("Done") { Location = this.TextBox.BottomLeft, LeftClickAction = Done };
            this.Callback = callback;
            this.Client.AddControls(this.TextBox, btnok);
            //this.Client.AlignTopToBottom();
        }
        public WindowSetStackSize(Action<int> callback)
            : this(1, callback)
        {
            //this.AutoSize = true;
            //this.Title = "Set stack size";
            //this.TextBox = new TextBox(100);
            //var btnok = new Button("Done") { Location = this.TextBox.BottomLeft, LeftClickAction = Done };
            //this.Callback = callback;
            //this.Client.AddControls(this.TextBox, btnok);
        }

        private void Done()
        {
            var txt = this.TextBox.Text;
            int amount;
            if (!Int32.TryParse(txt, out amount))
                return;
            this.Callback(amount);
            this.Hide();
        }
    }
}
