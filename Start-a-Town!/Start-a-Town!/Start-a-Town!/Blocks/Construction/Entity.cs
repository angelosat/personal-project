using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;

namespace Start_a_Town_.Blocks
{
    partial class BlockDesignation
    {
        public class Entity : BlockEntity
        {
            public BlockConstruction.ProductMaterialPair Product;
            public List<ItemRequirement> Materials = new List<ItemRequirement>();
            public Container Material;

            public Entity()
            {

            }
            public Entity(BlockConstruction.ProductMaterialPair product)
            {
                this.Product = product;
                this.Materials.Add(new ItemRequirement(product.Req));
                //this.Material = new Container(1, obj => (int)obj.ID == product.Req.ObjectID);
                this.Material = new Container(1, this.MaterialValid);

            }
            public bool MaterialValid(GameObject obj)
            {
                if (obj == null)
                    return false;
                return (int)obj.ID == this.Product.Req.ObjectID;
            }
            public bool MaterialsPresent()
            {
                foreach (var mat in this.Materials)
                    if (mat.Remaining > 0)
                        return false;
                return true;
            }
            public override object Clone()
            {
                return new Entity(this.Product);
            }
            public override void GetTooltip(UI.Control tooltip)
            {
                foreach (var mat in this.Materials)
                {
                    tooltip.Controls.Add(mat.GetUI(tooltip.Controls.BottomLeft));
                }
                //tooltip.Controls.Add(new Bar()
                //{
                //    Width = 200,
                //    Name = "Grows in: ",
                //    Location = tooltip.Controls.BottomLeft
                //});
            }

            public override SaveTag Save(string name)
            {
                SaveTag tag = new SaveTag(SaveTag.Types.Compound, name);
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Save()));
                return tag;
            }
            public override void Load(SaveTag tag)
            {
                tag.TryGetTag("Product", t => this.Product = new BlockConstruction.ProductMaterialPair(t));
            }

            public override void Write(BinaryWriter w)
            {
                this.Product.Write(w);
            }
            public override void Read(BinaryReader r)
            {
                this.Product = new BlockConstruction.ProductMaterialPair(r);
            }
        }
    }
}
