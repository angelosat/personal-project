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
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Stats;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.Components.Combat;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;

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

                obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Actor, ObjectType.Human, "Actor", "A character", saveName: true).Initialize(ItemSubType.Human);
                //obj.AddComponent<LightComponent>().Initialize(15);
                obj.AddComponent<MobileComponent>();
                obj.AddComponent<ClimbingComponent>();
                obj.AddComponent<HaulComponent>();
                //obj.AddComponent<PersonalInventoryComponent>().Initialize(16);
                obj.AddComponent(new PersonalInventoryComponent(16));
                obj.AddComponent<WorkComponent>();
                obj.AddComponent<AbilitiesComponent>();
                obj.AddComponent<AttributesComponent>().Initialize(
                    Start_a_Town_.Components.Attribute.Create(Components.Attribute.Types.Strength, 10),
                    Start_a_Town_.Components.Attribute.Create(Components.Attribute.Types.Intelligence, 10),
                    Start_a_Town_.Components.Attribute.Create(Components.Attribute.Types.Dexterity, 10)
                    );
                obj.AddComponent(new Components.Skills.New.SkillsNewComponent(new Components.Skills.New.SkillDigging()));
                //obj.AddComponent<NeedsComponent>().Initialize(new NeedsHierarchy() { 
                //        { "Physiological", new NeedsCollection(
                //            Need.Factory.Create(Need.Types.Hunger)
                //            //new NeedHunger()
                //            ) } });
                obj.AddComponent<NeedsComponent>().Initialize(new NeedsHierarchy().Add
                        ("Physiological", new NeedsCollection(
                            Need.Factory.Create(Need.Types.Hunger)
                    //new NeedHunger()
                            )));
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
                obj.AddComponent<ResourcesComponent>().Initialize(
                    Resource.Create(Resource.Types.Health, 1000, 1000),
                    Resource.Create(Resource.Types.Stamina, 100, 100)
                    //,
                    //Resource.Create(Resource.Types.Mana, 100, 100),
                    //Resource.Create(Resource.Types.Stamina, 100, 100)
                    );
                obj.AddComponent<AttackComponent>();
                obj.AddComponent<AdvertiseNeedComponent>().Initialize(new AIAction(Script.Types.Threat, Need.Types.Brains, 100));
                obj.AddComponent<SpellBookComponent>().Initialize(Spell.Types.Healing, Spell.Types.Fireball);
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

                var hipssprite = new Sprite("bodyparts/hips") { OverlayName = "Pants", Origin = new Vector2(4, 0) };
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
                    new Bone(Bone.Types.Hips, hipssprite, new Vector2(0), 0f,
                        new Bone(Bone.Types.Torso, torsosprite, Vector2.Zero, 0f,// new Vector2(0, -11), 0f,
                            new Bone(Bone.Types.RightHand, rarmsprite, new Vector2(-4, -9), -.003f,// new Vector2(-3, -10), -0.003f,
                                new Bone(Bone.Types.Mainhand, new Vector2(-2, 11), 0.0005f)
                                ,// { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp)},//, SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }, //0.0005f
                                //new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => GearComponent.GetSlot(o, GearType.Hauling) }),
                                new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot()}//.Slot }
                                ),
                            new Bone(Bone.Types.LeftHand, larmsprite, new Vector2(5, -9), .002f,// new Vector2(6, -10), 0.002f,
                                new Bone(Bone.Types.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) }),
                            new Bone(Bone.Types.Head, headsprite, new Vector2(-1, -14), -0.002f,
                                new Bone(Bone.Types.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) })),
                        new Bone(Bone.Types.RightFoot, rlegsprite, new Vector2(-2, 0), -.002f),// -0.001f),
                        new Bone(Bone.Types.LeftFoot, llegsprite, new Vector2(2, 0), -.001f)// new Vector2(3, -12), -0.001f) //0.001f)
                        ) { OriginGroundOffset = new Vector2(0, -12) };// { Origin = new Vector2(0, -11) };// { RestingFrame = new Keyframe(10, new Vector2(0, -12), 0) };//  Offset = new Vector2(0, -11) };


                var hips = new Bone(Bone.Types.Hips, hipssprite) { OriginGroundOffset = new Vector2(0, -12) };
                hips.AddJoint(Bone.Types.Torso, new Joint());
                hips.AddJoint(Bone.Types.RightFoot, new Joint(-2, 0));
                hips.AddJoint(Bone.Types.LeftFoot, new Joint(2, 0));
                //hips.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { SlotGetter = (o) => o.GetComponent<HaulComponent>().Slot });

                //hips.Origins.Add(Bone.Types.None, new Vector2(0,-12));// new Vector2(4, 12));

                var torso = new Bone(Bone.Types.Torso, torsosprite);
                torso.AddJoint(Bone.Types.Head, new Joint(-1, -14));
                torso.AddJoint(Bone.Types.RightHand, new Joint(-4, -9));
                torso.AddJoint(Bone.Types.LeftHand, new Joint(5, -9));
                //torso.Origins.Add(Bone.Types.Torso, Vector2.Zero);// + new Vector2(6, 11));

                var head = new Bone(Bone.Types.Head, headsprite) { Order = -.002f };
                head.AddJoint(Bone.Types.Helmet, new Joint(0, -6));
                //head.Origins.Add(Bone.Types.Head, Vector2.Zero);// + new Vector2(6, 11));

                var righthand = new Bone(Bone.Types.RightHand, rarmsprite) { Order = -.004f };
                //righthand.AddJoint(Bone.Types.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f });//, BoneGetter = () => { } });
                righthand.AddJoint(Bone.Types.Mainhand, new Joint(-2, 11) { Angle = 5 * (float)Math.PI / 4f, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Mainhand) });// BoneGetter = (o) => SpriteComponent.GetRootBone(GearComponent.GetSlot(o, GearType.Mainhand).Object) });
                //righthand.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Hauling) });
                //righthand.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => o.GetComponent<HaulComponent>().Slot });
                righthand.AddJoint(Bone.Types.Hauled, new Joint(-2, 11) { Angle = (float)Math.PI, SlotGetter = (o) => o.GetComponent<PersonalInventoryComponent>().GetHauling() });

                //righthand.Origins.Add(Bone.Types.RightHand, Vector2.Zero);// + new Vector2(6, 11));

                var lefthand = new Bone(Bone.Types.LeftHand, larmsprite) { Order = .002f };
                lefthand.AddJoint(Bone.Types.Offhand, new Joint(0, 4) { Angle = 5 * (float)Math.PI / 4f, SlotGetter = (o) => GearComponent.GetSlot(o, GearType.Offhand) });
                //lefthand.Origins.Add(Bone.Types.LeftHand, Vector2.Zero);// + new Vector2(6, 11));

                var rightfoot = new Bone(Bone.Types.RightFoot, rlegsprite) { Order = -.002f };
                var leftfoot = new Bone(Bone.Types.LeftFoot, llegsprite) { Order = -.001f };
                //rightfoot.Origins.Add(Bone.Types.RightFoot, Vector2.Zero);// + new Vector2(6, 11));
                //leftfoot.Origins.Add(Bone.Types.LeftFoot, Vector2.Zero);// + new Vector2(6, 11));

                torso.GetJoint(Bone.Types.Head).SetBone(head);
                torso.GetJoint(Bone.Types.RightHand).SetBone(righthand);
                torso.GetJoint(Bone.Types.LeftHand).SetBone(lefthand);

                hips.GetJoint(Bone.Types.Torso).SetBone(torso);
                hips.GetJoint(Bone.Types.RightFoot).SetBone(rightfoot);
                hips.GetJoint(Bone.Types.LeftFoot).SetBone(leftfoot);


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
                //    new Bone(Bone.Types.Hips, hipssprite, new Vector2(0), 0f,
                //        new Bone(Bone.Types.Torso, torsosprite, new Vector2(0, -11), 0f,
                //            new Bone(Bone.Types.RightHand, rhandsprite, new Vector2(-3, -10), -0.003f,
                //                new Bone(Bone.Types.Mainhand, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) }, //0.0005f
                //                new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => GearComponent.GetSlot(o, GearType.Hauling) }),
                //            new Bone(Bone.Types.LeftHand, lhandsprite, new Vector2(6, -10), 0.002f,
                //                new Bone(Bone.Types.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Offhand] }),
                //            new Bone(Bone.Types.Head, headsprite, new Vector2(0, -15), -0.002f,
                //                new Bone(Bone.Types.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Head] })),
                //        new Bone(Bone.Types.RightFoot, rlegsprite, new Vector2(-1, -12), -.002f),// -0.001f),
                //        new Bone(Bone.Types.LeftFoot, llegsprite, new Vector2(3, -12), -0.001f) //0.001f)
                //        );// { Offset = new Vector2(5, distanceofbodyfromground) };


                Sprite sprite = new Sprite("bodyparts/best2", new Vector2(9, 38)); //new Vector2(17 / 2, 38));
                //obj.AddComponent<SpriteComponent>().Initialize(body, sprite);
                obj.AddComponent<SpriteComponent>().Initialize(hips, sprite);

                return obj;
            }
        }
        static public GameObject Npc
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new NpcComponent(GameObject.Types.Npc, ObjectType.Human, "Npc", "A character");
                obj["Physics"] = new PhysicsComponent(height: 2, size: -1);
                obj.AddComponent<GuiComponent>();
                //InventoryComponent inventory = new InventoryComponent();
                //obj["Inventory"] = inventory;
                //inventory.AddContainer(16);
                //obj.AddComponent<StorageComponent>().Initialize(16);
                //obj.AddComponent<ParentComponent>();
                //obj.AddComponent<PersonalInventoryComponent>().Initialize(16);
                obj.AddComponent(new PersonalInventoryComponent(16));
                obj.AddComponent(new AttackComponent());
                obj.AddComponent(new BlockingComponent());
                obj.AddComponent<StatsComponentNew>().Initialize(
                    Stat.Create(Stat.Types.MaxWeight, 0),
                    Stat.Create(Stat.Types.WalkSpeed, 1),
                    Stat.Create(Stat.Types.DmgReduction, 0)
                    );
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
                //obj.AddComponent<HaulComponent>();
                obj.AddComponent(new HaulComponent());
                obj.AddComponent(new BloodComponent());
                obj["Equipment"] = BodyComponent.Actor;
                obj["Property"] = new PropertyComponent();
                obj["Speech"] = new SpeechComponent();
                obj.AddComponent<ResourcesComponent>().Initialize(
                    Resource.Create(Resource.Types.Health, 10, 10),// 1000, 1000),
                    Resource.Create(Resource.Types.Stamina, 100, 100));
                //obj["Needs"] = new NeedsComponent()
                //{
                //    NeedsHierarchy = new NeedsHierarchy() { 
                //    { "Physiological", new NeedsCollection() { 
                //        { "Food", new Need("Food", 100) }, 
                //        { "Water", new Need("Water", 100) }, 
                //        { "Sleep", new Need("Sleep", 100) }, 
                //    }},
                //    { "Esteem", new NeedsCollection() { 
                //        { "Achievement", new Need("Achievement", 100) }, 
                //        { "Work", new Need("Work", 100, tolerance: 100) }, 
                //    }}}
                //};
                obj.AddComponent<AttributesComponent>().Initialize(
                    Start_a_Town_.Components.Attribute.Create(Components.Attribute.Types.Strength, 10),
                    Start_a_Town_.Components.Attribute.Create(Components.Attribute.Types.Intelligence, 10),
                    Start_a_Town_.Components.Attribute.Create(Components.Attribute.Types.Dexterity, 10)
                    );
                //obj["Needs"] = new NeedsComponent()
                //{
                //    NeedsHierarchy = new NeedsHierarchy() { 
                //    { "Physiological", new NeedsCollection(
                //        Need.Types.Hunger.CreateNew(tolerance: 100f),
                //        Need.Types.Water.CreateNew(),
                //        Need.Types.Sleep.CreateNew()
                //        )},
                //    { "Esteem", new NeedsCollection(
                //        Need.Factory.Create(Need.Types.Achievement),
                //        Need.Factory.Create(Need.Types.Work, tolerance: 99f)
                //        )}}
                //};
                obj["Needs"] = new NeedsComponent()
                {
                    NeedsHierarchy = new NeedsHierarchy().Add(
                     "Physiological", new NeedsCollection(
                        //Need.Types.Hunger.CreateNew(tolerance: 100f)//,
                        new NeedHunger()
                        //Need.Types.Water.CreateNew(),
                        //Need.Types.Sleep.CreateNew()
                        ))
                .Add(
                     "Esteem", new NeedsCollection(
                         new NeedsWork()
                        //Need.Factory.Create(Need.Types.Achievement),
                        //Need.Factory.Create(Need.Types.Work, tolerance: 99f)
                        ))
                .Add("Cognitive", new NeedsCollection(new NeedCuriosity()))
                };
                obj.AddComponent<WorkerComponent>();
                //obj["Control"] = new ControlComponent();
                obj["Conditions"] = new StatusComponent();
                obj["Stats"] = new StatsComponent();
                obj["Skills"] = new SkillsComponent(0f);
                obj["Skills"][Stat.Mining.Name] = 0f;
                obj["Skills"][Stat.Lumberjacking.Name] = 0f;
                //obj["Health"] = new HealthComponent(20);
                //obj["AI"] = (new AIComponent(new Components.AI.Personality(reaction: Components.AI.ReactionType.Friendly),
                //    new AICombat(), new AICommunication(), new AICoop(), new AIAutonomy(), new AIIdle()));
                obj.AddComponent<MobileComponent>();
                obj.AddComponent<WorkComponent>();
                obj.AddComponent(new AttackComponent());
                obj.AddComponent<AIComponent>().Initialize(new Components.AI.Personality(reaction: Components.AI.ReactionType.Friendly),
                   new BehaviorSelector(
                       new AIAwareness(),
                       new AIMemory(),
                       new AICombat(),
                       new AIAggro(),
                       //new AI.AIMovement(),
                       new AIDialogue(),
                       new AICommands(),
                       new AIFollow(),
                       new BehaviorSequence(
                           //new BehaviorFindJob(),
                           new BehaviorFindJobNew(),
                           new BehaviorEvaluateJob(),
                           new AINextJobStep(),
                           new AI.AIMoveTo(),
                           new AIDoInteraction()),
                       new BehaviorSequence(
                           new AIWait(),
                           new AIWander())
                       ));

                //obj.AddComponent<AIComponent>().Initialize(new Components.AI.Personality(reaction: Components.AI.ReactionType.Friendly),
                //    new BehaviorSelector(
                //        new AIAwareness(),
                //        new AIMemory(),
                //        new AIAggro(),
                //        new BehaviorSequence(
                //            new BehaviorSelector(
                //                new BehaviorFindJob()//,
                //               // new BehaviorFindGoal()
                //                ),
                //         //   new AIMovement(),
                //         new AI.AIMovement(),
                //         new AIDoInteraction()),
                //         //   new AIExecuteScript()),
                //    //new AIIdle()
                //        new BehaviorSequence(
                //            new AIWait(),
                //            new AIWander())
                //        ));


                //int distanceofbodyfromground = 23;
                //Bone body =
                //new Bone(Bone.Types.Torso, new Sprite("bodyparts/chest", new Vector2(5, distanceofbodyfromground)), new Vector2(0), 0f,
                //    new Bone(Bone.Types.RightHand, new Vector2(-3, -21), -0.003f,
                //        new Bone(Bone.Types.Mainhand, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Mainhand] },
                //        new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] })
                //        .SetSprite(new Sprite("bodyparts/righthand", new Vector2(5, 2), new Vector2(5, 2))),
                //    new Bone(Bone.Types.LeftHand, new Vector2(6, -21), 0.002f,
                //        new Bone(Bone.Types.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Offhand] })
                //        .SetSprite(new Sprite("bodyparts/lefthand", new Vector2(0, 0), new Vector2(0, 0))),
                //    new Bone(Bone.Types.RightFoot, new Vector2(-1, -12), -0.001f).SetSprite(new Sprite("bodyparts/rightleg", new Vector2(4, 0), new Vector2(4, 0))),
                //    new Bone(Bone.Types.LeftFoot, new Vector2(3, -12), 0.001f).SetSprite(new Sprite("bodyparts/leftleg", new Vector2(3, 1), new Vector2(3, 1))),
                //    new Bone(Bone.Types.Head, new Vector2(0, -26), -0.002f,
                //        new Bone(Bone.Types.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Head] }
                //        ).SetSprite(new Sprite("bodyparts/head", new Vector2(6, 12), new Vector2(6, 12)))
                //    ) { Origin = new Vector2(5, distanceofbodyfromground) };


                var chest = new Sprite("bodyparts/chest", new Vector2(5, 0));
                var rhand = new Sprite("bodyparts/righthand", new Vector2(5, 2), new Vector2(5, 2));
                var lhand = new Sprite("bodyparts/lefthand", new Vector2(0, 0), new Vector2(0, 0));
                var rleg = new Sprite("bodyparts/rightleg", new Vector2(4, 0), new Vector2(4, 0));
                var lleg = new Sprite("bodyparts/leftleg", new Vector2(3, 1), new Vector2(3, 1));
                var head = new Sprite("bodyparts/head", new Vector2(6, 12), new Vector2(6, 12));

                int distanceofbodyfromground = 23;
                Bone body =
                new Bone(Bone.Types.Torso, chest, new Vector2(0), 0f,
                    new Bone(Bone.Types.RightHand, rhand, new Vector2(-3, -21), -0.003f,
                        new Bone(Bone.Types.Mainhand, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Mainhand) },
                        //new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling] }),
                        new Bone(Bone.Types.Hauled, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, (float)Math.PI), SlotFunc = o => o.GetComponent<HaulComponent>().GetSlot()}),//.Slot }),
                    new Bone(Bone.Types.LeftHand, lhand, new Vector2(6, -21), 0.002f,
                        new Bone(Bone.Types.Offhand, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => GearComponent.GetSlot(o, GearType.Offhand) }),
                    new Bone(Bone.Types.RightFoot, rleg, new Vector2(-1, -12), -0.001f),
                    new Bone(Bone.Types.LeftFoot, lleg, new Vector2(3, -12), 0.001f),
                    new Bone(Bone.Types.Head, head, new Vector2(0, -26), -0.002f,
                        new Bone(Bone.Types.Helmet, new Vector2(0, -6), -0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 0), SlotFunc = o => GearComponent.GetSlot(o, GearType.Head) }
                        )
                    ) { OriginGroundOffset = new Vector2(5, distanceofbodyfromground) };


 


                /*
                //Bone body =// Bone.Create("Body", new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 17, 11, 12) } }, new Vector2(5, 23)), new Vector2(0), 0f,
                //new Bone("Body", new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 17, 11, 12) } }, new Vector2(5, 23)), new Vector2(0), 0f,
                //    new Bone("Right Hand", new Vector2(-3, -21), -0.003f,
                //        new Bone(Stat.Mainhand.Name, new Vector2(-2, 11), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<GearComponent>().Holding }).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(13, 0, 7, 14) } }, new Vector2(5, 2))), //InventoryComponent>().Holding
                //    new Bone("Left Hand", new Vector2(6, -21), 0.002f,
                //        new Bone(Stat.Offhand.Name, new Vector2(0, 4), 0.0005f) { RestingFrame = new Keyframe(10, Vector2.Zero, 5 * (float)Math.PI / 4f, Interpolation.Lerp), SlotFunc = o => o.GetComponent<BodyComponent>().BodyParts[Stat.Offhand.Name].Wearing }).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(20, 0, 3, 10) } }, new Vector2(0, 0))), //BodyComponent>().BodyParts[Stat.Offhand.Name].Wearing
                //    new Bone("Right Foot", new Vector2(-1, -12), -0.001f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(13, 14, 7, 12) } }, new Vector2(4, 0))),
                //    new Bone("Left Foot", new Vector2(3, -12), 0.001f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(20, 14, 7, 12) } }, new Vector2(3, 1))),
                //    new Bone("Head", new Vector2(0, -26), -0.002f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, 13, 17) } }, new Vector2(6, 12)))
                //    );


                //Sprite sprite = new Sprite("bodyparts/best2", new Vector2(17 / 2, 38));
                //obj.AddComponent<SpriteComponent>().Initialize(body, sprite);
                */
                Sprite sprite = new Sprite("mobs/skeleton/full", new Vector2(17 / 2, 38));
                sprite.Origin = new Vector2(sprite.AtlasToken.Texture.Bounds.Width / 2, sprite.AtlasToken.Texture.Bounds.Height);

                obj.AddComponent<SpriteComponent>().Initialize(BodyTemplates.Skeleton, sprite);
                //obj.AddComponent(new SpriteComponent(Bone.Copy(GameObject.Objects[GameObject.Types.Actor].Body)));
                //obj.AddComponent(new SpriteComponent(Bone.Copy(GameObject.Objects[GameObject.Types.Actor].Body), sprite));


                return obj;
            }
        }
        static public GameObject Zombie
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Zombie, ObjectType.Undead, "Zombie", "A zombie.");
                obj["Physics"] = new PhysicsComponent(height: 4, size: -1);
                obj.AddComponent<GuiComponent>();
                obj["Equipment"] = BodyComponent.Zombie;
                //Need.Factory.Create(Need.Types.Hunger)
                //obj.AddComponent<NeedsComponent>().Initialize(new NeedsHierarchy() { 
                //        { "Physiological", new NeedsCollection() { 
                //            { "Hunger", new Need("Hunger", 100) } 
                //        } } });

                //obj.AddComponent<NeedsComponent>().Initialize(new NeedsHierarchy() { 
                //{ "Physiological", new NeedsCollection( 
                //    Need.Factory.Create(Need.Types.Brains, tolerance: 100f)
                //    )}});
                obj.AddComponent<NeedsComponent>().Initialize(new NeedsHierarchy().Add 
                ( "Physiological", new NeedsCollection( 
                    Need.Factory.Create(Need.Types.Brains, tolerance: 100f)
                    )));

                obj["Conditions"] = new StatusComponent();
                obj["Stats"] = new StatsComponent();// StatsComponent.Actor;
                obj.AddComponent<AIComponent>().Initialize(new Components.AI.Personality(Components.AI.ReactionType.Hostile, ObjectType.Human),
                    new BehaviorSelector(
                        new AIAwareness(),
                        new AIMemory(),
                        new AIAggro(),
                        new BehaviorSequence(
                            new BehaviorFindGoal(),
                            new AIMovement(),
                            new AIExecuteScript()),
                        new AIIdle()));
                 //   .SetBehaviors(new AICombat(), new AIIdle());
                //obj["Control"] = new ControlComponent();
               // obj["Health"] = new HealthComponent(100);
                obj["Loot"] = new LootComponent(new Loot(GameObject.Types.EpicShovel, 1, 1));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Characters/best/zombie");
                //obj["Sprite"] = new ActorSpriteComponent(new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, tex.Height)));

                Texture2D partsTex = Game1.Instance.Content.Load<Texture2D>("Graphics/Characters/best/parts2");
                Bone body =
                new Bone(Bone.Types.Torso, new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 17, 11, 12) } }, new Vector2(5, 23)), new Vector2(0), 0f,
                    new Bone(Bone.Types.RightHand, new Vector2(-3, -21), -0.003f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(13, 0, 7, 14) } }, new Vector2(5, 2))),
                    new Bone(Bone.Types.LeftHand, new Vector2(6, -21), 0.002f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(20, 0, 3, 10) } }, new Vector2(0, 0))),
                    new Bone(Bone.Types.RightFoot, new Vector2(-1, -12), -0.001f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(13, 14, 7, 12) } }, new Vector2(4, 0))),
                    new Bone(Bone.Types.LeftFoot, new Vector2(3, -12), 0.001f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(20, 14, 7, 12) } }, new Vector2(3, 1))),
                    new Bone(Bone.Types.Head, new Vector2(0, -26), -0.002f).SetSprite(new Sprite(partsTex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, 13, 17) } }, new Vector2(6, 12)))
                    );
                //  Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Characters/best/best2");
                Sprite sprite = new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, tex.Height));
                obj.AddComponent<SpriteComponent>().Initialize(body, sprite);
                return obj;
            }
        }
        static public GameObject TrainingDummy
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.TrainingDummy, ObjectType.Entity, "Training Dummy", "A Training Dummy.");
                obj.AddComponent<GuiComponent>().Initialize();
                obj["Physics"] = new PhysicsComponent(height: 4, size: -1, solid: true);
                
                obj["Conditions"] = new StatusComponent();
                obj.AddComponent<StatsComponent>().Initialize(new Dictionary<Stat.Types, float>(){
                    {Stat.Types.KnockbackResistance, 1f}
                });
                //StatsComponent.Add(obj, Tuple.Create(Stat.Types.KnockbackResistance, 1f));

                obj.AddComponent<ResourcesComponent>().Initialize(
                    Resource.Create(Resource.Types.Health, 100, 100)
                    );
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
                obj["Info"]  = new GeneralComponent(GameObject.Types.Crate, ObjectType.Container, "Crate", "Can store items.");
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/crate1");
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24)));//new Vector2(tex.Width / 2, tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2)));/// 2)));
                //obj.AddComponent(new SpriteComponent(new Sprite("blocks/furniture/chest", Sprite.CubeDepth) { Origin = new Vector2(16, 24) }));
                obj.AddComponent<GuiComponent>().Initialize(10, 1);
                obj["Physics"] = new PhysicsComponent(size: 1);
                obj.AddComponent<Components.Containers.StorageComponent>().Initialize(16);
                return obj;
            }
        }
        static public GameObject Tree
        {
            get
            {
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Trees/tree4d");
                tex.Name = "Graphics/Trees/tree4d";
                GameObject obj = new GameObject();
                obj.AddComponent<GuiComponent>().Initialize(0, 1);
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Tree, ObjectType.Plant, "Tree", "A lovely tree"));//, height: 8));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("tree4d", new Vector2(tex.Width / 2, tex.Height)));
                obj.Body.Material = Material.LightWood;
                //obj.AddComponent("Physics", new PhysicsComponent(solid: true, height: 4, size: 1, weight: 100));
                obj.AddComponent("Physics", new PhysicsComponent(solid: true, height: 4, size: -1, weight: 100)); 

                obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", Material.LightWood));
                //obj.AddComponent<RawMaterialComponent>().Initialize(Skill.Chopping);
                obj.AddComponent(new TreeComponent());
                return obj;
            }
        }
        static public GameObject BerryBush
        {
            get
            {
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Bushes/berrybush");
                //tex.Name = "Graphics/Bushes/berrybush";
                GameObject obj = new GameObject();
                obj.AddComponent<GuiComponent>().Initialize(0, 1);
                obj["Info"] = new GeneralComponent(GameObject.Types.BerryBush, ObjectType.Plant, "Berry Bush", "A lovely berry bush");
                //obj["Sprite"] = new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, tex.Width / 2, tex.Height) }, new Rectangle[] { new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height) } }, new Vector2(tex.Height / 2, tex.Height - 8));//tex.Height - 20));//, tex.Height - 8));//

                var sprgrowing = new Sprite("berrybush1", Map.BlockDepthMap) { Origin = new Vector2(tex.Height / 2, tex.Height - 8) };
                var sprgrown = new Sprite("berrybush2", Map.BlockDepthMap) { Origin = new Vector2(tex.Height / 2, tex.Height - 8) };
                //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("berrybush1", Map.BlockDepthMap) { Origin = new Vector2(tex.Height / 2, tex.Height - 8) });//tex.Height - 20));//, tex.Height - 8));//
                obj.AddComponent<SpriteComponent>().Initialize(sprgrowing);//tex.Height - 20));//, tex.Height - 8));//
                obj["Loot"] = new LootComponent(new Loot(GameObject.Types.Twig, 1f, 6));
                obj["Physics"] = new PhysicsComponent(solid: false, height: 2, size: 1, weight: 5);
                //obj["Harvest"] = new PlantComponent(new Loot(GameObject.Types.Berries, 0.7f, 6), growthTime: 720);//360);
                obj.AddComponent<PlantComponent>()
                    .Initialize(new Loot(GameObject.Types.Berries, 1, 1), 1)
                    .Initialize(sprgrowing, sprgrown); //720,new Loot(GameObject.Types.Berries, 0.7f, 6), 5); //720,
                obj.AddComponent<ResourcesComponent>().Initialize(
                    Resource.Create(Resource.Types.Health, 1, 1)
                    );
                return obj;
            }
        }
        static public GameObject Campfire
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Campfire, ObjectType.Lightsource, "Campfire", "Warm and lovely");
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
                    Lifetime = Engine.TargetFps,
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
                obj["Info"] = new GeneralComponent(GameObject.Types.BuildingPlan, ObjectType.BuildingPlan, "Building Plan", "A Building plan");
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
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Material, ObjectType.Material, "Material", "Base material item"));//, weight: 2));
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));//new Vector2(16)));
                obj.AddComponent<GuiComponent>().Initialize(12, 1);
                obj.AddComponent("Physics", new PhysicsComponent(size: 1));

                //obj["Material"] = new MaterialComponent();
                return obj;
            }
        }
        
        

        //static public GameObject FurnitureParts
        //{
        //    get
        //    {
        //        GameObject obj = MaterialTemplate;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.FurnitureParts, ObjectType.Material, "Furniture Parts", "Used to construct furniture.", Quality.Rare);
        //        //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[10] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("log", new Vector2(16, 24)));
        //        obj.AddComponent<GuiComponent>().Initialize(10, 8);
        //        obj["Physics"] = new PhysicsComponent(size: 1);
        //        //obj.AddComponent<MaterialComponent>().Initialize(Material.LightWood);
        //        obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", Material.LightWood));
        //        obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Furniture);
        //        return obj;
        //    }
        //}

        static public GameObject PickaxeHead
        {
            get
            {
                return Components.MaterialObjectFactory.Create(GameObject.Types.PickaxeHead, "Pickaxe Head", "Component of pickaxes.", size: 0);
            }
        }
        static public GameObject AxeHead
        {
            get
            {
                return Components.MaterialObjectFactory.Create(GameObject.Types.AxeHead, "Axe Head", "Component of axes.", size: 0);
            }
        }
        static public GameObject ShovelHead
        {
            get
            {
                return Components.MaterialObjectFactory.Create(GameObject.Types.ShovelHead, "Shovel Head", "Component of shovels.", size: 0);
            }
        }
        static public GameObject Handle
        {
            get
            {
                return Components.MaterialObjectFactory.Create(GameObject.Types.Handle, "Handle", "Component of tools and weapons.", size: 0);
            }
        }
        static public GameObject Twig
        {
            get
            {
                //GameObject obj = Components.MaterialObjectFactory.Create(GameObject.Types.Twig, "Twig", "Component of tools and weapons.", size: 0, spriteID: 11);
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Map.ItemSheet, 11, 32), stackMax: 10); 
                GameObject obj = new GameObject();
                obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Twig, ObjectType.Material, "Twigs", "Component of tools and weapons.");
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("twigs", new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize("twigs");
                obj.AddComponent<PhysicsComponent>();
                obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", Material.Twig));//LightWood));
                obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
                return obj;
            }
        }
        static public GameObject Cobble
        {
            get
            {
                //GameObject obj = Components.MaterialObjectFactory.Create(GameObject.Types.Cobble, "Cobble", "Component of tools and weapons.", size: 0, spriteID: 19);
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Map.ItemSheet, 19, 32), stackMax: 10);
                //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                //obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Stone, Reaction.Product.Types.Tools);

                GameObject obj = new GameObject();
                obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Cobble, ObjectType.Material, "Cobble", "Component of tools and weapons.");
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("boulder", new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize("boulder");
                obj.AddComponent<PhysicsComponent>();
                //obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Stone);
                obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", Material.Stone));
                obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
                return obj;
            }
        }
        static public GameObject Stone
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Stone, ObjectType.Material, "Stone", "It came from mining");
                obj.AddComponent<GuiComponent>().Initialize(12, 1);
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("stone", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });//new Vector2(10, 16)));////new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));

                obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
                //obj.AddComponent<MaterialComponent>().Initialize(Material.Stone);
                obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", Material.Stone));
                obj.AddComponent<ReagentComponent>().Initialize(Reaction.Product.Types.Tools);
                return obj;
            }
        }
        static public GameObject Coal
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Coal, ObjectType.Fuel, "Coal", "It came from mining");
                obj.AddComponent<GuiComponent>().Initialize(17, 1);
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[17] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("coal", new Vector2(16, 24)));
                obj["Physics"] = new PhysicsComponent(size: 1);
                obj["Fuel"] = new FuelComponent();
                return obj;
            }
        }
        static public GameObject Paper
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Paper, ObjectType.Material, "Paper", "A blank piece of paper");
                obj.AddComponent<GuiComponent>().Initialize(17, 1);
                obj["Sprite"] = new SpriteComponent(new Sprite("blankpage", new Vector2(16, 24),new Vector2(16, 24)));// Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[28] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
                obj["Physics"] = new PhysicsComponent(size: 0);
                obj["Fuel"] = new FuelComponent();
                return obj;
            }
        }
        //static public GameObject IronOre
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.IronOre, ObjectType.Material, "Iron ore", "Can be smelt to iron bars.");
        //        obj.AddComponent<GuiComponent>().Initialize(12, 64);
        //        obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[12] } }, new Vector2(16, 24));
        //        obj["Physics"] = new PhysicsComponent(size: 1);
        //        obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Iron);
        //        return obj;
        //    }
        //}
        //static public GameObject IronBar
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.IronBar, ObjectType.Material, "Iron Bar", "Used for crafting of weapons, armor, and tools.");
        //        obj.AddComponent<GuiComponent>().Initialize(17, 1);
        //        obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[25] } }, new Vector2(16, 24));//new Vector2(16, 32 - Tile.BlockHeight - Tile.Depth / 2f));//16));
        //        obj["Physics"] = new PhysicsComponent(size: 1);
        //        obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Iron, Reaction.Product.Types.Tools);
        //        return obj;
        //    }
        //}
        //static public GameObject Soilbag
        //{
        //    get
        //    {
        //        GameObject obj = MaterialTemplate;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Soilbag, ObjectType.Material, "Soilbag", "A bag containing soil");//, weight: 2));
        //        //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[13] } }, new Vector2(16, 24));// 32 - Tile.BlockHeight - Tile.Depth / 2f));//));
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("soilbag", Map.BlockDepthMap) { Origin = new Vector2(16, 24) });
        //        obj.AddComponent<GuiComponent>().Initialize(13, 64);
        //        obj.AddComponent<UseComponent>().Initialize(new ScriptPlaceBlock(Block.Types.Soil));

        //        return obj;
        //    }
        //}
        static public GameObject Bench
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Workbench, ObjectType.WorkBench, "Workbench", "Used for crafting and storing blueprints.");//, height: 2));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/crate1");
                //obj["Sprite"] = new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(16, 24));// new Vector2(tex.Width / 2, tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2));// 2))); tex.Height / 2)));
                obj.AddComponent<WorkbenchComponent>().Initialize(obj, materialCapacity: 4);
                //obj.AddComponent<WorkbenchReactionComponent>().Initialize(obj, materialCapacity: 4);
                obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1);
                obj.AddComponent<GuiComponent>().Initialize(0);//new Icon(tex, 0));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24), new Vector2(15, 24)));
                obj["Workshop"] = new WorkshopComponent();
                obj.AddComponent<PackableComponent>();
                return obj;
            }
        }
        //static public GameObject BenchReactions
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.BenchReactions, ObjectType.WorkBench, "WorkbenchReactions", "Used for crafting and storing blueprints.");//, height: 2));
        //        Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/crate1");
        //        //obj["Sprite"] = new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(16, 24));// new Vector2(tex.Width / 2, tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2));// 2))); tex.Height / 2)));
        //        //obj.AddComponent<WorkbenchComponent>().Initialize(obj, materialCapacity: 4);
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24), new Vector2(15, 32)));
        //        obj.AddComponent<WorkbenchReactionComponent>().Initialize(obj, materialCapacity: 4);
        //        obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1, weight: 6);
        //        obj.AddComponent<GuiComponent>().Initialize(0);//new Icon(tex, 0));

        //        obj["Workshop"] = new WorkshopComponent();
        //        obj.AddComponent<PackableComponent>();
        //        return obj;
        //    }
        //}
        static public GameObject ScribeBench
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.ScribeBench, ObjectType.WorkBench, "ScribeBench", "Used for copying and drafting blueprints.");//, height: 2));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/crate1");
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("crate1", "crate1-z", new Vector2(16, 24)));// new Vector2(tex.Width / 2, tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2));// 2))); tex.Height / 2)));
                //obj.AddComponent<WorkbenchComponent>().Initialize(obj, materialCapacity: 4);
                obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1);
                obj.AddComponent<GuiComponent>().Initialize(0);//new Icon(tex, 0));
                //obj.AddComponent<StorageComponent>().Initialize(new ItemContainer(obj, 8, (GameObject o) => o.HasComponent<BlueprintComponent>()));
                //obj["Workshop"] = new WorkshopComponent();
                obj.AddComponent<ScribeComponent>().Initialize(4, 2, 8, o => true, o => o.HasComponent<BlueprintComponent>()); //o.ID == GameObject.Types.Paper
                obj.AddComponent<PackableComponent>();
                return obj;
            }
        }
        //static public GameObject Smeltery
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Smeltery, ObjectType.Smeltery, "Smeltery", "Used for smelting things.");
        //        //obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Cobblestone]);
        //        obj["Sprite"] = new SpriteComponent(new Sprite(Block.Stone.Variations.First().Name, Map.BlockDepthMap) { Origin = Block.OriginCenter, MouseMap = Block.BlockMouseMap });//new Sprite(Block.Stone.Variations.First()));
        //        //obj["Sprite"] = new SpriteComponent(new Sprite(Block.Stone.GetObject().GetSprite()));//new Sprite(Block.Stone.Variations.First()));
        //        obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1);
        //        obj.AddComponent<GuiComponent>().Initialize(0);
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Activate);
        //        obj["Crafting"] = new SmelteryComponent();
        //        return obj;
        //    }
        //}

        //static public GameObject Furnace
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Furnace, ObjectType.Furnace, "Furnace", "Used for burning or cooking things.");
        //        //obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.WoodenDeck]);
        //        obj["Sprite"] = new SpriteComponent(new Sprite(Block.WoodenDeck.Variations.First()));
        //        obj["Physics"] = new PhysicsComponent(solid: false, height: 1, size: 1);
        //        obj.AddComponent<GuiComponent>().Initialize(0);
        //        //obj["Interactive"] = new InteractiveComponent(Ability.GetAbilityObject(Script.Types.Activate));
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Activate);
        //        obj["Crafting"] = new FurnaceComponent();
        //        return obj;
        //    }
        //}

        static public GameObject Package
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Package, ObjectType.Package, "Package", "An object is packed inside this."));//, height: 2));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/item-box");
                //obj.AddComponent("Sprite", new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, 24)));//tex.Height - Tile.Depth/2f - (Tile.Height - tex.Height)*2)));
                //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("box", new Vector2(tex.Width / 2, 24)));
                obj.AddComponent(new SpriteComponent((new Sprite("box", Sprite.CubeDepth) { Origin = new Vector2(16, 24) })));

                obj.AddComponent<GuiComponent>().Initialize(9);
                obj.AddComponent("Box", new BoxComponent());
                obj["Package"] = new PackageComponent();
                obj["Physics"] = new PhysicsComponent(1);
                obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Unpack);
                return obj;
            }
        }
        static public GameObject BodyPart
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.BodyPart, "Body Part", "Base body part");
                obj.AddComponent<GuiComponent>().Initialize(iconID: 8, stackMax: 1);
                obj["Equip"] = new EquipComponent();
                //obj["Bonuses"] = new StatsComponent();
                obj["Stats"] = new StatsComponent();
                obj["Abilities"] = new FunctionComponent();
                return obj;
            }
        }
        static public GameObject Fists
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Fists, ObjectType.BodyPart, "Fists", "Your fists.");
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
                obj["Info"] = new GeneralComponent(GameObject.Types.BareHands, ObjectType.BodyPart, "Bare Hands", "Your bare hands.");
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
                obj["Info"] = new GeneralComponent(GameObject.Types.BareFeet, ObjectType.BodyPart, "Bare Feet", "Your bare feet.");
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
                obj["Info"] = new GeneralComponent(GameObject.Types.RottenFeet, ObjectType.BodyPart, "Rotten Feet", "They double stink.");
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
                obj["Info"] = new GeneralComponent(GameObject.Types.Shoe, objType: ObjectType.Equipment, name: "Shoes", description: "Comfortable for the feet.", quality: Quality.Cheating);
                obj.AddComponent<GuiComponent>().Initialize(iconID: 0);
                //obj["Equip"] = new EquipComponent(Stat.Feet.Name);//, 4);
                obj.AddComponent<EquipComponent>().Initialize(GearType.Feet);
                obj["Physics"] = new PhysicsComponent();
                obj["Sprite"] = new SpriteComponent(Sprite.Default);//Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16));
                EquipComponent.Add(obj, Tuple.Create(Stat.Types.JumpHeight, 1f), Tuple.Create(Stat.Types.WalkSpeed, 1f));
                //BonusesComponent bonuses = BonusesComponent.Create(obj, "Bonuses");
                //bonuses[Stat.WalkSpeed.Name] = new Bonus(0.1f, BonusType.Percentile);
                return obj;
            }
        }
        static public GameObject TestJob
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.TestJob, ObjectType.Job, "Job", "Insturctions for a job.");
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
                obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16));
                obj.AddComponent<GuiComponent>().Initialize(0, 1);
                obj["Physics"] = new PhysicsComponent(size: 0);
                obj["Job"] = new JobComponent();
                //obj["Equip"] = new EquipComponent(Stat.Mainhand.Name);//, 4);
                obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                //obj.AddComponent<FunctionComponent>().Initialize(Ability.Attack, Ability.AssignJob);
                return obj;
            }
        }
        static public GameObject JobBoard
        {
            get 
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.JobBoard, ObjectType.Furniture, "Job Board", "Contains job listings.");
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/sign");
                obj["Sprite"] = new SpriteComponent(Sprite.Default);//tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, 24));
                obj["Physics"] = new PhysicsComponent(size: 1, weight: 50f, height: 1, solid: false);
                obj["Jobs"] = new JobBoardComponent();
                return obj;
            }
        }
        static public GameObject Weapon
        {
            get
            {
                GameObject obj = new GameObject();
                //obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Sword, objType: ObjectType.Weapon, name: "Sword", description: "A basic sword");
                ItemComponent.Add(obj, 1, 20);
                //obj.AddComponent<MaterialComponent>();
                return obj;
            }
        }
        //static public GameObject Sword
        //{
        //    get
        //    {
        //        GameObject obj = Weapon;// new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Sword, objType: ObjectType.Weapon, name: "Sword", description: "A basic sword");
        //        obj.AddComponent<GuiComponent>().Initialize(iconID: 22);
        //        //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[22] } }, new Vector2(16, 24)) { Joint = new Vector2(25, 6) });//21, 11) });
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("sword", new Vector2(16, 24), new Vector2(25, 6)));
        //        obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
        //        EquipComponent.Add(obj, Tuple.Create(Stat.Types.Knockback, 1f));
        //        obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
        //      //  obj["Damage"] = new StatsComponent() { Properties = new ComponentPropertyCollection() { { Stat.Slash.Name, 20f } } };
        //        obj["Weapon"] = new WeaponComponent(1, Tuple.Create(Stat.Types.Slash, 10f));
        //        obj.AddComponent<MaterialComponent>();
        //        return obj;
        //    }
        //}
        static public GameObject Shield
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Shield, objType: ObjectType.Shield, name: "Shield", description: "A basic Shield");
                obj.AddComponent<GuiComponent>().Initialize(iconID: 26);
                //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[26] } }, new Vector2(16, 24)) { Joint = new Vector2(16, 16) });
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("shield", new Vector2(16, 24), new Vector2(16, 16)));
                obj.AddComponent<EquipComponent>().Initialize(GearType.Offhand);
                obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
                obj.AddComponent<MaterialComponent>();
                return obj;
            }
        }
        static public GameObject Helmet
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Helmet, objType: ObjectType.Armor, name: "Helmet", description: "A basic Helmet");
                obj.AddComponent<GuiComponent>().Initialize(iconID: 27);
                //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[27] } }, new Vector2(16, 24)) { Joint = new Vector2(16, 16) });
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("helmet", new Vector2(16, 24), new Vector2(16, 16)));
                obj.AddComponent<EquipComponent>().Initialize(GearType.Head);
                obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);
                obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping);
                obj.AddComponent<MaterialComponent>();
                return obj;
            }
        }
        //static public GameObject Worktool
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject(GameObject.Types.Worktool, "Work tool", "Base work tool.", ObjectType.Equipment);
        //        //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[2] } }, new Vector2(16, 24));//32 - Tile.BlockHeight - Tile.Depth / 2f));//16)));
        //        obj.AddComponent<GuiComponent>().Initialize(iconID: 2, stackMax: 1);
        //        //obj["Equip"] = new EquipComponent(Stat.Mainhand.Name);//, 4));
        //        obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
        //        //obj.AddComponent("Damage", new StatsComponent());//.Damage);
        //        //obj["Damage"][Stat.AtkSpeed.Name] = 0.5f;
        //        //obj["Skills"] = new StatsComponent();

        //        obj["Physics"] = new PhysicsComponent(weight: 1, size: 0);



        //        obj["Stats"] = new StatsComponent();
        //        //obj["Bonuses"] = new BonusesComponent();
        //       // obj["Interactive"] = new InteractiveComponent(Ability.GetAbilityObject(Script.Types.Equipping), Ability.GetAbilityObject(Script.Types.PickUp));
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
        //        //obj["Item"] = new ItemComponent(1, 20);
        //        obj.AddComponent<ItemComponent>().Initialize(level: 1, durability: 20);
        //        //obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Wood);
        //        return obj;
        //    }
        //}
        //static public GameObject Shovel
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.Worktool;
        //        obj.ID = GameObject.Types.Shovel;
        //        obj.Name = "Shovel";
        //        obj.Description = "Used to dig soil and dirt.";

        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("shovel", new Vector2(16, 24), new Vector2(16, 16)));//16));
        //        obj.AddComponent<GuiComponent>().Initialize(21, 1);
        //        obj.AddComponent<MaterialComponent>();

        //        //obj.AddComponent<FunctionComponent>().Initialize(Ability.Attack, Ability.Digging);

        //        //obj["Use"] = new UseComponent()
        //        //{
        //        //    Ability = Ability.GetAbilityObject(Script.Types.Digging)
        //        //};
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Digging);
        //        obj.AddComponent<SkillComponent>().Initialize(Skill.Digging);
        //        return obj;
        //    }
        //}
        //static public GameObject EpicShovel
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.Shovel;
        //        obj.Name = "Epic Shovel";
        //        obj.ID = GameObject.Types.EpicShovel;
        //        // obj["Bonuses"][Stat.Digging.Name] = new Bonus(0.9f, BonusType.Percentile);
        //        obj["Info"]["Quality"] = Quality.Epic;
        //        obj["Stats"][Stat.Digging.Name] = 5f;
        //        obj["Stats"][Stat.Pierce.Name] = 10f;
        //        obj["Stats"][Stat.Blunt.Name] = 40f;
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[21] } }, new Vector2(16, 24), new Vector2(16, 16)));//16));
        //        // obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[0] } }, new Vector2(16, 16));
        //        // obj.AddComponent<GuiComponent>().Initialize(0, 1);
        //        //    obj["Abilities"]["Primary"] = Message.Types.Shovel;
        //        //   obj["Abilities"]["Secondary"] = Message.Types.Shovel;
        //        // obj["Abilities"] = new FunctionComponent(Message.Types.Attack, Message.Types.Shovel);
        //        obj["Item"]["Level"] = 20;
        //        //  EquipComponent.Add(obj, Tuple.Create(Stat.Types.Digging, 1f));

        //        //ItemFactory.GetPrefix(Affix.Types.Durable).Apply(obj); 
        //        //ItemFactory.GetSuffix(Affix.Types.Shoveling).Apply(obj);

        //        ItemFactory.Apply(obj,
        //            Affix.Types.Durable,
        //            Affix.Types.Shoveling
        //            );
        //        return obj;
        //    }
        //}
        //static public GameObject Hoe
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.Worktool;
        //        obj.ID = GameObject.Types.Hoe;
        //        obj.Name = "Hoe";
        //        obj.Description = "Used to dig soil and dirt.";
        //        obj["Info"]["Quality"] = Quality.Uncommon;
        //        //obj["Damage"][Stat.Pierce.Name] = 10f;
        //        //obj["Damage"][Stat.Digging.Name] = 5f;
        //        //obj["Bonuses"][Stat.Tilling.Name] = new Bonus(0.1f, BonusType.Percentile);

        //        obj["Sprite"] = new SpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[4] } }, new Vector2(16, 24), new Vector2(16, 16)));//16));
        //        obj.AddComponent<GuiComponent>().Initialize(4, 1);
        //        //obj.AddComponent<FunctionComponent>().Initialize(Ability.GetAbilityObject(Script.Types.Tilling));//Ability.Attack, Ability.Tilling, 
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Tilling);
        //      //  obj["Use"] = new UseComponent()
        //      //  {
        //      //      Ability = Ability.GetAbilityObject(Script.Types.Tilling),
        //      ////      AbilityID = Script.Types.Tilling
        //      // //     Use = (a) => AbilityComponent.Perform(GameObject.Types.AbilityTilling, new ActionArgs(a.Actor, a.Target, a.Face))
        //      //      //Use = (a) =>
        //      //      //{
        //      //      //    Interaction.StartNew(
        //      //      //        a.Actor,
        //      //      //        new Interaction(
        //      //      //            TimeSpan.FromSeconds(1),
        //      //      //            (actor, target) =>
        //      //      //            {
        //      //      //                a.Target.PostMessage(Message.Types.Till, a.Actor, a.Face);
        //      //      //            },
        //      //      //            a.Target,
        //      //      //            "Till",
        //      //      //            "Tilling"),
        //      //      //            a.Face);
        //      //      //}
        //      //  };
        //        obj.AddComponent<MaterialComponent>();
        //        return obj;
        //    }
        //}
        //static public GameObject Pickaxe
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.Worktool;
        //        GeneralComponent info = obj.GetInfo();
        //        obj.ID = GameObject.Types.Pickaxe;
        //        obj.Name = "Pickaxe";
        //        obj.Description = "Used to mine rock.";
        //        //obj["Sprite"] = new ActorSpriteComponent(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[1] } }, new Vector2(16, 24), new Vector2(16, 16)));//16));
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("pickaxe", 0) { Origin = new Vector2(16, 24), Joint = new Vector2(16, 16) });

        //        obj.AddComponent<GuiComponent>().Initialize(1, 1);
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Mining);
        //        obj.AddComponent<MaterialComponent>().Initialize();
        //        return obj;
        //    }
        //}
        //static public GameObject Axe
        //{
        //    get
        //    {
        //        //StaticObject obj = new StaticObject(GameObject.Types.Axe, "Axe", "Chops down trees.");
        //        //obj.AddComponent("Sprite", new SpriteComponent(ItemManager.Instance.ItemSheet, new Rectangle[][] { new Rectangle[] {ItemManager.Instance.Icons[2]}}, new Vector2(16, 16)));
        //        //obj.AddComponent<GuiComponent>().Initialize(2, 1));
        //        //obj.AddComponent("Equip", new EquipComponent("Mainhand", 4));
        //        //obj.AddComponent("Damage", new DamageComponent(chop: 50));
        //        //obj.AddComponent("Physics", new PhysicsComponent());
        //        //obj.GetInfo().Weight = 1;
        //        ////obj.AddComponent("Interactions", new InteractionComponent(new int[] { 4 }));
        //        ////obj.AddComponent("Materials", new CraftableComponent(obj, new Dictionary<int, int>() { { 1, 1 } }));
        //        //return obj;

        //        GameObject obj = GameObjectDb.Worktool; // StaticObject.Create(GameObject.Types.Worktool);
        //        GeneralComponent info = obj.GetInfo();
        //        obj.ID = GameObject.Types.Axe;
        //        obj.Name = "Axe";
        //        obj.Description = "Chops down trees";

        //        //obj.AddComponent<ActorSpriteComponent>().Initialize(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[2] } }, new Vector2(16, 24), new Vector2(16, 16)));//16));
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("axe", new Vector2(16, 24), new Vector2(16, 16)));//16));

        //        obj.AddComponent<GuiComponent>().Initialize(2, 1);
        //        //obj.AddComponent<FunctionComponent>().Initialize(Ability.GetAbilityObject(Script.Types.Chopping));//Ability.Attack, Ability.Chop);
        //        obj["Stats"][Stat.Lumberjacking.Name] = 2f;
        //        //obj["Bonuses"][Stat.Lumberjacking.Name] = new Bonus(2f, BonusType.Flat);
        //     //   obj.AddComponent<AdvertiseNeedComponent>().Initialize(new AIAction(Script.Types.PickUp, Need.Types.Work, 100));
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Chopping);
        //        //obj["Use"] = new UseComponent()
        //        //{
        //        //    Ability = Ability.GetAbilityObject(Script.Types.Chopping)
        //        //};
        //        obj.AddComponent<MaterialComponent>();
        //        return obj;
        //    }
        //}
        //static public GameObject Handsaw
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.Worktool;  //StaticObject.Create(GameObject.Types.Worktool);
        //        GeneralComponent info = obj.GetInfo();
        //        obj.ID = GameObject.Types.Handsaw;
        //        obj.Name = "Handsaw";
        //        obj.Description = "Converts logs to planks.";
        //        //obj["Damage"]["Slash"] = 50f; //.Value = 50f;

        //        obj.Components["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[14] } }, new Vector2(16, 24));//16));
        //        obj.AddComponent<GuiComponent>().Initialize(14, 1);
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Sawing);

        //        //obj.AddComponent<MaterialComponent>();
        //        return obj;
        //    }
        //}
        //static public GameObject Hammer
        //{
        //    get
        //    {
        //        //StaticObject obj = new StaticObject(GameObject.Types.Hammer, "Hammer", "Used to cut down trees");
        //        //obj.AddComponent("Sprite", new SpriteComponent(ItemManager.Instance.ItemSheet, new Rectangle[][] { new Rectangle[] { ItemManager.Instance.Icons[3] } }, new Vector2(16, 16)));
        //        //obj.AddComponent<GuiComponent>().Initialize(3, 1));
        //        //obj.AddComponent("Equip", new EquipComponent("Mainhand", 4));
        //        //obj.AddComponent("Damage", new DamageComponent(blunt: 5));
        //        ////obj.AddComponent("Interactions", new InteractionComponent(new int[] { 4 }));
        //        ////obj.AddComponent("Materials", new CraftableComponent(obj, new Dictionary<int, int>() { { 1, 1 } }));
        //        //return obj;

        //        GameObject obj = GameObjectDb.Worktool;  //StaticObject.Create(GameObject.Types.Worktool);
        //        GeneralComponent info = obj.GetInfo();
        //        obj.ID = GameObject.Types.Hammer;
        //        obj.Name = "Hammer";
        //        obj.Description = "Used for constructions.";
        //        obj.AddComponent<MaterialComponent>();

        //        //obj.AddComponent<ActorSpriteComponent>().Initialize(new Sprite(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[3] } }, new Vector2(16, 24), new Vector2(23, 8)));// new Vector2(16, 16)));//16));
        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("hammer", new Vector2(16, 24), new Vector2(23, 8)));
                
        //        obj.AddComponent<GuiComponent>().Initialize(3, 1);
        //        obj.AddComponent<MaterialComponent>();
        //        obj.AddComponent<UseComponent>().Initialize(Script.Types.Build, Script.Types.BuildFootprint);
        //        obj.AddComponent<SkillComponent>().Initialize(Skill.Building);
        //        return obj;
        //    }
        //}
        //static public GameObject CheatHammer
        //{
        //    get
        //    {
        //        GameObject obj = Hammer;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.CheatHammer, objType: ObjectType.Equipment, name: "Hammer of Cheating", description: "Testing", quality: Quality.Unique);
        //        //obj["Stats"] = new StatsComponent();
        //        //obj["Stats"][Stat.MaterialRecovery.Name] = 1f;
        //        EquipComponent.Add(obj, Tuple.Create(Stat.Types.MatRecover, 1f));
        //        return obj;
        //    }
        //}
        static public GameObject Construction
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Construction, ObjectType.Construction, "Construction", "An object that awaits to be constructed"));//, height: 2));
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/construction");//item-box");
                //obj["Sprite"] = new ActorSpriteComponent(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, new Vector2(tex.Width / 2, 24));//tex.Height / 2)));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("construction", new Vector2(16, 24)));
               // obj.AddComponent("Sprite", new SpriteComponent(Map.TerrainSprites, new Rectangle[][] { new Rectangle[] { new Rectangle(1 * 32, 6 * 32, 32, 32) } }, new Vector2(16)));
                obj.AddComponent<GuiComponent>().Initialize(9);
              //  obj.AddComponent("Health", new HealthComponent(maxHealth: 100));
                obj["Physics"] = new PhysicsComponent(size: -1, solid: true, height: 1, weight: 0);// new StickyComponent(height: 1, size: 1, solid: true);

                //obj.AddComponent<ConstructionFootprint>();
                obj.AddComponent<StructureComponent>();
                return obj;
            }
        }
        static public GameObject BlockEmpty
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Block, ObjectType.Block, "Tile", "A tile"));
                obj.AddComponent<GuiComponent>().Initialize(8);
                obj.AddComponent<BlockComponent>();
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
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Tile, ObjectType.Block, "Tile", "A tile"));
                obj.AddComponent<GuiComponent>().Initialize(8);
                //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.DropOnTarget);
                obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Framing);//, Script.Types.DropOnTarget);
                //obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
                return obj;
            }
        }
        //static public GameObject Air
        //{
        //    get
        //    {
        //        // TODO: add density property to tile
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Air, ObjectType.Block, "Air", "An air");
        //        obj.AddComponent<GuiComponent>().Initialize(8);
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Air]);
        //        return obj;
        //    }
        //}
        //static public GameObject Soil
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Soil, ObjectType.Block, "Soil", "Can grow plants on it");
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Soil]));//Map.TerrainSprites, 32));
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Soil]);
        //        obj["Sprite"]["Hidden"] = true;
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Soil, transparency: 0, density: 1);
        //        obj["Activate"] = new FarmingComponent();
        //        obj["Convertible"] = new ProductionComponent(Message.Types.Shovel, "Dig", "Digging", new TimeSpan(0, 0, 1), agent => 1 * (1 + BonusesComponent.GetBonusOrDefault(agent, Stat.Digging.Name, 0)),// agent => 1 + StatsComponent.GetStat(agent, Stat.Shoveling.Name) / 10f, // BonusesComponent.GetStat(agent, Stat.Lumberjacking.Name),
        //             new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
        //             new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
        //             new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)
        //              );
        //        obj["Packable"] = new PackableComponent();
        //     //   InteractiveComponent.Add(obj, Ability.GetAbilityObject(Script.Types.Digging), Ability.GetAbilityObject(Script.Types.Tilling));
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Digging, Script.Types.Tilling);
        //        obj.AddComponent<SoilComponent>();
        //        return obj;
        //    }
        //}
        static public GameObject ConstructionBlockOld
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.ConstructionBlock, ObjectType.Construction, "Construction Block", "Construction Block.");
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Construction]));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("construction", new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
                obj.AddComponent<PhysicsComponent>().Initialize(size: -1);
                obj.AddComponent<ConstructionComponent>();
                return obj;
            }
        }
        static public GameObject ConstructionBlock
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.ConstructionBlock, ObjectType.Construction, "Construction Block", "Construction Block.");
                //obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Construction]));
                //obj.AddComponent<SpriteComponent>().Initialize(new Sprite("construction", new Vector2(16, 24)));

                obj.AddComponent(new SpriteComponent(new Bone(Bone.Types.Torso, new Sprite("box", Sprite.CubeDepth) { Origin = new Vector2(16, 24) })));
                //obj.AddComponent(new SpriteComponent((new Sprite("box", Sprite.CubeDepth) { Origin = new Vector2(16, 24) })));

                obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
                obj.AddComponent<PhysicsComponent>().Initialize(size: -1);
                obj.AddComponent<ConstructionComponent>();
                return obj;
            }
        }
        //static public GameObject Farmland
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Farmland, ObjectType.Block, "Farmland", "Soil ready for planting.");
        //        Texture2D tex = Map.TerrainSprites;// Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Farmland]));//Map.TerrainSprites, 40));
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Farmland]);
        //        obj["Sprite"]["Hidden"] = true;
        //        obj["Physics"] = FarmlandComponent.Create(obj);
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Farmland);//.Initialize(onUpdate: (net, o) => { });
        //        obj["Convertible"] = new ProductionComponent(Message.Types.Shovel, "Dig", "Digging", new TimeSpan(0, 0, 1), agent => 1 * (1 + BonusesComponent.GetBonusOrDefault(agent, Stat.Digging.Name, 0)),//agent => 1 + StatsComponent.GetStat(agent, Stat.Shoveling.Name) / 10f, // BonusesComponent.GetStat(agent, Stat.Lumberjacking.Name),
        //             new Loot(GameObject.Types.Soilbag, chance: 0.5f, count: 1),
        //             new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
        //             new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)
        //              );
        //        //add blockentitycomponent
        //        return obj;
        //    }
        //}
        //static public GameObject Grass
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();// GameObjectDb.BlockDefault; //new StaticObject();
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Grass]));//Map.TerrainSprites, 23 * 8));
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Grass, ObjectType.Block, "Grass", "Grassy pleasure");
        //        //obj["Sprite"] = new ActorSpriteComponent(Block.TileSprites[Block.Types.Grass]);
        //        //obj["Sprite"]["Hidden"] = true;
        //        obj.AddComponent<SpriteComponent>().Initialize(Block.TileSprites[Block.Types.Grass]);
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Grass, hasData: true, transparency: 0, density: 1);
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Cobblestone);
        //        obj["Activate"] = new FarmingComponent();
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Digging, Script.Types.Tilling, Script.Types.Framing);
        //        obj.AddComponent<GrassComponent>();
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        //obj["Convertible"] = new ProductionComponent(Message.Types.Shovel, "Dig", "Digging", new TimeSpan(0, 0, 1), agent => 1 * (1 + BonusesComponent.GetBonusOrDefault(agent, Stat.Digging.Name, 0)), //agent => 1 + StatsComponent.GetStat(agent, Stat.Shoveling.Name) / 10f, 
        //        //     new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
        //        //     new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
        //        //     new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)
        //        //      )
        //        //      {
        //        //          OnSuccess = (net, actor, target) =>
        //        //          {
        //        //              Skill.Award(net, actor, target, Skill.Types.Digging, 1);
        //        //          }
        //        //      };
        //        obj.AddComponent<BlockableComponent>().Initialize(Block.Types.Grass);
        //        obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Soil);
        //        return obj;
        //    }
        //}
        //static public GameObject Gravel
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault; //new StaticObject();
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Gravel]));//Map.TerrainSprites, 23 * 8));
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Gravel, ObjectType.Block, "Gravel", "Gravel marvel");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Gravel]);
        //        obj["Sprite"]["Hidden"] = true;
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Gravel, hasData: true, transparency: 0, density: 1);
        //        return obj;
        //    }
        //}
        //static public GameObject Door
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();//GameObject.Types.Door, ObjectType.Block, "Door", "Opens and closes");
        //        obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Door, ObjectType.Block, "Door", "Opens and closes");
        //        //Sprite full = new Sprite(Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/doorFull"), new Rectangle[][] { new Rectangle[] { 
        //        //    new Rectangle(0,0,32,64)
        //        //} }, new Vector2(16, 56));
        //        //obj["Sprite"] = new ActorSpriteComponent(full) { Hidden = true };

        //        obj.AddComponent<SpriteComponent>().Initialize(new Sprite("doorFull", new Vector2(16, 56)));

        //        obj.AddComponent<PackableComponent>();
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Activate);
        //        obj.AddComponent<DoorComponent>();
        //        obj.AddComponent<GuiComponent>().Initialize();
        //        obj.AddComponent<MaterialComponent>().Initialize(Material.LightWood);//, Reaction.Product.Types.Blocks);
        //        return obj;
        //    }
        //}
        //static public GameObject Flowers
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Flowers, ObjectType.Block, "Flowers", "Mmmm flowers");
        //     //   obj["Loot"] = new LootComponent(new Loot(GameObject.Types.Soilbag, chance: 0.5f, count: 1));
        //        //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet cubes");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Flowers]);
        //        //    tex, new Rectangle[][] { 
        //        //    new Rectangle[] { new Rectangle(0 * Tile.Width, 2 * Tile.Height, Tile.Width, Tile.Height) },
        //        //    new Rectangle[] { new Rectangle(1 * Tile.Width, 2 * Tile.Height, Tile.Width, Tile.Height) } ,
        //        //    new Rectangle[] { new Rectangle(2 * Tile.Width, 2 * Tile.Height, Tile.Width, Tile.Height) } ,
        //        //    new Rectangle[] { new Rectangle(3 * Tile.Width, 2 * Tile.Height, Tile.Width, Tile.Height) } 
        //        //},
        //        //    Tile.OriginCenter, Tile.TileMouseMap);
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Flowers]));
        //        obj["Sprite"]["Hidden"] = true;
        //        obj.AddComponent<BlockComponent>().Initialize(Block.Types.Flowers, hasData: true, transparency: 0, density: 1);
        //        obj["Activate"] = new FarmingComponent();
        //        //InteractiveComponent.Add(obj, Ability.GetAbilityObject(Script.Types.Digging), Ability.GetAbilityObject(Script.Types.Tilling));
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Digging, Script.Types.Tilling);
        //      //  obj["Health"] = new DiggableComponent(new Components.Interaction(Message.Types.Shovel, "Dig", 50f), 100);
        //      //  obj["Diggable"] = new DiggableComponent(new Components.Interaction(new TimeSpan(0, 0, 1), Message.Types.Shovel, obj, "Dig", "Digging", targettype: Components.TargetType.Self, stat: Stat.Shoveling));
        //        obj["Convertible"] = new ProductionComponent(Message.Types.Shovel, "Dig", "Digging", new TimeSpan(0, 0, 1), agent => 1 * (1 + BonusesComponent.GetBonusOrDefault(agent, Stat.Digging.Name, 0)),//agent => 1 + StatsComponent.GetStat(agent, Stat.Shoveling.Name) / 10f,// BonusesComponent.GetStat(agent, Stat.Lumberjacking.Name),
        //             new Loot(GameObject.Types.Soilbag, chance: 1f, count: 1),
        //             new Loot(GameObject.Types.Cobble, chance: 0.25f, count: 1),
        //             new Loot(GameObject.Types.Twig, chance: 0.25f, count: 1)
        //              );//2));//6));
        //        return obj;
        //    }
        //}
        //static public GameObject Rock
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Rock, ObjectType.Block, "Rock", "Can be mined for stone");
        //      //  obj["Loot"] = new LootComponent(new Loot(GameObject.Types.Stone, chance: 0.75f, count: 4));
        //       // Texture2D tex = Map.TerrainSprites;//Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Stone]);
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Stone]));//
        //           // tex, new Rectangle[][] { new Rectangle[] { new Rectangle(4 * 32, 5 * 32, 32, 32) } }, Tile.OriginCenter, Tile.TileMouseMap); //new Rectangle(0, 0, 32, 32)
        //        obj["Sprite"]["Hidden"] = true;
        //        //obj["Tile"] = new TileComponent(TileBase.Types.Stone);
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Stone, hasData: true, transparency: 0, density: 1);
        //        //obj["Health"] = new HealthComponent(100, 1, resistances: 0);
        //        //obj["Health"][Stat.Mining.Name] = 0f;
        //        obj["Convertible"] = new ProductionComponent(Message.Types.Mine, "Mine", "Mining", new TimeSpan(0, 0, 1), agent => 1 * (1 + BonusesComponent.GetBonusOrDefault(agent,Stat.Mining.Name, 0)),// agent => 1 + StatsComponent.GetStat(agent, Stat.Mining.Name) / 10f, //agent => StatsComponent.GetStat(agent, Stat.Mining.Name),// BonusesComponent.GetStat(agent, Stat.Lumberjacking.Name),
        //            new Loot(GameObject.Types.Stone, chance: 0.75f, count: 4),
        //            new Loot(GameObject.Types.Cobble, chance: 0.75f, count: 4)
        //             );//2));//6));
        //        //  obj["Diggable"] = new DiggableComponent(new Components.Interaction(new TimeSpan(0, 0, 1), Message.Types.Mine, obj, "Mine", "Mining", targettype: Components.TargetType.Self, stat: Stat.Mining));
        //      //  InteractiveComponent.Add(obj, Script.Types.Mining); 
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Mining);
        //        return obj;
        //    }
        //}
        //static public GameObject Iron
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Iron, ObjectType.Block, "Iron", "Can be mined for Iron");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Iron]);
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Iron]));
        //        obj["Sprite"]["Hidden"] = true;
        //        obj.AddComponent<BlockComponent>().Initialize(Block.Types.Iron, hasData: true, transparency: 0, density: 1);
        //        obj["Convertible"] = new ProductionComponent(Message.Types.Mine, "Mine", "Mining", new TimeSpan(0, 0, 1), agent => 1 * (1 + StatsComponent.GetStatOrDefault(agent, Stat.Types.Mining, 0)),
        //            new Loot(GameObject.Types.IronOre, chance: 0.75f, count: 4),
        //            new Loot(GameObject.Types.Cobble, chance: 0.75f, count: 4)
        //             );
        //        //InteractiveComponent.Add(obj, Script.Types.Mining);
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Mining);
        //        return obj;
        //    }
        //}
        //static public GameObject Cobblestone
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.CobblestoneItem, ObjectType.Block, "Cobblestone", "Can be mined for stone");
        //        obj.AddComponent<SpriteComponent>().Initialize(Block.TileSprites[Block.Types.Cobblestone]);
        //        //Sprite sprite = new Sprite(Block.TileSprites[Block.Types.Cobblestone]);
        //        //sprite.Joint = new Vector2(Block.Width/2, Block.Height);
        //        //obj.AddComponent<ActorSpriteComponent>().Initialize(sprite);
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Cobblestone]));
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        obj.AddComponent<UseComponent>().Initialize(new ScriptPlaceBlock(Block.Types.Cobblestone));
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Cobblestone);
        //        obj.AddComponent<BlockableComponent>().Initialize(Block.Types.Cobblestone);
        //        obj.AddComponent<MaterialComponent>().Initialize(Components.Materials.Material.Stone, Reaction.Product.Types.Blocks);
        //        return obj;
        //    }
        //}
        //static public GameObject Mineral
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Mineral, ObjectType.Block, "Mineral", "Can be mined for minerals");
        //        obj["Loot"] = new LootComponent(new Loot(GameObject.Types.Coal, chance: 0.5f, count: 2));
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Coal]);
        //        obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.Coal]);           
        //        obj["Sprite"]["Hidden"] = true;
        //        return obj;
        //    }
        //}
        //static public GameObject Water
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Water, ObjectType.Block, "Water", "Water in liquid form.");
        //       // Texture2D tex = Map.TerrainSprites;// Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Water]);
        //            //tex, new Rectangle[][] { new Rectangle[] { new Rectangle(4 * 32, 9 * 32, 32, 32) } }, Tile.OriginCenter, Tile.TileMouseMap); //new Rectangle(0, 0, 32, 32)
        //        obj["Sprite"]["Hidden"] = true;
        //        obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.Water]);
        //        //obj["Tile"] = TileComponent.Create(obj, TileBase.Types.Water);
        //        obj["Physics"] = FluidComponent.Create(obj, Block.Types.Water, transparency: 0.5f, density: 0.5f);// 0.8f);// 1); hasData: true, 
        //     //   obj["Physics"][Stat.Density.Name] = 0.8f;
        //        return obj;
        //    }
        //}
        //static public GameObject Sand
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject(); //GameObjectDb.BlockDefault; //new StaticObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Sand, ObjectType.Block, "Sand", "Sandy pleasure");
        //        obj["Loot"] = new LootComponent(new Loot(GameObject.Types.Soilbag, chance: 0.2f, count: 1));
        //       // Texture2D tex = Map.TerrainSprites;//Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        //obj["Sprite"] = new ActorSpriteComponent(Block.TileSprites[Block.Types.Sand]);
        //        //obj["Sprite"]["Hidden"] = true;
        //        obj.AddComponent<SpriteComponent>().Initialize(Block.TileSprites[Block.Types.Sand]);
        //        obj.AddComponent<GuiComponent>().Initialize(new Icon(Block.TileSprites[Block.Types.Sand]));
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Sand);
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Sand, hasData: true, transparency: 0, density: 1);
        //        obj.AddComponent<BlockableComponent>().Initialize(Block.Types.Sand);
        //        obj["Convertible"] = new ProductionComponent(Message.Types.Shovel, "Dig", "Digging", new TimeSpan(0, 0, 0, 0, 500), agent => 1 * (1 + BonusesComponent.GetBonusOrDefault(agent, Stat.Digging.Name, 0)) //agent => 1 + StatsComponent.GetStat(agent, Stat.Shoveling.Name) / 10f, 

        //              );

        //        //obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Digging);
        //        return obj;
        //    }
        //}
        //static public GameObject WoodenDeck
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault; //new StaticObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.WoodenDeck, ObjectType.Block, "Wooden Deck", "A nice wooden floor");
        //        obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.WoodenDeck]);

        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.WoodenDeck]);
        //        //obj["Sprite"]["Hidden"] = true;


        //        obj["Loot"] = new LootComponent(new Loot(GameObject.Types.WoodenPlank, chance: 1f, count: 2));
        //        //obj.AddComponent<GuiComponent>().Initialize(8));
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.WoodenDeck, hasData: false, transparency: 0, density: 1);
        //        obj["Destroyable"] = new DiggableComponent(new Components.Interaction(new TimeSpan(0, 0, 1), Message.Types.Chop, obj, "Chop"), 50);
        //        obj["Packable"] = new PackableComponent();
        //        obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
        //        obj.AddComponent<BlockableComponent>().Initialize(Block.Types.WoodenDeck);
        //        obj.AddComponent<MaterialComponent>().Initialize(Material.LightWood);// Reaction.Product.Types.Blocks);
        //        return obj;
        //    }
        //}
        static public GameObject BlueprintBlock
        {
            get
            {
                GameObject obj = GameObjectDb.BlockDefault;
                obj["Info"] = new GeneralComponent(GameObject.Types.BlueprintBlock, ObjectType.Block, "Blueprint Block", "Represents a world block that will be ignored during construction.");
                //obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.Blueprint]);
                //obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Blueprint]);
                obj["Sprite"]["Hidden"] = true;
                return obj;
            }
        }
        //static public GameObject Scaffolding
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault; //new StaticObject();
        //        obj["Info"] = new GeneralComponent(GameObject.Types.Scaffolding, ObjectType.Block, "Scaffolding", "A nice Scaffolding");
        //    //    Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.Scaffolding]);
        //            //Map.TerrainSprites, new Rectangle[][] { new Rectangle[] { new Rectangle(0 * Tile.Width, 8 * Tile.Height, Tile.Width, Tile.Height) } }, Tile.OriginCenter, Tile.TileMouseMap); //new Rectangle(0, 0, 32, 32)
        //        obj["Sprite"]["Hidden"] = true;
        //        obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.Scaffolding]);
        //        obj["Loot"] = new LootComponent(new Loot(GameObject.Types.WoodenPlank, chance: 0.5f, count: 2));
        //        //obj.AddComponent<GuiComponent>().Initialize(8));
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.Scaffolding, hasData: false, transparency: 0, density: 1);
        //        //obj["Tile"] = new TileComponent(TileBase.Types.WoodenDeck);
                
        //        return obj;
        //    }
        //}
        //static public GameObject WoodenFrame
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.BlockDefault;
        //        obj["Info"] = new GeneralComponent(GameObject.Types.WoodenFrame, ObjectType.Block, "Wooden Frame", "A nice wooden frame");
        //        obj["Sprite"] = new SpriteComponent(Block.TileSprites[Block.Types.WoodenFrame]);
        //        obj["Sprite"]["Hidden"] = true;
        //        obj["Gui"]["Icon"] = new Icon(Block.TileSprites[Block.Types.WoodenFrame]);
        //        obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.BuildFootprint);
        //        obj.AddComponent<LadderComponent>();
        //        //obj.AddComponent<BlockComponent>().Initialize(Block.Types.WoodenFrame, opaque: true);// false);
        //        //obj.AddComponent<BlockObject>();
        //        obj["Construction"] = new ConstructionFrame();
        //        return obj;
        //    }
        //}
        static public GameObject ConstructionReservedTile
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.ConstructionReservedTile, ObjectType.Block, "ConstructionReservedTile", "ConstructionReservedTile");
                Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
                obj.AddComponent<GuiComponent>().Initialize(new Icon(Map.TerrainSprites, 182, 32));
                obj["Sprite"] = new SpriteComponent(Map.TerrainSprites, new Rectangle[][] { new Rectangle[] { new Rectangle(1 * 32, 6 * 32, 32, 32) } }, Block.OriginCenter, Block.BlockMouseMap); //new Rectangle(0, 0, 32, 32)
                obj["Sprite"]["Shadow"] = false;

                return obj;
            }
        }
        static public GameObject Consumable
        {
            get
            {
                GameObject obj = new GameObject();
                obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Consumable, "Consumable", "Base consumable", ObjectType.Consumable));
                //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
                obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[15] } }, new Vector2(16, 32 - Block.BlockHeight - Block.Depth / 2f));
                obj.AddComponent<GuiComponent>().Initialize(15);
                obj.AddComponent("Physics", new PhysicsComponent());
                obj.AddComponent("Consumable", new ConsumableComponent(Verbs.Consume));
                obj.AddComponent<InteractiveComponent>().Initialize(Script.Types.Equipping, Script.Types.PickUp);
                return obj;
            }
        }
        static public GameObject Berries
        {
            get
            {
                GameObject obj = GameObjectDb.Consumable;
                obj.ID = GameObject.Types.Berries;
                obj.Name = "Berries";
                obj.GetInfo().StackMax = 8;
                //obj["Sprite"] = new ActorSpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[7] } }, new Vector2(16, 24));// 32 - Tile.BlockHeight - Tile.Depth / 2f));//));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("berries", new Vector2(16, 24), new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize(7, 64);
                obj.AddComponent(new ConsumableComponent(Verbs.Eat,
                    StatusCondition.Create(Message.Types.Buff, "Berry Freshness", "The super tasty berries have given you their boon.", Stat.AtkSpeed, 100f, 180f),
                    StatusCondition.Create(Message.Types.RestoreHealth, "Restore Health", "The super tasty berries have given you their boon.", Stat.Health, 100f)
                    )
                    {
                        NeedEffects = new List<AIAdvertisement>() { new AIAdvertisement("Food", 100) },
                        Byproducts = new LootTable(new Loot(() => GameObjectDb.Seeds, 1, 1, 2, 4)),// GameObjectDb.Seeds,
                        Effects = new List<ConsumableEffect>() { new NeedEffect(Need.Types.Hunger, 50) }
                    }
                    );
                //obj.AddComponent<UseComponentOld>().Initialize(Script.Types.Planting);

                //obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                //obj.AddComponent<SkillComponent>().Initialize(Skill.Eating);

                //obj.AddComponent<FunctionComponent>().Initialize(Ability.Planting, Ability.Activate); //Ability.GetAbilityObject(Script.Types.Chopping)
                //  obj.AddComponent<FunctionComponent>().Initialize(Ability.GetAbilityObject(Script.Types.pla, Ability.Activate); //Ability.GetAbilityObject(Script.Types.Chopping)
                return obj;
            }
        }
        static public GameObject Seeds
        {
            get
            {
                GameObject obj = MaterialTemplate;// new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Seeds, ObjectType.Material, "Seeds", "Base seeds") { StackMax = 16 };//, weight: 2));
                //obj["Sprite"] = new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[6] } }, new Vector2(16, 24));//new Vector2(16));
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("seeds", new Vector2(16, 24), new Vector2(16, 24)));
                obj.AddComponent<GuiComponent>().Initialize(6, 64);
                obj["Physics"] = new PhysicsComponent(size: 0);
                obj.AddComponent<SeedComponent>().Initialize(GameObject.Types.BerryBush);
                obj.AddComponent<EquipComponent>().Initialize(GearType.Mainhand);
                obj.AddComponent<SkillComponent>().Initialize(Skill.Planting);
                return obj;
            }
        }
        static public GameObject Fertilizer
        {
            get
            {
                GameObject obj = MaterialTemplate;// new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Fertilizer, ObjectType.Material, "Fertilizer", "Speeds up growth of plants");//, weight: 2));
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
                obj.AddComponent<GeneralComponent>().Initialize(GameObject.Types.Brain, ObjectType.Consumable, "Brain", "A typical snack for zombies");
                obj.AddComponent<SpriteComponent>().Initialize(Sprite.Default);
                obj.AddComponent<GuiComponent>().Initialize(7, 64);
                return obj;
            }
        }
        static public GameObject StrengthPotion
        {
            get
            {
                GameObject obj = GameObjectDb.Consumable;
                obj.ID = GameObject.Types.StrengthPotion;
                obj.Name = "Potion of Strength";
                obj["Info"]["Quality"] = Quality.Legendary;
                obj.AddComponent<SpriteComponent>().Initialize(new Sprite("potion", new Vector2(16, 24), new Vector2(16, 24)));
                obj["Consumable"] = new ConsumableComponent(Verbs.Drink, 
                    StatusCondition.Create(Message.Types.Buff, "Potion of Strength", "You feel stronger after a refreshing drink.", Stat.Strength, 100f, 180f)
                    )
                    {
                        NeedEffects = new List<AIAdvertisement>() { new AIAdvertisement("Water", 20) }
                    };
                //ConsumableComponent cons = obj.GetComponent<ConsumableComponent>("Consumable");
                ////cons.Properties.Add("Conditions", new StatusConditionCollection());

                ////cons.Conditions.Add(Stat.Types.Strength, 100, 3600);
                ////cons.Conditions.Add(Stat.StatDB[Stat.Types.Strength], 100, 3600);
                //cons.Conditions.Add(Stat.Strength, 100, 180);
                return obj;
            }
        }
        //static public GameObject StatusCondition
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj.AddComponent("Info", new InfoComponent(GameObject.Types.StatusCondition, "Status Condition", "Base status condition"));
        //        Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet");
        //        obj.AddComponent("Sprite", new SpriteComponent(Map.ItemSheet, new Rectangle[][] { new Rectangle[] { Map.Icons[6] } }, new Vector2(16)));
        //        obj.AddComponent<GuiComponent>().Initialize(6));
        //        obj.AddComponent("Condition", new ConditionComponent());
        //        return obj;
        //    }
        //}

        //static public GameObject StatModCondition
        //{
        //    get
        //    {
        //        GameObject obj = GameObjectDb.StatusCondition;
        //        //     obj.GetComponent<StatusComponent>("Conditions").Properties.Add(
        //        return obj;
        //    }
        //}
        static public GameObject SkillObj
        {
            get
            {
                GameObject obj = new GameObject();
                obj["Info"] = new GeneralComponent(GameObject.Types.Skill, "Skill", "Base skill");
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
                obj["Info"] = new GeneralComponent(GameObject.Types.SkillMining, Stat.Mining.Name, "Mining skill");
                obj.AddComponent<GuiComponent>().Initialize(1);
                return obj;
            }
        }
        static public GameObject SkillLumberjacking
        {
            get
            {
                GameObject obj = GameObjectDb.SkillObj;
                obj["Info"] = new GeneralComponent(GameObject.Types.SkillLumberjacking, Stat.Lumberjacking.Name, "Lumberjacking skill");
                obj.AddComponent<GuiComponent>().Initialize(2);
                return obj;
            }
        }
        #region Abilities
        
        //static public GameObject AbilityDigging
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new InfoComponent(GameObject.Types.AbilityDigging, ObjectType.Ability, "Dig", "Digging Interaction");
        //        obj.AddComponent<GuiComponent>().Initialize(21);
        //        obj["Ability"] = new AbilityComponent(Message.Types.Shovel);
        //        return obj;
        //    }
        //}
        //static public GameObject AbilityAttack
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new InfoComponent(GameObject.Types.AbilityAttack, ObjectType.Ability, "Attack", "Attack Interaction");
        //        obj.AddComponent<GuiComponent>().Initialize(0);
        //        obj["Ability"] = new AbilityComponent(Message.Types.Attack);
        //        return obj;
        //    }
        //}
        //static public GameObject AbilityConsume
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new InfoComponent(GameObject.Types.AbilityConsume, ObjectType.Ability, "Consume", "AbilityConsume");
        //        obj.AddComponent<GuiComponent>().Initialize(0);
        //        obj["Ability"] = new AbilityComponent(Message.Types.Consume);
        //        return obj;
        //    }
        //}
        //static public GameObject AbilityConsume
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj["Info"] = new InfoComponent(GameObject.Types.AbilityConsume, ObjectType.Ability, "Consume", "AbilityConsume");
        //        obj.AddComponent<GuiComponent>().Initialize(0);
        //        obj["Ability"] = new AbilityComponent(Message.Types.Consume);
        //        return obj;
        //    }
        //}
        #endregion
        //static public GameObject Wall
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();//10, "Wall", "It is a wall.");
        //        obj.AddComponent("Sprite", new ActorSpriteComponent(new Sprite(
        //            Map.TerrainSprites,
        //            //Game1.Instance.Content.Load<Texture2D>("Graphics/singlewall"),
        //            //new Rectangle[][] { new Rectangle[] {new Rectangle(0,0,32,80)}},
        //            //new Rectangle[][] { new Rectangle[] { new Rectangle(0 * 32, 9 * 32, 32, 80) } },
        //            new Rectangle[][] { new Rectangle[] 
        //            {
        //                new Rectangle(0 * 32, 9 * 32, 32, 80),
        //                new Rectangle(1 * 32, 9 * 32, 32, 80),
        //                new Rectangle(2 * 32, 9 * 32, 32, 80),
        //                new Rectangle(3 * 32, 9 * 32, 32, 80)
        //            },new Rectangle[] 
        //            {
        //                new Rectangle(0 * 32, 9 * 32 + 80, 32, 80),
        //                new Rectangle(1 * 32, 9 * 32 + 80, 32, 80),
        //                new Rectangle(2 * 32, 9 * 32 + 80, 32, 80),
        //                new Rectangle(3 * 32, 9 * 32 + 80, 32, 80)
        //            },new Rectangle[] 
        //            {
        //                new Rectangle(0 * 32, 9 * 32 + 2 * 80, 32, 80),
        //                new Rectangle(1 * 32, 9 * 32 + 2 * 80, 32, 80),
        //                new Rectangle(2 * 32, 9 * 32 + 2 * 80, 32, 80),
        //                new Rectangle(3 * 32, 9 * 32 + 2 * 80, 32, 80)
        //            },new Rectangle[] 
        //            {
        //                new Rectangle(0 * 32, 9 * 32 + 3 * 80, 32, 80),
        //                new Rectangle(1 * 32, 9 * 32 + 3 * 80, 32, 80),
        //                new Rectangle(2 * 32, 9 * 32 + 3 * 80, 32, 80),
        //                new Rectangle(3 * 32, 9 * 32 + 3 * 80, 32, 80)
        //            },new Rectangle[] 
        //            {
        //                new Rectangle(0 * 32, 9 * 32 + 4 * 80, 32, 80),
        //                new Rectangle(1 * 32, 9 * 32 + 4 * 80, 32, 80),
        //                new Rectangle(2 * 32, 9 * 32 + 4 * 80, 32, 80),
        //                new Rectangle(3 * 32, 9 * 32 + 4 * 80, 32, 80)
        //            },new Rectangle[] 
        //            {
        //                new Rectangle(0 * 32, 9 * 32 + 5 * 80, 32, 80),
        //                new Rectangle(1 * 32, 9 * 32 + 5 * 80, 32, 80),
        //                new Rectangle(2 * 32, 9 * 32 + 5 * 80, 32, 80),
        //                new Rectangle(3 * 32, 9 * 32 + 5 * 80, 32, 80)
        //            }},
        //            //new Vector2(16, 72),
        //            new Vector2(16, 64),
        //            Block.WallMouseMap))
        //            //TileBase.TileSprites[TileBase.Types.Wall].Origin)
        //            );
        //        obj.AddComponent("Info", new GeneralComponent(GameObject.Types.Wall, "Wall", "It is a wall."));//, 8));
        //        obj.AddComponent<GuiComponent>().Initialize(8);

        //        obj["Physics"] = new WallComponent();// WallComponent.Create(obj);
        //        return obj;
        //    }
        //}
        //static public GameObject WallQuarter
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj.AddComponent("Sprite", new ActorSpriteComponent(new Sprite(
        //            Map.TerrainSprites,
        //            new Rectangle[][] { new Rectangle[] 
        //            {
        //                new Rectangle(4 * 32, 18 * 32, 32, 32),
        //                new Rectangle(5 * 32, 18 * 32, 32, 32),
        //                new Rectangle(6 * 32, 18 * 32, 32, 32),
        //                new Rectangle(7 * 32, 18 * 32, 32, 32)
        //            }},
        //            new Vector2(16, 16),
        //            Block.WallQuarterMouseMap))
        //            );
        //        obj.AddComponent("Info", new GeneralComponent(GameObject.Types.WallQuarter, "Wall 1/4", "It is a quarter of a wall."));
        //        obj.AddComponent<GuiComponent>().Initialize(8);

        //        obj["Physics"] = new WallComponent(height: 2);
        //        return obj;
        //    }
        //}
        //static public GameObject WallHalf
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject();
        //        obj.AddComponent("Sprite", new ActorSpriteComponent(new Sprite(
        //            Map.TerrainSprites,
        //            new Rectangle[][] { new Rectangle[] 
        //            {
        //                new Rectangle(4 * 32, 16 * 32, 32, 64),
        //                new Rectangle(5 * 32, 16 * 32, 32, 64),
        //                new Rectangle(6 * 32, 16 * 32, 32, 64),
        //                new Rectangle(7 * 32, 16 * 32, 32, 64)
        //            }},
        //            new Vector2(16, 48),
        //            Block.WallHalfMouseMap))
        //            );
        //        obj.AddComponent("Info", new GeneralComponent(GameObject.Types.WallHalf, "Wall 1/2", "It is a half wall."));
        //        obj.AddComponent<GuiComponent>().Initialize(8);

        //        obj["Physics"] = new WallComponent(height: 4);
        //        return obj;
        //    }
        //}


        //static public GameObject Bed
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject(GameObject.Types.Bed, "Bed", "A bigger bed than a small bed.", ObjectType.Furniture);
        //        Texture2D fulltex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bigbed2");
        //        fulltex.Name = "FullBigBedTexture";
        //        Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bigbedparts2");
        //        tex.Name = "BigBedTextureParts";
        //        Sprite full = new Sprite(fulltex, new Rectangle[][] { new Rectangle[] { 
        //            new Rectangle(0, 0, 80, 80),
        //            new Rectangle(80, 0, 80, 80),
        //            new Rectangle(160, 0, 80, 80),
        //            new Rectangle(240, 0, 80, 80)
        //        } }, new Vector2(80 - 32, 80 - 8));//16));
        //        obj.AddComponent<GuiComponent>().Initialize(0);
        //        obj["Ownership"] = new OwnershipComponent();
        //        obj["Sprite"] = new SpriteComponent(full);
        //        obj["Multi"] = new MultiTile2Component(full,
        //            new Dictionary<Vector3, Sprite>()
        //            {
        //                {Vector3.Zero, new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //                    new Rectangle(0*32, 4*64, 32, 64),
        //                    new Rectangle(1*32, 4*64, 32, 64),
        //                    new Rectangle(2*32, 4*64, 32, 64),
        //                    new Rectangle(3*32, 4*64, 32, 64),
        //                }}, new Vector2(16, 64-8))},
        //                {new Vector3(-1,0,0),new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //                    new Rectangle(0*32, 2*64, 32, 64),
        //                    new Rectangle(1*32, 2*64, 32, 64),
        //                    new Rectangle(2*32, 2*64, 32, 64),
        //                    new Rectangle(3*32, 2*64, 32, 64),
        //                }}, new Vector2(16, 64-8))},
        //                {new Vector3(-2,0,0),new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //                    new Rectangle(0*32, 0*64, 32, 64),
        //                    new Rectangle(1*32, 0*64, 32, 64),
        //                    new Rectangle(2*32, 0*64, 32, 64),
        //                    new Rectangle(3*32, 0*64, 32, 64),
        //                }}, new Vector2(16, 64-8))},
        //                {new Vector3(0,-1,0),new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //                    new Rectangle(0*32, 5*64, 32, 64),
        //                    new Rectangle(1*32, 5*64, 32, 64),
        //                    new Rectangle(2*32, 5*64, 32, 64),
        //                    new Rectangle(3*32, 5*64, 32, 64),
        //                }}, new Vector2(16, 64-8))},
        //                {new Vector3(-1,-1,0),new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //                    new Rectangle(0*32, 3*64, 32, 64),
        //                    new Rectangle(1*32, 3*64, 32, 64),
        //                    new Rectangle(2*32, 3*64, 32, 64),
        //                    new Rectangle(3*32, 3*64, 32, 64),
        //                }}, new Vector2(16, 64-8))},
        //                {new Vector3(-2,-1,0),new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //                    new Rectangle(0*32, 1*64, 32, 64),
        //                    new Rectangle(1*32, 1*64, 32, 64),
        //                    new Rectangle(2*32, 1*64, 32, 64),
        //                    new Rectangle(3*32, 1*64, 32, 64),
        //                }}, new Vector2(16, 64-8))},
        //            });
        //        obj["Physics"] = new PhysicsComponent(solid: true, height: 4, weight: 100f, size: -1);
        //        //obj["Activate"] = new InteractableComponent(new SortedDictionary<Message.Types, Component>() { 
        //        //{ Message.Types.Activate, new BedComponent() }, 
        //        //{ Message.Types.Mechanical, new PackableComponent() } 
        //        //});
        //        obj["Bed"] = new BedComponent();
        //        obj["Packable"] = new PackableComponent();
        //       // obj["Activate"] = new PackableComponent();
        //      //  obj["
        //        return obj;
        //    }
        //}

        //static public GameObject Bed
        //{
        //    get
        //    {
        //        GameObject obj = new GameObject(GameObject.Types.Bed, "Bed", "Provides sleep");
        //        Texture2D fulltex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bed");
        //        Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bed4");
        //        obj["Sprite"] = new MultiTile2Component(new Sprite(fulltex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, fulltex.Width, fulltex.Height) } }, new Vector2(fulltex.Width - 16, fulltex.Height - 16)),
        //            new Dictionary<Vector3, Sprite>() {
        //        { Vector3.Zero, new Sprite(tex, new Rectangle[][] { new Rectangle[] {
        //        ///ROTATIONS
        //         new Rectangle(0*32, 64, 32, 64),
        //         new Rectangle(1*32, 64, 32, 64),
        //         new Rectangle(2*32, 64, 32, 64),
        //         new Rectangle(3*32, 64, 32, 64) 
        //        }}, new Vector2(16, 64-16)) } ,
        //        { new Vector3(-1, 0 ,0), new Sprite(tex, new Rectangle[][] { new Rectangle[] { 
        //            new Rectangle(0*32, 0, 32, 64),
        //            new Rectangle(1*32, 0, 32, 64),
        //            new Rectangle(2*32, 0, 32, 64),
        //            new Rectangle(3*32, 0, 32, 64) 
        //        } }, new Vector2(16, 64- 16)) } ,
        //        });

        //        //Texture2D fulltex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bed");
        //        //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bed2");
        //        //obj["Sprite"] = new MultiTileComponent(new Sprite(fulltex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, fulltex.Width, fulltex.Height) } }, new Vector2(fulltex.Width - 16, fulltex.Height - 16)),
        //        //    new Dictionary<Vector3, Sprite>() {
        //        //{ Vector3.Zero, new Sprite(tex, new Rectangle[][] { new Rectangle[] { new Rectangle(tex.Width / 2, 0, tex.Width / 2, tex.Height) } }, new Vector2(16, tex.Height-16)) } ,
        //        //{ new Vector3(-1, 0 ,0), new Sprite(tex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, tex.Width / 2, tex.Height) } }, new Vector2(16, tex.Height- 16)) } ,
        //        //});

        //        //Texture2D tex = Game1.Instance.Content.Load<Texture2D>("Graphics/Objects/bed");
        //        //obj["Sprite"] = new SpriteComponent(new Sprite(tex, new Rectangle[][] { new Rectangle[] { new Rectangle(0, 0, tex.Width, tex.Height) } }, new Vector2(tex.Width - 16, tex.Height - 16))
        //        //);

        //        obj["Physics"] = new PhysicsComponent(solid: true, height: 4, weight: 100f);
        //        return obj;
        //    }
        //}
    }
}
