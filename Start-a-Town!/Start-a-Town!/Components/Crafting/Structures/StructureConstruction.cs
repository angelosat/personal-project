using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Crafting
{
    public partial class StructureConstruction
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return IDRange + _IDSequence++; } }
        const int IDRange = 15000;
        static Dictionary<int, StructureConstruction> _Dictionary;
        public static Dictionary<int, StructureConstruction> Dictionary
        {
            get
            {
                if (_Dictionary is null)
                    _Dictionary = new Dictionary<int, StructureConstruction>();
                return _Dictionary;
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public List<Reaction.Reagent> Reagents { get; set; }
        public Reaction.Product Product { get; set; }

        StructureConstruction(string name, List<Reaction.Reagent> reagents, Reaction.Product products)
        {
            this.ID = IDSequence;
            this.Name = name;
            this.Reagents = reagents;
            this.Product = products;
            GameObject.Objects.Add(this.ToObject());
            Dictionary[ID] = this;
        }
        
        public GameObject ToObject()
        {
            GameObject obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(this.ID, ObjectType.Blueprint, "Reaction: " + this.Name, "A blueprint containing a crafting recipe");
            obj.AddComponent<PhysicsComponent>();
            //obj.AddComponent<ReactionComponent>().Initialize(this);
            obj.AddComponent<GuiComponent>().Initialize(24, 64);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[24] } }, new Vector2(16, 16)));
            return obj;
        }

        //static public readonly StructureConstruction Workbench = new StructureConstruction(
        //    "Workbench",
        //    Reaction.Reagent.Create(new Reaction.Reagent("Body", Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), Reaction.Reagent.CanProduce(Reaction.Product.Types.Workbenches))),
        //    new Reaction.Product(new ItemTemplate.Workbench.Factory().Create)
        //    );
    }
}
