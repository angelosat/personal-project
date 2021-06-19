using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Abilities
{
    class Abilities
    {
        /*
         * Abilities are performed immediately and can start a new interaction
         * Abilities are interaction factories, contain logic for initiating interactions,
         * such as script for interaction duration, range, requirement checks, and final actions.
         * Must be contained locally in both server and client because transferring methods over network is impossible. 
         * They are reusable interactions between old and new objects essentially
         */

        public static GameObjectSlot AbilityJump
        {
            get
            {
                GameObjectSlot scriptSlot = Script.Create(Script.Types.Jumping, 0, "Jump", "Jumping",
                   (net, recipient, p) =>
                   {
                       MovementComponent pos = recipient.GetPosition();
                       if (pos.GetProperty<Vector3>("Speed").Z == 0)
                           pos["Speed"] = pos.GetProperty<Vector3>("Speed") + new Vector3(0, 0, PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(recipient, Stat.Types.JumpHeight, 0f)));// (float)a.Actor["Stats"]["Jump Force"]);

                           //pos["Speed"] = pos.GetProperty<Vector3>("Speed") + new Vector3(0, 0, PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(a.Actor, Stat.Types.JumpHeight, 0f)));// (float)a.Actor["Stats"]["Jump Force"]);
                   },
                   range: (a1, t) => true);
                AbilityComponent script = scriptSlot.Object["Ability"] as AbilityComponent;
                script.TargetSelector = (actor, target) => new TargetArgs(actor);
                script.Execute = (net, no, data) =>
                   {
                    //   TargetArgs finalTarget = script.TargetSelector(actor, target);
                       GameObject jumper = data[0] as GameObject;
                       MovementComponent pos = jumper.GetPosition();
                       if (pos.GetProperty<Vector3>("Speed").Z == 0)
                           pos["Speed"] = pos.GetProperty<Vector3>("Speed") + new Vector3(0, 0, PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(jumper, Stat.Types.JumpHeight, 0f)));// (float)a.Actor["Stats"]["Jump Force"]);
                   };
                return scriptSlot;
                //return Script.Create(Script.Types.Jumping, 0, "Jump", "Jumping",
                //   a =>
                //   {
                //       MovementComponent pos = a.Actor.GetPosition();
                //       if (pos.GetProperty<Vector3>("Speed").Z == 0)
                //           pos["Speed"] = pos.GetProperty<Vector3>("Speed") + new Vector3(0, 0, PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(a.Actor, Stat.Types.JumpHeight, 0f)));// (float)a.Actor["Stats"]["Jump Force"]);
                //   },
                //   range: (a1, t) => true);
            }
        }
        public static GameObjectSlot AbilityTill
        {
            get
            {
                return Script.Create(Script.Types.Tilling, 4, "Tilling", "Prepares soil for planting.",
                    (net, recipient, p) =>
                    {
                        //a.Target.PostMessage(Message.Types.Till, a.Actor, a.Face);
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Till));

                        //ObjectEventArgs.Create(Message.Types.Till, new TargetArgs(a.Actor), TargetArgs.Empty).Post(a.Net);

                        //a.Target.PostMessage(a.Net, Message.Types.Till, writer => { writer.Write(a.Face); });
                    },
                    //a =>
                    //{
                    //    //a.Target.PostMessage(Message.Types.Till, a.Actor, a.Face);
                    //    a.Net.PostLocalEvent(a.Target, ObjectLocalEventArgs.Create(Message.Types.Till, new TargetArgs(a.Actor)));

                    //    //ObjectEventArgs.Create(Message.Types.Till, new TargetArgs(a.Actor), TargetArgs.Empty).Post(a.Net);

                    //    //a.Target.PostMessage(a.Net, Message.Types.Till, writer => { writer.Write(a.Face); });
                    //},
                    //   actor => Formula.GetValue(actor, Formula.Types.TillingSpeed)
                    3,
                    Formula.GetFormula(Formula.Types.TillingSpeed),
                    new InteractionConditionCollection(
                        new InteractionCondition((actor, target)=>FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Tilling), "Requires appropriate tool")
                        )
                    );
            }
        }
        public static GameObjectSlot AbilityDigging
        {
            get
            {
                return Script.Create(Script.Types.Digging, 21, "Digging", "Removes a block of dirt.",
                    //a =>
                    //{
                    //    //a.Target.PostMessage(Message.Types.Shovel, a.Actor);
                    //    a.Target.PostMessage(a.Net, Message.Types.Shovel, writer => { writer.Write(a.Face); });
                    //},
                    (net, recipient, args) =>
                    {
                        //a.Target.PostMessage(Message.Types.Shovel, a.Actor);
                        //GameObject sender = args[0] as GameObject;
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Shovel, args));//new object[] {sender}));
                        //a.Target.PostMessage(a.Net, Message.Types.Shovel, writer => { writer.Write(a.Face); });
                    },
                    3,
                    //a => Formula.GetValue(a, Formula.Types.DiggingSpeed)
                    Formula.GetFormula(Formula.Types.DiggingSpeed)
                    );
            }
        }
        public static GameObjectSlot AbilityMining
        {
            get
            {
                return Script.Create(Script.Types.Mining, 21, "Mining", "Mines a mineral block for resources.",
                    //a =>
                    //{
                    //    //a.Target.PostMessage(Message.Types.Mine, a.Actor);
                    //    a.Target.PostMessage(a.Net, Message.Types.Mine, writer => { writer.Write(a.Face); });
                    //},
                    (net, recipient, args) =>
                    {
                        //a.Target.PostMessage(Message.Types.Mine, a.Actor);
                        
                        //a.Target.PostMessage(a.Net, Message.Types.Mine, writer => { writer.Write(a.Face); });
                       // GameObject sender = args[0] as GameObject;
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Mine,args));// new object[] { sender }));
                    },
                    3,
                    Formula.GetFormula(Formula.Types.MiningSpeed),
                    new InteractionConditionCollection(
                        new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Mining), "Requires appropriate tool")
                        )
                    );
            }
        }
        public static GameObjectSlot AbilitySawing
        {
            get
            {
                return Script.Create(Script.Types.Sawing, 14, "Sawing", "Convert logs to planks.",
                    //(a) =>
                    //{
                    //    //args.Target.PostMessage(Message.Types.Saw, args.Actor);
                    //    a.Target.PostMessage(a.Net, Message.Types.Saw, writer => { writer.Write(a.Face); });
                    //},
                    (net, recipient, args) =>
                    {
                        //args.Target.PostMessage(Message.Types.Saw, args.Actor);
                        //a.Target.PostMessage(a.Net, Message.Types.Saw, writer => { writer.Write(a.Face); });
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Saw));
                    },
                    3,
                    new Formula(Formula.Types.SawingSpeed, "Sawing speed")
                    {
                        Function = (actor) =>
                        {
                            return SkillsComponent.GetSkillLevel(actor, Skill.Types.Carpentry) / 200f;
                        }
                    });
            }
        }
        public static GameObjectSlot AbilityFraming
        {
            get
            {
                return Script.Create(Script.Types.Framing, 0, "Framing", "Constructs a frame.",
                    (net, recipient, args) =>
                    {
                        GameObject actor = args[0] as GameObject;
                        TargetArgs target = args[1] as TargetArgs;

                        Block.Build(net, actor, target, GameObject.Types.WoodenPlank, GameObject.Types.WoodenFrame,
                          block =>
                          {
                              net.PostLocalEvent(block, ObjectLocalEventArgs.Create(Message.Types.Crafted, new object[] { actor, GameObject.Types.WoodenPlank }));
                              if (ConstructionFrame.AutoShow)
                                  net.PostLocalEvent(actor, ObjectLocalEventArgs.Create(Message.Types.Interface, new object[]{ new TargetArgs(block)}));
                          });

                        //Block.Deploy(a, GameObject.Types.WoodenPlank, GameObject.Types.WoodenFrame,
                        //  block =>
                        //  {
                        //      //block.PostMessage(Message.Types.Crafted, a.Actor, GameObject.Types.WoodenPlank);
                        //      //if (ConstructionFrame.AutoShow)
                        //      //    a.Actor.PostMessage(Message.Types.Interface, block);

                        //      block.PostMessage(Message.Types.Crafted, a.Actor, a.Net, new byte[] { (int)GameObject.Types.WoodenPlank });
                        //      if (ConstructionFrame.AutoShow)
                        //          a.Net.PostLocalEvent(a.Actor, ObjectLocalEventArgs.Create(Message.Types.Interface, new TargetArgs(block)));
                        //  });
                    },
                    3,
                    new Formula(Formula.Types.ConstructionSpeed, "Construction Speed")
                    {
                        Function = (actor) =>
                        {
                            return SkillsComponent.GetSkillLevel(actor, Skill.Types.Carpentry) / 200f;
                        }
                    });
            }
        }
        public static GameObjectSlot AbilityEquipping
        {
            get
            {
                //return Script.Create(Script.Types.Equipping, 0, "Equip", "Equips this piece of equipment.",
                //    (a) =>
                //    {
                //        if (a.Net.IsNull())
                //            throw new Exception();
                //        a.Target.PostMessage(a.Net, Message.Types.Equip, writer => { writer.Write(a.Actor.NetworkID); });// args.Actor);
                //    });

                return Script.Create(Script.Types.Equipping, 0, "Equip", "Equips this piece of equipment.",
                    (net, recipient, data) =>
                    {
                        //if (net.IsNull())
                        //    throw new Exception();
                      
                        //a.Target.PostMessage(a.Net, Message.Types.Equip, writer => { writer.Write(a.Actor.NetworkID); });// args.Actor);
                        GameObject actor = data[0] as GameObject;
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Equip, new object[] { actor }));
                    });
            }
        }
        public static GameObjectSlot AbilityPickUp
        {
            get
            {
                return Script.Create(Script.Types.PickUp, 0, "Pick Up", "Pick up this item.",
                    //(a) =>
                    (net, target, args) =>
                    {
                        GameObject recipient = args[0] as GameObject;
                        GameObject source = args[1] as GameObject;
                        //a.Target.PostMessage(a.Net, Message.Types.PickUp, writer => { writer.Write(a.Actor.NetworkID); });// args.Actor);
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.PickUp, new object[] { source }));
                    });
            }
        }
        public static GameObjectSlot AbilityChopping
        {
            get
            {
                return Script.Create(Script.Types.Chopping, 0, "Chopping", "Chops trees down for logs.",
                    //(args) =>
                    //{
                    //    //args.Target.PostMessage(Message.Types.Chop, args.Actor);
                    //    args.Target.PostMessage(args.Net, Message.Types.Chop, w => { TargetArgs.Write(w, args.Target); });
                    //},
                    (net, recipient, data) =>
                    {
                        GameObject sender = data[0] as GameObject;
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Chop, new object[] { sender }));
                    },
                    5,
                    new Formula(Formula.Types.ChoppingSpeed, "Chopping Speed")
                    {
                        Function = (actor) =>
                        {
                            return SkillsComponent.GetSkillLevel(actor, Skill.Types.Lumberjacking) / 200f;
                        }
                    },
                    new InteractionConditionCollection(
                        new InteractionCondition(
                            (actor, target) =>
                            {
                                return FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Chopping);
                            },
                            "Requires appropriate tool"
                            )));
            }
        }
        public static GameObjectSlot AbilityActivate
        {
            get
            {
                return Script.Create(Script.Types.Activate, 0, "Activate", "Activate this object.",
                    (net, recipient, args) =>
                    {
                        //args.Target.PostMessage(Message.Types.Activate, args.Actor, args.Face);
                        GameObject actor = args[0] as GameObject;
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Activate,new object[] { actor }));
                    });

            }
        }
        public static GameObjectSlot AbilityCrafting
        {
            get
            {
                return Script.Create(Script.Types.Crafting, 0, "Crafting", "Craft items.",
                    (net, recipient, args) =>
                    {
                       //args.Target.PostMessage(Message.Types.Craft, args);// args.Actor, args.Face, args.Parameters);
                        GameObject source = args[0] as GameObject;
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Craft, new object[] { source }));
                    },
                    2,
                    new Formula(Formula.Types.Default, "Default"),
                    range: (a1, a2)=>true);

            }
        }
        public static GameObjectSlot AbilityConstruction
        {
            get
            {
                return Script.Create(Script.Types.Construction, 0, "Construct", "Construct structures.",
                    (net, recipient, args) =>
                    {
                        //args.Target.PostMessage(Message.Types.Construct, args);
                        net.PostLocalEvent(recipient, ObjectLocalEventArgs.Create(Message.Types.Construct));
                    },
                    2,
                    new Formula(Formula.Types.Default, "Default"),
                    range: (a1, a2) => true);
            }
        }
    }
}
