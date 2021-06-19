using Start_a_Town_.Components.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class Ingredient
    {
        class Modifier
        {
            public string Label;
            Predicate<ItemDef> Condition;

            public Modifier(string label, Predicate<ItemDef> condition)
            {
                Label = label;
                Condition = condition;
            }
            public bool Evaluate(ItemDef def)
            {
                return this.Condition(def);
            }
        }
        public ItemDef ItemDef;
        public Material Material;
        public MaterialType MaterialType;
        public int Amount = 1;
        public string Name;
        //public List<Predicate<ItemDef>> Modifiers = new List<Predicate<ItemDef>>();
        readonly List<Modifier> Modifiers = new();
        readonly HashSet<ItemCategory> AllowedCategories = new();
        readonly public HashSet<Material> AllowedMaterials = new();
        private HashSet<ItemDef> ValidItemDefs;
        private readonly HashSet<ItemDef> SpecifiedItemDefs = new();
        readonly HashSet<MaterialToken> MaterialTokens = new();
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
            //if (item == null)
            //    throw new ArgumentNullException();
            //this.ValidItemDefs.Add(item);
            ItemDef = item;
            Material = material;
            MaterialType = materialType;
            Amount = amount;
        }
        public IEnumerable<Material> GetAllValidMaterials()
        {
            if (this.Material != null)
            {
                yield return this.Material;
            }
            else if (this.MaterialType != null)
            {
                foreach (var m in this.MaterialType.SubTypes)
                    yield return m;
            }
            else if (this.ItemDef != null)
            {
                if (this.MaterialType != null && this.ItemDef.DefaultMaterialType != this.MaterialType)
                    throw new Exception();
                if (this.Material != null)
                {
                    if (!this.ItemDef.DefaultMaterialType.SubTypes.Contains(this.Material))
                        throw new Exception();
                    yield return this.Material;
                }
                else
                {
                    foreach (var m in this.ItemDef.DefaultMaterialType.SubTypes)
                        yield return m;
                }
            }
            else
            {
                var all = Def.Database.Values.OfType<ItemDef>();
                foreach (var item in all)
                {
                    if (!this.Modifiers.All(m => m.Evaluate(item)))
                        continue;
                    if (item.DefaultMaterial != null)
                        yield return item.DefaultMaterial;
                    else
                        foreach (var m in item.DefaultMaterialType.SubTypes)
                            yield return m;
                }
            }
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
                    //foreach (var m in item.PreferredMaterialType.SubTypes)
                    //    yield return new ItemDefMaterialAmount(item, m, this.Amount);
                    foreach (var m in item.GenerateVariants(this.Amount))
                    {
                        yield return m;
                    }
                }
            }
        }
        //public IEnumerable<ItemDef> GetValidItemDefs()
        //{
        //    foreach (var d in this.SpecifiedItemDefs)
        //        yield return d;
        //    foreach (var d in this.ValidItemDefs ??= ResolveItemDefs())
        //        yield return d;
        //}
        public IEnumerable<ItemDef> GetAllValidItemDefs()
        {
            //foreach (var d in this.SpecifiedItemDefs)
            //    yield return d;
            foreach (var d in this.ValidItemDefs ??= ResolveItemDefs())
                yield return d;
            yield break;
            if (this.ItemDef != null)
            {
                yield return this.ItemDef;
            }
            else
            {
                var all = Def.Database.Values.OfType<ItemDef>();
                foreach (var item in all)
                {
                    if (!this.Modifiers.All(m => m.Evaluate(item)))
                        continue;
                    yield return item;
                }
            }
        }
        public bool Evaluate(Entity item)
        {
            return this.ValidItemDefs.Contains(item.Def)
                && this.AllowedMaterials.Contains(item.PrimaryMaterial)
                && this.SpecialFilters.All(f => f(item));

            bool mods = true;
            if (this.Modifiers.Any())
                mods = this.Modifiers.All(m => m.Evaluate(item.Def));
                    
            return mods
                    && (this.ItemDef == null || this.ItemDef == item.Def)
                    && (this.Material == null || this.Material == item.Body.Material)
                    && (this.MaterialType == null || this.MaterialType == item.Body.Material.Type)
                    ;
        }
        public Ingredient IsBuildingMaterial()
        {
            this.Modifiers.Add(new Modifier("Any building material", def => def.CraftingProperties?.IsBuildingMaterial ?? false));
            //this.Modifiers.Add((ItemDef def) => def.CraftingProperties?.IsBuildingMaterial ?? false);
            return this;
        }
        public Ingredient IsCraftingMaterial()
        {
            this.Modifiers.Add(new Modifier("Any crafting material", def => def.CraftingProperties?.IsCraftingMaterial ?? false));
            //this.Modifiers.Add((ItemDef def) => def.CraftingProperties?.IsBuildingMaterial ?? false);
            return this;
        }
        public Ingredient HasReactionClass(ReactionClass reactionClass)
        {
            //this.Modifiers.Add(new Modifier("Reaction Class", def => def.DefaultMaterialType?.ReactionClass == reactionClass));
            //this.Modifiers.Add(new Modifier("Reaction Class", def => (def.PreferredMaterialType ?? def.DefaultMaterial.Type).ReactionClass == reactionClass));
            this.Modifiers.Add(new Modifier("Reaction Class", def => def.MaterialType.ReactionClass == reactionClass));

            //this.Modifiers.Add((ItemDef def) => def.CraftingProperties?.IsBuildingMaterial ?? false);
            return this;
        }
        public Ingredient SetAllow(IEnumerable<MaterialToken> tokens, bool allow)
        {
            foreach (var t in tokens)
                //this.MaterialTokens.Add(t);
                this.SetAllow(t, allow);
            return this;
        }
        public Ingredient SetAllow(MaterialToken token, bool allow)
        {
            if (allow)
                this.MaterialTokens.Add(token);
            else
                this.MaterialTokens.Remove(token);
            return this;
        }
        public Ingredient SetAllow(IEnumerable<MaterialType> types, bool allow)
        {
            foreach (var t in types)
                //this.MaterialTokens.Add(t);
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
            if (allow)
                this.AllowedMaterials.Add(mat);
            else
                this.AllowedMaterials.Remove(mat);
            return this;
        }
        public Ingredient SetAllow(MaterialType matType, bool allow)
        {
            foreach (var m in matType.SubTypes)
                this.SetAllow(m, allow);
            return this;
        }
        public Ingredient SetAllow(ItemDef def, bool allow)
        {
            if (allow)
                this.SpecifiedItemDefs.Add(def);
            else
                this.SpecifiedItemDefs.Remove(def);
            return this;
        }
        private HashSet<ItemDef> ResolveItemDefs()
        {

            //HashSet<ItemDef> list = new();
            var allDefs = Def.GetDefs<ItemDef>();
            //foreach(var c in this.AllowedCategories)
            //foreach(var d in allDefs)
            //{
            //    if()
            //}
            var list =
                new HashSet<ItemDef>(
                    this.SpecifiedItemDefs.Concat(
                    allDefs.Where(d =>
                this.AllowedCategories.Contains(d.Category)
                &&
                // TODO find better way to imply that all materials are allowed when allowedmaterials is empty
                (!this.AllowedMaterials.Any() || d.ValidMaterialTypes.Any(t => this.AllowedMaterials.Any(m => m.Type == t)))
                //this.MaterialTokens.Any(d.MadeFrom.Contains)
                ))
                    );

            ResolveAllowedMaterials(list);
            return list;
        }

        private void ResolveAllowedMaterials(HashSet<ItemDef> allowedItemDefs)
        {
            if (!this.AllowedMaterials.Any())
                foreach (var m in allowedItemDefs.SelectMany(i => i.GetValidMaterials()))
                    this.AllowedMaterials.Add(m);
        }

        internal string GetLabel()
        {
            return string.Format("{0}x {1} {2} {3}", this.Amount, this.Material?.Name ?? "", this.ItemDef?.Label ?? "", this.Modifiers.FirstOrDefault()?.Label ?? "");
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
