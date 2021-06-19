using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptBuildFrame : Script
    {
        //public ScriptArgs Args { get; set; }
        public double Time { get; set; }
        public double Length { get; set; }
        public ScriptBuildFrame()
        {
            this.ID = Script.Types.BuildFrame;
            this.Name = "Building";
            this.BaseTimeInSeconds = 3;
         //   this.SpeedBonus = Formula.GetFormula(Formula.Types.ConstructionSpeed);
            this.RangeCheck = DefaultRangeCheck;
            this.Conditions = 
                new ConditionCollection(
                    //new InteractionCondition((actor, target) => FunctionComponent.HasAbility(InventoryComponent.GetHeldObject(actor).Object, Script.Types.Mining), "Requires appropriate tool")
                    //new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<GearComponent>().Holding.Object, Script.Types.BuildFootprint), "Requires appropriate tool")
                    new Condition((actor, target) => UseComponentOld.HasAbility(actor.GetComponent<HaulComponent>().Holding.Object, Script.Types.BuildFootprint), "Requires appropriate tool")

                    );
            this.AddComponent(new ScriptEvaluations(this.Fail,
                new ScriptEvaluation(
                    a =>
                    {
                        return a.Target.Object.GetComponent<ConstructionFootprint>().Ready;
                    }, Message.Types.InvalidTarget
                //a => ConstructionFrame.MaterialsAvailable(a.Net, a.Target.Object), Message.Types.InvalidTarget
                    )));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Building", 5));//, this.Success));
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
        }

        //float Percentage
        //{
        //    get { return (float)(this.Time / this.Length); }
        //}

        //public override void OnStart(ScriptArgs args)
        //{    
        //    this.Time = 0;
        //    this.Length = GetTimeInMs(args.Actor);
        //    this.ScriptState = ScriptState.Running;
        //    this.ArgsSnapshot = args;

        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.Start(Graphics.AnimationCollection.Working);
        //}

        //public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        //{
        //    // if moving, interrupt
        //    if (parent.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Walk))
        //    {
        //        Stop(new ScriptArgs(net, parent));
        //        return;
        //    }
        //    this.Time += 1000 / (float)Engine.TargetFps;
        //    if (this.Time < this.Length)
        //        return;
        //    //OnSuccess(this.Args);
        //    this.Success(this.ArgsSnapshot);
        //    this.ScriptState = ScriptState.Finished;
        //}

        public override void OnSuccess(ScriptArgs args)
        {
            args.Target.Object.GetComponent<ConstructionFootprint>().Finish(args.Net, args.Target.Object, args.Actor);
            //base.Success(args);
            //List<GameObject> mats;
            //if (ConstructionFootprint.MaterialsAvailable(args.Net, args.Target.Object, out mats))
            //{
            //    foreach (var mat in mats)
            //    {
            //        args.Net.Despawn(mat);
            //        args.Net.SyncDisposeObject(mat);
            //    }
            //}
            //args.Net.PostLocalEvent(args.Target.Object, Message.Types.Construct, args.Actor);

        }

        //public override void Finish(ScriptArgs args)
        //{
        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        //}

        //public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        //{
        //    Vector3 global = parent.Global;

        //    Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
        //    Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
        //    Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
        //    Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
        //    InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, Percentage);
        //    UIManager.DrawStringOutlined(sb, this.Name, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        //}

        public override object Clone()
        {
            return new ScriptBuildFrame();
        }
    }
}
