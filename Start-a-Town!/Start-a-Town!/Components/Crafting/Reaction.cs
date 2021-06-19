using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.UI;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction : Def
    {
        static int _IDSequence = 0;
        //public static int IDSequence { get { return _IDSequence++; } }
        static public int GetNextID()
        {
            return _IDSequence++;
        }
        const int ReactionObjectIDRange = 1000;

        static Dictionary<int, Reaction> _Dictionary;
        public static Dictionary<int, Reaction> Dictionary
        {
            get
            {
                if (_Dictionary == null)
                    _Dictionary = new Dictionary<int, Reaction>();// Initialize();
                return _Dictionary;
            }
        }

        public float Fuel;
        public int ID;// { get; set; }
        //public string Name;// { get; set; }
        public GameObject.Types Building;// { get; set; }
        public List<Reagent> Reagents = new();// { get; set; }
        public List<Product> Products = new();// { get; set; }
        public SkillDef CraftSkill;
        public ToolRequirement Skill;// { get; set; }
        public ToolRequirement Tool;// { get; set; }
        public List<IsWorkstation.Types> ValidWorkshops = new();// { get; set; }
        public JobDef Labor;
        public Reaction SetSkill(SkillDef skill)
        {
            this.CraftSkill = skill;
            return this;
        }
        public Reagent GetReagent(string name)
        {
            return this.Reagents.First(r => r.Name == name);
        }
        public Ingredient GetIngredient(string name)
        {
            return this.Reagents.First(r => r.Name == name).Ingredient;
        }
        //public Reaction(string name, GameObject.Types building, List<Reagent> reagents, List<Product> products)
        //    : this(name, new List<int>() { (int)building }, reagents, products)
        //{
        //    //this.ID = IDSequence;
        //    //this.Name = name;
        //    //this.Building = building;
        //    //this.Reagents = reagents;
        //    //this.Products = products;
        //    //GameObject.Objects.Add(this.ToObject());
        //    //Dictionary[ID] = this;
        //}
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
        public Reaction(string name, IsWorkstation.Types[] sites, List<Reagent> reagents, List<Product> products, JobDef labor = null) : base(name)
        {
            //this.Name = name;
            this.ValidWorkshops = sites.ToList();
            this.Reagents = reagents;
            this.Products = products;
            //GameObject.Objects.Add(this.ToObject());
            this.Labor = labor;
        }
        public Reaction(string name, List<IsWorkstation.Types> sites, List<Reagent> reagents, List<Product> products, SkillDef skill, JobDef labor = null):base(name)
        {
            this.ID = GetNextID();
            //this.Name = name;
            this.ValidWorkshops = sites;
            this.Reagents = reagents;
            this.Products = products;
            //GameObject.Objects.Add(this.ToObject());
            Dictionary[ID] = this;
            this.Labor = labor;
            this.CraftSkill = skill;
        }
        public Reaction(string name, SkillDef skill, JobDef labor = null) : base(name)
        {
            this.ID = GetNextID();
            //this.Name = name;
            //this.ValidWorkshops = sites.ToList();
            //this.Reagents = reagents.ToList();
            //this.Products = products.ToList();
            //GameObject.Objects.Add(this.ToObject());
            this.Labor = labor;
            this.CraftSkill = skill;
        }
        public Reaction(string name, JobDef labor, IsWorkstation.Types[] sites) : base(name)
        {
            this.ID = GetNextID();
            this.ValidWorkshops = sites.ToList();
            this.Labor = labor;
        }
        public Reaction AddIngredient(Ingredient ingredient)
        {
            this.Reagents.Add(new Reagent(ingredient.Name, ingredient));
            return this;
        }
        public Reaction AddIngredients(IEnumerable<Ingredient> ingredients)
        {
            //this.Reagents.Add(new Reagent(ingredient.Name, ingredient));
            foreach (var i in ingredients)
                this.Reagents.Add(new Reagent(i.Name, i));
            return this;
        }
        public Reaction AddIngredient(string ingredientName, Ingredient ingredient)
        {
            this.Reagents.Add(new Reagent(ingredientName, ingredient));
            return this;
        }
        public Reaction AddIngredient(params Reagent[] reagents)
        {
            foreach(var r in reagents)
                this.Reagents.Add(r);
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
        //static public List<int> CanBeMadeAt(params int[] siteIDs)
        //{ 
        //    return new List<int>(siteIDs); 
        //}
        static public List<IsWorkstation.Types> CanBeMadeAt(params IsWorkstation.Types[] blocks)
        {
            return new List<IsWorkstation.Types>(blocks);
        }
        static public void Initialize()
        {
            //_Dictionary = new Dictionary<int, Reaction>(){
            //    {Pickaxe.ID, Pickaxe},
            //    {WoodenDeck.ID, WoodenDeck},
            //    {Cobblestone.ID, Cobblestone},
            //};
        }

        public GameObject ToObject()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(ReactionObjectIDRange + this.ID, ObjectType.Blueprint, "Reaction: " + this.Name, "A blueprint containing a crafting recipe");
            obj.AddComponent<PhysicsComponent>();
            obj.AddComponent<ReactionComponent>().Initialize(this);
            //obj.AddComponent<GuiComponent>().Initialize(24, 64);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("writtenpage", new Vector2(16, 24), new Vector2(16, 24)));//Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[24] } }, new Vector2(16, 16)));
            return obj;
        }

        public class ToolRequirement
        {
            public ToolAbilityDef Skill;
            public bool ToolRequired;
            public ToolRequirement(ToolAbilityDef skill, bool toolRequired)
            {
                this.Skill = skill;
                this.ToolRequired = toolRequired;
            }
        }

        static public List<Reaction> GetAvailableRecipes(IsWorkstation.Types workshop)
        {
            return (from reaction in Reaction.Dictionary.Values
                    where reaction.ValidWorkshops.Count == 0 || reaction.ValidWorkshops.Contains(workshop)
                    select reaction).ToList();
        }

        static readonly Lazy<ListBoxNew<Reaction, Button>> RecipeListUI = new(() => new ListBoxNew<Reaction, Button>(200, 400, r => new Button(r.Name)).HideOnAnyClick() as ListBoxNew<Reaction, Button>);
        static public void ShowRecipeListUI(Func<Reaction, bool> filter, Action<Reaction> selectAction)
        {
            //var list = new ButtonList<Reaction>(Dictionary.Values.Where(filter), 200, 400, r => r.Name, (r, b) => b.LeftClickAction = () => selectAction(r));
            RecipeListUI.Value
                .Clear()
                .AddItems(Dictionary.Values.Where(filter), selectAction)
                //.SetLocation(UIManager.Mouse)
                .ToContextMenu("Select recipe")
                .Show();
        }

    }
}
