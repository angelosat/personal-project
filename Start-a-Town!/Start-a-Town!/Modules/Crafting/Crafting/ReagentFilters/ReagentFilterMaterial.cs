﻿namespace Start_a_Town_
{
    public class ReagentFilterMaterial
    {
        public MaterialDef SpecificMaterial;
        public MaterialType SpecificMaterialType;

        public ReagentFilterMaterial()
        {
        }

        public ReagentFilterMaterial(MaterialDef mat, MaterialType matType)
        {
            this.SpecificMaterial = mat;
            this.SpecificMaterialType = matType;
        }

        public bool Condition(MaterialDef def)
        {
            if (this.SpecificMaterial != null)
                return def == this.SpecificMaterial;
            else
                return this.SpecificMaterialType == null || def.Type == this.SpecificMaterialType;
        }
    }
}
