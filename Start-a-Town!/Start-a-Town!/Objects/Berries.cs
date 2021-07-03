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
    class Berries
    {
        static public readonly GameObject Template = Berries.Create();
        internal static void Initialize()
        {
            GameObject.Objects.Add(Template);
        }

        static public Entity Create()
        {
            var obj = new Entity();// Food();
            obj.Def = ItemDefOf.Fruit;
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("berries", new Vector2(16, 32), new Vector2(16, 24)));
            return obj;
        }
    }
}
