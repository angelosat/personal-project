using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns.Constructions
{
    class UIBlockVariationPickerNew : GroupBox
    {
        static UIBlockVariationPickerNew _Instance;
        public static UIBlockVariationPickerNew Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new UIBlockVariationPickerNew();
                return _Instance;
            }
        }

        Panel Panel = new Panel();

        static public void Refresh(Block block, Action<BlockRecipe.ProductMaterialPair> action)
        {
            //var variants = block.Reagent.Def.PreferredMaterialType.SubTypes.Select(m => new BlockRecipe.ProductMaterialPair(block, m));//.Recipe.GetVariants();
            //var variants = block.Ingredient.GetAllValidMaterials().Select(m => new BlockRecipe.ProductMaterialPair(block, m));
            //var variants = block.GetAllValidConstructionMaterials().Select(m => new BlockRecipe.ProductMaterialPair(block, m));
            var variants = block.GetAllValidConstructionMaterialsNew().Select(m => new BlockRecipe.ProductMaterialPair(block, m)).GroupBy(p => p.Requirement.Material);

            Instance.Panel.Controls.Clear();
            Instance.Panel.AutoSize = true;
            Instance.ClearControls();
            //var buttonGrid = new ButtonGridIcons<BlockRecipe.ProductMaterialPair>(4, 2, variants, (btn, product) =>
            //{
            //    btn.PaintAction = () =>
            //    {
            //        block.PaintIcon(btn.Width, btn.Height, product.Data);
            //    };
            //    btn.HoverFunc = product.GetName;
            //    btn.LeftClickAction = () =>
            //    {
            //        action(product);
            //    };
            //});

            var list = new ListBoxNew<BlockRecipe.ProductMaterialPair, ButtonNew>(200, 300);
            foreach (var group in variants)
                foreach (var variant in group)
                    list.AddItem(variant, v => "", (item, btn) => //v.Requirement.ToString()
                    {
                    //btn.BackgroundStyle = UI.BackgroundStyle.LargeButton;
                    //btn.AddControls(new PictureBox(variant.Block.PaintIcon(variant.Data)) { MouseThrough = true, Location = new Vector2(btn.BackgroundStyle.Left.Width, btn.Height / 2), Anchor = new Vector2(0, .5f) });//) { DrawAction = () => block.PaintIcon(Block.Width, Block.Height, variant.Data) });
                    //btn.AddControlsTopRight(new Label(item.Requirement.ToString()) { MouseThrough = true });

                    btn.BackgroundStyle = UI.BackgroundStyle.LargeButton;
                    //btn.AddControls(new PictureBox(variant.Block.PaintIcon(variant.Data)) { MouseThrough = true, Location = new Vector2(btn.BackgroundStyle.Left.Width, btn.Height / 2), Anchor = new Vector2(0, .5f) });//) { DrawAction = () => block.PaintIcon(Block.Width, Block.Height, variant.Data) });
                    //btn.AddControlsTopRight(new Label(item.Requirement.ToString()) { MouseThrough = true });
                    var padding = btn.BackgroundStyle.Left.Width;
                        var picbox = new PictureBox(variant.Block.PaintIcon(variant.Data)) { MouseThrough = true, Location = new Vector2(padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };//) { DrawAction = () => block.PaintIcon(Block.Width, Block.Height, variant.Data) });
                    var label = new Label(item.Requirement.ToString()) { Location = picbox.TopRight + Vector2.UnitX * padding, MouseThrough = true };
                        btn.AddControls(picbox, label);
                        btn.LeftClickAction = () => { 
                            action(variant); 
                            Instance.Hide(); };
                    });



            //Instance.Panel.AddControls(buttonGrid);
            Instance.Panel.AddControls(list);

            Instance.AddControls(Instance.Panel);
            if (Instance.Show())
                Instance.Location = UIManager.Mouse;
        }

        //public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    this.Hide();
        //    base.HandleLButtonUp(e);
        //}
        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Hide();
            base.HandleRButtonDown(e);
        }
    }

    //class UIBlockVariationPickerNew : GroupBox
    //{
    //    static UIBlockVariationPickerNew _Instance;
    //    public static UIBlockVariationPickerNew Instance
    //    {
    //        get
    //        {
    //            if (_Instance == null)
    //                _Instance = new UIBlockVariationPickerNew();
    //            return _Instance;
    //        }
    //    }

    //    Panel Panel = new Panel();

    //    static public void Refresh(Block block, Action<BlockRecipe.ProductMaterialPair> action)
    //    {
    //        //var variants = block.Reagent.Def.PreferredMaterialType.SubTypes.Select(m => new BlockRecipe.ProductMaterialPair(block, m));//.Recipe.GetVariants();
    //        //var variants = block.Ingredient.GetAllValidMaterials().Select(m => new BlockRecipe.ProductMaterialPair(block, m));
    //        //var variants = block.GetAllValidConstructionMaterials().Select(m => new BlockRecipe.ProductMaterialPair(block, m));
    //        var variants = block.GetAllValidConstructionMaterialsNew().Select(m => new BlockRecipe.ProductMaterialPair(block, m));

    //        Instance.Panel.Controls.Clear();
    //        Instance.Panel.AutoSize = true;
    //        Instance.ClearControls();
    //        //List<Cell> variations = new List<Cell>();
    //        //foreach (var item in variants)
    //        //    variations.Add(new Cell() { Block = block, BlockData = item.Data });
    //        var buttonGrid = new ButtonGridIcons<BlockRecipe.ProductMaterialPair>(4, 2, variants, (btn, product) =>
    //        {
    //            //var cell = new Cell() { Block = block, BlockData = product.Data };
    //            //btn.Tag = cell;
    //            btn.PaintAction = () =>
    //            {
    //                //cell.Block.PaintIcon(btn.Width, btn.Height, cell.BlockData);
    //                block.PaintIcon(btn.Width, btn.Height, product.Data);
    //            };
    //            btn.HoverFunc = product.GetName;// cell.Block.GetName(cell.BlockData);
    //            btn.LeftClickAction = () =>
    //            {
    //                action(product);// (btn.Tag as Cell).BlockData);
    //            };
    //        });
    //        Instance.Panel.AddControls(buttonGrid);
    //        Instance.AddControls(Instance.Panel);
    //        if (Instance.Show())
    //            Instance.Location = UIManager.Mouse;
    //    }

    //    public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
    //    {
    //        this.Hide();
    //        base.HandleLButtonUp(e);
    //    }
    //    public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
    //    {
    //        this.Hide();
    //        base.HandleRButtonDown(e);
    //    }
    //}
}
