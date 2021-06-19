using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Materials.RawMaterials
{
    class Rocks : MaterialType.RawMaterial
    {
        //public override GameObject Create(int id)
        protected override GameObject Template()// public override GameObject Initialize()
        {
            this.ID = IDSequence;
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, "Rock", "A piece of rock", Quality.Common).Initialize(ItemSubType.Rock);
            obj.GetInfo().StackMax = 8;

            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks);
            return obj;
        }

        public GameObject CreateFrom(Material mat)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, mat.Name + " " + "Rock", "A piece of rock", Quality.Common).Initialize(ItemSubType.Rock);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });
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
