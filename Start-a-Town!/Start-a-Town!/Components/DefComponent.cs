using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class DefComponent : EntityComponent
    {
        public override string ComponentName => "Info";
            
        public ItemDef Def;
       
        public bool InCatalogue = true;
        public Quality Quality = Quality.Common;

        public string CustomName = "";
        public string Name
        {
            get => string.IsNullOrEmpty(this.CustomName) ? this.Def.Label : this.CustomName; 
            set => this.CustomName = value;
        }

        public string Description => this.Def.Description;
        public int StackMax => this.Def.StackCapacity;

        public string Prefix = "";
        public bool SaveWithChunk;
        int _StackSize;
        public int StackSize
        {
            get => this._StackSize; 
            set
            {
                if (value < 0)
                    throw new Exception();
                this._StackSize = value;
            }
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
            Quality = Quality.Common;
            this.StackSize = 1;
        }

       
        public override object Clone()
        {
            DefComponent phys = new DefComponent();
            phys.Def = this.Def;
            phys.CustomName = this.CustomName;
            phys.Quality = this.Quality;
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
