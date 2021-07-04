using System;
using System.Collections.Generic;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Towns.Constructions
{
    [Obsolete]
    class UIBlockVariationPicker : GroupBox
    {
        static UIBlockVariationPicker _Instance;
        public static UIBlockVariationPicker Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new UIBlockVariationPicker();
                return _Instance;
            }
        }

        Panel Panel;
        UIBlockVariationPicker()
        {
            this.Panel = new Panel();
        }
        static public void Refresh(Block block, Action<BlockRecipe.ProductMaterialPair> action)
        {
            var variants = block.Recipe.GetVariants();
            Instance.Panel.Controls.Clear();
            Instance.Panel.AutoSize = true;
            Instance.ClearControls();
            List<Cell> variations = new List<Cell>();
            var buttonGrid = new ButtonGridIcons<BlockRecipe.ProductMaterialPair>(8, 2, variants, (btn, product) =>
            {
                var cell = new Cell() { Block = block, BlockData = product.Data };
                btn.Tag = cell;
                btn.PaintAction = () =>
                {
                    cell.Block.PaintIcon(btn.Width, btn.Height, cell.BlockData);
                };
                btn.HoverFunc = product.GetName;
                btn.LeftClickAction = () =>
                {
                    action(product);
                };
            });
            Instance.Panel.AddControls(buttonGrid);
            Instance.AddControls(Instance.Panel);
            if (Instance.Show())
                Instance.Location = UIManager.Mouse;
        }
        static public void RefreshOld(Block block, Action<Block, byte> action)
        {
            Instance.Panel.Controls.Clear();
            Instance.Panel.AutoSize = true;
            Instance.ClearControls();
            List<Cell> variations = new List<Cell>();
            foreach (var item in block.GetCraftingVariations())
                variations.Add(new Cell() { Block = block, BlockData = item });
            var buttonGrid = new ButtonGrid<Cell>(8, 2, variations, (btn, cell) =>
            {
                btn.Tag = cell;
                btn.PaintAction = () =>
                {
                    cell.Block.PaintIcon(btn.Width, btn.Height, cell.BlockData);
                };
                btn.HoverFunc = () => cell.Block.GetName(cell.BlockData);
                btn.LeftClickAction = () =>
                {
                    action(block, (btn.Tag as Cell).BlockData);
                };
            });
            Instance.Panel.AddControls(buttonGrid);
            Instance.AddControls(Instance.Panel);
            if(Instance.Show())
                Instance.Location = UIManager.Mouse;
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Hide();
            base.HandleLButtonUp(e);
        }
        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Hide();
            base.HandleRButtonDown(e);
        }
    }
}
