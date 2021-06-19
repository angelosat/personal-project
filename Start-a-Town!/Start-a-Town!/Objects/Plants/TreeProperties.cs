using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
