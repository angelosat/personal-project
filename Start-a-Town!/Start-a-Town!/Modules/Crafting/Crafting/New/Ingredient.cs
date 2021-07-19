using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class Ingredient
    {
        public ItemDef ItemDef;
        public Material Material;
        public MaterialType MaterialType;
        public int Amount = 1;
        public string Name;
        readonly List<Modifier> Modifiers = new();
        readonly HashSet<ItemCategory> SpecifiedCategories = new();
        readonly HashSet<Material> SpecifiedMaterials = new();
        readonly HashSet<ItemDef> SpecifiedItemDefs = new();
        public IngredientRestrictions DefaultRestrictions = new();
        readonly List<Func<Entity, bool>> SpecialFilters = new();
        IEnumerable<Material> ResolvedMaterials;
        HashSet<ItemDef> ResolvedItemDefs;
        bool Resolved;

        public bool IsPreserved;

        public Ingredient(string name)
        {
            this.Name = name;
        }
        public Ingredient()
        {

        }
        public Ingredient(ItemDef item = null, Material material = null, MaterialType materialType = null, int amount = 1)
        {
            ItemDef = item;
            Material = material;
            MaterialType = materialType;
            Amount = amount;
        }

        public IEnumerable<Material> GetAllValidMaterials()
        {
            if (this.ResolvedMaterials is null)
            {
                var defs = this.GetAllValidItemDefs().ToList();
                this.ResolvedMaterials = defs.SelectMany(d => d.GetValidMaterials()).Distinct().Intersect(this.SpecifiedMaterials);
            }
            foreach (var i in ResolvedMaterials)
                yield return i;
        }

        public IEnumerable<ItemDefMaterialAmount> GetAllValidMaterialsNew()
        {
            if (this.ItemDef != null)
            {
                if (this.MaterialType != null)
                {
                    if (this.Material != null)
                        yield return new ItemDefMaterialAmount(this.ItemDef, this.Material, this.Amount);
                    else
                        foreach (var m in this.MaterialType.SubTypes)
                            yield return new ItemDefMaterialAmount(this.ItemDef, m, this.Amount);
                }
                else
                {
                    if(this.Material != null)
                        yield return new ItemDefMaterialAmount(this.ItemDef, this.Material, this.Amount);
                    else
                        foreach (var m in this.ItemDef.DefaultMaterialType.SubTypes)
                            yield return new ItemDefMaterialAmount(this.ItemDef, m, this.Amount);
                }
            }
            else
            {
                var all = Def.Database.Values.OfType<ItemDef>();
                foreach (var item in all)
                {
                    if (!this.Modifiers.All(m => m.Evaluate(item)))
                        continue;
                    foreach (var m in item.GenerateVariants(this.Amount))
                    {
                        yield return m;
                    }
                }
            }
        }
        
        public IEnumerable<ItemDef> GetAllValidItemDefs()
        {
            this.TryResolve();
            foreach (var d in this.ResolvedItemDefs)
                yield return d;
            //foreach (var d in this.ValidItemDefs ??= ResolveItemDefs())
            //    yield return d;
        }
        public bool Evaluate(Entity item)
        {
            return this.ResolvedItemDefs.Contains(item.Def)
                && this.ResolvedMaterials.Contains(item.PrimaryMaterial)
                && this.SpecialFilters.All(f => f(item));
        }
        public Ingredient IsBuildingMaterial()
        {
            this.Modifiers.Add(new Modifier("Any building material", def => def.CraftingProperties?.IsBuildingMaterial ?? false));
            return this;
        }
       
        public Ingredient SetAllow(IEnumerable<MaterialType> types, bool allow)
        {
            foreach (var t in types)
                this.SetAllow(t, allow);
            return this;
        }
        public Ingredient SetAllow(ItemCategory category, bool allow)
        {
            if (allow)
                this.SpecifiedCategories.Add(category);
            else
                this.SpecifiedCategories.Remove(category);
            return this;
        }
        public Ingredient SetAllow(Material mat, bool allow)
        {
            if (mat is null)
                throw new Exception(); 
            if (allow)
                this.SpecifiedMaterials.Add(mat);
            else
                this.SpecifiedMaterials.Remove(mat);
            return this;
        }
        public Ingredient SetAllow(MaterialType matType, bool allow)
        {
            if (matType is null)
                throw new Exception(); 
            foreach (var m in matType.SubTypes)
                this.SetAllow(m, allow);
            return this;
        }
        public Ingredient SetAllow(ItemDef def, bool allow)
        {
            if (def is null)
                throw new Exception();
            if (allow)
                this.SpecifiedItemDefs.Add(def);
            else
                this.SpecifiedItemDefs.Remove(def);
            return this;
        }
        private void TryResolve()
        {
            if (this.Resolved)
                return;
            var allDefs = Def.GetDefs<ItemDef>();
           
            if (this.SpecifiedItemDefs.Any())
                this.ResolvedItemDefs = this.SpecifiedItemDefs;
            else
            {
                if (this.Modifiers.Any())
                    this.ResolvedItemDefs = new(allDefs.Where(d => this.Modifiers.All(m => m.Evaluate(d))));
                else if (this.SpecifiedMaterials.Any())
                    this.ResolvedItemDefs = new(allDefs.Where(d => d.ValidMaterialTypes.Any(t => this.SpecifiedMaterials.Any(m => m.Type == t))));
            }
            if (!this.SpecifiedMaterials.Any())
                ResolveAllowedMaterials(this.ResolvedItemDefs);
            this.Resolved = true;
        }
        private void ResolveAllowedMaterials(HashSet<ItemDef> allowedItemDefs)
        {
            if (!this.SpecifiedMaterials.Any())
                foreach (var m in allowedItemDefs.SelectMany(i => i.GetValidMaterials()))
                    this.SpecifiedMaterials.Add(m);
        }
      
        internal string GetLabel()
        {
            return $"{this.Amount}x {this.Material?.Name ?? ""} {this.ItemDef?.Label ?? ""} {this.Modifiers.FirstOrDefault()?.Label ?? ""}";
        }

        public Ingredient AddResourceFilter(ResourceDef resDef)
        {
            this.SpecialFilters.Add(e => e.GetResource(resDef)?.Percentage < 1);
            return this;
        }
        public Ingredient Preserve()
        {
            this.IsPreserved = true;
            return this;
        }
    }
}
