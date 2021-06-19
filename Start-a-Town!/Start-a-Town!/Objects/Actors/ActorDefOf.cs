using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
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
            Skills = //Def.GetDefs<SkillDef>().ToArray()
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


        static public readonly ItemDef Npc = new ItemDef("Npc")
        {
            ItemClass = typeof(Actor),
            Description = "A person.",
            Height = 1.5f,// 1.9f, //2
            Weight = 50,
            //Body = BodyDef.Skeleton,
            Body = BodyDef.NpcNew,
            //Body = BodyDef.SkeletonNew,

            DefaultMaterial = MaterialDefOf.Human,
            //DefaultSprite = ItemContent.SkeletonFull,
            ActorProperties = NpcProps,
            Factory = Actor.Create
            //CompProps = new List<ComponentProps>()
            //{
            //    new NeedsComponent.Props(
            //        NeedDef.Energy,
            //        NeedDef.Hunger,
            //        NeedDef.Social,
            //        NeedDef.Work),
            //    new AttributesComponent.Props(
            //        AttributeDef.Strength,
            //        AttributeDef.Intelligence,
            //        AttributeDef.Dexterity),
            //    new ResourcesComponent.Props(
            //        ResourceDef.Health,
            //        ResourceDef.Stamina),
            //    new GearComponent.Props(
            //        GearType.Offhand,
            //        GearType.Head,
            //        GearType.Chest,
            //        GearType.Feet,
            //        GearType.Hands,
            //        GearType.Legs ),
            //    new ComponentNpcSkills.Props(
            //        SkillDef.Digging,
            //        SkillDef.Construction),
            //    new PersonalityComponent.Props(
            //        TraitDef.Attention,
            //        TraitDef.Composure,
            //        TraitDef.Patience,
            //        TraitDef.Activity,
            //        TraitDef.Planning)}
        };

        

        //static public readonly ActorDef NpcOld = new ActorDef("Npc")
        //{
        //    Description = "A person.",
        //    Height = 2,
        //    Weight = 50,
        //    Body = BodyDef.Skeleton,
        //    Needs = new NeedDef[] {
        //        NeedDef.Energy,
        //        NeedDef.Hunger,
        //        NeedDef.Social,
        //        NeedDef.Work },
        //    Attributes = new AttributeDef[] {
        //        AttributeDef.Strength,
        //        AttributeDef.Intelligence,
        //        AttributeDef.Dexterity },
        //    Resources = new ResourceDef[]
        //    {
        //        ResourceDef.Health,
        //        ResourceDef.Stamina
        //    },
        //    GearSlots = new Components.GearType[] { GearType.Mainhand,
        //        GearType.Offhand,
        //        GearType.Head,
        //        GearType.Chest,
        //        GearType.Feet,
        //        GearType.Hands,
        //        GearType.Legs },
        //    Skills = new SkillDef[] { 
        //        SkillDef.Digging, 
        //        SkillDef.Construction,
        //        SkillDef.Cooking,
        //        SkillDef.Tinkering,
        //        SkillDef.Argiculture,
        //    },
        //    Traits = new TraitDef[]
        //    {
        //        TraitDef.Attention, 
        //        TraitDef.Composure, 
        //        TraitDef.Patience, 
        //        TraitDef.Activity, 
        //        TraitDef.Planning
        //    }
        //};

        static ActorDefOf()
        {
            Def.Register(Npc);
        }
    }
}
