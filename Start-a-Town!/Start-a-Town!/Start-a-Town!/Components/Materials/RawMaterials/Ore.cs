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
    class Ore : MaterialType.RawMaterial
    {
        //public override GameObject Create(int id)
        protected override GameObject Template()//public override GameObject Initialize()
        {
            this.ID = IDSequence;
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(this.ID, ObjectType.Material, "Ore", "A piece of mineral ore", Quality.Common).Initialize(ItemSubType.Ore);
            obj.GetInfo().StackMax = 6;

            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent(new StackableComponent(8));
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks);
            return obj;
        }
    }
}
