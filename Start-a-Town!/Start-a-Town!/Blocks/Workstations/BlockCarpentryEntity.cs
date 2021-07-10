namespace Start_a_Town_.Blocks
{
        class BlockCarpentryEntity : BlockEntity
        {
            public BlockCarpentryEntity()
            {
                this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Carpentry));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            }
            public override object Clone()
            {
                return new BlockCarpentryEntity();
            }
        }

}
