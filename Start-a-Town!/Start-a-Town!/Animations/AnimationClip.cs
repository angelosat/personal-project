using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Animations;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public enum WarpMode { Once, Loop, PingPong, Clamp }
    public enum AnimationStates { Running, Finished, Removed }
    public class AnimationClip
    {
        //public float Weight;//// { get; set; }//MUTABLE

        //public float Fade { get { return this.FadeValue / (float)this.FadeLength; } }
        //public float WeightChange;// { get; set; }//MUTABLE
        //public int FadeLength;// { get; set; }
        //public int FadeValue;// { get; set; }//MUTABLE
        //public float Frame;// { get; set; } //MUTABLE
        //public bool Enabled;// { get; set; }
        //public float Layer;// { get; set; }
        public List<Keyframe> Keyframes = new List<Keyframe>();
        public Func<float, float, float, float> FadeInterpolation = Interpolation.Lerp;
        public bool PreFade;// { get; set; }
        public int Duration; // if duration is negative, the state will remain running instead of finished when it's through
        public int FrameCount;
        //public WarpMode WarpMode;
        //public float Speed = 1f;// { get; set; }
        public string Name;// { get; set; }
        public AnimationBlending Blending;// { get; set; }
        public AnimationStates State;// { get; set; }//MUTABLE
        public WarpMode WarpMode;
     
        public Dictionary<float, Action<GameObject>> Actions = new Dictionary<float, Action<GameObject>>();


        public AnimationClip(WarpMode loop, params Keyframe[] kfs)
            : base()
        {
            this.State = AnimationStates.Running;
            //this.Weight = 1;
            this.Blending = AnimationBlending.Override;
            //this.Layer = 0;
            //this.Frame = 0;
            //this.WeightChange = this.FadeLength = 0;
            //this.Speed = 1f;
            this.WarpMode = loop;
            foreach (Keyframe kf in kfs)
                this.AddFrame(kf);
            this.Duration = FrameCount;
        }

        

        private void PerformActions(float prevframe, float nextframe, GameObject entity)
        {
            //if (this.State == States.Finished)
            //    return;
            foreach(var action in this.Actions)
            {
                if(prevframe < action.Key && action.Key <= nextframe)
                {
                    action.Value(entity);
                }
            }
        }
        public AnimationClip AddEvent(float frame, Action<GameObject> action)
        {
            this.Actions.Add(frame, action);
            return this;
        }

        

        public void AddFrame(Keyframe kf)
        {
            this.Add(kf);
            FrameCount = Math.Max(FrameCount, kf.Time);
        }

        public void Add(Keyframe kf)
        {
            this.Keyframes.Add(kf);
        }
        public Keyframe this[int id]
        {
            get { return this.Keyframes[id]; }
        }
       

        public void Update(Bone bone)
        {
        }

        

        public void GetValue(float frame, float fade, out Vector2 offset, out float angle)
        {
            // if fading in, return a percentage of the first frame
            //var fade = ani.Fade;
            if (fade < 1)
            {
                Keyframe first = this[0];
                var t = this.FadeInterpolation(0, 1, fade);//this.Fade);
                angle = first.Angle * t;
                offset = first.Offset * t;
                return;
            }

            float f;
            if (this.FrameCount == 0)
                f = 0;
            else
                f = this.WarpMode == WarpMode.Loop ? frame % this.FrameCount : frame;

            var count = this.Keyframes.Count;
            for (int i = 0; i < count - 1; i++)
            {
                Keyframe a = this[i], b = this[i + 1];
                if (!(a.Time <= f && f < b.Time))
                    continue;
                float t = 1 - ((b.Time - f) / (float)(b.Time - a.Time));
                angle = b.Interpolation(a.Angle, b.Angle, t);
                offset = new Vector2(b.Interpolation(a.Offset.X, b.Offset.X, t), b.Interpolation(a.Offset.Y, b.Offset.Y, t));
                return;
            }
            // CALL THIS WHEN WEIGHT = 0 INSTEAD?
            //if (this.FinishAction != null)
            //{
            //    FinishAction();
            //    FinishAction = null;
            //}
            angle = this.WarpMode == WarpMode.Loop ? this[0].Angle : this[count - 1].Angle;
            offset = this.WarpMode == WarpMode.Loop ? this[0].Offset : this[count - 1].Offset;
        }


        public int Count { get { return this.Keyframes.Count; } }



        
    }
}
