﻿using System.Collections.Generic;

namespace Start_a_Town_
{
    class ApparelDefOf
    {
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
