using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ReagentFilterMaterial
    {
        public Material SpecificMaterial;
        public MaterialType SpecificMaterialType;

        public ReagentFilterMaterial()
        {
        }

        public ReagentFilterMaterial(Material mat, MaterialType matType)
        {
            this.SpecificMaterial = mat;
            this.SpecificMaterialType = matType;
        }

        public bool Condition(Material def)
        {
            if (this.SpecificMaterial != null)
                return def == this.SpecificMaterial;
            else
                return this.SpecificMaterialType == null || def.Type == this.SpecificMaterialType;

            //return
            //    (this.SpecificMaterial == null || def == this.SpecificMaterial) ||
            //    (this.SpecificMaterialType == null || def.Type == this.SpecificMaterialType);
        }
    }
}
