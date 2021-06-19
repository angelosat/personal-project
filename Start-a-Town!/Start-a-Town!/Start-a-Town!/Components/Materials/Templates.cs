using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Components.Materials
{
    partial class MaterialType
    {
        //public class Template : GameObject
        //{

        //}


        //static Template _Planks
        //{
        //    get
        //    {
        //        Template obj = new Template(); ;
        //        obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.WoodenPlank, ObjectType.Material, "Plank", "Made out of wood", Quality.Common);//, weight: 1));)
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 24), new Vector2(16, 24)));
        //        obj.AddComponent<GuiComponent>().Initialize(10, 8);
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Framing);
        //        obj.AddComponent<MaterialComponent>().Initialize(null, Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
        //        return obj;
        //    }
        //}
        //static Template _Ore
        //{
        //    get
        //    {
        //        Template obj = new Template(); ;
        //        obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Ore, ObjectType.Material, "Ore", "A piece of mineral ore", Quality.Common);//, weight: 1));)
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });
        //        obj.AddComponent<GuiComponent>().Initialize(10, 8);
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        obj.AddComponent<MaterialComponent>().Initialize(null, Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks);
        //        return obj;
        //    }
        //}
        //static Template _Bars
        //{
        //    get
        //    {
        //        Template obj = new Template();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Bar, ObjectType.Material, "Bar", "Used for crafting of weapons, armor, and tools.");
        //        obj.AddComponent<GuiComponent>().Initialize(17, 1);
        //        //obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[25] } }, new Vector2(16, 24));
        //        obj["Sprite"] = new SpriteComponent(new Sprite("metalbarbw", Map.BlockDepthMap) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) });
        //        obj["Physics"] = new PhysicsComponent(size: 1);
        //        obj.AddComponent<MaterialComponent>().Initialize(null, Reaction.Product.Types.Tools);
        //        return obj;
        //    }
        //}

        //static public readonly Template Planks = _Planks;
        //static public readonly Template Ore = _Ore;
        //static public readonly Template Bars = _Bars;

        
    }
}
