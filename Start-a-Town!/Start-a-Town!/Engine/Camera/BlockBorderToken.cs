using System;
using Microsoft.Xna.Framework;


namespace Start_a_Town_
{
    public class BlockBorderToken
    {
        [Flags]
        public enum Sides { None, Left, Right, Bottom }
        public Vector3 Global;
        public Block Block;
        public Cell Cell;
        public Sides Side;
        public BlockBorderToken(Vector3 global, Block block, Cell cell, Sides side)
        {
            this.Global = global;
            this.Block = block;
            this.Cell = cell;
            this.Side = side;
        }
    }
}
