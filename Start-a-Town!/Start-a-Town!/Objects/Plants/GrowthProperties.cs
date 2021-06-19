using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class GrowthProperties
    {
        public ItemDef GrowthItemDef;
        public Material GrowthMaterial;
        public int MaxYieldHarvest;
        public int GrowthLengthTicks;

        public GrowthProperties(ItemDef growthItemDef, Material growthMaterial, int maxYieldHarvest, int growthLengthTicks)
        {
            this.GrowthItemDef = growthItemDef;
            this.GrowthMaterial = growthMaterial;
            this.MaxYieldHarvest = maxYieldHarvest;
            this.GrowthLengthTicks = growthLengthTicks * Engine.TicksPerSecond;
        }

        public GameObject CreateEntity()
        {
            return this.GrowthItemDef.CreateFrom(this.GrowthMaterial).SetStackSize(this.MaxYieldHarvest);
        }
    }
}
