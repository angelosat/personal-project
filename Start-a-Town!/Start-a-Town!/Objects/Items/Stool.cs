using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Graphics;
using Start_a_Town_.Tokens;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        //public class Stool
        //{
        //    static public readonly int ID = IDSequence;
        //    static public void Initialize()
        //    {
        //        Factory.Initialize();
        //        GameObject.Objects.Add(Factory.Default);

        //        var entitycrafting = new Reaction(
        //            "Stool",
        //            //Reaction.CanBeMadeAt(Reaction.Site.World, ItemTemplate.Workbench.ID),
        //            //CanBeMadeAt(Site.Person, ItemTemplate.Workbench.ID),
        //            Reaction.CanBeMadeAt(IsWorkstation.Types.Workbench),
        //            Reaction.Reagent.Create(
        //                new Reaction.Reagent("Body", Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), Reaction.Reagent.CanProduce(Reaction.Product.Types.Furniture))),// { Quantity = 2 },
        //                //new Reaction.Reagent("Misc", Reaction.Reagent.IsOfMaterialType(MaterialType.Wood)) { Quantity = 3 }),
        //            Reaction.Product.Create(new Reaction.Product(Factory.New)) // maybe not create a new factory?
        //        );
        //    }
        //    public class Factory : IItemFactory
        //    {
        //        public static void Initialize() { }
        //        static public GameObject New(List<GameObjectSlot> materials)
        //        {
        //            return new Factory().Create(materials);
        //        }
        //        public GameObject Create(List<GameObjectSlot> materials)
        //        {
        //            return Create();
        //        }
        //        static GameObject Create()
        //        {
        //            GameObject obj = new GameObject();
        //            obj["Info"] = new GeneralComponent(ID, ObjectType.Furniture, "Stool", "Used to rest your butt.").Initialize(ItemSubType.Stool);
        //            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("furniture/stool", new Vector2(16, 24), new Vector2(16, 24)));
        //            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("furniture/stool", Sprite.CubeDepth) { OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) });
        //            obj["Physics"] = new PhysicsComponent(solid: true, height: .99f, size: 1, weight: 5);
        //            obj.AddComponent<GuiComponent>().Initialize(0);
        //            return obj;
        //        }
        //        static public readonly GameObject Default = Create();
        //    }
        //}
    }
}
