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

namespace Start_a_Town_.Components.Materials
{
    class Logs : MaterialType.RawMaterial
    {
        //static public int ID;

        //public override GameObject Create(int id)
        static public Sprite SpriteLogs;
        protected override Entity CreateTemplate()//Initialize()
        {
            ID = MaterialType.RawMaterial.GetNextID();
            //GameObject obj = ItemTemplate.Item;// new Entity();
            var obj = new Entity();

            obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Material, "Logs", "It came from a tree", Quality.Common).Initialize(ItemSubType.Logs);
            obj.GetInfo().StackMax = 6;
            obj.AddComponent<GuiComponent>().Initialize(5, 64);
            var asd = Map.BlockDepthMap;
            var sprite = new Sprite("logsbw", asd) { OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) };
            SpriteLogs = sprite;
            obj.AddComponent<SpriteComponent>().Initialize(sprite);
            //obj.AddComponent(new SpriteComponent(new Bone(BoneDef.Torso, sprite)));
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 4);
            //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Sawing);
            //obj.AddComponent<RawMaterialComponent>().Initialize(Skill.Carpentry);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);

            return obj;
        }
    }
}
