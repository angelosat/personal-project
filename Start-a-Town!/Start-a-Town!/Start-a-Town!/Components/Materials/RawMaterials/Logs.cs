using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Materials.RawMaterials
{
    class Logs : MaterialType.RawMaterial
    {
        //public override GameObject Create(int id)
        protected override GameObject Template()//Initialize()
        {
            this.ID = IDSequence;
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, "Logs", "It came from a tree", Quality.Common).Initialize(ItemSubType.Logs);
            obj.GetInfo().StackMax = 6;
            obj.AddComponent<GuiComponent>().Initialize(5, 64);
            var asd = Map.BlockDepthMap;
            var sprite = new Sprite("logsbw", asd) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) };
            obj.AddComponent<SpriteComponent>().Initialize(sprite);
            //obj.AddComponent(new SpriteComponent(new Bone(Bone.Types.Torso, sprite)));
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 4);
            //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Sawing);
            //obj.AddComponent<RawMaterialComponent>().Initialize(Skill.Carpentry);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
            return obj;
        }

        //public GameObject CreateFrom(Material mat)
        //{
        //    GameObject obj = new GameObject();
        //    obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, mat.Name + " " + "Log", "It came from a tree", Quality.Common).Initialize(ItemSubType.Logs);
        //    //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24)));
        //    var sprite = new Sprite("logsbw", Map.BlockDepthMap) { Origin = new Vector2(16, 28), Joint = new Vector2(16, 24) };
        //    obj.AddComponent(new SpriteComponent(new Bone(Bone.Types.Torso, sprite) { Material = mat }));
        //    obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 1);
        //    obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
        //    return obj;
        //}
    }
}
