namespace Start_a_Town_.Blocks
{
    partial class BlockPrefab : Block
    {
        class BlockPrefabEntity : BlockEntity
        {
            public override object Clone()
            {
                return new BlockPrefabEntity();
            }
        }
    }
}
