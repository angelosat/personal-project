namespace Start_a_Town_.Blocks
{
        class BlockCarpentryEntity : BlockEntity
        {
            public BlockCarpentryEntity(IntVec3 originGlobal)
                : base(originGlobal)
            {
                this.AddComp(new BlockEntityCompWorkstation(IsWorkstation.Types.Carpentry));
                this.AddComp(new BlockEntityCompDeconstructible());
            }
        }
}
