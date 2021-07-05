using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Particles;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    [Obsolete]
    class GameObjectDb
    {
        static public GameObject MaterialTemplate
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new DefComponent(GameObject.Types.Material, ObjectType.Material, "Material", "Base material item"));//, weight: 2));
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));//new Vector2(16)));
                obj.AddComponent("Physics", new PhysicsComponent(size: 1));

                //obj["Material"] = new MaterialComponent();
                return obj;
            }
        }
  
    }
}
