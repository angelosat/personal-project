namespace Start_a_Town_.Blocks
{
        class BlockCarpentryEntity : BlockEntity
        {
            public BlockCarpentryEntity(IntVec3 originGlobal)
                : base(originGlobal)
            {
                this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Carpentry));
                this.Comps.Add(new BlockEntityCompDeconstructible());
            }
        }
}
