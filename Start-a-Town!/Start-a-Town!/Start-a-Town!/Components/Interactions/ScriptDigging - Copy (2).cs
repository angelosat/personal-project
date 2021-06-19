using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Scripts
{
    class ScriptDigging : Script
    {
        //public ScriptArgs Args { get; set; }

        public ScriptDigging()
        {
            this.ID = Script.Types.Digging;
            this.Name = "Digging";
            this.BaseTimeInSeconds = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.DiggingSpeed);
            this.RangeCheck = DefaultRangeCheck;
            //this.Requirements =
            //    new InteractionConditionCollection(
            //        //new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Digging), "Requires appropriate tool")
            //        new InteractionCondition((actor, target) => UseComponent.HasAbility(actor.GetComponent<InventoryComponent>().Holding.Object, Script.Types.Digging), "Requires appropriate tool")
            //        );
        }

        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);

            this.AddComponent(ScriptTimer2.StartNew(GetTimeInMs(args.Actor), () => Success(this.ArgsSnapshot)));
            ControlComponent control = args.Actor.GetComponent<ControlComponent>();
            this.AddComponent(new ScriptCondition(() => control.RunningScripts.ContainsKey(Script.Types.Walk), () => Stop(args)));

            this.ArgsSnapshot = args;

            args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        }

        public override void Success(ScriptArgs args)
        {
            base.Success(args);
            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Shovel, new object[] { args.Actor }));
        }

        public override void Finish(ScriptArgs args)
        {
            base.Finish(args);
            args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        }

        //public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        //{
        //    ScriptTimer timer = this.ScriptComponents.FirstOrDefault(c => c is ScriptTimer) as ScriptTimer;
        //    if (timer.IsNull())
        //        return;
        //    timer.DrawUI(sb, camera, parent);
        //}

        public override object Clone()
        {
            return new ScriptDigging();
        }
    }
}
