using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    static class ItemFactory
    {
        static public Entity CreateTool(ItemDef def)
        {
            var obj = new Entity
            {
                Def = def,
            };

            obj.AddComponent<ToolAbilityComponent>();
            obj.AddComponent(new SpriteComponent(def));
                                                       
            obj.InitComps();

            return obj;
        }
        static public Entity CreateTool(ItemDef def, Dictionary<string, Entity> ingredients)
        {
            var obj = CreateTool(def);
            obj.SetMaterials(ingredients.ToDictionary(i => i.Key, i => i.Value.PrimaryMaterial));
            return obj;
        }
        static public Entity CreateToolFromRandomMaterials(ItemDef def)
        {
            var mats = GetRandomMaterialsFor(def);
            var obj = CreateTool(def);
            obj.SetMaterials(mats);
            obj.SetQuality(Quality.GetRandom());
            return obj;
        }
        static public Entity CreateItem(ItemDef def)
        {
            var obj = Activator.CreateInstance(def.ItemClass) as Entity;
            obj.Def = def;
            obj.AddComponent(new SpriteComponent(def));
            obj.InitComps();
            return obj;
        }
        
        static public Entity CreateFrom(ItemDef def, Material mat)
        {
            var obj = CreateItem(def);
            obj.SetMaterial(mat);
            return obj;
        }

        static public Dictionary<string, Material> GetRandomMaterialsFor(ItemDef def)
        {
            var dic = new Dictionary<string, Material>();
            foreach (var r in def.CraftingProperties.Reagents)
            {
                var i = r.Value.Ingredient.GetAllValidItemDefs();
                var m = i.SelectMany(i => i.GetValidMaterials());
                var md = m.Distinct();
                var mat = md.ToArray().SelectRandom(Material.Randomizer);
                dic[r.Value.Name] = mat;
            }
            return dic;
        }
    }
}
