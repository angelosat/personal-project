using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    public partial class Reaction
    {
        public sealed partial class Reagent : ILabeled
        {

            //public abstract class ReagentFilter
            //{
            //    public abstract string Name { get; }
            //    public abstract bool Condition(Entity obj);
            //}
            public class ReagentFilter
            {
                public virtual string Name { get; set; }
                public Func<Entity, bool> _Condition;
                public Func<ItemDef, bool> _DefCondition;
                public Func<Material, bool> _MatCondition;

                public virtual bool Condition(Entity obj)
                {
                    return this._Condition(obj);
                }
            }

            public string Name { get; set; }

            public string Label => Name;
            public Ingredient Ingredient;

            public int Quantity => this.Ingredient.Amount;//= 1;
            //public Filter Condition;
            public ReagentFilterMaterial MaterialFilter = new();
            public ReagentFilterItem ItemFilter = new();
            public List<Func<Material, bool>> Modifiers = new();
            public List<ReagentFilter> Conditions = new();
            public static Reagent Create(string name, ItemDef itemDef, Material mat, MaterialType matType, int amount = 1)
            {
                var r = new Reagent
                {
                    Name = name,
                    MaterialFilter = new ReagentFilterMaterial(mat, matType),
                    ItemFilter = new ReagentFilterItem(itemDef),
                    //Quantity = amount
                };
                return r;
            }
            Reagent()
            {

            }
            public Reagent(string name, Ingredient ing)
            {
                this.Name = name;
                this.Ingredient = ing;
            }
            public Reagent(string name, params ReagentFilter[] conditions)
            {
                this.Name = name;
                //this.Condition = condition;
                foreach (var cond in conditions)
                    this.Conditions.Add(cond);
            }
            static public List<Reagent> Create(params Reagent[] reagents)
            {
                return new List<Reagent>(reagents);
            }
            internal static ReagentFilter HasSeeds()
            {
                return new ReagentFilterCustom("Has Seeds", o =>
                {
                    if (o.TryGetComponent<ConsumableComponent>(out ConsumableComponent comp))
                        return comp.Seeds != null;
                    return false;
                });
            }
            internal static ReagentFilter IsEdible()
            {
                //return new ReagentFilter() obj.IDType == GameObject.Types.Berries;
                return new IsEdible();
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
            internal static ReagentFilter IsOfType(ItemCategoryOld type)
            {
                return new IsOfType(type);
            }
            internal static ReagentFilter IsOfSubType(params ItemSubType[] types)
            {
                return new IsOfSubType(types);
            }
            internal static ReagentFilter ToolHasUse(ToolAbilityDef skill)
            {
                return new ToolHasUse(skill);
            }

            public bool Filter(GameObject entity)
            {
                return this.Ingredient.Evaluate(entity as Entity);
                //return
                //    this.Filter(entity.Def) &&
                //    this.Filter(entity.DominantMaterial);
            }
            public bool Filter(ItemDef def)
            {
                return this.ItemFilter.Condition(def);

            }
            public bool Filter(Material mat)
            {
                return
                    this.MaterialFilter.Condition(mat) &&
                    this.Modifiers.All(mod => mod(mat));

            }

            public override string ToString()
            {
                return this.Name;
            }

            internal IEnumerable<GameObject> GetValidCraftingItems()
            {
                return ReagentComponent.Registry.Where(mat => this.Filter(GameObject.Objects[mat] as Entity)).Select(mat => GameObject.Objects[mat]);
            }

            public Reagent HasReactionClass(ReactionClass type)
            {
                this.Modifiers.Add(mat => mat.Type.ReactionClass == type);
                return this;
            }



            //class ReagentFilterMaterial
            //{
            //    public Material SpecificMaterial;
            //    public MaterialType SpecificMaterialType;
            //    public bool Condition(Material def)
            //    {
            //        return def == this.SpecificMaterial || def.Type == this.SpecificMaterialType;
            //    }
            //}
            //class ReagentFilterItem
            //{
            //    public ItemDef Specific;

            //    public bool Condition(ItemDef def)
            //    {
            //        return this.Specific != null ? def == this.Specific : true;
            //    }
            //}
        }
    }
}
