using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Materials
{
    class Rocks : MaterialType.RawMaterial
    {
        //static public int ID;
        protected override Entity CreateTemplate()//GameObject Template()
        {
            var obj = new Entity();
            //ID = MaterialType.RawMaterial.GetNextID();
            this.ID = GetNextID();
            obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Material, "Rock", "A piece of rock", Quality.Common).Initialize(ItemSubType.Rock);
            obj.GetInfo().StackMax = 8;

            var sprite = new Sprite("boulder", Map.BlockDepthMap);
            var handleSprite = new Sprite(sprite) { OriginGround = new Vector2(16, 16), WhiteSpace = 3 };
            Bone handle = new Bone(BoneDef.EquipmentHandle, handleSprite) { OriginGroundOffset = new Vector2(0, 0), Order = 0.001f };
            obj.AddComponent(new SpriteComponent(handle, new Sprite("boulder") { OriginGround = new Vector2(16, 24), WhiteSpace = 3 }));



            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks);
            return obj;
        }

        public GameObject CreateFrom(Material mat)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Material, mat.Name + " " + "Rock", "A piece of rock", Quality.Common).Initialize(ItemSubType.Rock);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { OriginGround = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks);
            Sprite sprite = new Sprite(obj.GetSprite()) { Tint = mat.Color, Shininess = mat.Type.Shininess }; // this is the one 
            obj.GetComponent<SpriteComponent>().Sprite = sprite;
            obj.Body.Sprite = sprite;
            obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
            return obj;
        }
    }
}
