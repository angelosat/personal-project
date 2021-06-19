using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ReagentComponent : EntityComponent
    {
        readonly static public string Name = "Reagent";

        public override string ComponentName
        {
            get
            {
                return Name;// "Reagent";
            }
        }

        public static HashSet<int> Registry = new HashSet<int>();


        public override object Clone()
        {
            return new ReagentComponent() { Products = this.Products }; //should i deep copy??
        }

        public List<Reaction.Product.Types> Products = new List<Reaction.Product.Types>();// { get { return (List<Reaction.Product.Types>)this["ProductTypes"]; } set { this["ProductTypes"] = value; } }

        internal bool CanProduce(Reaction.Product.Types type)
        {
            return this.Products.Contains(type);
        }

        public ReagentComponent Initialize(params Reaction.Product.Types[] products)
        {
            foreach (var p in products)
                this.Products.Add(p);
            return this;
        }

        //public override void MakeChildOf(GameObject parent)
        //{
        //    Registry.Add((int)parent.IDType);
        //}
        public ReagentComponent()
        {

        }
        public ReagentComponent(params Reaction.Product.Types[] canProduce)
        {
            this.Products = new List<Reaction.Product.Types>(canProduce);
        }
        public ReagentComponent(ItemDef def)
        {
            this.Products = new List<Reaction.Product.Types>(def.CanProcessInto);
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {

            GroupBox tip = new GroupBox() { Location = tooltip.Controls.BottomLeft };

            if (this.Products.Count > 0)
            {
                tip.Controls.Add(new Label(tip.Controls.BottomLeft, "Can produce:") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
                foreach (var product in this.Products)
                    tip.Controls.Add(new Label(tip.Controls.BottomLeft, " " + product.ToString()));// { TextColorFunc = () => Color.GreenYellow });
            }
            tooltip.Controls.Add(tip);
        }

        //public override void Write(System.IO.BinaryWriter writer)
        //{
        //    writer.Write(this.Material.ID);
        //}
        //public override void Read(System.IO.BinaryReader reader)
        //{
        //    this.Material = Components.Materials.Material.Templates[reader.ReadInt32()];
        //}
        //internal override List<SaveTag> Save()
        //{
        //    var save = new List<SaveTag>();
        //    save.Add(new SaveTag(SaveTag.Types.Int, "Material", this.Material.ID));
        //    return save;
        //}
        //internal override void Load(SaveTag save)
        //{
        //    save.TryGetTagValue<int>("Material", v => this.Material = Materials.Material.Templates[v]);
        //}
    }    
}
