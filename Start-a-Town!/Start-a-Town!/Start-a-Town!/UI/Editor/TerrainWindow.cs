using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;

namespace Start_a_Town_.UI.Editor
{
    class TerrainWindow : Window
    {
        static TerrainWindow _Instance;
        public static TerrainWindow Instance
        {
            get
            {
                if (_Instance.IsNull()) _Instance = new TerrainWindow();
                return _Instance;
            }
        }
        TerrainWindow()
        {
            this.Title = "Block Browser";
            this.AutoSize = true;
            this.Movable = true;
            this.Client.Controls.Add(new BlockBrowser());//500, 500));
        }
    }
    class BlockBrowser : GroupBox// ScrollableBox
    {
        PanelLabeled Panel_Blocks, Panel_Variants;
        SlotGrid<Slot<Block>, Block> GridBlocks;
        SlotGrid<Slot<Cell>, Cell> GridVariations;
        SlotGridCustom<SlotCustom<Cell>, Cell> GridVariations2;

        //public BlockBrowser(int w, int h)
        //    : base(new Rectangle(0, 0, w, h))
        public BlockBrowser ()
        {
            this.Panel_Blocks = new PanelLabeled("Blocks") { AutoSize = true };
            var blocks = Block.Registry.Values.Skip(1).ToList(); //skip air
            this.GridBlocks = new SlotGrid<Slot<Block>, Block>(blocks, 8, (slot, bl) =>
            {
                slot.LeftClickAction = () =>
                {
                    this.RefreshVariations(bl);
                    //ScreenManager.CurrentScreen.ToolManager.ActiveTool = new BlockPainter(bl);
                };
            }) { Location = this.Panel_Blocks.Controls.BottomLeft };
            this.Panel_Blocks.Controls.Add(this.GridBlocks);

            this.Panel_Variants = new PanelLabeled("Variations") {
             //Size = new Rectangle(0,0,this.GridBlocks.Width/2, this.GridBlocks.Height),
            Size = this.GridBlocks.Size,
                Location = this.Panel_Blocks.BottomLeft };
            //this.GridVariations = new SlotGrid<Slot<Cell>, Cell>(new List<Cell>(), 8, (slot, cell) =>
            //{

            //});
            //this.Panel_Variants.Controls.Add(this.GridVariations);
            this.Controls.Add(this.Panel_Blocks, this.Panel_Variants);
        }

        void RefreshVariations(Block block)
        {
            if (this.GridVariations != null)
                this.Panel_Variants.Controls.Remove(this.GridVariations);
            if (this.GridVariations2 != null)
                this.Panel_Variants.Controls.Remove(this.GridVariations2);
            List<Cell> variations = new List<Cell>();
            foreach (var item in block.GetVariations())
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
                    var token = tag.Block.Variations[0];
                    Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);
                    var loc = new Vector2(rect.X, rect.Y);
                    Effect fx = Game1.Instance.Content.Load<Effect>("blur");
                    MySpriteBatch mysb = new MySpriteBatch(gd);
                    fx.CurrentTechnique = fx.Techniques["Combined"];
                    fx.Parameters["Viewport"].SetValue(new Vector2(slot.Width, slot.Height));
                    gd.Textures[0] = Block.Atlas.Texture;
                    gd.Textures[1] = Block.Atlas.DepthTexture;
                    fx.CurrentTechnique.Passes["Pass1"].Apply();
                    var material = tag.Block.GetColor(tag.BlockData);// tag.Block.GetMaterial(tag.BlockData);
                    var bounds = new Vector4(3, 3 - Block.Depth / 2, token.Texture.Bounds.Width, token.Texture.Bounds.Height);
                    //mysb.DrawBlock(Block.Atlas.Texture, bounds,
                    //    tag.Block.Variations[Math.Min(tag.Variation, tag.Block.Variations.Count - 1)],
                    //    1, Color.Transparent, Color.White, material, Color.White, Vector4.One, Vector4.Zero, 0.5f);
                    var cam = new Camera();
                    cam.SpriteBatch = mysb;
                    //tag.Block.Draw(cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, tag.BlockData);
                    tag.Block.Draw(mysb, Vector3.Zero, cam, bounds, Color.White, Vector4.One, Color.Transparent, Color.White, 0.5f, 0, 0, tag.BlockData);

                    mysb.Flush();
                };
            }) { Location = this.Panel_Variants.Controls.BottomLeft };
            this.Panel_Variants.Controls.Add(this.GridVariations2);
        }
    }



    //class BlockBrowser : ScrollableBox
    //{
    //    SlotGrid<Slot<Block>, Block> BlockGrid;
    //    public BlockBrowser(int w, int h)
    //        : base(new Rectangle(0, 0, w, h))
    //    {
    //        var blocks = Block.Registry.Values.Skip(1).ToList();
    //        this.BlockGrid = new SlotGrid<Slot<Block>, Block>(blocks, 8, (slot, bl) =>
    //        {
    //            slot.LeftClickAction = () =>
    //            {
    //                ScreenManager.CurrentScreen.ToolManager.ActiveTool = new BlockPainter(bl);
    //            };
    //        });
    //        this.Controls.Add(this.BlockGrid);
    //    }
    //}
}
