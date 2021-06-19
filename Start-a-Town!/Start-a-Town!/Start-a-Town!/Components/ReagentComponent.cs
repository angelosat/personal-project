using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ReagentComponent : Component
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
            return new ReagentComponent() { Products = this.Products }; //should i clone?
        }

        public List<Reaction.Product.Types> Products { get { return (List<Reaction.Product.Types>)this["ProductTypes"]; } set { this["ProductTypes"] = value; } }


        public ReagentComponent Initialize(params Reaction.Product.Types[] products)
        {
            foreach (var p in products)
                this.Products.Add(p);
            return this;
        }

        public override void MakeChildOf(GameObject parent)
        {
            Registry.Add((int)parent.ID);
        }
        public ReagentComponent()
        {
            this.Products = new List<Reaction.Product.Types>();
        }



        public override void GetTooltip(GameObject parent, UI.Control tooltip)
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
