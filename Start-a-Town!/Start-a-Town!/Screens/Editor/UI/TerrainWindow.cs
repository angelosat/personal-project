using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;

namespace Start_a_Town_.Editor
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

        //public BlockBrowser(int w, int h)
        //    : base(new Rectangle(0, 0, w, h))
        public BlockBrowser ()
        {
            this.Panel_Blocks = new PanelLabeled("Blocks") { AutoSize = true };
            var blocks = Block.Registry.Values.Skip(1).ToList();
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
            List<Cell> variations = new List<Cell>();
            foreach (var item in block.GetCraftingVariations())
                variations.Add(new Cell() { Block = block, BlockData = item });
            this.GridVariations = new SlotGrid<Slot<Cell>, Cell>(variations, 8, (slot, cell) =>
                {
                    slot.LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new Editor.BlockPainter(cell.Block, cell.BlockData);
                }) { Location = this.Panel_Variants.Controls.BottomLeft };
            this.Panel_Variants.Controls.Add(this.GridVariations);
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
