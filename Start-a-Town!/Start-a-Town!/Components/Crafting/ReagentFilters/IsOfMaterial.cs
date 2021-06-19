using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfMaterial : Reaction.Reagent.ReagentFilter
    {
        Material Material;
        public override string Name
        {
            get
            {
                return "Is of material";
            }
        }
        public IsOfMaterial(Material material)
        {
            this.Material = material;
        }
        public override bool Condition(Entity obj)
        {
            //return obj.GetComponent<MaterialComponent>().Material == this.Material;
            //return obj.GetComponent<MaterialsComponent>().Parts["Body"].Material == this.Material;
            return obj.Body.Material == this.Material;

        }
        public override string ToString()
        {
            return Name + ": " + this.Material.ToString();
        }
    }
}
