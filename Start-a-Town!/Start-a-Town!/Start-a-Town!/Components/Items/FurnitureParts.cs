using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Components.Items
{
    class FurnitureParts : IItemFactory
    {
        static int ID;
        static public void Initialize()
        {
            ID = ItemManager.Register();
            GameObject.Objects.Add(Template());
        }

        string ReagentName;

        public FurnitureParts(string reagentName)
        {
            this.ReagentName = reagentName;
        }

        public GameObject Create(List<GameObjectSlot> materials)
        {
            var mat = materials.First(s => s.Name == this.ReagentName).Object.Body.Material;
            return Create(mat);
        }

        // CANT have it as a readonly field because it's created before we are assigned an id from the itemmanager in initialize()
        //static public readonly GameObject Template = Create(Material.LightWood);
        static public GameObject Template()
        {
            return Create(Material.LightWood);
        }

        static public GameObject Create(Material material)
        {
            GameObject obj = new GameObject();
            obj["Info"] = new GeneralComponent(ID, ObjectType.Material, "Furniture Parts", "Used to construct furniture.", Quality.Rare);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("log", new Vector2(16, 24)));
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj["Physics"] = new PhysicsComponent(size: 1);
            //obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", Material.LightWood));
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Furniture);
            obj.Body.Material = material;
            return obj;
        }

        static public readonly Reaction Reaction = 
            new Reaction(
                "Furniture Parts",
                Reaction.CanBeMadeAt(IsWorkstation.Types.Carpentry),
                Reaction.Reagent.Create(new Reaction.Reagent("Material", Reaction.Reagent.IsOfSubType(ItemSubType.Planks))),
                Reaction.Product.Create(new Reaction.Product(new FurnitureParts("Material"), new Quantity(4)))
                );

    }
}
