using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class ItemDef : EntityDef, ILabeled
    {
        public int StackCapacity = 1;
        public int StackDimension = 1;
        public ItemSubType SubType;
        public string ObjType;
        public int ID;
        public ItemCategory Category;// = StorageCategory.Unlisted;
        public Sprite DefaultSprite;
        public Material DefaultMaterial;// = Material.LightWood;
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
        //public virtual Entity Create()
        //{
        //    throw new Exception();
        //}
        public ItemDef(string name) : base(name)
        {
            //this.Name = name;
        }
        public Dictionary<BoneDef, ReactionIngredientIndex> CraftingIngredientIndices = new();

        string _Label;
        //public string Label => _Label ?? Name;
        public string Label { get => _Label ?? Name; set => _Label = value; }

        public T AddIngredientIndex<T>(BoneDef bone, string index) where T : ItemDef
        {
            this.CraftingIngredientIndices.Add(bone, new ReactionIngredientIndex(index));
            return this as T;
        }

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
            //item.Name = mat.Name + " " + item.Name;
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
            //IEnumerable<Material> validMats = Material.GetMaterials(m => this.ValidMaterialTypes.Contains(m.Type));
            IEnumerable<Material> validMats = this.ValidMaterialTypes.SelectMany(t => t.SubTypes);
            foreach (var m in validMats)
                yield return m;
            //yield break;
            //if (this.DefaultMaterial != null)
            //    yield return this.DefaultMaterial;
            //else if (this.DefaultMaterialType != null)
            //    foreach (var m in this.DefaultMaterialType.SubTypes)
            //        yield return m;
        }
        public IEnumerable<Entity> CreateFromAllMAterials()
        {
            //foreach (var m in this.DefaultMaterialType.SubTypes)
            foreach (var m in this.ValidMaterialTypes.SelectMany(t => t.SubTypes))
                yield return ItemFactory.CreateFrom(this, m);
        }
        public IEnumerable<Entity> CreateFromAllMAterialsNew()
        {
            var mats= Material.GetMaterialsAny(this.MadeFrom.ToArray());
            foreach (var m in mats)
                yield return ItemFactory.CreateFrom(this, m);
        }

        internal Reaction CreateRecipe()
        {
            return this.RecipeProperties.CreateRecipe(this);
        }
    }
}
