using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components.Materials
{
    class Planks : MaterialType.RawMaterial
    {
        //static public int ID;
        protected override Entity CreateTemplate()//GameObject Template()
        {
            var obj = new Entity();
            //ID = MaterialType.RawMaterial.GetNextID();
            this.ID = GetNextID();
            obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Material, "Planks", "Processed logs", Quality.Common).Initialize(ItemSubType.Planks);
            obj.GetInfo().StackMax = 8;

            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("planksbw", new Vector2(16, 28), new Vector2(16, 24)));
            obj.AddComponent<GuiComponent>().Initialize(10, 8);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 1);
            //obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Framing);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks, Reaction.Product.Types.Workbenches);
            return obj;
        }

        
    }
}
