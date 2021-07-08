using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class ItemDef : EntityDef, ILabeled
    {
        public int StackCapacity = 1;
        public int StackDimension = 1;
        public string ObjType;
        public int ID;
        public ItemCategory Category;
        public Sprite DefaultSprite;
        public Material DefaultMaterial;
        public bool MadeFromMaterials;
        public List<Reaction.Product.Types> CanProcessInto = new();
        public int BaseValue = 1;
        public MaterialType DefaultMaterialType;
        public List<Material> CanBeMadeFrom = new();
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

        public ItemDef(string name) : base(name)
        {
        }

        string _Label;
        public string Label { get => _Label ?? Name; set => _Label = value; }

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
        public Entity CreateFrom(Material mat)
        {
            var item = ItemFactory.CreateFrom(this, mat);
            return item;
        }
        public IEnumerable<ItemDefMaterialAmount> GenerateVariants(int amount = 1)
        {
            if (this.DefaultMaterialType == null)
                yield break;
            foreach (var m in this.DefaultMaterialType.SubTypes)
                yield return new ItemDefMaterialAmount(this, m, amount);
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
        public IEnumerable<Material> GetValidMaterials()
        {
            IEnumerable<Material> validMats = this.ValidMaterialTypes.SelectMany(t => t.SubTypes);
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
    }
}
