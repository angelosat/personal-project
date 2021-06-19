using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class ActorDef : ItemDef
    {
        public NeedDef[] Needs;
        public AttributeDef[] Attributes;
        public ResourceDef[] Resources;
        public SkillDef[] Skills;
        public TraitDef[] Traits;
        public GearType[] GearSlots;


        public ActorDef(string name):base(name)
        {
            this.ItemClass = typeof(Actor);
        }
        public override Entity Create()
        {
            return Create(this);
        }
        [Obsolete]
        static Actor Create(ActorDef def)
        {
            throw new Exception();
            var obj = new Actor
            {
                Def = def
            };
            obj.Physics.Height = def.Height;
            obj.Physics.Weight = def.Weight;
           
            //obj.AddComponent(new GearComponent(
            //    //GearType.Hauling,
            //    GearType.Mainhand,
            //    GearType.Offhand,
            //    GearType.Head,
            //    GearType.Chest,
            //    GearType.Feet,
            //    GearType.Hands,
            //    GearType.Legs
            //    ));
            //obj.AddComponent(new ResourcesComponent(ResourceDef.Health, ResourceDef.Stamina));
            //obj.AddComponent(new AttributesComponent(AttributeDef.Strength, AttributeDef.Intelligence, AttributeDef.Dexterity));
            //obj.AddComponent(new NeedsComponent(NeedDef.Energy, NeedDef.Hunger, NeedDef.Social, NeedDef.Work));
            //obj.AddComponent(new ComponentNpcSkills(SkillDef.Digging, SkillDef.Construction));// new NpcSkillDigging()));
            //obj.AddComponent(new PersonalityComponent(TraitDef.Attention, TraitDef.Composure, TraitDef.Patience, TraitDef.Activity, TraitDef.Planning));

            obj.AddComponent(new GearComponent());
            obj.AddComponent(new ResourcesComponent());
            obj.AddComponent(new AttributesComponent());
            obj.AddComponent(new NeedsComponent());
            obj.AddComponent(new NpcSkillsComponent());
            obj.AddComponent(new PersonalityComponent());
            obj.AddComponent(new NpcComponent());
            obj.AddComponent(new PossessionsComponent());
            obj.AddComponent(new SpriteComponent(def.Body));
            obj.AddComponent(new PersonalInventoryComponent(16));
            obj.AddComponent(new StatsComponentNew());
            obj.AddComponent(new MobileComponent());
            obj.AddComponent(new WorkComponent());
            obj.AddComponent(new MoodComp());
            obj.AddComponent<AIComponent>().Initialize(//new Personality(TraitDef.Attention, TraitDef.Composure, TraitDef.Patience),// reaction: ReactionType.Friendly),
               new BehaviorQueue(
                   new AIAwareness(),
                   new AIMemory(),
                   new BehaviorCombat(),
                   new BehaviorHandleResources(),
                   //new AIDialogue(),
                   //new AIFollow(),
                   //new BehaviorMoodlets(),
                   new BehaviorHandleOrders(),
                   new AI.Behaviors.Tasks.BehaviorFindTask(),

                   //new BehaviorSatisfyNeed(),
                   //new BehaviorFindJobNew(),
                   //new BehaviorAct(),

                   new BehaviorIdle()
                   ));

            Sprite sprite = new Sprite("mobs/skeleton/full", new Vector2(17 / 2, 38));
            sprite.OriginGround = new Vector2(sprite.AtlasToken.Texture.Bounds.Width / 2, sprite.AtlasToken.Texture.Bounds.Height);
            sprite.OriginGround = new Vector2(sprite.AtlasToken.Texture.Bounds.Width / 2, sprite.AtlasToken.Texture.Bounds.Height);

            obj.AddComponent<SpriteComponent>().Initialize(BodyDef.Skeleton, sprite);
            foreach (var b in obj.Body.GetAllBones())
                b.Material = MaterialDefOf.Human;
            obj.ObjectCreated();
            return obj;
        }

        

    }
}
