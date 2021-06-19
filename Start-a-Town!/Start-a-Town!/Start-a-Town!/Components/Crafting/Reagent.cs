using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Crafting
{
    public partial class Reaction
    {
        public class Reagent
        {
            public abstract class ReagentFilter
            {
                public abstract string Name { get; }
                public abstract bool Condition(GameObject obj);
            }
            public string Name { get; set; }
            //public Filter Condition;
            public List<ReagentFilter> Conditions = new List<ReagentFilter>();
            public Reagent(string name, params ReagentFilter[] conditions)
            {
                this.Name = name;
                //this.Condition = condition;
                foreach(var cond in conditions)
                    this.Conditions.Add(cond);
            }
            static public List<Reagent> Create(params Reagent[] reagents)
            {
                return new List<Reagent>(reagents);
            }


            internal static ReagentFilter IsOfMaterial(Material material)
            {
                return new IsOfMaterial(material);
            }
            internal static ReagentFilter IsOfMaterialType(params MaterialType[] materialTypes)
            {
                return new IsOfMaterialType(materialTypes);
            }
            internal static ReagentFilter CanProduce(Product.Types type)
            {
                return new CanProduce(type);
            }
            internal static ReagentFilter HasHardness(float min, float max)
            {
                return new HasHardness(min, max);
            }
            internal static ReagentFilter IsOfType(ItemCategory type)
            {
                return new IsOfType(type);
            }
            internal static ReagentFilter IsOfSubType(params ItemSubType[] types)
            {
                return new IsOfSubType(types);
            }
            internal static ReagentFilter ToolHasUse(Skills.Skill skill)
            {
                return new ToolHasUse(skill);
            }
            public bool Filter(GameObject obj)
            {
                foreach (var cond in this.Conditions)
                    if (!cond.Condition(obj))
                        return false;
                return true;
            }

            public List<GameObject> GetMaterials()
            {
                return (from mat in MaterialComponent.Registry
                        let obj = GameObject.Objects[mat]
                        where this.Filter(obj)
                        select obj).ToList();
            }
            public override string ToString()
            {
                return this.Name;
            }
        }

    }
}
