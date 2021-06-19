using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Graphics;
//using Start_a_Town_.Items;
using Start_a_Town_.Components.Consumables;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Stats;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Particles;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    class GameObjectDb
    {
        static public GameObject Actor
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent<PhysicsComponent>().Initialize(height: 2, size: -1);

                obj.AddComponent<DefComponent>().Initialize(GameObject.Types.Actor, ObjectType.Human, "Actor", "A character", saveName: true).Initialize(ItemSubType.Human);
                //obj.AddComponent<LightComponent>().Initialize(15);
                obj.AddComponent<MobileComponent>();
                obj.AddComponent<ClimbingComponent>();
                obj.AddComponent<HaulComponent>();
                //obj.AddComponent<PersonalInventoryComponent>().Initialize(16);
                obj.AddComponent(new PersonalInventoryComponent(16));
                obj.AddComponent<WorkComponent>();
                //obj.AddComponent<AbilitiesComponent>();
                obj.AddComponent(new AttributesComponent(
                    AttributeDef.Strength,
                    AttributeDef.Intelligence,
                    AttributeDef.Dexterity));

                //obj.AddComponent<NeedsComponent>().Initialize(new NeedsHierarchy().Add
                //        ("Physiological", new NeedsCollection(
                //            Need.Factory.Create(Need.Types.Hunger)
                //            )));

                obj.AddComponent<BodyComponent>().Initialize(BodyComponent.Actor);
                //obj.AddComponent<GearComponent>().Initialize(obj, //"Hauling", "Head", "Chest", "Feet", "Hands", "Legs", "Mainhand", "Offhand");
                obj.AddComponent(new GearComponent(
                    //GearType.Hauling,
                    GearType.Mainhand,
                    GearType.Offhand,
                    GearType.Head,
                    GearType.Chest,
                    GearType.Feet,
                    GearType.Hands,
                    GearType.Legs
                    ));

                //obj.AddComponent<ResourcesComponent>().Initialize(
                //    ResourceDef.Create(ResourceDef.ResourceTypes.Health, 1000, 1000),
                //    ResourceDef.Create(ResourceDef.ResourceTypes.Stamina, 100, 100)
                //    );
                obj.AddComponent(new ResourcesComponent(ResourceDef.Health, ResourceDef.Stamina));
          


                obj.AddComponent(new ResourcesComponent(ResourceDef.Health, ResourceDef.Stamina));

                obj.AddComponent<AttackComponent>();
                //obj.AddComponent<AdvertiseNeedComponent>().Initialize(new AIAction(Script.Types.Threat, Need.Types.Brains, 100));
                obj.AddComponent<SpellBookComponent>().Initialize(Spell.SpellTypes.Healing, Spell.SpellTypes.Fireball);
                obj.AddComponent<StatusComponent>();
                obj.AddComponent<StatsComponentNew>().Initialize(
                    Stat.Create(Stat.Types.MaxWeight, 0),
                    Stat.Create(Stat.Types.WalkSpeed, 1),
                    Stat.Create(Stat.Types.DmgReduction, 0)
                    );
                //obj.AddComponent(new StatsComponentNew(new Dictionary<Stat.Types,Stat>(){
                //    {Stat.Types.WalkSpeed, Stat.Create(Stat.Types.WalkSpeed)}
                //}));
                obj.AddComponent<SpeechComponent>();
                obj.AddComponent<SkillsComponent>().Initialize(3,
                    SkillOld.Create(SkillOld.Types.Lumberjacking),
                    SkillOld.Create(SkillOld.Types.Mining),
                    SkillOld.Create(SkillOld.Types.Digging),
                    SkillOld.Create(SkillOld.Types.Crafting),
                    SkillOld.Create(SkillOld.Types.Carpentry),
                    SkillOld.Create(SkillOld.Types.Farming),
                    SkillOld.Create(SkillOld.Types.Construction),
                    SkillOld.Create(SkillOld.Types.Argiculture, a => a.Level = 5)
                    );
                obj.AddComponent<PartyComponent>().Initialize(4);
                obj.AddComponent<KnowledgeComponent>().Initialize(
                    new Knowledge2(GameObject.Types.BlueprintHandle, 1),
                    new Knowledge2(GameObject.Types.BlueprintWorkbench, 1),
                    new Knowledge2(GameObject.Types.BlueprintHammer, 1),
                    new Knowledge2(GameObject.Types.BlueprintAxeHead, 1),
                    new Knowledge2(GameObject.Types.BlueprintAxe, 1),
                    //new Knowledge2(GameObject.Types.Handsaw, 1),
                    new Knowledge2(GameObject.Types.BlueprintCobblestone, 1),
                    new Knowledge2(GameObject.Types.BlueprintSoil, 1),
                    new Knowledge2(GameObject.Types.BlueprintScaffold, 1)
                    )
                    .Initialize(
                    new BlueprintMemory(GameObject.Types.BlueprintAxeHead, 130),
                    new BlueprintMemory(GameObject.Types.BlueprintHandle, 70),
                    new BlueprintMemory(GameObject.Types.BlueprintDoor, 50),
                    new BlueprintMemory(GameObject.Types.BlueprintAxe, 120)
                    );
                //obj.AddComponent<ControlComponent>();
                obj.AddComponent(new BloodComponent());
                obj.AddComponent(new BlockingComponent());

                int distanceofbodyfromground = 23;
                //Bone body =
                //new Bone("Body", new Sprite("bodyparts/chestbw") { OverlayName = "Shirt", Origin = new Vector2(5, distanceofbodyfromground) }, new Vector2(0), 0f,
                //    new Bone("Right Hand", new Vector2(-3, -21), -0.003f,
                //    //new Bone(Stat.Mainhand.Name, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Mainhand] }, //0.0005f
                //        new Bone(Stat.Mainhand.Name, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }, //0.0005f
                //    //new Bone("Hauled", new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] })
                //        new Bone("Hauled", new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => GearComponent.GetSlot(o, GearType.Hauling) })// o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] })                 
                //.SetSprite(new Sprite("bodyparts/rightHand/hand", new Vector2(5, 2), new Vector2(5, 2)) { OverlayName = "Skin" }
                //        .AddOverlay("Shirt", new Sprite("bodyparts/rightHand/sleeve") { OverlayName = "Shirt" })),
                //    new Bone("Left Hand", new Vector2(6, -21), 0.002f,
                //        new Bone(Stat.Offhand.Name, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Offhand] })
                //    .SetSprite(new Sprite("bodyparts/leftHand/hand", new Vector2(0, 0), new Vector2(0, 0)) { OverlayName = "Skin" }
                //        .AddOverlay("Shirt", new Sprite("bodyparts/leftHand/sleeve") { OverlayName = "Shirt" })),
                //    new Bone("Right Foot", new Vector2(-1, -12), -0.001f)
                //    .SetSprite(new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                //        .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" })),
                //    new Bone("Left Foot", new Vector2(3, -12), 0.001f)
                //    .SetSprite(new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                //        .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" })),
                //    new Bone("Head", new Vector2(0, -26), -0.002f,
                //        new Bone("Helmet", new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Head] })
                //    .SetSprite(new Sprite("bodyparts/head/head", new Vector2(6, 12), new Vector2(6, 12)) { OverlayName = "Skin" }
                //        .AddOverlay("Hair", new Sprite("bodyparts/hair1") { OverlayName = "Hair" })
                //        .AddOverlay("Eyes", new Sprite("bodyparts/head/eyes") { OverlayName = "Eyes" }))
                //    );


                //var hips = new Bone("Hips", new Vector2(5, distanceofbodyfromground), 0);
                //var torso = new Bone("Torso", new Sprite("bodyparts/chestbw"), Vector2.Zero, -.001f);
                //var rfoot = 
                //    new Bone("Right Foot", new Vector2(-1, -12), -0.001f)
                //    .SetSprite(new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                //    .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" }));
                //var lfoot = new Bone("Left Foot", new Vector2(3, -12), 0.001f)
                //    .SetSprite(new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                //    .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" }));
                //var rhand = new Bone("Right Hand", new Vector2(-3, -21), -0.003f);

                var hipssprite = new Sprite("bodyparts/hips") { OverlayName = "Pants", OriginGround = new Vector2(4, 0) };
                var torsosprite = new Sprite("bodyparts/chestbw", new Vector2(6, 11), new Vector2(6, 11)) { OverlayName = "Shirt" };//, Origin = new Vector2(6, 11) };
                var rarmsprite = new Sprite("bodyparts/rightHand/hand", new Vector2(5, 2), new Vector2(5, 2)) { OverlayName = "Skin" }
                        .AddOverlay("Shirt", new Sprite("bodyparts/rightHand/sleeve") { OverlayName = "Shirt" });
                var larmsprite = new Sprite("bodyparts/leftHand/hand", new Vector2(0, 0), new Vector2(0, 0)) { OverlayName = "Skin" }
                        .AddOverlay("Shirt", new Sprite("bodyparts/leftHand/sleeve") { OverlayName = "Shirt" });
                var rlegsprite = new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                        .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" });
                var llegsprite = new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                        .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" });
                var headsprite = new Sprite("bodyparts/head/head", new Vector2(6, 12), new Vector2(6, 12)) { OverlayName = "Skin" }
                        .AddOverlay("Hair", new Sprite("bodyparts/hair1") { OverlayName = "Hair" })
                        .AddOverlay("Eyes", new Sprite("bodyparts/head/eyes") { OverlayName = "Eyes" });

                Bone body =
                    new Bone(BoneDef.Hips, hipssprite, new Vector2(0), 0f,
                        new Bone(BoneDef.Torso, torsosprite, Vector2.Zero, 0f,// new Vector2(0, -11), 0f,
                            new Bone(BoneDef.RightHand, rarmsprite, new Vector2(-4, -9), -.003f,// new Vector2(-3, -10), -0.003f,
                                new Bone(BoneDef.Mainhand, new Vector2(-2, 11), 0.0005f)
                                ,// { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp)},//, SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }, //0.0005f
                                //new Bone(BoneDef.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => GearComponent.GetSlot(o, GearType.Hauling) }),
                                new Bone(BoneDef.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot()}//.Slot }
                                ),
                            new Bone(BoneDef.LeftHand, larmsprite, new Vector2(5, -9), .002f,// new Vector2(6, -10), 0.002f,
                                new Bone(BoneDef.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) }),
                            new Bone(BoneDef.Head, headsprite, new Vector2(-1, -14), -0.002f,
                                new Bone(BoneDef.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) })),
                        new Bone(BoneDef.RightFoot, rlegsprite, new Vector2(-2, 0), -.002f),// -0.001f),
                        new Bone(BoneDef.LeftFoot, llegsprite, new Vector2(2, 0), -.001f)// new Vector2(3, -12), -0.001f) //0.001f)
                        ) { OriginGroundOffset = new Vector2(0, -12) };// { Origin = new Vector2(0, -11) };// { RestingFrame = new Keyframe(10, new Vector2(0, -12), 0) };//  Offset = new Vector2(0, -11) };


                var hips = new Bone(BoneDef.Hips, hipssprite) { OriginGroundOffset = new Vector2(0, -12) };
                //var hips = new Bone(BoneDef.Hips, hipssprite) { OriginGroundOffset = new Vector2(-1, -12) };
                hips.AddJoint(BoneDef.Torso, new Joint());
                hips.AddJoint(BoneDef.RightFoot, new Joint(-2, 0));
                hips.AddJoint(BoneDef.LeftFoot, new Joint(2, 0));
                //hips.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { SlotGetter = (o) => o.GetComponent<HaulComponent>().Slot });
                //test for sleeping position //hips.RestingFrame = new Keyframe(0, Vector2.Zero, -(float)(Math.PI / 3f));
                //hips.Origins.Add(BoneDef.None, new Vector2(0,-12));// new Vector2(4, 12));

                var torso = new Bone(BoneDef.Torso, torsosprite);
                torso.AddJoint(BoneDef.Head, new Joint(-1, -14));
                torso.AddJoint(BoneDef.RightHand, new Joint(-4, -9));
                torso.AddJoint(BoneDef.LeftHand, new Joint(5, -9));
                //torso.Origins.Add(BoneDef.Torso, Vector2.Zero);// + new Vector2(6, 11));

                var head = new Bone(BoneDef.Head, headsprite) { Order = -.002f };
                head.AddJoint(BoneDef.Helmet, new Joint(0, -6));
                //head.Origins.Add(BoneDef.Head, Vector2.Zero);// + new Vector2(6, 11));

                var righthand = new Bone(BoneDef.RightHand, rarmsprite) { Order = -.004f };
                //righthand.AddJoint(BoneDef.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f });//, BoneGetter = () => { } });
                righthand.AddJoint(BoneDef.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Mainhand) });// BoneGetter = (o) => SpriteComponent.GetRootBone(GearComponent.GetSlot(o, GearType.Mainhand).Object) });
                //righthand.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Hauling) });
                //righthand.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => o.GetComponent<HaulComponent>().Slot });
                righthand.AddJoint(BoneDef.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => o.GetComponent<PersonalInventoryComponent>().GetHauling() });

                //righthand.Origins.Add(BoneDef.RightHand, Vector2.Zero);// + new Vector2(6, 11));

                var lefthand = new Bone(BoneDef.LeftHand, larmsprite) { Order = .002f };
                lefthand.AddJoint(BoneDef.Offhand, new Joint(0, 4) { Angle = 5 * (float)Math.PI / 4f, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Offhand) });
                //lefthand.Origins.Add(BoneDef.LeftHand, Vector2.Zero);// + new Vector2(6, 11));

                var rightfoot = new Bone(BoneDef.RightFoot, rlegsprite) { Order = -.002f };
                var leftfoot = new Bone(BoneDef.LeftFoot, llegsprite) { Order = -.001f };
                //rightfoot.Origins.Add(BoneDef.RightFoot, Vector2.Zero);// + new Vector2(6, 11));
                //leftfoot.Origins.Add(BoneDef.LeftFoot, Vector2.Zero);// + new Vector2(6, 11));

                torso.GetJoint(BoneDef.Head).SetBone(head);
                torso.GetJoint(BoneDef.RightHand).SetBone(righthand);
                torso.GetJoint(BoneDef.LeftHand).SetBone(lefthand);

                hips.GetJoint(BoneDef.Torso).SetBone(torso);
                hips.GetJoint(BoneDef.RightFoot).SetBone(rightfoot);
                hips.GetJoint(BoneDef.LeftFoot).SetBone(leftfoot);


                //var hipssprite = new Sprite("bodyparts/hips") { OverlayName = "Pants", Origin = new Vector2(3, 12) };
                //var torsosprite = new Sprite("bodyparts/chestbw", new Vector2(5, 11), new Vector2(5, 12)) { OverlayName = "Shirt" };//, Origin = new Vector2(6, 11) };
                //var rhandsprite = new Sprite("bodyparts/rightHand/hand", new Vector2(5, 2), new Vector2(5, 2)) { OverlayName = "Skin" }
                //        .AddOverlay("Shirt", new Sprite("bodyparts/rightHand/sleeve") { OverlayName = "Shirt" });
                //var lhandsprite = new Sprite("bodyparts/leftHand/hand", new Vector2(0, 0), new Vector2(0, 0)) { OverlayName = "Skin" }
                //        .AddOverlay("Shirt", new Sprite("bodyparts/leftHand/sleeve") { OverlayName = "Shirt" });
                //var rlegsprite = new Sprite("bodyparts/rightleg/rightleg", new Vector2(4, 0), new Vector2(4, 0)) { OverlayName = "Pants" }
                //        .AddOverlay("Shoe", new Sprite("bodyparts/rightleg/rightshoe") { OverlayName = "Shoes" });
                //var llegsprite = new Sprite("bodyparts/leftleg/leftleg", new Vector2(3, 0), new Vector2(3, 0)) { OverlayName = "Pants" }
                //        .AddOverlay("Shoe", new Sprite("bodyparts/leftleg/leftshoe") { OverlayName = "Shoes" });
                //var headsprite = new Sprite("bodyparts/head/head", new Vector2(6, 12), new Vector2(6, 12)) { OverlayName = "Skin" }
                //        .AddOverlay("Hair", new Sprite("bodyparts/hair1") { OverlayName = "Hair" })
                //        .AddOverlay("Eyes", new Sprite("bodyparts/head/eyes") { OverlayName = "Eyes" });
                //
                //Bone body =
                //    new Bone(BoneDef.Hips, hipssprite, new Vector2(0), 0f,
                //        new Bone(BoneDef.Torso, torsosprite, new Vector2(0, -11), 0f,
                //            new Bone(BoneDef.RightHand, rhandsprite, new Vector2(-3, -10), -0.003f,
                //                new Bone(BoneDef.Mainhand, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }, //0.0005f
                //                new Bone(BoneDef.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => GearComponent.GetSlot(o, GearType.Hauling) }),
                //            new Bone(BoneDef.LeftHand, lhandsprite, new Vector2(6, -10), 0.002f,
                //                new Bone(BoneDef.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Offhand] }),
                //            new Bone(BoneDef.Head, headsprite, new Vector2(0, -15), -0.002f,
                //                new Bone(BoneDef.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Head] })),
                //        new Bone(BoneDef.RightFoot, rlegsprite, new Vector2(-1, -12), -.002f),// -0.001f),
                //        new Bone(BoneDef.LeftFoot, llegsprite, new Vector2(3, -12), -0.001f) //0.001f)
                //        );// { Offset = new Vector2(5, distanceofbodyfromground) };


                Sprite sprite = new Sprite("bodyparts/best2", new Vector2(9, 38)); //new Vector2(17 / 2, 38));

                //obj.AddComponent<SpriteComponent>().Initialize(body, sprite);
                obj.AddComponent<SpriteComponent>().Initialize(hips, sprite);

                return obj;
            }
        }
        
      
        static public GameObject TrainingDummy
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.TrainingDummy, ObjectType.Entity, "Training Dummy", "A Training Dummy.");
                obj.AddComponent<GuiComponent>().Initialize();
                obj["Physics"] = new PhysicsComponent(height: 4, size: -1, solid: false);// true);
                
                obj["Conditions"] = new StatusComponent();
                obj.AddComponent<StatsComponent>().Initialize(new Dictionary<Stat.Types, float>(){
                    {Stat.Types.KnockbackResistance, 1f}
                });
                //StatsComponent.Add(obj, Tuple.Create(Stat.Types.KnockbackResistance, 1f));

                //obj.AddComponent<ResourcesComponent>().Initialize(
                //    ResourceDef.Create(ResourceDef.ResourceTypes.Health, 100, 100)
                //    );
                obj.AddComponent(new ResourcesComponent(ResourceDef.Health));


                //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Characters/best/zombie");
                //obj["Sprite"] = new SpriteComponent(new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, tex.Height)));// - Tile.BlockHeight));// 8)));
                //obj.AddComponent<SpriteComponent>().Initialize(BodyTemplates.Human, new Sprite("bodyparts/zombie", new Vector2(17 / 2, 38)));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("bodyparts/zombie", new Vector2(17 / 2, 38)));

                return obj;
            }
        }
        static public GameObject Crate
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Ownership"] = new OwnershipComponent();
                obj["Info"]  = new DefComponent(GameObject.Types.Crate, ObjectType.Container, "Crate", "Can store items.");
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/crate1");
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24)));//new Vector2(tex.Width / 2, tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2)));/// 2)));
                //obj.AddComponent(new SpriteComponent(new Sprite("blocks/furniture/chest", Sprite.CubeDepth) { Origin = new Vector2(16, 24) }));
                obj.AddComponent<GuiComponent>().Initialize(10, 1);
                obj["Physics"] = new PhysicsComponent(size: 1);
                obj.AddComponent<Components.Containers.StorageComponent>().Initialize(16);
                return obj;
            }
        }
        
        static public GameObject Campfire
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Campfire, ObjectType.Lightsource, "Campfire", "Warm and lovely");
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[20] } }, new Vector2(16, 24), Block.TileMouseMap);
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("campfire", new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize(20, 1);
                obj.AddComponent<PhysicsComponent>();
                //obj["Light"] = new LightComponent(15);
                obj.AddComponent<LightComponent>().Initialize(15);
                //obj.AddComponent(new ParticlesComponent(new ParticleEmitter(new Vector3(0, 0, .5f), Engine.TargetFps, .2f, .1f)));
                obj.AddComponent(new ParticlesComponent(new ParticleEmitterSphere()
                {
                    Offset = new Vector3(0, 0, .5f),
                    Lifetime = Engine.TicksPerSecond,
                    Radius = .2f,
                    ParticleWeight = -.1f,
                    Force = .01f,
                    Friction = .5f,
                    ColorBegin = Color.Yellow,
                    ColorEnd = Color.Red,
                    SizeBegin = 3,
                    SizeEnd = 1
                }));// .01f)));

                return obj;
            }
        }
        static public GameObject BuildingPlan
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent(new DefComponent(GameObject.Types.BuildingPlan, ObjectType.BuildingPlan, "Building Plan", "A Building plan") { InCatalogue = false });
                obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));
                obj.AddComponent<GuiComponent>().Initialize(12, 1);
                obj["Physics"] = new PhysicsComponent(size: -1);

               // obj["Map"] = new MapComponent();
                obj["Project"] = new BuildingPlanComponent();
                return obj; 
            }
        }
        static public GameObject MaterialTemplate
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new DefComponent(GameObject.Types.Material, ObjectType.Material, "Material", "Base material item"));//, weight: 2));
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));//new Vector2(16)));
                obj.AddComponent<GuiComponent>().Initialize(12, 1);
                obj.AddComponent("Physics", new PhysicsComponent(size: 1));

                //obj["Material"] = new MaterialComponent();
                return obj;
            }
        }
  

        static public GameObject PickaxeHead
        {
            get
            {
                return Components.ItemCraftingComponentFactory.Create(GameObject.Types.PickaxeHead, "Pickaxe Head", "Component of pickaxes.", size: 0);
            }
        }
        static public GameObject AxeHead
        {
            get
            {
                return Components.ItemCraftingComponentFactory.Create(GameObject.Types.AxeHead, "Axe Head", "Component of axes.", size: 0);
            }
        }
        static public GameObject ShovelHead
        {
            get
            {
                return Components.ItemCraftingComponentFactory.Create(GameObject.Types.ShovelHead, "Shovel Head", "Component of shovels.", size: 0);
            }
        }
        static public GameObject Handle
        {
            get
            {
                return Components.ItemCraftingComponentFactory.Create(GameObject.Types.Handle, "Handle", "Component of tools and weapons.", size: 0);
            }
        }
        //static public GameObject Twig
        //{
        //    get
        //    {
        //        //GameObject obj = Components.MaterialObjectFactory.Create(GameObject.Types.Twig, "Twig", "Component of tools and weapons.", size: 0, spriteID: 11);
        //        //obj.AddComponent<GuiComponent>().Initialize(new Icon(Map.ItemSheet, 11, 32), stackMax: 10); 
        //        GameObject obj = new GameObject();
        //        obj.AddComponent<DefComponent>().Initialize(GameObject.Types.Twig, ObjectType.Material, "Twigs", "Component of tools and weapons.");
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("twigs", new Vector2(16, 24)));
        //        obj.AddComponent<GuiComponent>().Initialize("twigs");
        //        obj.AddComponent<PhysicsComponent>();
        //        obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", MaterialDefOf.Twig));//LightWood));
        //        obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
        //        return obj;
        //    }
        //}
        static public GameObject Cobblestones
        {
            get
            {
                //GameObject obj = Components.MaterialObjectFactory.Create(GameObject.Types.Cobble, "Cobble", "Component of tools and weapons.", size: 0, spriteID: 19);
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Map.ItemSheet, 19, 32), stackMax: 10);
                //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                //obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Stone, Reaction.Product.Types.Tools);

                GameObject obj = new GameObject();
                obj.AddComponent<DefComponent>().Initialize(GameObject.Types.Cobblestones, ObjectType.Material, "Cobblestones", "Component of tools and weapons.");
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize("boulder");
                obj.AddComponent<PhysicsComponent>();
                //obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Stone);
                obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", MaterialDefOf.Stone));
                obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
                return obj;
            }
        }
        static public GameObject Stone
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Stone, ObjectType.Material, "Stone", "It came from mining");
                obj.AddComponent<GuiComponent>().Initialize(12, 1);
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("stone", Map.BlockDepthMap) { OriginGround = new Vector2(16, 24) });//new Vector2(10, 16)));////new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));

                obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
                //obj.AddComponent<MaterialComponent>().Initialize(Material.Stone);
                obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", MaterialDefOf.Stone));
                obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
                return obj;
            }
        }
        //static public GameObject Coal
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Coal, ObjectType.Fuel, "Coal", "It came from mining");
        //        obj.AddComponent<GuiComponent>().Initialize(17, 1);
        //        //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[17] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("coal", new Vector2(16, 24)));
        //        obj["Physics"] = new PhysicsComponent(size: 1);
        //        obj["Fuel"] = new FuelComponent();
        //        return obj;
        //    }
        //}
        static public GameObject Paper
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Paper, ObjectType.Material, "Paper", "A blank piece of paper");
                obj.AddComponent<GuiComponent>().Initialize(17, 1);
                //obj["Sprite"] = new SpriteComponent(new Sprite("blankpage", new Vector2(16, 24),new Vector2(16, 24)));// Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[28] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
                obj["Physics"] = new PhysicsComponent(size: 0);
                obj["Fuel"] = new FuelComponent();
                return obj;
            }
        }
        

        static public GameObject Package
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new DefComponent(GameObject.Types.Package, ObjectType.Package, "Package", "An object is packed inside this."));//, height: 2));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/item-box");
                //obj.AddComponent("Sprite", new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, 24)));//tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2)));
                //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("box", new Vector2(tex.Width / 2, 24)));
                //obj.AddComponent(new SpriteComponent((new Sprite("box", Sprite.CubeDepth) { OriginGround = new Vector2(16, 24) })));

                obj.AddComponent<GuiComponent>().Initialize(9);
                obj.AddComponent("Box", new BoxComponent());
                obj["Package"] = new PackageComponent();
                obj["Physics"] = new PhysicsComponent(1);
                //obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Unpack);
                return obj;
            }
        }
        static public GameObject BodyPart
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.BodyPart, "Body Part", "Base body part");
                obj.AddComponent<GuiComponent>().Initialize(iconID: 8, stackMax: 1);
                obj["Equip"] = new EquipComponent();
                //obj["Bonuses"] = new StatsComponent();
                obj["Stats"] = new StatsComponent();
                //obj["Abilities"] = new FunctionComponent();
                return obj;
            }
        }
        static public GameObject Fists
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent(new DefComponent(GameObject.Types.Fists, ObjectType.BodyPart, "Fists", "Your fists.") { InCatalogue = false });

                obj.AddComponent<GuiComponent>().Initialize(iconID: 0, stackMax: 1);
                obj["Equip"] = new BodypartComponent(Stat.Mainhand.Name);
                obj["Stats"] = new StatsComponent().SetValue(Stat.Blunt.Name, 5f);
                obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                //obj.AddComponent<FunctionComponent>().Initialize(Ability.Attack, Ability.Digging, Ability.Consume);

                obj["Weapon"] = new WeaponComponent(1, Tuple.Create(Stat.Types.Blunt, 5f));
                return obj;
            }
        }
        static public GameObject BareHands
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent(new DefComponent(GameObject.Types.BareHands, ObjectType.BodyPart, "Bare Hands", "Your bare hands.") { InCatalogue = false });
                obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                //obj.AddComponent(new SpriteComponent()
                obj.AddComponent<GuiComponent>().Initialize(iconID: 0, stackMax: 1);
                obj["Equip"] = new BodypartComponent(Stat.Hands.Name);//, 4);
                obj["Stats"] = new StatsComponent();

                
                //obj.AddComponent<FunctionComponent>().Initialize(Ability.Activate, Ability.PickingUp);
                return obj;
            }
        }
        static public GameObject BareFeet
        {
            get
            {
                GameObject obj = BodyPart;// new GameObject();
                obj.AddComponent(new DefComponent(GameObject.Types.BareFeet, ObjectType.BodyPart, "Bare Feet", "Your bare feet.") { InCatalogue = false });
                obj.AddComponent<GuiComponent>().Initialize(iconID: 0, stackMax: 1);
                //obj["Equip"] = new EquipComponent(Stat.Feet.Name);
                //obj.AddComponent<EquipComponent>().Initialize(Stat.Feet.Name, Script.Types.Jumping);
                obj.AddComponent<EquipComponent>().Initialize(GearType.Feet);
                obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                return obj;
            }
        }
        static public GameObject RottenFeet
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent(new DefComponent(GameObject.Types.RottenFeet, ObjectType.BodyPart, "Rotten Feet", "They double stink.") { InCatalogue = false });
                obj.AddComponent<GuiComponent>().Initialize(iconID: 0, stackMax: 1);
                obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                obj["Equip"] = new BodypartComponent(Stat.Feet.Name);//, 4);
                obj["Stats"] = new StatsComponent();
                obj["Stats"][Stat.WalkSpeed.Name] = 0.05f; // new Bonus(0.1f, BonusType.Flat);
                obj["Stats"][Stat.Strength.Name] = 5f; // new Bonus(5f, BonusType.Flat);
                obj.AddComponent<EquipComponent>().Initialize(Tuple.Create(Stat.Types.WalkSpeed, -0.5f));
                return obj;
            }
        }
        static public GameObject CheatShoes
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Shoe, objType: ObjectType.Equipment, name: "Shoes", description: "Comfortable for the feet.", quality: Quality.Cheating);
                obj.AddComponent<GuiComponent>().Initialize(iconID: 0);
                //obj["Equip"] = new EquipComponent(Stat.Feet.Name);//, 4);
                obj.AddComponent<EquipComponent>().Initialize(GearType.Feet);
                obj["Physics"] = new PhysicsComponent();
                //obj["Sprite"] = new SpriteComponent(Sprite.Default);//Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16));
                EquipComponent.Add(obj, Tuple.Create(Stat.Types.JumpHeight, 1f), Tuple.Create(Stat.Types.WalkSpeed, 1f));
                //BonusesComponent bonuses = BonusesComponent.Create(obj, "Bonuses");
                //bonuses[Stat.WalkSpeed.Name] = new Bonus(0.1f, BonusType.Percentile);
                return obj;
            }
        }
        
        static public GameObject Weapon
        {
            get
            {
                GameObject obj = new GameObject();
                //obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Sword, objType: ObjectType.Weapon, name: "Sword", description: "A basic sword");
                //ItemComponent.Add(obj, 1, 20);
                obj.AddComponent(new ItemComponent(1));//,20);
                //obj.AddComponent<MaterialComponent>();
                return obj;
            }
        }
        
        static public GameObject Shield
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Shield, objType: ObjectType.Shield, name: "Shield", description: "A basic Shield");
                obj.AddComponent<GuiComponent>().Initialize(iconID: 26);
                //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[26] } }, new Vector2(16, 24)) { Joint = new Vector2(16, 16) });
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("shield", new Vector2(16, 24), new Vector2(16, 16)));
                obj.AddComponent<EquipComponent>().Initialize(GearType.Offhand);
                obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
                obj.AddComponent<ItemCraftingComponent>();
                return obj;
            }
        }
        //static public GameObject Helmet
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new DefComponent(GameObject.Types.Helmet, objType: ObjectType.Armor, name: "Helmet", description: "A basic Helmet");
        //        obj.AddComponent<GuiComponent>().Initialize(iconID: 27);
        //        //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[27] } }, new Vector2(16, 24)) { Joint = new Vector2(16, 16) });
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("helmet", new Vector2(16, 24), new Vector2(16, 16)));
        //        obj.AddComponent<EquipComponent>().Initialize(GearType.Head);
        //        obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
        //        //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping);
        //        obj.AddComponent<ItemCraftingComponent>();
        //        return obj;
        //    }
        //}
        
        static public GameObject Construction
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new DefComponent(GameObject.Types.Construction, ObjectType.Construction, "Construction", "An object that awaits to be constructed"));//, height: 2));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/construction");//item-box");
                //obj["Sprite"] = new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, 24));//tex.Height / 2)));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("construction", new Vector2(16, 24)));
               // obj.AddComponent("Sprite", new SpriteComponent(Map.TerrainSprites, new Rectangle[][] { new Rectangle[] { new Rectangle(1 * 32, 6 * 32, 32, 32) } }, new Vector2(16)));
                obj.AddComponent<GuiComponent>().Initialize(9);
              //  obj.AddComponent("Health", new HealthComponent(maxHealth: 100));
                obj["Physics"] = new PhysicsComponent(size: -1, solid: true, height: 1, weight: 0);// new StickyComponent(height: 1, size: 1, solid: true);

                //obj.AddComponent<ConstructionFootprint>();
                //obj.AddComponent<StructureComponent>();
                return obj;
            }
        }
        static public GameObject BlockEmpty
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new DefComponent(GameObject.Types.Block, ObjectType.Block, "Tile", "A tile"));
                obj.AddComponent<GuiComponent>().Initialize(8);
                //obj.AddComponent<BlockComponent>();
                //obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                return obj;
            }
        }
        static public GameObject BlockDefault
        {
            get
            {
                // TODO: add density property to tile
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new DefComponent(GameObject.Types.Tile, ObjectType.Block, "Tile", "A tile"));
                obj.AddComponent<GuiComponent>().Initialize(8);
                //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.DropOnTarget);
                //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Framing);//, Script.Types.DropOnTarget);
                //obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
                return obj;
            }
        }
        
        static public GameObject ConstructionBlockOld
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent<DefComponent>().Initialize(GameObject.Types.ConstructionBlock, ObjectType.Construction, "Construction Block", "Construction Block.");
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Construction]));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("construction", new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize(new UI.Icon(obj.GetSprite()));
                obj.AddComponent<PhysicsComponent>().Initialize(size: -1);
                //obj.AddComponent<ConstructionComponent>();
                return obj;
            }
        }
        static public GameObject ConstructionBlock
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent<DefComponent>().Initialize(GameObject.Types.ConstructionBlock, ObjectType.Construction, "Construction Block", "Construction Block.");
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Construction]));
                //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("construction", new Vector2(16, 24)));

                obj.AddComponent(new SpriteComponent(new Bone(BoneDef.Torso, new Sprite("box", Sprite.CubeDepth) { OriginGround = new Vector2(16, 24) })));
                //obj.AddComponent(new SpriteComponent((new Sprite("box", Sprite.CubeDepth) { Origin = new Vector2(16, 24) })));

                obj.AddComponent<GuiComponent>().Initialize(new UI.Icon(obj.GetSprite()));
                obj.AddComponent<PhysicsComponent>().Initialize(size: -1);
                //obj.AddComponent<ConstructionComponent>();
                return obj;
            }
        }
        
        static public GameObject BlueprintBlock
        {
            get
            {
                GameObject obj = GameObjectDb.BlockDefault;
                obj["Info"] = new DefComponent(GameObject.Types.BlueprintBlock, ObjectType.Block, "Blueprint Block", "Represents a world block that will be ignored during construction.");
                //obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.Blueprint]);
                //obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Blueprint]);
                obj["Sprite"]["Hidden"] = true;
                return obj;
            }
        }
        
        static public GameObject ConstructionReservedTile
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.ConstructionReservedTile, ObjectType.Block, "ConstructionReservedTile", "ConstructionReservedTile");
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
                obj.AddComponent<GuiComponent>().Initialize(new UI.Icon(Map.TerrainSprites, 182, 32));
                obj["Sprite"] = new SpriteComponent(Map.TerrainSprites, new Rectangle[][] { new Rectangle[] { new Rectangle(1 * 32, 6 * 32, 32, 32) } }, Block.OriginCenter, Block.BlockMouseMap); //new Rectangle(0, 0, 32, 32)
                obj["Sprite"]["Shadow"] = false;

                return obj;
            }
        }

        //static public GameObject Consumable
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj.AddComponent("Info", new DefComponent(GameObject.Types.Consumable, "Consumable", "Base consumable", ObjectType.Consumable));
        //        //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[15] } }, new Vector2(16, 32 - Block.BlockHeight - Block.Depth / 2f));
        //        obj.AddComponent<GuiComponent>().Initialize(15);
        //        obj.AddComponent("Physics", new PhysicsComponent());
        //        obj.AddComponent("Consumable", new ConsumableComponent(Verbs.Consume));
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
        //        return obj;
        //    }
        //}
        
        static public GameObject Fertilizer
        {
            get
            {
                GameObject obj = MaterialTemplate;// new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Fertilizer, ObjectType.Material, "Fertilizer", "Speeds up growth of plants");//, weight: 2));
                //obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[6] } }, new Vector2(16, 24));//new Vector2(16));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("soilbagbw", new Vector2(16, 24), new Vector2(16, 24)));

                obj.AddComponent<GuiComponent>().Initialize(6, 64);
                obj["Physics"] = new PhysicsComponent(size: 1); //0);
                obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                //obj.AddComponent<SkillComponent>().Initialize(Skill.Fertilizing);
                obj.AddComponent(new FertilizerComponent(1));
                return obj;
            }
        }
        static public GameObject Brain
        {
            get
            {
                GameObject obj = new GameObject();
                //obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Brain, ObjectType.Consumable, "Brain", "A typical snack for zombies");
                obj.AddComponent(new DefComponent(GameObject.Types.Brain, ObjectType.Consumable, "Brain", "A typical snack for zombies"));
                obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                obj.AddComponent<GuiComponent>().Initialize(7, 64);
                return obj;
            }
        }
        //static public GameObject StrengthPotion
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.Consumable;
        //        obj.IDType = GameObject.Types.StrengthPotion;
        //        obj.Name = "Potion of Strength";
        //        obj["Info"]["Quality"] = Quality.Legendary;
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("potion", new Vector2(16, 24), new Vector2(16, 24)));
        //        obj["Consumable"] = new ConsumableComponent(Verbs.Drink, 
        //            StatusCondition.Create(Message.Types.Buff, "Potion of Strength", "You feel stronger after a refreshing drink.", Stat.Strength, 100f, 180f)
        //            )
        //            {
        //                NeedEffects = new List<AIAdvertisement>() { new AIAdvertisement("Water", 20) }
        //            };
        //        //ConsumableComponent cons = obj.GetComponent<ConsumableComponent>("Consumable");
        //        ////cons.Properties.Add("Conditions", new StatusConditionCollection());

        //        ////cons.Conditions.Add(Stat.Types.Strength, 100, 3600);
        //        ////cons.Conditions.Add(Stat.StatDB[Stat.Types.Strength], 100, 3600);
        //        //cons.Conditions.Add(Stat.Strength, 100, 180);
        //        return obj;
        //    }
        //}
        
        static public GameObject SkillObj
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new DefComponent(GameObject.Types.Skill, "Skill", "Base skill");
                obj.AddComponent<GuiComponent>().Initialize(8);
                //obj["Skill"] = new SkillComponent();
                //obj["Skill"][Stat.Value.Name] = 0;
                return obj;
            }
        }
        static public GameObject SkillMining
        {
            get
            {
                GameObject obj = GameObjectDb.SkillObj;
                obj["Info"] = new DefComponent(GameObject.Types.SkillMining, Stat.Mining.Name, "Mining skill");
                obj.AddComponent<GuiComponent>().Initialize(1);
                return obj;
            }
        }
        static public GameObject SkillLumberjacking
        {
            get
            {
                GameObject obj = GameObjectDb.SkillObj;
                obj["Info"] = new DefComponent(GameObject.Types.SkillLumberjacking, Stat.Lumberjacking.Name, "Lumberjacking skill");
                obj.AddComponent<GuiComponent>().Initialize(2);
                return obj;
            }
        }
    
        
    }
}
