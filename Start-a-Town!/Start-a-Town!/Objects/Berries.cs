using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_
{
    class Berries
    {
        static public readonly GameObject Template = Berries.Create();
        internal static void Initialize()
        {
            GameObject.Objects.Add(Template);
            //StorageCategory.Food.Add(Template);
        }

        static public Entity Create()
        {
            var obj = new Entity();// Food();
            obj.Def = ItemDefOf.Fruit;
            //obj.AddComponent(new DefComponent(GameObject.Types.Berries, "Consumable", "Berries", ObjectType.Consumable) { StackMax = 8,StoreCategory = StorageCategory.Food });
            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("berries", new Vector2(16, 24), new Vector2(16, 24)));
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("berries", new Vector2(16, 32), new Vector2(16, 24)));
            //obj.AddComponent(new PhysicsComponent());
            obj.AddComponent<GuiComponent>().Initialize(7, 64);
            //obj.AddComponent(new ConsumableComponent(Verbs.Eat,
            //    StatusCondition.Create(Message.Types.Buff, "Berry Freshness", "The super tasty berries have given you their boon.", Stat.AtkSpeed, 100f, 180f),
            //    StatusCondition.Create(Message.Types.RestoreHealth, "Restore Health", "The super tasty berries have given you their boon.", Stat.Health, 100f)
            //    )
            //{
            //    NeedEffects = new List<AIAdvertisement>() { new AIAdvertisement("Food", 100) },
            //    //Byproducts = new LootTable(new Loot(() => GameObjectDb.Seeds, 1, 1, 2, 4)),// GameObjectDb.Seeds,
            //    Byproducts = new LootTable(new Loot(() => ItemSeeds.Generate(), 1, 1, 2, 4)),// GameObjectDb.Seeds,

            //    Effects = new List<ConsumableEffect>() { new NeedEffect(NeedDef.Hunger, 50) },
            //    Seeds = GameObject.Objects[GameObject.Types.Seeds]
            //}
            //    );
            obj.AddComponent<FoodComponent>();

            //obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Planting);

            //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
            //obj.AddComponent<SkillComponent>().Initialize(Skill.Eating);

            //obj.AddComponent<FunctionComponent>().Initialize(Ability.Planting, Ability.Activate); //Ability.GetAbilityObject(Script.Types.Chopping)
            //  obj.AddComponent<FunctionComponent>().Initialize(Ability.GetAbilityObject(Script.Types.pla, Ability.Activate); //Ability.GetAbilityObject(Script.Types.Chopping)
            return obj;
        }

        
    }
}
