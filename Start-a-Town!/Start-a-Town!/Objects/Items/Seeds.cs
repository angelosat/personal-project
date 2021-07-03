using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class ItemSeeds
    {
        static public readonly GameObject Template = Generate();

        static public void Initialize()
        {
            GameObject.Objects.Add(Template);
            Start_a_Town_.ItemCategory.RawMaterials.Add((int)GameObject.Types.Seeds);
        }
        static public Entity Generate()
        {
            var obj = new Entity();
            obj["Info"] = new DefComponent(GameObject.Types.Seeds, ObjectType.Material, "Seeds", "Base seeds") { StackMax = 16 };//, weight: 2));
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("seeds", new Vector2(16, 32), new Vector2(16, 24)));
            obj["Physics"] = new PhysicsComponent(size: 0);
            //obj.AddComponent<SeedComponent>().Initialize(GameObject.Types.BerryBush);
            obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
            obj.AddComponent<ToolAbilityComponent>().Initialize(ToolAbilityDef.Planting);
            obj.AddComponent(new Components.Vegetation.PlantableComponent(Components.Vegetation.PlantableComponent.PlantSeed));
            return obj;

            //GameObject obj = MaterialTemplate;// new GameObject();
            //obj["Info"] = new DefComponent(GameObject.Types.Seeds, ObjectType.Material, "Seeds", "Base seeds") { StackMax = 16 };//, weight: 2));
            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("seeds", new Vector2(16, 32), new Vector2(16, 24)));
            //obj.AddComponent<GuiComponent>().Initialize(6, 64);
            //obj["Physics"] = new PhysicsComponent(size: 0);
            //obj.AddComponent<SeedComponent>().Initialize(GameObject.Types.BerryBush);
            //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
            //obj.AddComponent<SkillComponent>().Initialize(Skill.Planting);
            ////obj.AddComponent(new ConsumableComponent(Verbs.Eat));
            //obj.AddComponent(new Components.Vegetation.PlantableComponent(Components.Vegetation.PlantableComponent.PlantSeed));
            //return obj;
        }
    }
}
