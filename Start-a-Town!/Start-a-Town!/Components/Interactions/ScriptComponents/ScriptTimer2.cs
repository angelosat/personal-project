using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    public enum TimerState { Stopped, Running }
    public class ScriptTimer2 : ScriptComponent
    {
        public override string ComponentName
        {
            get { return "ScriptTimer2"; }
        }
        public override object Clone()
        {
            return new ScriptTimer2(this.Name, this.Length, this.Callback);
        }

        public TimerState State { get; private set; }
        public string Name { get; private set; }
        public float Length { get; private set; }
        public float Time { get; private set; }
        public Action Callback { get; private set; }

        public ScriptTimer2(float length, Action callback)
        {
            this.Length = this.Time = length;
            this.Callback = callback;
            this.Name = "";
            this.State = TimerState.Stopped;
        }
        public ScriptTimer2(string name, float length, Action callback)
        {
            this.Length = this.Time = length;
            this.Callback = callback;
            this.Name = name;
            this.State = TimerState.Stopped;
        }
        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
        //    base.Update(net, parent, chunk);
        //}
        //public override void Update()
        //{
            if (this.State == TimerState.Stopped)
                return;
            this.Time -= Engine.Tick;
            if (this.Time > 0)
                return;
            this.Callback();
            this.Time = this.Length;
        }

        public ScriptTimer2 Start()
        {
            this.State = TimerState.Running;
            return this;
        }
        public ScriptTimer2 Stop()
        {
            this.State = TimerState.Stopped;
            return this;
        }

        public float Percentage
        {
            get { return (float)(1 - this.Time / this.Length); }
        }

        static public ScriptTimer2 StartNew(float length, Action callback, string name = "")
        {
            return new ScriptTimer2(name, length, callback).Start();
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
    }
}
