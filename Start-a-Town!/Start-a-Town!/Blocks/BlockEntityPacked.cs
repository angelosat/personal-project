using System.Collections.Generic;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
namespace Start_a_Town_.Blocks
{
    class BlockEntityPacked : IItemFactory
    {
        public Entity Create(Dictionary<string, Entity> materials)
        {
            var obj = new Entity();

            return obj;
        }

        static public Entity Create(Block block, byte data)
        {
            var obj = new Entity();
            obj.AddComponent<DefComponent>().Initialize(Block.EntityIDRange * 10 + block.EntityID, ObjectType.Package, "Package: " + block.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("box", Map.BlockDepthMap) { OriginGround = new Vector2(16, 24), Joint = new Vector2(16, 24) });
            return obj;
        }
    }
}
