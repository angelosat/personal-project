using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI.Editor
{
    class BlockBrowser : GroupBox
    {
        PanelLabeled Panel_Blocks, Panel_Variants;
        SlotGrid<Slot<Block>, Block> GridBlocks;
        SlotGrid<Slot<Cell>, Cell> GridVariations;
        SlotGridCustom<SlotCustom<Cell>, Cell> GridVariations2;

        public BlockBrowser()
        {
            this.Panel_Blocks = new PanelLabeled("Blocks") { AutoSize = true };
            var blocks = Block.Registry.Values.Skip(1).ToList(); //skip air
            this.GridBlocks = new SlotGrid<Slot<Block>, Block>(blocks, 8, (slot, bl) =>
            {
                slot.LeftClickAction = () =>
                {
                    this.RefreshVariations(bl);
                };
            }) { Location = this.Panel_Blocks.Controls.BottomLeft };
            this.Panel_Blocks.Controls.Add(this.GridBlocks);

            this.Panel_Variants = new PanelLabeled("Variations") {
            Size = this.GridBlocks.Size,
                Location = this.Panel_Blocks.BottomLeft };
            this.Controls.Add(this.Panel_Blocks, this.Panel_Variants);
        }

        void RefreshVariations(Block block)
        {
            if (this.GridVariations != null)
                this.Panel_Variants.Controls.Remove(this.GridVariations);
            if (this.GridVariations2 != null)
                this.Panel_Variants.Controls.Remove(this.GridVariations2);
            List<Cell> variations = new List<Cell>();
            foreach (var item in block.GetCraftingVariations())
                variations.Add(new Cell() { Block = block, BlockData = item });
            this.GridVariations = new SlotGrid<Slot<Cell>, Cell>(variations, 8, (slot, cell) =>
                {
                    slot.LeftClickAction = () => { 
                        ScreenManager.CurrentScreen.ToolManager.ActiveTool = new BlockPainter(cell.Block, cell.BlockData); 
                    };
                }) { Location = this.Panel_Variants.Controls.BottomLeft };

            this.GridVariations2 = new SlotGridCustom<SlotCustom<Cell>, Cell>(variations, 8, (slot, cell) =>
            {
                slot.LeftClickAction = () =>
                {
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new BlockPainter(cell.Block, cell.BlockData);
                };
                slot.PaintAction = (sb, tag) =>
                {
                    GraphicsDevice gd = sb.GraphicsDevice;
                    var token = tag.Block.GetDefault();
                    Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);
                    var loc = new Vector2(rect.X, rect.Y);
                    Effect fx = Game1.Instance.Content.Load<Effect>("blur");
                    MySpriteBatch mysb = new MySpriteBatch(gd);
                    fx.CurrentTechnique = fx.Techniques["Combined"];
                    fx.Parameters["Viewport"].SetValue(new Vector2(slot.Width, slot.Height));
                    gd.Textures[0] = Block.Atlas.Texture;
                    gd.Textures[1] = Block.Atlas.DepthTexture;
                    fx.CurrentTechnique.Passes["Pass1"].Apply();
                    var material = tag.Block.GetColor(tag.BlockData);
                    var bounds = new Vector4(3, 3 - Block.Depth / 2, token.Texture.Bounds.Width, token.Texture.Bounds.Height);
                    var cam = new Camera();
                    cam.SpriteBatch = mysb;
                    tag.Block.Draw(mysb, Vector3.Zero, cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, tag.BlockData);
                    mysb.Flush();
                };
            }) { Location = this.Panel_Variants.Controls.BottomLeft };
            this.Panel_Variants.Controls.Add(this.GridVariations2);
        }
    }
}
