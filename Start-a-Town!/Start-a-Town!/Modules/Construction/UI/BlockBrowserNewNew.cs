using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Towns.Constructions;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class BlockBrowserNewNew : GroupBox
    {
        readonly Dictionary<Block, ProductMaterialPair> LastSelectedVariant = new();
        ConstructionCategory SelectedCategory;
        readonly Panel Panel_Blocks;
        readonly UIToolsBox ToolBox;
        Block CurrentSelected;
        readonly Dictionary<ConstructionCategory, ButtonGridIcons<Block>> Categories = new();
       
        public BlockBrowserNewNew()
        {
            this.Panel_Blocks = new Panel() { AutoSize = true };

            var blocks = Block.Registry.Values.Skip(1).ToList(); //skip air LOL FIX THIS
            this.ToolBox = new UIToolsBox(null, this.OnToolSelected);
            var categories = ConstructionsManager.AllCategories;
            foreach (var cat in categories)
            {
                var list = cat.List.Where(b => b.Ingredient != null);

                var grid = new ButtonGridIcons<Block>(4, 6, list, (slot, block) =>
                {
                    slot.Tag = block;
                    slot.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolDrawing drawing && drawing.Block == block;
                    slot.PaintAction = () => block.PaintIcon(slot.Width, slot.Height, block.GetDataFromMaterial(this.GetLastSelectedVariantOrDefault(block).Requirement.Material));
                    slot.LeftClickAction = () =>
                    {
                        this.CurrentSelected = block;
                        var product = this.GetLastSelectedVariantOrDefault(block);
                        this.ToolBox.SetProduct(product, this.OnToolSelected);
                        var tools = product.Block.ConstructionCategory.GetAvailableTools(() => this.GetLastSelectedVariantOrDefault(this.CurrentSelected));

                        this.ToolBox.SetProduct(product, this.OnToolSelected);
                        this.OnToolSelected(this.ToolBox.LastSelectedTool.GetType());
                        var win = this.ToolBox.GetWindow();
                        if (win is null)
                        {
                            win = this.ToolBox.ToWindow("Brushes");
                            win.HideAction = () => ToolManager.SetTool(null);
                        }
                        if (win.Show())
                            win.Location = this.GetWindow().BottomLeft;
                    };
                    slot.RightClickAction = () =>
                    {
                        UIBlockVariationPickerNew.Refresh(block, this.OnVariationSelected);
                    };

                    slot.HoverFunc = () => $"{block.Name}\n{this.GetLastSelectedVariantOrDefault(block).Requirement}\nTool necessity: {block.BuildProperties.ToolSensitivity:##0%}\nRight click to select variation";
                })
                { Location = this.Panel_Blocks.Controls.BottomLeft };
                this.Categories[cat] = grid;
            }
            this.SelectedCategory = this.Categories.First().Key;
            this.Panel_Blocks.Controls.Add(this.Categories[this.SelectedCategory]);

            var cbox = new ComboBoxNew<ConstructionCategory>(
                        new ButtonGridGenericNew<ConstructionCategory>(
                            categories,
                            (c, b) =>
                            {
                                b.LeftClickAction = () =>
                                {
                                    this.Panel_Blocks.ClearControls();
                                    this.Panel_Blocks.AddControls(this.Categories[c]);
                                    this.SelectedCategory = c;
                                };
                            }),
                        this.SelectedCategory.Name,
                        this.Categories[this.SelectedCategory].Width);

            this.AddControlsVertically(
                cbox.ToPanel(),
                this.Panel_Blocks
                );
        }

        void OnToolSelected(Type toolType)
        {
            var tool = this.SelectedCategory.CreateTool(toolType, this.GetLastSelectedVariantOrDefault(this.CurrentSelected));
            this.ToolBox.LastSelectedTool = tool;
            ToolManager.SetTool(tool);
        }

        private ProductMaterialPair GetLastSelectedVariantOrDefault(Block block)
        {
            if (this.LastSelectedVariant.ContainsKey(block))
                return this.LastSelectedVariant[block];
            else
                return new ProductMaterialPair(block, block.GetAllValidConstructionMaterialsNew().First()); // store last selected variant instead of getting first in list
        }
        private void OnVariationSelected(ProductMaterialPair product)
        {
            if (product is null)
                return;
            this.LastSelectedVariant[product.Block] = product;
            this.CurrentSelected = product.Block;
            if (this.ToolBox.LastSelectedTool != null)
            {
                var tool = this.SelectedCategory.CreateTool(this.ToolBox.LastSelectedTool.GetType(), product);
                this.ToolBox.LastSelectedTool = tool;
                ToolManager.SetTool(tool);
            }
            this.Categories[this.SelectedCategory].FindChild(c => c.Tag == product.Block).Invalidate();
        }
        public override bool Hide()
        {
            this.CurrentSelected = null;
            if (this.ToolBox.GetWindow() != null)
                this.ToolBox.GetWindow().Hide();
            ToolManager.SetTool(null);
            return base.Hide();
        }
    }
}
