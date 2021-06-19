﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Towns.Constructions;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_
{
    class BlockBrowserNew : GroupBox// ScrollableBox
    {
        Dictionary<Block, BlockRecipe.ProductMaterialPair> LastSelectedVariant = new Dictionary<Block, BlockRecipe.ProductMaterialPair>();
        ConstructionCategory SelectedCategory;
        Panel Panel_Blocks;//, Panel_Variants;
        ButtonGridIcons<BlockRecipe> GridBlocks;
        UIToolsBox ToolBox;
        BlockRecipe CurrentSelected;
        Dictionary<ConstructionCategory, ButtonGridIcons<BlockRecipe>> Categories = new Dictionary<ConstructionCategory, ButtonGridIcons<BlockRecipe>>();
        public BlockBrowserNew(ConstructionCategory category)
        {

        }
        public BlockBrowserNew()
        {
            //this.Panel_Blocks = new PanelLabeled("Constsructions") { AutoSize = true };
            this.Panel_Blocks = new Panel() { AutoSize = true };

            var blocks = Block.Registry.Values.Skip(1).ToList(); //skip air
            //this.Category = category;
            //var list = this.Category.List;
            this.ToolBox = new UIToolsBox(null, OnToolSelected);
            var categories = Start_a_Town_.Towns.Constructions.ConstructionsManager.AllCategories;
            foreach (var cat in categories)
            {
                var list = cat.List;
                var grid = new ButtonGridIcons<BlockRecipe>(4, 6, list, (slot, bl) =>
                {
                    var block = bl.BlockProduct.Block;
                    slot.Tag = block;
                    //slot.Togglable = true;
                    //var variants = bl.BlockProduct.Block.GetCraftingVariations();

                    slot.IsToggledFunc = () => { var drawing = ToolManager.Instance.ActiveTool as ToolDrawing; return drawing != null ? drawing.Block == block : false; };
                    //slot.IsToggledFunc = () => this.CurrentSelected != null && slot.Tag == this.CurrentSelected.BlockProduct.Block;
                    slot.PaintAction = () =>
                    {
                        bl.BlockProduct.Block.PaintIcon(slot.Width, slot.Height, GetLastSelectedVariantOrDefault(block).Data);
                    };
                    slot.LeftClickAction = () =>
                    {
                        this.CurrentSelected = bl;
                        BlockRecipe.ProductMaterialPair product = GetLastSelectedVariantOrDefault(bl);

                        this.ToolBox.SetProduct(product, OnToolSelected);
                        var tools = product.Recipe.Category.GetAvailableTools(() => GetLastSelectedVariantOrDefault(this.CurrentSelected));
                        //ToolManager.SetTool(this.ToolBox.CurrentTool);
                        OnToolSelected(this.ToolBox.LastSelectedTool.GetType());
                        var win = this.ToolBox.GetWindow();
                        if (win == null)
                        {
                            win = this.ToolBox.ToWindow("Brushes");
                            win.HideAction = () => ToolManager.SetTool(null);
                        }
                        if (win.Show())
                            win.Location = this.GetWindow().BottomLeft;
                    };
                    slot.RightClickAction = () =>
                    {
                        UIBlockVariationPicker.Refresh(bl.Block, this.OnVariationSelected);
                    };
                    slot.HoverFunc = () => string.Format(
@"{0}

Right click to select variation", GetLastSelectedVariantOrDefault(bl.Block).GetName());
                }) { Location = this.Panel_Blocks.Controls.BottomLeft };
                this.Categories[cat] = grid;
            }
            this.SelectedCategory = this.Categories.First().Key;
            this.Panel_Blocks.Controls.Add(this.Categories[SelectedCategory]);

            var dropdownpanel = new Panel() { AutoSize = true }
                .AddControls(
                    new ComboBoxNew<ConstructionCategory>(
                        new ButtonGridGenericNew<ConstructionCategory>(
                            categories, 
                            (c, b) => {
                                b.LeftClickAction = () =>
                                {
                                    this.Panel_Blocks.ClearControls();
                                    this.Panel_Blocks.AddControls(this.Categories[c]);
                                    this.SelectedCategory = c;
                                };
                            }),
                        this.SelectedCategory.Name,
                        this.Categories[SelectedCategory].Width));


            //this.Panel_Variants = new PanelLabeled("Variations")
            //{
            //    Location = this.Panel_Blocks.BottomLeft
            //};

            this.AddControlsVertically(
                dropdownpanel, 
                //this.ComboCategories,
                this.Panel_Blocks
                );
        }

        
        void OnToolSelected(Type toolType)
        {
            var tool = this.SelectedCategory.CreateTool(toolType, this.GetLastSelectedVariantOrDefault(this.CurrentSelected.Block));// this.LastSelectedVariant[this.CurrentSelected.Block]);
            this.ToolBox.LastSelectedTool = tool;
            //tool.Block = this.CurrentSelected.Block;
            ToolManager.SetTool(tool);
        }
        private BlockRecipe.ProductMaterialPair GetLastSelectedVariantOrDefault(BlockRecipe bl)
        {
            return this.GetLastSelectedVariantOrDefault(bl.Block);
        }
        private BlockRecipe.ProductMaterialPair GetLastSelectedVariantOrDefault(Block block)
        {
            if (this.LastSelectedVariant.ContainsKey(block))
                return this.LastSelectedVariant[block];
            else
                return block.Recipe.GetVariants().First(); // store last selected variant instead of getting first in list
        }
        private void OnVariationSelected(BlockRecipe.ProductMaterialPair product)
        {
            if (product == null)
                return;
            this.LastSelectedVariant[product.Block] = product;
            this.CurrentSelected = product.Recipe;
            if (this.ToolBox.LastSelectedTool != null)
            {
                var tool = this.SelectedCategory.CreateTool(this.ToolBox.LastSelectedTool.GetType(), product);// product.Recipe.GetVariant(data));
                //tool.Block = product.Block;
                this.ToolBox.LastSelectedTool = tool;
                ToolManager.SetTool(tool);
            }
            //this.GridBlocks.FindChild(c => c.Tag == product.Block).Invalidate();
            this.Categories[this.SelectedCategory].FindChild(c => c.Tag == product.Block).Invalidate();
        }
        private void PaintIcon(IconButton slot, Cell cell)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            var token = cell.Block.GetDefault();
            Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);
            var loc = new Vector2(rect.X, rect.Y);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.Parameters["Viewport"].SetValue(new Vector2(slot.Width, slot.Height));
            //gd.Textures[0] = Block.Atlas.Texture;
            //gd.Textures[1] = Block.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //var material = tag.Block.GetColor(tag.BlockData);// tag.Block.GetMaterial(tag.BlockData);
            var bounds = new Vector4((slot.Width - Block.Width) / 2, (slot.Height - Block.Height) / 2, token.Texture.Bounds.Width, token.Texture.Bounds.Height);
            var cam = new Camera();
            cam.SpriteBatch = mysb;
            Block.Atlas.Begin(mysb);
            cell.Block.Draw(mysb, Vector3.Zero, cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, cell.BlockData);
            mysb.Flush();
        }
        public override bool Hide()
        {
            this.CurrentSelected = null;
            if (this.ToolBox.GetWindow()!=null)
            this.ToolBox.GetWindow().Hide();
            ToolManager.SetTool(null);
            return base.Hide();
        }
        public override void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.HandleRButtonDown(e);
            //if (e.Handled)
            //    return;
            //if (this.CurrentSelected != null)
            //{
            //    this.CurrentSelected = null;
            //    ToolManager.SetTool(null);
            //    e.Handled = true;
            //}
        }
    }


}
