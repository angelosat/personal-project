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
        public AbilityArgs Args { get; set; }
        public double Time { get; set; }
        public double Length { get; set; }
        public ScriptDigging()
        {
            this.ID = Script.Types.Digging;
            this.Name = "Digging";
            this.BaseTime = 3;
            this.SpeedBonus = Formula.GetFormula(Formula.Types.DiggingSpeed);
            this.RangeCheck = DefaultRangeCheck;
            this.Requirements =
                new InteractionConditionCollection(
                    //new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Digging), "Requires appropriate tool")
                    new InteractionCondition((actor, target) => UseComponent.HasAbility(actor.GetComponent<InventoryComponent>().Holding.Object, Script.Types.Digging), "Requires appropriate tool")
                    );
        }

        float Percentage
        {
            get { return (float)(this.Time / this.Length); }
        }

        public override void Start(AbilityArgs args)
        {
            this.Time = 0;
            this.Length = GetTimeInMs(args.Actor);
            this.ScriptState = ScriptState.Running;
            this.Args = args;

            args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            // if moving, interrupt
            if (parent.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Walk))
            {
                Cancel(new AbilityArgs(net, parent));
                return;
            }
            this.Time += 1000 / (float)Engine.TargetFps;
            if (this.Time < this.Length)
                return;
            Success(this.Args);
         //   this.State = ScriptState.Finished;
        }

        public override void Success(AbilityArgs args)
        {
            args.Net.PostLocalEvent(args.Target.Object, ObjectEventArgs.Create(Message.Types.Shovel, new object[] { args.Actor }));
        }

        public override void Finish(AbilityArgs args)
        {
            args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            Vector3 global = parent.Global;

            Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, Percentage);
            UIManager.DrawStringOutlined(sb, this.Name, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }

        public override object Clone()
        {
            return new ScriptDigging();
        }
    }
}
