namespace Start_a_Town_
{
    abstract class BlockPlacer
    {
        protected abstract Block Block { get; }
        byte CellData;
        int Orientation;
        int Variation;
        public void Place(IMap map, IntVec3 global, bool notify = true)
        {
            Block.Place(map, global, this.CellData, this.Variation, this.Orientation, notify);
        }
    }
}
