﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class GetPartMaterialFromReagent : Reaction.Product.Modifier
    {
        public string ReagentName { get; set; }
        string PartName { get; set; }
        public GetPartMaterialFromReagent(string reagentName, string partName)
        {
            this.ReagentName = reagentName;
            this.PartName = PartName;
        }
        public override void Apply(GameObject material, GameObject product)
        {
            Materials.Material mat = material.GetComponent<MaterialComponent>().Material;
            product.GetComponent<MaterialsComponent>().Parts[this.PartName].Material = mat;
        }
    }
}
