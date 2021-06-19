using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptWorkbench : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.CraftingWorkbench;
            }
        }
        public override string Name
        {
            get
            {
                return "Crafting";
            }
        }

        GameObject.Types BlueprintID { get; set; }

        public ScriptWorkbench()
        {
            this.Conditions =
                new ConditionCollection(
                  new Condition((actor, target, a) =>
                  {
                      GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(a, 0);
                      GameObject bp = GameObject.Objects[bpID];
                      return BlueprintComponent.MaterialsAvailable(bp, target.GetComponent<WorkbenchComponent>().Slots);
                  }, "Materials missing!")//,
                  //new Condition((actor, target, a) =>
                  //{
                  //    // check if actor has space
                  //    GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(a, 0);
                  //    GameObject bp = GameObject.Objects[bpID];
                  //    return target.GetComponent<WorkbenchComponent>().Slots
                  //        .Find(slot => slot.HasValue ? (slot.Object.ID == bp.GetComponent<BlueprintComponent>().Blueprint.ProductID && slot.StackSize < slot.StackMax) : true) != null;
                  //}, "Not enough space!")
                  );
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Crafting", 1));//, this.Success));
            this.AddComponent(new ScriptRangeCheck(DefaultRangeCheck, InteractionOld.DefaultRange));
        }

        public override void OnStart(ScriptArgs args)
        {
            this.BlueprintID = (GameObject.Types)BitConverter.ToInt32(args.Args, 0);
            //this.AddComponent(ScriptTimer2.StartNew(3000, () => Success(args), "Crafting"));
            //ControlComponent control = args.Actor.GetComponent<ControlComponent>();
            //this.AddComponent(new ScriptCondition(() => control.RunningScripts.ContainsKey(Script.Types.Walk), () => Stop(args)));
            //args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        }

        public override void Success(ScriptArgs args)
        {
            base.Success(args);

            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Craft, new object[] { this.BlueprintID, args.Actor }));


            args.Actor.GetComponent<SpriteComponent>().Body.Stop(Graphics.AnimationCollection.Working);
        }



        public override object Clone()
        {
            return new ScriptWorkbench();
        }
    }
}
