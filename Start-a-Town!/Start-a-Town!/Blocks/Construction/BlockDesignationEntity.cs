﻿using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;

namespace Start_a_Town_.Blocks
{
    partial class BlockDesignation
    {
        public class BlockDesignationEntity : BlockEntity, IConstructible
        {
            ProductMaterialPair _product;
            public ProductMaterialPair Product
            {
                get => this._product;
                set
                {
                    if (this._product is null)
                        this.BuildProgress = new Progress(0, value.Block.WorkAmount, 0);
                    this._product = value;
                }
            }
            public Container Material;
            public Progress BuildProgress { get; set; }
            public List<Vector3> Children { get; set; } = new List<Vector3>();
            public BlockDesignationEntity()
            {
            }
            public BlockDesignationEntity(ProductMaterialPair product, Vector3 origin)
            {
                this.OriginGlobal = origin;
                this.Product = product;
            }

            public bool IsValidHaulDestination(ItemDef def)
            {
                var valid = this.Product.Requirement.Def == def;
                if (!valid)
                {

                }
                return valid;
            }
           
            public bool IsReadyToBuild(out ItemDef def, out Material mat, out int amount)
            {
                var product = this.Product;
                def = product.Requirement.Def;
                amount = product.Requirement.Amount;
                mat = product.Requirement.Material;
                return false;
            }
            
            public override object Clone()
            {
                return new BlockDesignationEntity(this.Product, this.OriginGlobal);
            }
            public override void GetTooltip(Control tooltip)
            {
                var product = this.Product;
                var req = product.Requirement;
                tooltip.AddControlsBottomLeft(new Label()
                {
                    TextFunc = () => $"{product.Requirement.Material.Name} {product.Requirement.Def.Label} {0} / {product.Requirement.Amount}"
                });
            }
            internal override void GetSelectionInfo(IUISelection info, MapBase map, Vector3 vector3)
            {
                var product = this.Product;
                var req = product.Requirement;
                info.AddInfo(new Label() { TextFunc = () => $"{req.Material.Name} {req.Def.Label} {0} / {req.Amount}" });
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
                tag.Add(this.Children.Save("Children"));
                tag.Add(this.BuildProgress.Save("BuildProgress"));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Product", t => this.Product = new ProductMaterialPair(t));
                tag.TryGetTagValue<List<SaveTag>>("Children", t => this.Children.Load(t));
                tag.TryGetTag("BuildProgress", v => this.BuildProgress = new Progress(v));
            }

            protected override void WriteExtra(BinaryWriter w)
            {
                this.Product.Write(w);
                this.BuildProgress.Write(w);
                w.Write(this.Children);

            }
            protected override void ReadExtra(BinaryReader r)
            {
                this.Product = new ProductMaterialPair(r);
                this.BuildProgress = new Progress(r);
                this.Children = r.ReadListVector3();
            }
            public int GetMissingAmount(ItemDef def)
            {
                return this.Product.GetMaterialRequirement(def);
            }
        }
    }
}
