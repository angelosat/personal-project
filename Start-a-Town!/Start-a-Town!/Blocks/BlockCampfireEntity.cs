using Start_a_Town_.Particles;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Blocks
{
    //class BlockEntityDef : BlockEntity
    //{
    //    public override object Clone()
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //    public BlockEntityDef AddComp(BlockEntityComp comp)
    //    {
    //        this.Comps.Add(comp);
    //        return this;
    //    }
    //}


    partial class BlockCampfire
    {
        //public override BlockEntityWorkstation CreateWorkstationBlockEntity()
        //{
        //    return new BlockKitchenEntity();
        //}


        class BlockCampfireEntity : BlockEntity
        {
            //static FuelFilters

            public BlockCampfireEntity()
            {
                var switchable = new BlockEntityCompSwitchable();
                this.Comps.Add(switchable);
                //var refuel = new EntityCompRefuelable(100).SetFuelTypes(ItemSubType.Logs);
                var refuel = new EntityCompRefuelable(100);
                    //.SetFuelTypes(FuelDef.Organic)
                    //.SetPermittedItemTypes(ItemSubType.Logs, ItemSubType.Planks)
                    //.SetDefaultFilter(o => o.GetInfo().ItemSubType == ItemSubType.Logs);
                this.Comps.Add(refuel);
                this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Baking));
                var lightComp = new BlockEntityLuminance(15, refuel, 1, switchable.IsSwitchedOn);
                this.Comps.Add(lightComp);
                this.Comps.Add(new EntityCompParticles(ParticleEmitter.Fire.SetRateFunc(() => (lightComp.Powered && switchable.SwitchedOn) ? 1 : 0)));
                this.Comps.Add(new BlockEntityCompDeconstructible());
            }

            public override object Clone()
            {
                return new BlockCampfireEntity();
            }
        }
    }
}
