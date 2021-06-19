using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    class ScriptFraming : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.Framing;
            }
        }
        public override string Name
        {
            get
            {
                return "Framing";
            }
        }
        public override float BaseTimeInSeconds
        {
            get
            {
                return 0.5f;
            }
        }

        //public ScriptArgs Args { get; set; }
        public double Time { get; set; }
        public double Length { get; set; }

        GameObject FramingMaterial { get; set; }

        float Percentage
        {
            get { return (float)(this.Time / this.Length); }
        }

        public ScriptFraming()
        {
            this.RangeCheck = DefaultRangeCheck;
            this.AddComponent(new ScriptAnimation(Graphics.AnimationCollection.Working));
            this.AddComponent(new ScriptTimer(a => GetTimeInMs(a.Actor), "Framing", 3));//, this.Success));
            this.AddComponent(new ScriptEvaluations((a) => this.ScriptState = Components.ScriptState.Finished,
                new ScriptEvaluation(
                    a => a.Target.Object.IsBlock(),
                    a => a.Net.EventOccured(Message.Types.InvalidTarget, a.Actor)),
                new ScriptEvaluation(
                    a => DefaultRangeCheck(a.Actor, a.Target, this.RangeValue),
                    a => a.Net.EventOccured(Message.Types.OutOfRange, a.Actor))
                )
            );
            //this.AddComponent(new ScriptConsumeHeldItem());
        }

        public override void OnStart(ScriptArgs args)
        {
            //base.Start(args);
            //this.Time = 0;
            //this.Length = GetTimeInMs(args.Actor);
            //this.ScriptState = ScriptState.Running;
            //this.ArgsSnapshot = args;
            //this.FramingMaterial = args.Actor.GetComponent<GearComponent>().Holding.Object;
            this.FramingMaterial = args.Actor.GetComponent<HaulComponent>().Holding.Object;

           // Success(args);

        }

        //public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        //{
        //    // if moving, interrupt
        //    if (parent.GetComponent<ControlComponent>().RunningScripts.ContainsKey(Script.Types.Walk))
        //    {
        //        Stop(new ScriptArgs(net, parent));
        //        return;
        //    }
        //    this.Time += Engine.Tick;
        //    if (this.Time < this.Length)
        //        return;
        //    this.Success(this.ArgsSnapshot);
            
        //}

        public override void Success(ScriptArgs args)
        {
            base.Success(args);

            GameObject objBlock = GameObject.Create(GameObject.Types.WoodenFrame);
            objBlock.Global = args.Target.FinalGlobal;

            args.Net.Spawn(objBlock);
            //args.Net.PostLocalEvent(objBlock, ObjectEventArgs.Create(Message.Types.Crafted, new object[] { args.Actor, GameObject.Types.WoodenPlank }));
            if (ConstructionFrame.AutoShow)
                args.Net.PostLocalEvent(args.Actor, ObjectEventArgs.Create(Message.Types.Interface, new object[] { new TargetArgs(objBlock) }));
            Finish(args);
        }

        //public override void Finish(ScriptArgs args)
        //{
        //    base.Finish(args);
        //    args.Actor.GetComponent<ActorSpriteComponent>().Body.FadeOut(Graphics.AnimationCollection.Working);
        //}

        public override object Clone()
        {
            return new ScriptFraming();
        }
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
    }
}
