using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        class Rock
        {
            static readonly int ID = GetNextID();
            static public void Initialize()
            {
                Factory.Initialize();
                //GameObject.Objects.Add(Factory.Default);
            }
            public class Factory :IItemFactory
            {
                public static void Initialize() { }
                public Entity Create(Dictionary<string, Entity> materials)
                {
                    throw new Exception();
                    //return Create(materials["Body"].GetComponent<MaterialsComponent>().Parts["Body"].Material);

                }
                static GameObject Create(Material material)

                {
                    GameObject obj = new GameObject();
                    obj.AddComponent<DefComponent>().Initialize(ID, ObjectType.Material, "Rock", "A piece of rock", Quality.Common).Initialize(ItemSubType.Rock);
                    obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", Map.BlockDepthMap) { OriginGround = new Vector2(16, 24) });
                    obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
                    obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools, Reaction.Product.Types.Blocks);
                    return obj;
                }
            }
        }
    }
}
