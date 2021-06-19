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
    class Bags : MaterialType.RawMaterial
    {
        //public override GameObject Create(int id)
        protected override GameObject Template()// Initialize()
        {
            this.ID = IDSequence;
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, "Bag", "A bag containing soil").Initialize(ItemSubType.Bags);
            obj.GetInfo().StackMax = 8;
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("soilbagbw", Map.BlockDepthMap) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(13, 64);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Blocks);
            return obj;
        }
    }
}
