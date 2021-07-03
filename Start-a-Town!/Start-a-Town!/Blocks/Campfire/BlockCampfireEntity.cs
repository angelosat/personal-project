using Start_a_Town_.Particles;

namespace Start_a_Town_.Blocks
{
    partial class BlockCampfire
    {
        class BlockCampfireEntity : BlockEntity
        {
            public BlockCampfireEntity()
            {
                var switchable = new BlockEntityCompSwitchable();
                this.Comps.Add(switchable);
                var refuel = new EntityCompRefuelable(100);
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
