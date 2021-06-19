using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    partial class ItemTemplate
    {
        //public class Chair
        //{
        //    static public readonly int ID = IDSequence;
        //    static public void Initialize()
        //    {
        //        Factory.Initialize();
        //        GameObject.Objects.Add(Factory.Default);
        //    }
        //    public class Factory : IItemFactory
        //    {
        //        public static void Initialize() { }
        //        public GameObject Create(List<GameObjectSlot> materials)
        //        {
        //            return Create();
        //        }
        //        static GameObject Create()
        //        {
        //            GameObject obj = new GameObject();
        //            obj["Info"] = new GeneralComponent(ID, ObjectType.Furniture, "Chair", "Used to rest your butt.").Initialize(ItemSubType.Stool);
        //            //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("furniture/stool", new Vector2(16, 24), new Vector2(16, 24)));


        //            // TODO: put orientations inside the same bone (or sprite) instead of creating a separate bone for each orientation
        //            //obj.GetComponent<SpriteComponent>().SetOrientations(
        //            //    Bone.Create(BoneDef.Torso, new Sprite("furniture/chair", Sprite.HalfCubeDepth) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) }),
        //            //    Bone.Create(BoneDef.Torso, new Sprite("furniture/chairback", Sprite.HalfCubeDepth) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) }),
        //            //    Bone.Create(BoneDef.Torso, new Sprite("furniture/chairback2", Sprite.HalfCubeDepth) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) }),
        //            //    Bone.Create(BoneDef.Torso, new Sprite("furniture/chair2", Sprite.HalfCubeDepth) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) })
        //            //    );
        //            var body = new Bone(BoneDef.Torso);
        //            body.SetOrientations(
        //                new Sprite("furniture/chair", Sprite.HalfCubeDepth) { OriginGround = new Vector2(16, 24) },
        //                new Sprite("furniture/chair2", Sprite.HalfCubeDepth) { OriginGround = new Vector2(16, 24) },
        //                new Sprite("furniture/chairback2", Game1.Instance.Content.Load<Texture2D>("graphics/items/furniture/chairback2depth")) { OriginGround = new Vector2(16, 24) },
        //                new Sprite("furniture/chairback", Game1.Instance.Content.Load<Texture2D>("graphics/items/furniture/chairbackdepth")) { OriginGround = new Vector2(16, 24) }
        //                );
        //            obj.AddComponent(new SpriteComponent(body));//.Initialize(new Sprite("furniture/chair", Sprite.CubeDepth) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 24) });

        //            obj["Physics"] = new PhysicsComponent(solid: true, height: 0.49f, size: 1, weight: 5);
        //            obj.AddComponent<GuiComponent>().Initialize(0);
        //            return obj;
        //        }
        //        static public readonly GameObject Default = Create();
        //    }
        //}
    }
}
