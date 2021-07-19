﻿using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfMaterialType : Reaction.Reagent.ReagentFilter
    {
        List<MaterialType> ValidMaterialTypes = new List<MaterialType>();

        public override string Name => "Is of (any) type";
         
        public IsOfMaterialType(params MaterialType[] materialTypes)
        {
            this.ValidMaterialTypes = materialTypes.ToList();
        }
        public override bool Condition(Entity obj)
        {
            var body = obj.Body;
            
            if (body == null)
                return false;
            Material mat = body.Material;
            if (mat == null)
                return false;
            var type = mat.Type;
            return this.ValidMaterialTypes.Contains(type);
        }
        
        public override string ToString()
        {
            string txt = Name + ": ";
            foreach (var type in this.ValidMaterialTypes)
                txt += type.Name + ", ";
            return txt.TrimEnd(',');
        }
    }
}