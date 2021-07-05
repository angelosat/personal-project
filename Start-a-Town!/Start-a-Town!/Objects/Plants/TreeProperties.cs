namespace Start_a_Town_
{
    public class TreeProperties
    {
        public ItemDef TrunkType;
        public Material Material;
        public int Yield;

        public TreeProperties(Material material, int yield)
        {
            this.Material = material;
            this.TrunkType = RawMaterialDef.Logs;
            this.Yield = yield;
        }
    }
}
