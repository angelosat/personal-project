using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ApparelDefOf
    {
        //static public GameObject Helmet
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new DefComponent(GameObject.Types.Helmet, objType: ObjectType.Armor, name: "Helmet", description: "A basic Helmet");
        //        obj.AddComponent<GuiComponent>().Initialize(iconID: 27);
        //        //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[27] } }, new Vector2(16, 24)) { Joint = new Vector2(16, 16) });
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("helmet", new Vector2(16, 24), new Vector2(16, 16)));
        //        obj.AddComponent<EquipComponent>().Initialize(GearType.Head);
        //        obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
        //        //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping);
        //        obj.AddComponent<ItemCraftingComponent>();
        //        return obj;
        //    }
        //}

        static public readonly ItemDef Helmet = new("ItemHelmet")
        {
            BaseValue = 5,
            QualityLevels = true,
            Category = ItemCategory.Wearables,
            Description = "Protects the head but ruins the hairstyle.",
            DefaultSprite = ItemContent.HelmetFull,
            MadeFromMaterials = true,
            GearType = GearType.Head,
            ApparelProperties = new ApparelDef(GearType.Head, 10),
            DefaultMaterial = MaterialDefOf.Iron,
            Body = new Bone(BoneDef.Item, ItemContent.HelmetFull),
            CompProps = new List<ComponentProps>() { new ComponentProps() { CompType = typeof(OwnershipComponent) } }
        };

        static ApparelDefOf()
        {
            Def.Register(Helmet);

            
        }
    }
}
