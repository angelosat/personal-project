﻿using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class BodyDef
    {
        static readonly Sprite hips = new Sprite("bodyparts/hips") { OverlayName = "Pants", OriginGround = new Vector2(4, 0) };
        static readonly Sprite torso = new Sprite("bodyparts/chestbw", new Vector2(6, 11), new Vector2(6, 11)) { OverlayName = "Shirt" };
        static readonly Sprite rhand = new Sprite("bodyparts/rightHand/hand", new Vector2(5, 2), new Vector2(5, 2)) { OverlayName = "Skin" }
                        .AddOverlay("Shirt", new Sprite("bodyparts/rightHand/sleeve") { OverlayName = "Shirt" });
        static readonly Sprite lhand = new Sprite("bodyparts/leftHand/hand", new Vector2(0, 0), new Vector2(0, 0)) { OverlayName = "Skin" }
                 .AddOverlay("Shirt", new Sprite("bodyparts/leftHand/sleeve") { OverlayName = "Shirt" });
        static readonly Sprite rleg = new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                   .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" });
        static readonly Sprite lleg = new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                        .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" });
        static readonly Sprite head = new Sprite("bodyparts/head/head", new Vector2(6, 12), new Vector2(6, 12)) { OverlayName = "Skin" }
                        .AddOverlay("Hair", new Sprite("bodyparts/hair1") { OverlayName = "Hair" })
                        .AddOverlay("Eyes", new Sprite("bodyparts/head/eyes") { OverlayName = "Eyes" });

        static readonly int distanceofbodyfromground = 23;

        static readonly Sprite spriteRibCage = new Sprite("mobs/skeleton/ribcage") { OverlayName = "Bone", OriginGround = new Vector2(8, 13) };                
        static readonly Sprite spriteSkull = new Sprite("mobs/skeleton/skull") { OverlayName = "Bone", OriginGround = new Vector2(4, 8) };
        static readonly Sprite spriteArm = new Sprite("mobs/skeleton/hand") { OverlayName = "Bone", OriginGround = new Vector2(2, 1) };
        static readonly Sprite spriteLeg = new Sprite("mobs/skeleton/leg") { OverlayName = "Bone", OriginGround = new Vector2(3, 1) };

        static public readonly Bone Skeleton =
            new Bone(BoneDef.Hips, spriteRibCage) { OriginGroundOffset = new Vector2(0, -11)}
                .AddJoint(new Vector2(0, -12), new Bone(BoneDef.Head, spriteSkull, -.002f))
                .AddJoint(new Vector2(-7, -7), new Bone(BoneDef.RightHand, spriteArm, -0.003f)
                    .AddJoint(BoneDef.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, AttachmentFunc = o => GearComponent.GetSlot(o, GearType.Mainhand).Object })
                    .AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, AttachmentFunc = o => o.GetHauled() }))
                .AddJoint(new Vector2(6, -8), new Bone(BoneDef.LeftHand, spriteArm, 0.002f))
                .AddJoint(new Vector2(-3, 1), new Bone(BoneDef.RightFoot, spriteLeg,  0.001f))
                .AddJoint(new Vector2(2, 0), new Bone(BoneDef.LeftFoot, spriteLeg,0.002f));

        static public readonly Bone SkeletonNew =
            new Bone(BoneDef.Hips) { OriginGroundOffset = new Vector2(0, -11) }
                .AddJoint(Vector2.Zero, new Bone(BoneDef.Torso, spriteRibCage)
                    .AddJoint(new Vector2(0, -12), new Bone(BoneDef.Head, spriteSkull, -.002f))
                    .AddJoint(new Vector2(-7, -7), new Bone(BoneDef.RightHand, spriteArm, -0.003f)
                        .AddJoint(BoneDef.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, AttachmentFunc = o => GearComponent.GetSlot(o, GearType.Mainhand).Object })
                        .AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, AttachmentFunc = o => o.GetHauled() }))
                    .AddJoint(new Vector2(6, -8), new Bone(BoneDef.LeftHand, spriteArm, 0.002f)))
                .AddJoint(new Vector2(-3, 1), new Bone(BoneDef.RightFoot, spriteLeg, 0.001f))
                .AddJoint(new Vector2(2, 0), new Bone(BoneDef.LeftFoot, spriteLeg, 0.002f));

        static public readonly Bone NpcNew =
            new Bone(BoneDef.Hips, hips) { OriginGroundOffset = new Vector2(0, -12) }
                .AddJoint(new Vector2(-2, 0), new Bone(BoneDef.RightFoot, rleg, -.002f))
                .AddJoint(new Vector2(2, 0), new Bone(BoneDef.LeftFoot, lleg, -.001f))
                .AddJoint(Vector2.Zero, new Bone(BoneDef.Torso, torso)
                    .AddJoint(new Vector2(-1, -14), new Bone(BoneDef.Head, head, -.002f)
                        .AddJoint(BoneDef.Helmet, new Joint(0, -6)))
                    .AddJoint(new Vector2(5, -9), new Bone(BoneDef.LeftHand, lhand, .002f)
                        .AddJoint(BoneDef.Offhand, new Joint(0, 4) { Angle = 5 * (float)Math.PI / 4f, AttachmentFunc = (o) => GearComponent.GetSlot(o, GearType.Offhand).Object }))
                    .AddJoint(new Vector2(-4, -9), new Bone(BoneDef.RightHand, rhand, -.004f)
                        .AddJoint(BoneDef.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, AttachmentFunc = o => GearComponent.GetSlot(o, GearType.Mainhand).Object })
                        .AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, AttachmentFunc = o => o.GetHauled() })));
    }
}
