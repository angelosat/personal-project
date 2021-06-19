using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptBench : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.CraftingBench;
            }
        }
        public override string Name
        {
            get
            {
                return "BenchCrafting";
            }
        }

        GameObject.Types BlueprintID { get; set; }

        public ScriptBench()
        {
            //this.Conditions =
            //    new ConditionCollection(
            //      new Condition((actor, target, a) =>
            //      {
            //          GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(a, 0);
            //          GameObject bp = GameObject.Objects[bpID];
            //          return BlueprintComponent.MaterialsAvailable(bp, target.GetComponent<ScribeComponent>().Materi);
            //      }, "Materials missing!"),
            //      new Condition((actor, target, a) =>
            //      {
            //          // check if actor has space
            //          GameObject.Types bpID = (GameObject.Types)BitConverter.ToInt32(a, 0);
            //          GameObject bp = GameObject.Objects[bpID];
            //          return target.GetComponent<WorkbenchComponent>().Slots
            //              .Find(slot => slot.HasValue ? (slot.Object.ID == bp.GetComponent<BlueprintComponent>().Blueprint.ProductID && slot.StackSize < slot.StackMax) : true) != null;
            //      }, "Not enough space!")
            //      );
        }

        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);
            this.BlueprintID = (GameObject.Types)BitConverter.ToInt32(args.Args, 0);
            this.AddComponent(ScriptTimer2.StartNew(3000, () => Success(args), "Crafting"));
            ControlComponent control = args.Actor.GetComponent<ControlComponent>();
            this.AddComponent(new ScriptCondition(() => control.RunningScripts.ContainsKey(Script.Types.Walk), () => Stop(args)));
            args.Actor.GetComponent<SpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        }

        public override void Success(ScriptArgs args)
        {
            base.Success(args);

            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Craft, new object[] { this.BlueprintID, args.Actor }));


            args.Actor.GetComponent<SpriteComponent>().Body.Stop(Graphics.AnimationCollection.Working);
        }



        public override object Clone()
        {
            return new ScriptBench();
        }
    }
}
