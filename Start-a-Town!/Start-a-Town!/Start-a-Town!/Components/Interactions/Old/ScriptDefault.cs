using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptDefault : Script
    {
        public Action<ScriptArgs> OnFinish { get; set; }

        public override Script.Types ID
        {
            get
            {
                return Script.Types.Default;
            }
        }
        public ScriptDefault()
        {
        }
        public ScriptDefault(Script.Types id, string name, Action<ScriptArgs> onFinish, float basetime = 0, float rangeValue = InteractionOld.DefaultRange, Formula.Types speedBonus = Formula.Types.Default)
        {
            this.ID = id;
            this.Name = name;
            this.BaseTimeInSeconds = basetime;
            this.SpeedBonus = Formula.GetFormula(speedBonus);
            this.RangeCheck = DefaultRangeCheck;
            this.RangeValue = rangeValue;
            this.OnFinish = onFinish;
            //this.Requirements =
            //    new InteractionConditionCollection(
            //    //new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Digging), "Requires appropriate tool")
            //        new InteractionCondition((actor, target) => UseComponent.HasAbility(actor.GetComponent<InventoryComponent>().Holding.Object, id), "Requires appropriate tool")
            //        );
        }

        //public override void OnStart(ScriptArgs args)
        //{
        //    //base.Start(args);

        //    this.AddComponent(ScriptTimer2.StartNew(GetTimeInMs(args.Actor), () => Success(this.ArgsSnapshot)));
        //    ControlComponent control = args.Actor.GetComponent<ControlComponent>();
        //    this.AddComponent(new ScriptCondition(() => control.RunningScripts.ContainsKey(Script.Types.Walk), () => Stop(args)));

        //    this.ArgsSnapshot = args;

        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        //}

        //public override void Success(ScriptArgs args)
        //{
        //    base.Success(args);
        //    args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Shovel, new object[] { args.Actor }));
        //}

        //public override void Finish(ScriptArgs args)
        //{
        //    base.Finish(args);
        //    this.OnFinish(args);
        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        //}

        public override object Clone()
        {
            //return new ScriptDefault(this.ID, this.Name, this.OnFinish, this.BaseTimeInSeconds, this.RangeValue, this.SpeedBonus.ID);
            var scr = new ScriptDefault();
            foreach (var comp in this.ScriptBehaviors)
                scr.AddComponent(comp.Value.Clone() as ScriptComponent);
            return scr;
        }
    }
}
