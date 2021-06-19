using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    public class ScriptTimer : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptTimer"; }
        }
        public override object Clone()
        {
            return new ScriptTimer(this.GetLength, this.Name, this.Length, this.Callback);
        }

        public TimerState State { get; private set; }
        public string Name { get; private set; }
        public float Length { get; private set; }
        public float Time { get; private set; }
        public Action Callback { get; private set; }
        public Func<ScriptArgs, float> GetLength { get; set; }

        public ScriptTimer(float length, Action callback)
        {
            this.Length = this.Time = length * 1000;
            this.Callback = callback;
            this.Name = "";
            this.State = TimerState.Stopped;
            this.GetLength = (a) => this.Length;
        }
        public ScriptTimer(string name, float seconds) : this(name, seconds, () => { }) { }
        public ScriptTimer(string name, float seconds, Action callback)
        {
            this.Length = this.Time = seconds * 1000;
            this.Callback = callback;
            this.Name = name;
            this.State = TimerState.Stopped;
            this.GetLength = (a) => this.Length;
        }
        public ScriptTimer(Func<ScriptArgs, float> getLength, string name, float seconds) : this(getLength, name, seconds, () => { }) { }
        public ScriptTimer(Func<ScriptArgs, float> getLength, string name, float seconds, Action callback)
        {
            this.Length = this.Time = seconds * 1000;
            this.Callback = callback;
            this.Name = name;
            this.State = TimerState.Stopped;
            this.GetLength = (a) => this.Length;//= getLength;
        }
        public ScriptTimer(Func<ScriptArgs, float> getLength, string name, Action callback)
        {
            this.Callback = callback;
            this.Name = name;
            this.State = TimerState.Stopped;
            this.GetLength = getLength;
        }

        //public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        //{
        //    if (this.State == TimerState.Stopped)
        //        return;
        //    this.Time -= Engine.Tick;
        //    if (this.Time > 0)
        //        return;
        //    this.Callback();
        //    this.Time = this.Length;
        //}
        public override ScriptState Update(ScriptArgs args)
        {
            if (this.State == TimerState.Stopped)
                return ScriptState.Unstarted;
            this.Time -= Engine.Tick;
            if (this.Time > 0)
                return ScriptState.Running;
            this.Callback();
            this.Time = this.Length;
            return ScriptState.Finished;
        }
        public ScriptTimer Start()
        {
            this.State = TimerState.Running;
            return this;
        }
        public ScriptTimer Stop()
        {
            this.State = TimerState.Stopped;
            return this;
        }

        public float Percentage
        {
            get { return (float)(1 - this.Time / this.Length); }
        }

        static public ScriptTimer StartNew(float length, Action callback, string name = "")
        {
            return new ScriptTimer(name, length, callback).Start();
        }

        public override void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            Vector3 global = parent.Global;

            Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.Percentage);
            UIManager.DrawStringOutlined(sb, this.Name, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }

        public override void Start(ScriptArgs args)
        {
            this.Length = this.Time = this.GetLength(args);// *1000;
            this.Start();
        }
    }
}
