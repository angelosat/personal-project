﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI.Editor
{
    class BlockBrowser : GroupBox
    {
        readonly PanelLabeled Panel_Blocks, Panel_Variants;
        readonly SlotGrid<Slot<Block>, Block> GridBlocks;
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
            })
            { Location = this.Panel_Blocks.Controls.BottomLeft };
            this.Panel_Blocks.Controls.Add(this.GridBlocks);

            this.Panel_Variants = new PanelLabeled("Variations")
            {
                Size = this.GridBlocks.Size,
                Location = this.Panel_Blocks.BottomLeft
            };
            this.Controls.Add(this.Panel_Blocks, this.Panel_Variants);
        }

        void RefreshVariations(Block block)
        {
            if (this.GridVariations2 is not null)
                this.Panel_Variants.Controls.Remove(this.GridVariations2);
            var variations = new List<Cell>();
            foreach (var item in block.GetEditorVariations())
                variations.Add(new Cell() { Block = block, BlockData = item });
            this.GridVariations2 = new SlotGridCustom<SlotCustom<Cell>, Cell>(variations, 8, (slot, cell) =>
            {
                slot.LeftClickAction = () =>
                {
                    ScreenManager.CurrentScreen.ToolManager.ActiveTool = new BlockPainter(cell.Block, cell.BlockData);
                };
                slot.PaintAction = (sb, tag) =>
                {
                    var gd = sb.GraphicsDevice;
                    var token = tag.Block.GetDefault();
                    var rect = new Rectangle(3, 3, this.Width - 6, this.Height - 6);
                    var loc = new Vector2(rect.X, rect.Y);
                    var fx = Game1.Instance.Content.Load<Effect>("blur");
                    var mysb = new MySpriteBatch(gd);
                    fx.CurrentTechnique = fx.Techniques["Combined"];
                    fx.Parameters["Viewport"].SetValue(new Vector2(slot.Width, slot.Height));
                    gd.Textures[0] = Block.Atlas.Texture;
                    gd.Textures[1] = Block.Atlas.DepthTexture;
                    fx.CurrentTechnique.Passes["Pass1"].Apply();
                    var material = tag.Block.GetColor(tag.BlockData);
                    var bounds = new Vector4(3, 3 - Block.Depth / 2, token.Texture.Bounds.Width, token.Texture.Bounds.Height);
                    var cam = new Camera { SpriteBatch = mysb };
                    tag.Block.Draw(mysb, Vector3.Zero, cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, tag.BlockData);
                    mysb.Flush();
                };
            })
            { Location = this.Panel_Variants.Controls.BottomLeft };
            this.Panel_Variants.Controls.Add(this.GridVariations2);
        }
    }
}
