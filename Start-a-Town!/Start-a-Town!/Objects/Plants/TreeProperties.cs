namespace Start_a_Town_
{
    public class TreeProperties
    {
        public ItemDef TrunkType;
        public MaterialDef Material;
        public int Yield;
        public TreeProperties(MaterialDef material, int yield)
        {
            this.Material = material;
            this.TrunkType = RawMaterialDef.Logs;
            this.Yield = yield;
        }
    }
}
