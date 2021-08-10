using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction : Def, IListable
    {
        static int _IDSequence = 0;
        static public int GetNextID() => _IDSequence++;
        
        static Dictionary<int, Reaction> _Dictionary;
        public static Dictionary<int, Reaction> Dictionary => _Dictionary ??= new Dictionary<int, Reaction>();
        public float Fuel;
        public int ID;
        public List<Reagent> Reagents = new();
        public List<Product> Products = new();
        public SkillDef CraftSkill;
        public List<IsWorkstation.Types> ValidWorkshops = new();
        public JobDef Labor;

        public Reaction(string name, List<IsWorkstation.Types> sites, List<Reagent> reagents, List<Product> products, SkillDef skill, JobDef labor = null) : base(name)
        {
            this.ID = GetNextID();
            this.ValidWorkshops = sites;
            this.Reagents = reagents;
            this.Products = products;
            Dictionary[ID] = this;
            this.Labor = labor;
            this.CraftSkill = skill;
        }
        public Reaction(string name, SkillDef skill, JobDef labor = null) : base(name)
        {
            this.ID = GetNextID();
            this.Labor = labor;
            this.CraftSkill = skill;
        }
        public Reaction(string name, JobDef labor, IsWorkstation.Types[] sites) : base(name)
        {
            this.ID = GetNextID();
            this.ValidWorkshops = sites.ToList();
            this.Labor = labor;
        }
      
        static public int Register(Reaction reaction)
        {
            var id = GetNextID();
            reaction.ID = id;
            Dictionary[id] = reaction;
            return id;
        }
        static public Reaction GetReaction(int id)
        {
            return Dictionary[id];
        }
        public Ingredient GetIngredient(string name)
        {
            return this.Reagents.First(r => r.Name == name).Ingredient;
        }

        public Reaction AddIngredient(Ingredient ingredient)
        {
            this.Reagents.Add(new Reagent(ingredient.Name, ingredient));
            return this;
        }
        public Reaction AddIngredients(IEnumerable<Ingredient> ingredients)
        {
            foreach (var i in ingredients)
                this.Reagents.Add(new Reagent(i.Name, i));
            return this;
        }
        public Reaction AddIngredient(string ingredientName, Ingredient ingredient)
        {
            this.Reagents.Add(new Reagent(ingredientName, ingredient));
            return this;
        }
      
        public Reaction AddProduct(IEnumerable<Product> products)
        {
            foreach (var p in products)
                this.Products.Add(p);
            return this;
        }
        public Reaction AddProduct(params Product[] products)
        {
            foreach (var p in products)
                this.Products.Add(p);
            return this;
        }

        public Reaction AddBuildSite(params IsWorkstation.Types[] sites)
        {
            foreach (var s in sites)
                this.ValidWorkshops.Add(s);
            return this;
        }

        static public void AddRecipe(Reaction recipe)
        {
            Dictionary.Add(recipe.ID, recipe);
        }
      
        static public List<IsWorkstation.Types> CanBeMadeAt(params IsWorkstation.Types[] blocks)
        {
            return new List<IsWorkstation.Types>(blocks);
        }
        static public void Initialize()
        {
           
        }

        public Control GetListControlGui()//Action<IListable> callback)
        {
            return new Label(this.Label);//, () => callback(this));
        }
    }
}
