using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class ItemDef : EntityDef//, ILabeled
    {
        public int StackCapacity = 1;
        public int StackDimension = 1;
        public ItemCategory Category;
        public Sprite DefaultSprite;
        public MaterialDef DefaultMaterial;
        public bool MadeFromMaterials;
        public List<Reaction.Product.Types> CanProcessInto = new();
        public int BaseValue = 1;
        public MaterialType DefaultMaterialType;
        public List<MaterialDef> CanBeMadeFrom = new();
        public List<MaterialType> ValidMaterialTypes = new();
        public Func<ItemDef, Entity> Factory = ItemFactory.CreateItem;
        public ActorDef ActorProperties;
        public PlantProperties PlantProperties;
        public ItemToolDef ToolProperties;
        public ApparelDef ApparelProperties;
        public CraftingProperties CraftingProperties;
        public RecipeProperties RecipeProperties;
        public ConsumableProperties ConsumableProperties;
        public GearType GearType;
        public Func<ItemDef, GameObject> Randomizer;
        public List<MaterialToken> MadeFrom = new();
        public bool QualityLevels;
        internal IEnumerable<IItemDefVariator> StorageFilterVariations;
        public Func<Entity, string> NameGetter;
        public Func<Entity, Def> VariationGetter;

        public ItemDef(string name) : base(name)
        {
        }

        //string _Label;
        //public string Label { get => _Label ?? Name; set => _Label = value; }

        internal GameObject CreateRandom()
        {
            return this.Randomizer?.Invoke(this);
        }
        public override string ToString()
        {
            return this.GetType().Name + ": " + this.Name;
        }
        public virtual Entity Create()
        {
            return this.Factory(this);
        }
        public Entity CreateNew()
        {
            var obj = Activator.CreateInstance(this.ItemClass, this) as Entity;
            obj.InitComps();
            return obj;
        }
        public Entity CreateFrom(MaterialDef mat)
        {
            var item = ItemFactory.CreateFrom(this, mat);
            return item;
        }
        public IEnumerable<ItemMaterialAmount> GenerateVariants(int amount = 1)
        {
            if (this.DefaultMaterialType == null)
                yield break;
            foreach (var m in this.DefaultMaterialType.SubTypes)
                yield return new ItemMaterialAmount(this, m, amount);
        }

        public MaterialType MaterialType
        {
            get
            {
                return this.DefaultMaterial?.Type ?? this.DefaultMaterialType;
            }
        }
        
        public ItemDef SetMadeFrom(params MaterialToken[] tokens)
        {
            this.MadeFrom.AddRange(tokens);
            return this;
        }
        public ItemDef SetMadeFrom(params MaterialType[] types)
        {
            this.ValidMaterialTypes.AddRange(types);
            return this;
        }
        public IEnumerable<MaterialDef> GetValidMaterials()
        {
            IEnumerable<MaterialDef> validMats = this.ValidMaterialTypes.SelectMany(t => t.SubTypes);
            foreach (var m in validMats)
                yield return m;
        }
        public IEnumerable<Entity> CreateFromAllMAterials()
        {
            foreach (var m in this.ValidMaterialTypes.SelectMany(t => t.SubTypes))
                yield return ItemFactory.CreateFrom(this, m);
        }
        
        internal Reaction CreateRecipe()
        {
            return this.RecipeProperties.CreateRecipe(this);
        }

        public StorageFilterCategoryNewNew GetSpecialFilter()
        {
            if (this.StorageFilterVariations is null)
                return null;
            var filter = new StorageFilterCategoryNewNew(this);
            filter.AddLeafs(this.StorageFilterVariations.Select(v => v.GetFilter()));
            return filter;
        }
    }
}
