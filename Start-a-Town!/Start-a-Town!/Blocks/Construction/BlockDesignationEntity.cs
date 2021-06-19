using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;

namespace Start_a_Town_.Blocks
{
    partial class BlockDesignation
    {
        public class BlockDesignationEntity : BlockEntity, IConstructible
        {
            BlockRecipe.ProductMaterialPair _Product;
            public BlockRecipe.ProductMaterialPair Product
            {
                get { return this._Product; }
                set
                {
                    if (this._Product == null)
                        this.BuildProgress = new Progress(0, value.Recipe.WorkAmount, 0);
                    this._Product = value;
                }
            }
            public List<ItemRequirement> Materials = new List<ItemRequirement>();
            public Container Material;
            public Progress BuildProgress { get; set; }// = new Progress();
            public List<Vector3> Children { get; set; } = new List<Vector3>();
            //public Vector3 Origin { get; set; }
            public BlockDesignationEntity()
            {
                this.Material = new Container(1, this.MaterialValid);
            }
            public BlockDesignationEntity(BlockRecipe.ProductMaterialPair product, Vector3 origin)
            {
                this.OriginGlobal = origin;
                //this.Origin = origin;
                this.Product = product;
                //this.Materials.Add(new ItemRequirement(product.Req));
                //this.Material = new Container(1, this.MaterialValid);
                //this.BuildProgress = new Progress(0, product.Recipe.WorkAmount, 0);
            }

            public bool IsValidHaulDestination(ItemDef def)
            {
                var valid = this.Product.Requirement.Def == def;
                if (!valid)
                {

                }
                return valid;
                //return this.Materials.Any(m => m.ObjectID == itemID && m.Remaining > 0);
            }
            //public bool IsValidHaulDestination(int itemID)
            //{
            //    return this.Materials.Any(m => m.ObjectID == itemID && m.Remaining > 0);
            //}
            //public bool IsValidHaulDestination(int objectID)
            //{
            //    return this.Product.Materials.Any(m => m.ObjectID == objectID);
            //}
            public bool IsReadyToBuild()
            {
                return !this.Materials.Any(m => m.Remaining > 0);
            }
            //public bool IsReadyToBuild(out ItemDefAmount missingMat)
            //{
            //    missingMat = this.Product.Block.Reagent;
            //    return missingMat == null;
            //}
            public bool IsReadyToBuild(out ItemDef def, out Material mat, out int amount)
            {
                var product = this.Product;
                def = product.Requirement.Def;
                amount = product.Requirement.Amount;
                mat = product.Requirement.Material;
                return false;

                //var product = this.Product;
                //def = product.Block.Reagent.Def;
                //amount = product.Block.Reagent.Amount;
                //mat = product.MainMaterial;
                //return false;
            }
            //public bool IsReadyToBuild(out ItemRequirement missingMat)
            //{
            //    missingMat = this.Materials.FirstOrDefault(m => m.Remaining > 0);
            //    return missingMat == null;
            //}
            public bool MaterialValid(GameObject obj)
            {
                if (obj == null)
                    return false;
                return (int)obj.IDType == this.Product.Req.ObjectID && obj.StackSize >= this.Product.Req.AmountCurrent;
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
                return new BlockDesignationEntity(this.Product, this.OriginGlobal);
            }
            public override void GetTooltip(UI.Control tooltip)
            {
                var product = this.Product;
                var req = product.Requirement;
                //var block = product.Block;
                //var ing = block.Ingredient;
                tooltip.AddControlsBottomLeft(new Label()
                {
                    TextFunc = () => $"{product.Requirement.Material.Name} {product.Requirement.Def.Label} {0} / {product.Requirement.Amount}"
                    //TextFunc = () => $"{ing.Material.Name} {ing.ItemDef.Label} 0 / {ing.Amount}"
                });
                //foreach (var mat in this.Materials)
                //    tooltip.Controls.Add(mat.GetUI(tooltip.Controls.BottomLeft));
            }
            internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
            {
                var product = this.Product;
                //info.LabelName.Text = product.Block.Name + " (Designation)";
                info.AddInfo(new Label() { TextFunc = () => string.Format("{0} {1} {2} / {3}", product.MainMaterial.Name, product.Requirement.Material.Name, product.Requirement.Def.Label, 0, product.Requirement.Amount) });
            }
            public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global)
            {
                if (ToolManager.Instance.ActiveTool != null)
                    if (ToolManager.Instance.ActiveTool.Target != null)
                        if (ToolManager.Instance.ActiveTool.Target.Type == TargetType.Position && ToolManager.Instance.ActiveTool.Target.Global == global)
                            Bar.Draw(sb, cam, global + Vector3.UnitZ, "", this.BuildProgress.Percentage, cam.Zoom * .2f);
            }

            protected override void AddSaveData(SaveTag tag)
            {
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Product", this.Product.Save()));
                //tag.Add(new SaveTag(SaveTag.Types.Vector3, "Origin", this.Origin.Save()));
                //tag.Add(new SaveTag(SaveTag.Types.Vector3, "Origin", this.Origin));
                tag.Add(this.Children.Save("Children"));

                var matstag = new SaveTag(SaveTag.Types.List, "Materials", SaveTag.Types.Compound);
                foreach (var mat in this.Materials)
                    //matstag.Add(new SaveTag(SaveTag.Types.Compound, "", mat.Save()));
                    matstag.Add(mat.Save(""));
                tag.Add(matstag);
                tag.Add(this.BuildProgress.Save("BuildProgress"));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Product", t => this.Product = new BlockRecipe.ProductMaterialPair(t));
                //tag.TryGetTagValue<Vector3>("Origin", t => this.Origin = t);
                tag.TryGetTagValue<List<SaveTag>>("Children", t => this.Children.Load(t));

                SaveTag matstag;
                if (!tag.TryGetTag("Materials", out matstag))
                    return;
                foreach (var mat in matstag.Value as List<SaveTag>)
                    this.Materials.Add(new ItemRequirement(mat));
                tag.TryGetTag("BuildProgress", v => this.BuildProgress = new Progress(v));
            }

            protected override void WriteExtra(BinaryWriter w)
            {
                this.Product.Write(w);
                this.BuildProgress.Write(w);
                //w.Write(this.Origin);
                w.Write(this.Children);
                w.Write(this.Materials.Count);
                foreach (var mat in this.Materials)
                    mat.Write(w);

            }
            protected override void ReadExtra(BinaryReader r)
            {
                this.Product = new BlockRecipe.ProductMaterialPair(r);
                this.BuildProgress = new Progress(r);
                //this.Origin = r.ReadVector3();
                this.Children = r.ReadListVector3();
                var matcount = r.ReadInt32();
                for (int i = 0; i < matcount; i++)
                {
                    this.Materials.Add(new ItemRequirement(r));
                }
            }
            public int GetMissingAmount(ItemDef def)
            {
                return this.Product.GetMaterialRequirement(def);
            }
        }
    }
}
