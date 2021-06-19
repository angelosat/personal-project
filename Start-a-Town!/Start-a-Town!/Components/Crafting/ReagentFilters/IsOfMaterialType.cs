using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class IsOfMaterialType : Reaction.Reagent.ReagentFilter
    {
        //MaterialType MaterialType;
        List<MaterialType> ValidMaterialTypes = new List<MaterialType>();

        public override string Name
        {
            get
            {
                return "Is of (any) type";
            }
        }
        //public IsOfMaterialType(MaterialType materialType)
        //{
        //    this.MaterialType = materialType;
        //}
        public IsOfMaterialType(params MaterialType[] materialTypes)
        {
            this.ValidMaterialTypes = materialTypes.ToList();
        }
        public override bool Condition(Entity obj)
        {
            //Material mat = Material.LightWood;
            var body = obj.Body;
            
            if (body == null)
                return false;
            Material mat = body.Material;
            if (mat == null)
                return false;
            var type = mat.Type;
            return this.ValidMaterialTypes.Contains(type);
        }
        //public override bool Condition(GameObject obj)
        //{
        //    //Material mat = Material.LightWood;
        //    var body = obj.Body;
        //    Material mat = body.Material;
        //    if (body == null)
        //        return false;
        //    if (body.Material == null)
        //        return false;
        //    if (body.Material.Type != this.MaterialType)
        //        return false;
        //    return mat.Type == this.MaterialType;
        //}

        //public override string ToString()
        //{
        //    return Name + ": " + this.MaterialType.ToString();
        //}
        public override string ToString()
        {
            //return Name + ": " + this.MaterialType.ToString();
            string txt = Name + ": ";
            foreach (var type in this.ValidMaterialTypes)
                txt += type.Name + ", ";
            return txt.TrimEnd(',');
        }
    }
}
