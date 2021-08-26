namespace Start_a_Town_
{
    abstract class BlockPlacer
    {
        protected abstract Block Block { get; }
        protected MaterialDef Material;
        protected byte CellData;
        protected int Orientation;
        protected int Variation;
        public void Place(MapBase map, IntVec3 global, bool notify = true)
        {
            Block.Place(this.Block, map, global, this.Material ?? this.Block.DefaultMaterial, this.CellData, this.Variation, this.Orientation, notify);
        }
    }
}
