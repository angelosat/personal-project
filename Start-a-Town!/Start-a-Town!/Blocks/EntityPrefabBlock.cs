﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.Blocks
{
    class EntityPrefabBlock 
    {
        static public readonly int ID = ItemTemplate.GetNextID();
        internal static void Initialize()
        {
            Factory.Initialize();
        }

        static readonly Entity Default = GetDefault();
        static Entity GetDefault()
        {
            var obj = new Entity();
            obj["Ownership"] = new OwnershipComponent();
            obj["Info"] = new DefComponent(ID, ObjectType.Package, "Prefab Block", "A Prefab block that can be place.");
            Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/crate1");
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24)));
            obj["Physics"] = new PhysicsComponent(size: 1);
            return obj;
        }

        public class Factory : IItemFactory
        {
            public Entity Create(Dictionary<string, Entity> mats)
            {
                return Default;
            }
            
            internal static void Initialize()
            {
            }
        }
    }
}
