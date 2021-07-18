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
        readonly HashSet<ItemCategory> AllowedCategories = new();
        readonly public HashSet<Material> AllowedMaterials = new();
        private HashSet<ItemDef> ValidItemDefs;
        private readonly HashSet<ItemDef> SpecifiedItemDefs = new();
        public IngredientRestrictions DefaultRestrictions = new();
        readonly List<Func<Entity, bool>> SpecialFilters = new();
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

        IEnumerable<Material> _cachedMaterials;
        public IEnumerable<Material> GetAllValidMaterials()
        {
            if (_cachedMaterials is null)
            {
                var defs = this.GetAllValidItemDefs().ToList();
                _cachedMaterials = defs.SelectMany(d => d.GetValidMaterials()).Distinct().Intersect(this.AllowedMaterials);
            }
            foreach (var i in _cachedMaterials)
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
            foreach (var d in this.ValidItemDefs ??= ResolveItemDefs())
                yield return d;
        }
        public bool Evaluate(Entity item)
        {
            return this.ValidItemDefs.Contains(item.Def)
                && this.AllowedMaterials.Contains(item.PrimaryMaterial)
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
                this.AllowedCategories.Add(category);
            else
                this.AllowedCategories.Remove(category);
            return this;
        }
        public Ingredient SetAllow(Material mat, bool allow)
        {
            if (mat is null)
                throw new Exception(); 
            if (allow)
                this.AllowedMaterials.Add(mat);
            else
                this.AllowedMaterials.Remove(mat);
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
        HashSet<ItemDef> _resolvedItemDefs;
        private HashSet<ItemDef> ResolveItemDefs()
        {
            var allDefs = Def.GetDefs<ItemDef>();
            //var list =
            //    new HashSet<ItemDef>(
            //        this.SpecifiedItemDefs.Concat(
            //        allDefs.Where(d =>
            //            this.AllowedCategories.Contains(d.Category)
            //            &&
            //            // TODO find better way to imply that all materials are allowed when allowedmaterials is empty
            //            (!this.AllowedMaterials.Any() || d.ValidMaterialTypes.Any(t => this.AllowedMaterials.Any(m => m.Type == t)))
            //            )));
            //ResolveAllowedMaterials(list);
            //return list;

            if (this.SpecifiedItemDefs.Any())
                this._resolvedItemDefs = this.SpecifiedItemDefs;
            else
            {
                if (this.Modifiers.Any())
                    this._resolvedItemDefs = new(allDefs.Where(d => this.Modifiers.All(m => m.Evaluate(d))));
                else if (this.AllowedMaterials.Any())
                    this._resolvedItemDefs = new(allDefs.Where(d => d.ValidMaterialTypes.Any(t => this.AllowedMaterials.Any(m => m.Type == t))));
            }
            if (!this.AllowedMaterials.Any())
                ResolveAllowedMaterials(this._resolvedItemDefs);
            return this._resolvedItemDefs;
        }
        private void ResolveAllowedMaterials(HashSet<ItemDef> allowedItemDefs)
        {
            if (!this.AllowedMaterials.Any())
                foreach (var m in allowedItemDefs.SelectMany(i => i.GetValidMaterials()))
                    this.AllowedMaterials.Add(m);
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
