using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        const int ReactionObjectIDRange = 1000;

        static Dictionary<int, Reaction> _Dictionary;
        public static Dictionary<int, Reaction> Dictionary
        {
            get
            {
                if (_Dictionary.IsNull())
                    _Dictionary = new Dictionary<int, Reaction>();// Initialize();
                return _Dictionary;
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public GameObject.Types Building { get; set; }
        public List<Reagent> Reagents { get; set; }
        public List<Product> Products { get; set; }
        public ToolRequirement Skill { get; set; }
        public ToolRequirement Tool { get; set; }
        public List<IsWorkstation.Types> ValidWorkshops { get; set; }

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
        public Reaction(string name, List<IsWorkstation.Types> sites, List<Reagent> reagents, List<Product> products)
        {
            this.ID = IDSequence;
            this.Name = name;
            this.ValidWorkshops = sites;
            this.Reagents = reagents;
            this.Products = products;
            GameObject.Objects.Add(this.ToObject());
            Dictionary[ID] = this;
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
            obj.AddComponent<GeneralComponent>().Initialize(ReactionObjectIDRange + this.ID, ObjectType.Blueprint, "Reaction: " + this.Name, "A blueprint containing a crafting recipe");
            obj.AddComponent<PhysicsComponent>();
            obj.AddComponent<ReactionComponent>().Initialize(this);
            obj.AddComponent<GuiComponent>().Initialize(24, 64);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("writtenpage", new Vector2(16, 24), new Vector2(16, 24)));//Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[24] } }, new Vector2(16, 16)));
            return obj;
        }

        public class ToolRequirement
        {
            public Skill Skill;
            public bool ToolRequired;
            public ToolRequirement(Skill skill, bool toolRequired)
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

    }
}
