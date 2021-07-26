﻿namespace Start_a_Town_
{
    static class ActorDefOf
    {
        static public readonly ActorDef NpcProps = new("NpcProps")
        {
            Needs = new NeedDef[] {
                NeedDef.Energy,
                NeedDef.Hunger,
                NeedDef.Comfort,
                NeedDef.Social,
                NeedDef.Work
            },
            Attributes = new AttributeDef[] {
                AttributeDef.Strength,
                AttributeDef.Intelligence,
                AttributeDef.Dexterity },
            Resources = new ResourceDef[]
            {
                ResourceDef.Health,
                ResourceDef.Stamina
            },
            GearSlots = new GearType[] { GearType.Mainhand,
                GearType.Offhand,
                GearType.Head,
                GearType.Chest,
                GearType.Feet,
                GearType.Hands,
                GearType.Legs },
            Skills =
            new SkillDef[] {
                SkillDef.Digging,
                SkillDef.Mining,
                SkillDef.Construction,
                SkillDef.Cooking,
                SkillDef.Tinkering,
                SkillDef.Argiculture,
                SkillDef.Carpentry,
                SkillDef.Crafting }
            ,
            Traits = new TraitDef[]
            {
                TraitDefOf.Attention,
                TraitDefOf.Composure,
                TraitDefOf.Patience,
                TraitDefOf.Activity,
                TraitDefOf.Planning,
                TraitDefOf.Resilience
            }
        };

        static public readonly ItemDef Npc = new("Npc")
        {
            ItemClass = typeof(Actor),
            Description = "A person.",
            Height = 1.5f,
            Weight = 50,
            Body = BodyDef.NpcNew,
            DefaultMaterial = MaterialDefOf.Human,
            ActorProperties = NpcProps,
            Factory = Actor.Create
        };

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}
