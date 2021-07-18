using System;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns.Constructions
{
    class UIBlockVariationPickerNew : GroupBox
    {
        static UIBlockVariationPickerNew _Instance;
        public static UIBlockVariationPickerNew Instance => _Instance ??= new UIBlockVariationPickerNew();

        Panel Panel = new Panel();

        static public void Refresh(Block block, Action<ProductMaterialPair> action)
        {
            var variants = block.GetAllValidConstructionMaterialsNew().Select(m => new ProductMaterialPair(block, m)).GroupBy(p => p.Requirement.Material);

            Instance.Panel.Controls.Clear();
            Instance.Panel.AutoSize = true;
            Instance.ClearControls();

            var list = new ListBoxNew<ProductMaterialPair, ButtonNew>(200, 300);
            foreach (var group in variants)
                foreach (var variant in group)
                    list.AddItem(variant, v => "", (item, btn) =>
                    {
                        btn.BackgroundStyle = UI.BackgroundStyle.LargeButton;
                        var padding = btn.BackgroundStyle.Left.Width;
                        var picbox = new PictureBox(variant.Block.PaintIcon(variant.Data)) { MouseThrough = true, Location = new Vector2(padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };
                        var label = new Label(item.Requirement.ToString()) { Location = picbox.TopRight + Vector2.UnitX * padding, MouseThrough = true };
                        btn.AddControls(picbox, label);
                        btn.LeftClickAction = () =>
                        {
                            action(variant);
                            Instance.Hide();
                        };
                    });
            Instance.Panel.AddControls(list);

            Instance.AddControls(Instance.Panel);
            if (Instance.Show())
                Instance.Location = UIManager.Mouse;
        }

        public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Hide();
            base.HandleRButtonDown(e);
        }
    }
}
