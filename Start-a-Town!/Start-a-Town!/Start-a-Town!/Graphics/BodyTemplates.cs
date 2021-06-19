using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.Graphics
{
    class BodyTemplates
    {
        static public Bone Human
        {
            get
            {
                int distanceofbodyfromground = 23;
                Bone body =
                    new Bone(Bone.Types.Torso, new Sprite("bodyparts/chestbw") { OverlayName = "Shirt", Origin = new Vector2(5, distanceofbodyfromground) }, new Vector2(0), 0f,
                        new Bone(Bone.Types.RightHand, new Vector2(-3, -21), -0.003f,
                            new Bone(Bone.Types.Mainhand, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }
                            , //0.0005f
                    //new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] })
                            new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot() }//.Slot }
                            )
                        .SetSprite(new Sprite("bodyparts/rightHand/hand", new Vector2(5, 2), new Vector2(5, 2)) { OverlayName = "Skin" }
                            .AddOverlay("Shirt", new Sprite("bodyparts/rightHand/sleeve") { OverlayName = "Shirt" })),
                        new Bone(Bone.Types.LeftHand, new Vector2(6, -21), 0.002f,
                            new Bone(Bone.Types.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) })
                        .SetSprite(new Sprite("bodyparts/leftHand/hand", new Vector2(0, 0), new Vector2(0, 0)) { OverlayName = "Skin" }
                            .AddOverlay("Shirt", new Sprite("bodyparts/leftHand/sleeve") { OverlayName = "Shirt" })),
                        new Bone(Bone.Types.RightFoot, new Vector2(-1, -12), -0.001f)
                        .SetSprite(new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                            .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" })),
                        new Bone(Bone.Types.LeftFoot, new Vector2(3, -12), 0.001f)
                        .SetSprite(new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                            .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" })),
                        new Bone(Bone.Types.Head, new Vector2(0, -26), -0.002f,
                            new Bone(Bone.Types.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) })
                        .SetSprite(new Sprite("bodyparts/head/head", new Vector2(6, 12), new Vector2(6, 12)) { OverlayName = "Skin" }
                            .AddOverlay("Hair", new Sprite("bodyparts/hair1") { OverlayName = "Hair" })
                            .AddOverlay("Eyes", new Sprite("bodyparts/head/eyes") { OverlayName = "Eyes" }))
                        );
                return body;
            }
        }

        static public Bone Skeleton
        {
            get
            {

                var spriteRibCage = new Sprite("mobs/skeleton/ribcage") { OverlayName = "Bone", Origin = new Vector2(8, 13) };// Vector2.Zero };//new Vector2(5, distanceofbodyfromground) };                
                var spriteSkull = new Sprite("mobs/skeleton/skull") { OverlayName = "Bone", Origin = new Vector2(4, 8) };//7) };
                var spriteArm = new Sprite("mobs/skeleton/hand") { OverlayName = "Bone", Origin = new Vector2(2, 1) };//0) };
                var spriteLeg = new Sprite("mobs/skeleton/leg") { OverlayName = "Bone", Origin = new Vector2(3, 1) };//0) };

                Bone body =
                    new Bone(Bone.Types.Hips, spriteRibCage, Vector2.Zero, 0f,
                        new Bone(Bone.Types.RightHand, spriteArm, -spriteRibCage.Origin + new Vector2(1, 5), -0.003f,
                            new Bone(Bone.Types.Mainhand, -spriteRibCage.Origin + new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) },
                            //new Bone(Bone.Types.Hauled, -spriteRibCage.Origin + new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] }),
                            new Bone(Bone.Types.Hauled, -spriteRibCage.Origin + new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot()}),//.Slot }),
                        new Bone(Bone.Types.LeftHand, spriteArm, -spriteRibCage.Origin + new Vector2(14, 4), 0.002f,
                            new Bone(Bone.Types.Offhand, -spriteRibCage.Origin + new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) }),
                        new Bone(Bone.Types.RightFoot, spriteLeg, -spriteRibCage.Origin + new Vector2(5, 13), 0.0005f),
                        new Bone(Bone.Types.LeftFoot, spriteLeg, -spriteRibCage.Origin + new Vector2(11, 12), 0.001f),
                        new Bone(Bone.Types.Head, spriteSkull, -spriteRibCage.Origin + new Vector2(8, 0), -0.002f,
                            new Bone(Bone.Types.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) })
                        ) 
                        { OriginGroundOffset = new Vector2(0, -12) };// 
                       // { RestingFrame = new Keyframe(10, new Vector2(0, -12), 0) };

                Bone root = new Bone(Bone.Types.Hips, spriteRibCage) { OriginGroundOffset = new Vector2(0, -11) };
                root.AddJoint(Bone.Types.Head, new Joint(0, -12));
                root.AddJoint(Bone.Types.RightHand, new Joint(-7, -7));//9));
                root.AddJoint(Bone.Types.LeftHand, new Joint(6, -8));//-10));
                root.AddJoint(Bone.Types.RightFoot, new Joint(-3, 1));
                root.AddJoint(Bone.Types.LeftFoot, new Joint(2, 0));

                Bone head = new Bone(Bone.Types.Head, spriteSkull) { Order = -.002f };
                Bone rhand = new Bone(Bone.Types.RightHand, spriteArm) { Order = -0.003f };
                Bone lhand = new Bone(Bone.Types.LeftHand, spriteArm) { Order = 0.002f };
                Bone rleg = new Bone(Bone.Types.RightFoot, spriteLeg) { Order = 0.001f };
                Bone lleg = new Bone(Bone.Types.LeftFoot, spriteLeg) { Order = 0.002f };

                rhand.AddJoint(Bone.Types.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Mainhand) });// BoneGetter = (o) => SpriteComponent.GetRootBone(GearComponent.GetSlot(o, GearType.Mainhand).Object) });
                //rhand.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Hauling) });
                //rhand.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => PersonalInventoryComponent.GetHauling(o) });//.Slot });o.GetComponent<HaulComponent>().GetSlot()});//.Slot });
                rhand.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => o.GetComponent<PersonalInventoryComponent>().GetHauling() });

                root.GetJoint(Bone.Types.Head).SetBone(head);
                root.GetJoint(Bone.Types.RightHand).SetBone(rhand);
                root.GetJoint(Bone.Types.LeftHand).SetBone(lhand);
                root.GetJoint(Bone.Types.RightFoot).SetBone(rleg);
                root.GetJoint(Bone.Types.LeftFoot).SetBone(lleg);
                
                return root;
            }
        }
    }
}
