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
         * Abilities' execute function receive an actor object and a target targetargs as arguments, and decide how to manipulate them
         * either by doing some actions, or passing a message to either one of them.
         */

        /// <summary>
        /// maybe move this in the bodycomponent?
        /// </summary>
        //public static GameObjectSlot AbilityJump
        //{
        //    get
        //    {
        //        GameObjectSlot scriptSlot = Ability.Create(Script.Types.Jumping, 0, "Jump", "Jumping",
        //           (net, actor, target, args) =>
        //           {
        //               if (actor.Velocity.Z == 0)
        //                   actor.Velocity += new Vector3(0, 0, PhysicsComponent.Jump * (1 + StatsComponent.GetStatOrDefault(actor, Stat.Types.JumpHeight, 0f)));// (float)a.Actor["Stats"]["Jump Force"]);
        //           },
        //           range: (a1, t) => true);
        //        Script script = scriptSlot.Object["Ability"] as Script;
        //        script.TargetSelector = (actor, target) => new TargetArgs(actor);
        //        return scriptSlot;
        //    }
        //}
        //public static GameObjectSlot AbilityTill
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Tilling, 4, "Tilling", "Prepares soil for planting.",
        //            (net, actor, target, args) =>
        //            {
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Till));
        //            },
        //            3,
        //            Formula.GetFormula(Formula.Types.TillingSpeed),
        //            new InteractionConditionCollection(
        //                new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Tilling), "Requires tool")
        //                )
        //            );
        //    }
        //}
        //public static GameObjectSlot AbilityDigging
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Digging, 21, "Digging", "Removes a block of dirt.",
        //            //a =>
        //            //{
        //            //    //a.Target.PostMessage(Message.Types.Shovel, a.Actor);
        //            //    a.Target.PostMessage(a.Net, Message.Types.Shovel, writer => { writer.Write(a.Face); });
        //            //},
        //            (net, actor, target, args) =>
        //            {
        //                //a.Target.PostMessage(Message.Types.Shovel, a.Actor);
        //                //GameObject sender = args[0] as GameObject;
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Shovel, new object[] { actor }));
        //                //a.Target.PostMessage(a.Net, Message.Types.Shovel, writer => { writer.Write(a.Face); });
        //            },
        //            3,
        //            //a => Formula.GetValue(a, Formula.Types.DiggingSpeed)
        //            Formula.GetFormula(Formula.Types.DiggingSpeed)
        //            );
        //    }
        //}
        //public static GameObjectSlot AbilityMining
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Mining, 21, "Mining", "Mines a mineral block for resources.",
        //            //a =>
        //            //{
        //            //    //a.Target.PostMessage(Message.Types.Mine, a.Actor);
        //            //    a.Target.PostMessage(a.Net, Message.Types.Mine, writer => { writer.Write(a.Face); });
        //            //},
        //            (net, actor, target, args) =>
        //            {
        //                //a.Target.PostMessage(Message.Types.Mine, a.Actor);

        //                //a.Target.PostMessage(a.Net, Message.Types.Mine, writer => { writer.Write(a.Face); });
        //                // GameObject sender = args[0] as GameObject;
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Mine, new object[] { actor }));
        //            },
        //            3,
        //            Formula.GetFormula(Formula.Types.MiningSpeed),
        //            new InteractionConditionCollection(
        //                new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Mining), "Requires appropriate tool")
        //                )
        //            );
        //    }
        //}

        //public static GameObjectSlot AbilitySawing
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Sawing, 14, "Sawing", "Convert logs to planks.",
        //            //(a) =>
        //            //{
        //            //    //args.Target.PostMessage(Message.Types.Saw, args.Actor);
        //            //    a.Target.PostMessage(a.Net, Message.Types.Saw, writer => { writer.Write(a.Face); });
        //            //},
        //            (net, actor, target, args) =>
        //            {
        //                //args.Target.PostMessage(Message.Types.Saw, args.Actor);
        //                //a.Target.PostMessage(a.Net, Message.Types.Saw, writer => { writer.Write(a.Face); });
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Saw));
        //            },
        //            3,
        //            new Formula(Formula.Types.SawingSpeed, "Sawing speed")
        //            {
        //                Function = (actor) =>
        //                {
        //                    return SkillsComponent.GetSkillLevel(actor, Skill.Types.Carpentry) / 200f;
        //                }
        //            });
        //    }
        //}

        //public static GameObjectSlot AbilityFraming
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Framing, 0, "Framing", "Constructs a frame.",
        //            (net, actor, target, args) =>
        //            {
        //                //GameObject actor = args[0] as GameObject;
        //                //TargetArgs target = args[1] as TargetArgs;

        //                Block.Build(net, actor, target, GameObject.Types.WoodenPlank, GameObject.Types.WoodenFrame,
        //                  block =>
        //                  {
        //                      net.PostLocalEvent(block, ObjectEventArgs.Create(Message.Types.Crafted, new object[] { actor, GameObject.Types.WoodenPlank }));
        //                      if (ConstructionFrame.AutoShow)
        //                          net.PostLocalEvent(actor, ObjectEventArgs.Create(Message.Types.Interface, new object[] { new TargetArgs(block) }));
        //                  });

        //                //Block.Deploy(a, GameObject.Types.WoodenPlank, GameObject.Types.WoodenFrame,
        //                //  block =>
        //                //  {
        //                //      //block.PostMessage(Message.Types.Crafted, a.Actor, GameObject.Types.WoodenPlank);
        //                //      //if (ConstructionFrame.AutoShow)
        //                //      //    a.Actor.PostMessage(Message.Types.Interface, block);

        //                //      block.PostMessage(Message.Types.Crafted, a.Actor, a.Net, new byte[] { (int)GameObject.Types.WoodenPlank });
        //                //      if (ConstructionFrame.AutoShow)
        //                //          a.Net.PostLocalEvent(a.Actor, ObjectLocalEventArgs.Create(Message.Types.Interface, new TargetArgs(block)));
        //                //  });
        //            },
        //            3,
        //            new Formula(Formula.Types.ConstructionSpeed, "Construction Speed")
        //            {
        //                Function = (actor) =>
        //                {
        //                    return SkillsComponent.GetSkillLevel(actor, Skill.Types.Carpentry) / 200f;
        //                }
        //            });
        //    }
        //}
        //public static GameObjectSlot AbilityEquipping
        //{
        //    get
        //    {
        //        //return Script.Create(Script.Types.Equipping, 0, "Equip", "Equips this piece of equipment.",
        //        //    (a) =>
        //        //    {
        //        //        if (a.Net.IsNull())
        //        //            throw new Exception();
        //        //        a.Target.PostMessage(a.Net, Message.Types.Equip, writer => { writer.Write(a.Actor.NetworkID); });// args.Actor);
        //        //    });

        //        return Ability.Create(Script.Types.Equipping, 0, "Equip", "Equips this piece of equipment.",
        //            (net, actor, target, args) =>
        //            {
        //                //if (net.IsNull())
        //                //    throw new Exception();

        //                //a.Target.PostMessage(a.Net, Message.Types.Equip, writer => { writer.Write(a.Actor.NetworkID); });// args.Actor);
        //                //   GameObject actor = data[0] as GameObject;

        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Equip, new object[] { actor }));
        //            });
        //    }
        //}
        //public static GameObjectSlot AbilityPickUp
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.PickUp, 0, "Pick Up", "Pick up this item.",
        //            //(a) =>
        //            (net, actor, target, args) =>
        //            {
        //                //GameObject recipient = args[0] as GameObject;
        //                //GameObject source = args[1] as GameObject;
        //                //a.Target.PostMessage(a.Net, Message.Types.PickUp, writer => { writer.Write(a.Actor.NetworkID); });// args.Actor);
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.PickUp, new object[] { actor }));
        //            });
        //    }
        //}
        //public static GameObjectSlot AbilityDrop
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Drop, 0, "Drop", "Drops carried item.",
        //            (net, actor, target, args) =>
        //            {
        //                // maybe have a different event for dropping without a target surface?

        //                // actor drops item in place (not placing it on a specific target block)
        //                //if (target.Object.IsNull())
        //                //{
        //                //    net.PostLocalEvent(actor, ObjectEventArgs.Create(Message.Types.Drop));//, w => w.Write(Vector3.Zero)));
        //                //}
        //                //else
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.DropOn, () =>
        //                {
        //                    net.PostLocalEvent(actor, ObjectEventArgs.Create(Message.Types.Dropped));
        //                }, new object[] { actor, target.Face }));
        //            }, range: (a, b, r) => true);
        //    }
        //}
        //public static GameObjectSlot AbilityChopping
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Chopping, 0, "Chopping", "Chops trees down for logs.",
        //            //(args) =>
        //            //{
        //            //    //args.Target.PostMessage(Message.Types.Chop, args.Actor);
        //            //    args.Target.PostMessage(args.Net, Message.Types.Chop, w => { TargetArgs.Write(w, args.Target); });
        //            //},
        //            (net, actor, target, args) =>
        //            {
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Chop, new object[] { actor }));
        //            },
        //            5,
        //            new Formula(Formula.Types.ChoppingSpeed, "Chopping Speed")
        //            {
        //                Function = (actor) =>
        //                {
        //                    return SkillsComponent.GetSkillLevel(actor, Skill.Types.Lumberjacking) / 200f;
        //                }
        //            },
        //            new InteractionConditionCollection(
        //                new InteractionCondition(
        //                    (actor, target) =>
        //                    {
        //                        return FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Chopping);
        //                    },
        //                    "Requires appropriate tool"
        //                    )));
        //    }
        //}
        //public static GameObjectSlot AbilityActivate
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Activate, 0, "Activate", "Activate this object.",
        //            (net, actor, target, args) =>
        //            {
        //                //args.Target.PostMessage(Message.Types.Activate, args.Actor, args.Face);
        //                //GameObject actor = args[0] as GameObject;
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Activate, new object[] { actor }));
        //            });

        //    }
        //}
        //public static GameObjectSlot ScriptCraftingWorkbench
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.CraftingWorkbench, 0, "Crafting", "Craft items.",
        //            (net, actor, target, args) =>
        //            {
        //                GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                //GameObject bp = GameObject.Objects[bpID];
        //                //net.PostLocalEvent(bp, ObjectEventArgs.Create(Message.Types.Craft, new object[] { actor }));
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Craft, new object[] { bpID, actor }));
        //                //bp.GetComponent<BlueprintComponent>().Blueprint.Craft(
        //            },
        //            2,
        //            new Formula(Formula.Types.Default, "Default"),
        //            new InteractionConditionCollection(
        //              new InteractionCondition((actor, target, args) =>
        //              {
        //                  GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                  GameObject bp = GameObject.Objects[bpID];
        //                  return BlueprintComponent.MaterialsAvailable(bp, target.GetComponent<WorkbenchComponent>().Slots);
        //              }, "Materials missing!"),
        //              new InteractionCondition((actor, target, args)=>
        //              {
        //                  // check if actor has space
        //                  GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                  GameObject bp = GameObject.Objects[bpID];
        //                  return target.GetComponent<WorkbenchComponent>().Slots
        //                      .Find(slot => slot.HasValue ? (slot.Object.ID == bp.GetComponent<BlueprintComponent>().Blueprint.ProductID && slot.StackSize < slot.StackMax) : true) != null;
        //              }, "Not enough space!")
        //              )
        //            ,
        //            range: (a1, a2, r) => true);
        //    }
        //}
        //public static GameObjectSlot ScriptCraftingPerson
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Crafting, 0, "Crafting", "Craft items.",
        //            (net, actor, target, args) =>
        //            {
        //                GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                GameObject bpObj = GameObject.Objects[bpID];
        //                ItemContainer container = actor.GetComponent<InventoryComponent>().Containers.First();
        //                //bp.GetComponent<BlueprintComponent>().Blueprint.Craft(container, out product);
        //                Blueprint bp = bpObj.GetComponent<BlueprintComponent>().Blueprint;
        //                GameObject product = bp.Craft();

        //                // consume materials
        //                foreach (var mat in bp.Stages.First())
        //                {
        //                    int amount = mat.Value;
        //                    //foreach 
        //                    var foundMats = (
        //                        from slot in container
        //                        where slot.HasValue
        //                        where slot.Object.ID == mat.Key
        //                        select slot);
        //                    //{

        //                    //}
        //                    var queue = new Queue<GameObjectSlot>(foundMats);
        //                    while (amount > 0 && queue.Count > 0)
        //                    {
        //                        var slot = queue.Peek();
        //                        amount--;
        //                        slot.StackSize--;
        //                        if (slot.StackSize == 0)
        //                            queue.Dequeue();
        //                    }
        //                    if (amount > 0)
        //                        throw new Exception("Invalid materials");
        //                }

        //                // send object
        //                net.PostLocalEvent(actor, ObjectEventArgs.Create(Message.Types.Receive, new object[] { product }));

        //                //net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Craft, new object[] { bpID, actor }));
        //            },
        //            2,
        //            new Formula(Formula.Types.Default, "Default"),
        //            new InteractionConditionCollection(
        //              new InteractionCondition((actor, target, args) =>
        //              {
        //                  GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                  GameObject bp = GameObject.Objects[bpID];
        //                  return BlueprintComponent.MaterialsAvailable(bp, target.GetComponent<InventoryComponent>().Containers.First());
        //              }, "Materials missing!"),
        //              new InteractionCondition((actor, target, args)=>
        //              {
        //                  // check if actor has space
        //                  GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(args, 0);
        //                  GameObject bp = GameObject.Objects[bpID];
        //                  return actor.GetComponent<InventoryComponent>().Containers.First()
        //                      .Find(slot => slot.HasValue ? (slot.Object.ID == bp.GetComponent<BlueprintComponent>().Blueprint.ProductID && slot.StackSize < slot.StackMax) : true) != null;
        //              }, "Not enough space!")
        //              )
        //            ,
        //            range: (a1, a2, r) => true);
        //    }
        //}
        //public static GameObjectSlot AbilityConstruction
        //{
        //    get
        //    {
        //        return Ability.Create(Script.Types.Construction, 0, "Construct", "Construct structures.",
        //            (net, actor, target, args) =>
        //            {
        //                //args.Target.PostMessage(Message.Types.Construct, args);
        //                net.PostLocalEvent(target.Object, ObjectEventArgs.Create(Message.Types.Construct));
        //            },
        //            2,
        //            new Formula(Formula.Types.Default, "Default"),
        //            range: (a1, a2, r) => true);
        //    }
        //}
    }
}
