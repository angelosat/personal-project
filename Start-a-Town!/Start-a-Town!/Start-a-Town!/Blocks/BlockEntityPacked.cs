using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Items;
using Microsoft.Xna.Framework;
namespace Start_a_Town_.Blocks
{
    class BlockEntityPacked : IItemFactory
    {
        public GameObject Create(List<GameObjectSlot> materials)
        {
            GameObject obj = new GameObject();

            return obj;
        }

        static public GameObject Create(Block block, byte data)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(Block.EntityIDRange * 10 + block.EntityID, ObjectType.Package, "Package: " + block.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<BlockComponent>().Initialize(block, data);
            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite(block.First().Name, Map.BlockDepthMap) { Origin = Block.OriginCenter, MouseMap = BlockMouseMap });
            //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/item-box");
            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("box", new Vector2(16, 24)));
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("box", Map.BlockDepthMap) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) });
            obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
            return obj;
        }
    }
}
