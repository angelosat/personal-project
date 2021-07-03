using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_
{
    class ItemPie
    {
        static public readonly GameObject Template = Create();
        internal static void Initialize()
        {
            GameObject.Objects.Add(Template);
            //StorageCategory.Food.Add(Template);
        }
        static public Entity Create()
        {
            var obj = new Entity();
            obj.AddComponent(new DefComponent(GameObject.Types.Pie, "Consumable", "Pie", ObjectType.Consumable) { StackMax = 8 });
            obj.AddComponent<SpriteComponent>();//.Initialize(new Sprite("berries", new Vector2(16, 32), new Vector2(16, 24)));
            obj.AddComponent(new PhysicsComponent());
            
            return obj;
        }
    }
}
