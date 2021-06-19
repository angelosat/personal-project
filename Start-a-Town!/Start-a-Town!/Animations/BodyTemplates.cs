using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class BodyTemplates
    {
        static public Bone Human
        {
            get
            {
                int distanceofbodyfromground = 23;
                Bone body =
                    new Bone(BoneDef.Torso, new Sprite("bodyparts/chestbw") { OverlayName = "Shirt", OriginGround = new Vector2(5, distanceofbodyfromground) }, new Vector2(0), 0f,
                        new Bone(BoneDef.RightHand, new Vector2(-3, -21), -0.003f,
                            new Bone(BoneDef.Mainhand, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }
                            , //0.0005f
                    //new Bone(BoneDef.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] })
                            new Bone(BoneDef.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot() }//.Slot }
                            )
                        .SetSprite(new Sprite("bodyparts/rightHand/hand", new Vector2(5, 2), new Vector2(5, 2)) { OverlayName = "Skin" }
                            .AddOverlay("Shirt", new Sprite("bodyparts/rightHand/sleeve") { OverlayName = "Shirt" })),
                        new Bone(BoneDef.LeftHand, new Vector2(6, -21), 0.002f,
                            new Bone(BoneDef.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) })
                        .SetSprite(new Sprite("bodyparts/leftHand/hand", new Vector2(0, 0), new Vector2(0, 0)) { OverlayName = "Skin" }
                            .AddOverlay("Shirt", new Sprite("bodyparts/leftHand/sleeve") { OverlayName = "Shirt" })),
                        new Bone(BoneDef.RightFoot, new Vector2(-1, -12), -0.001f)
                        .SetSprite(new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                            .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" })),
                        new Bone(BoneDef.LeftFoot, new Vector2(3, -12), 0.001f)
                        .SetSprite(new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                            .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" })),
                        new Bone(BoneDef.Head, new Vector2(0, -26), -0.002f,
                            new Bone(BoneDef.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) })
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

                var spriteRibCage = new Sprite("mobs/skeleton/ribcage") { OverlayName = "Bone", OriginGround = new Vector2(8, 13) };// Vector2.Zero };//new Vector2(5, distanceofbodyfromground) };                
                var spriteSkull = new Sprite("mobs/skeleton/skull") { OverlayName = "Bone", OriginGround = new Vector2(4, 8) };//7) };
                var spriteArm = new Sprite("mobs/skeleton/hand") { OverlayName = "Bone", OriginGround = new Vector2(2, 1) };//0) };
                var spriteLeg = new Sprite("mobs/skeleton/leg") { OverlayName = "Bone", OriginGround = new Vector2(3, 1) };//0) };

                Bone body =
                    new Bone(BoneDef.Hips, spriteRibCage, Vector2.Zero, 0f,
                        new Bone(BoneDef.RightHand, spriteArm, -spriteRibCage.OriginGround + new Vector2(1, 5), -0.003f,
                            new Bone(BoneDef.Mainhand, -spriteRibCage.OriginGround + new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) },
                            //new Bone(BoneDef.Hauled, -spriteRibCage.Origin + new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] }),
                            new Bone(BoneDef.Hauled, -spriteRibCage.OriginGround + new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot()}),//.Slot }),
                        new Bone(BoneDef.LeftHand, spriteArm, -spriteRibCage.OriginGround + new Vector2(14, 4), 0.002f,
                            new Bone(BoneDef.Offhand, -spriteRibCage.OriginGround + new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) }),
                        new Bone(BoneDef.RightFoot, spriteLeg, -spriteRibCage.OriginGround + new Vector2(5, 13), 0.0005f),
                        new Bone(BoneDef.LeftFoot, spriteLeg, -spriteRibCage.OriginGround + new Vector2(11, 12), 0.001f),
                        new Bone(BoneDef.Head, spriteSkull, -spriteRibCage.OriginGround + new Vector2(8, 0), -0.002f,
                            new Bone(BoneDef.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) })
                        ) 
                        { OriginGroundOffset = new Vector2(0, -12) };// 
                       // { RestingFrame = new Keyframe(10, new Vector2(0, -12), 0) };

                Bone root = new Bone(BoneDef.Hips, spriteRibCage) { OriginGroundOffset = new Vector2(0, -11) };
                root.AddJoint(BoneDef.Head, new Joint(0, -12));
                root.AddJoint(BoneDef.RightHand, new Joint(-7, -7));//9));
                root.AddJoint(BoneDef.LeftHand, new Joint(6, -8));//-10));
                root.AddJoint(BoneDef.RightFoot, new Joint(-3, 1));
                root.AddJoint(BoneDef.LeftFoot, new Joint(2, 0));

                Bone head = new Bone(BoneDef.Head, spriteSkull) { Order = -.002f };
                Bone rhand = new Bone(BoneDef.RightHand, spriteArm) { Order = -0.003f };
                Bone lhand = new Bone(BoneDef.LeftHand, spriteArm) { Order = 0.002f };
                Bone rleg = new Bone(BoneDef.RightFoot, spriteLeg) { Order = 0.001f };
                Bone lleg = new Bone(BoneDef.LeftFoot, spriteLeg) { Order = 0.002f };

                rhand.AddJoint(BoneDef.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Mainhand) });// BoneGetter = (o) => SpriteComponent.GetRootBone(GearComponent.GetSlot(o, GearType.Mainhand).Object) });
                //rhand.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Hauling) });
                //rhand.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => PersonalInventoryComponent.GetHauling(o) });//.Slot });o.GetComponent<HaulComponent>().GetSlot()});//.Slot });
                rhand.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => o.GetComponent<PersonalInventoryComponent>().GetHauling() });

                root.GetJoint(BoneDef.Head).SetBone(head);
                root.GetJoint(BoneDef.RightHand).SetBone(rhand);
                root.GetJoint(BoneDef.LeftHand).SetBone(lhand);
                root.GetJoint(BoneDef.RightFoot).SetBone(rleg);
                root.GetJoint(BoneDef.LeftFoot).SetBone(lleg);
                
                return root;
            }
        }
    }
}
