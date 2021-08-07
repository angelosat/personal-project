namespace Start_a_Town_
{
    abstract class BlockPlacer
    {
        protected abstract Block Block { get; }
        MaterialDef Material;
        byte CellData;
        int Orientation;
        int Variation;
        public void Place(MapBase map, IntVec3 global, bool notify = true)
        {
            this.Block.Place(map, global, this.Material, this.CellData, this.Variation, this.Orientation, notify);
        }
    }
}
