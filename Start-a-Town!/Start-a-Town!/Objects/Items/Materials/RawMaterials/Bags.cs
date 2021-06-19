using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components.Materials
{
    class Bags : MaterialType.RawMaterial
    {
        //static public int ID;
        //public override GameObject Create(int id)
        protected override Entity CreateTemplate()// Initialize()
        {
            ID = MaterialType.RawMaterial.GetNextID();
            var obj = new Entity();
            obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Material, "Bag", "A bag containing soil").Initialize(ItemSubType.Bags);
            obj.GetInfo().StackMax = 8;
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("soilbagbw", Map.BlockDepthMap) { OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) });
            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("soilbagbw", Map.BlockDepthMap) { Origin = new Vector2(16, 32), Joint = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(13, 64);
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1, weight: 2);
            obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Blocks);
            return obj;
        }

        
    }
}
