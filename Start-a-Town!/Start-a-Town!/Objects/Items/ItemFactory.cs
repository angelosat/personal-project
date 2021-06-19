using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static class ItemFactory
    {
        //static Item Create(params Material[] mats)
        //{
        //    return null;
        //}
        //static public Item CreateTool<T>(T def) where T: ItemToolDef
        static public Entity CreateTool(ItemDef def)
        {
            var obj = new Entity
            {
                Def = def,
                //Quality = Quality.GetRandom()
            };// ItemTemplate.Item;// new GameObject();

            //obj.AddComponent(new GuiComponent() { Icon = new UI.Icon(def.DefaultSprite) });

            obj.AddComponent<ToolAbilityComponent>();
            //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);

            obj.AddComponent(new SpriteComponent(def));// DefaultSprite)
                                                       //.AddMaterial(BoneDef.EquipmentHandle, handleMat)
                                                       //.AddMaterial(BoneDef.EquipmentHead, headMat));// 24) }));
            obj.InitComps();

            return obj;
        }
        static public Entity CreateTool(ItemDef def, Dictionary<string, Entity> ingredients)
        {
            var obj = CreateTool(def);
            //obj.GetComponent<SpriteComponent>().InitMaterials(def, ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
            obj.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
            return obj;
        }
        static public Entity CreateToolFromRandomMaterials(ItemDef def)
        {
            var mats = CraftingIngredients.GetRandomMaterialsFor(def);
            var obj = CreateTool(def);
            //obj.InitComps();
            //obj.GetComponent<SpriteComponent>().InitMaterials(def, mats);
            obj.SetMaterials(mats);
            obj.SetQuality(Quality.GetRandom());
            return obj;
        }
        static public Entity CreateItem(ItemDef def)
        {
            var obj = Activator.CreateInstance(def.ItemClass) as Entity;// new Item(def);
            //var obj = def.Create();
            obj.Def = def;
            //obj.AddComponent(new ReagentComponent(def));
            obj.AddComponent(new SpriteComponent(def));
            obj.InitComps();
            return obj;
        }
        static public Entity CreateItem<T>(T def) where T : ItemDef
        {
            var obj = Activator.CreateInstance(def.ItemClass) as Entity;// new Item(def);
            obj.Def = def;
            //obj.AddComponent(new ReagentComponent(def));
            obj.AddComponent(new SpriteComponent(def));
            obj.InitComps();
            return obj;
        }
        static public GameObject CreateEntity(EntityDef def)
        {
            var obj = Activator.CreateInstance(def.ItemClass) as GameObject;// new Item(def);
            throw new NotImplementedException();
            //obj.Def = def;
            //obj.AddComponent(new ReagentComponent(def));
            //obj.InitComps();
            //return obj;
        }
        //[Obsolete]
        ///// <summary>
        ///// // TODO pass plantproperties instead of plantdef?
        ///// </summary>
        ///// <param name="plantDef"></param>
        ///// <returns></returns>
        //static public Entity CreateSeeds(ItemDef plantDef) // TODO pass plantproperties instead of plantdef?
        //{
        //    throw new Exception();
        //    if (plantDef.PlantProperties is null)
        //        throw new Exception();
        //    var obj = CreateItem(ItemDefOf.Seeds);
        //    obj.AddComponent(new SeedComponent(plantDef));
        //    //obj.GetComponent<SeedComponent>().PlantDef = plantDef;

        //    return obj;
        //}
        static public Entity CreateSeeds(PlantProperties plantprops) // TODO pass plantproperties instead of plantdef?
        {
            var obj = CreateItem(ItemDefOf.Seeds);
            obj.GetComp<SeedComponent>().SetPlant(plantprops);
            return obj;
        }
        //static public GameObject Create(EntityDef def)
        //{
        //    var obj = Activator.CreateInstance(def.ItemClass) as Item;// new Item(def);
        //    obj.Def = def;
        //    obj.AddComponent(new ReagentComponent(def));
        //    obj.InitComps();
        //    return obj;
        //}

        static public Entity CreateFrom(ItemDef def, Material mat)
        {
            var obj = CreateItem(def);
            //obj.Sprite.InitMaterials(mat);
            ////obj.Name = mat.Name + " " + obj.Name;
            //obj.Name = $"{mat.Prefix} {obj.Name}";
            obj.SetMaterial(mat);
            return obj;
        }
        //static public Entity Craft(ItemDef def, Dictionary<string, Entity> ingredients)
        //{
        //    var obj = CreateItem(def);
        //    //obj.AddComponent(new SpriteComponent(def).InitMaterials(def, ingredients.Ingredients));
        //    obj.GetComponent<SpriteComponent>().InitMaterials(def, ingredients);
        //    obj.InitComps();
        //    return obj;
        //}
        static public Entity CreateMeal(ItemDef def, ItemDefMaterialAmount[] ingredients)
        {
            var meal = CreateItem(def);
            meal.GetComponent<ConsumableComponent>().InitIngredients(ingredients);
            meal.InitComps();
            return meal;
        }
    }
}
