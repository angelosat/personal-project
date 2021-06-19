using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Animations;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics
{
    public enum WarpMode { Once, Loop, PingPong, Clamp }
    public class Animation
    {
        public enum States { Running, Finished }
      
        public float Weight;//// { get; set; }//MUTABLE
        public List<Keyframe> Keyframes = new List<Keyframe>();
        public Func<float, float, float, float> FadeInterpolation = Interpolation.Lerp;
        public bool PreFade;// { get; set; }
        public float Fade { get { return this.FadeValue / (float)this.FadeLength; } }
        public float WeightChange;// { get; set; }//MUTABLE
        public int FadeLength;// { get; set; }
        public int FadeValue;// { get; set; }//MUTABLE
        public float Frame;// { get; set; } //MUTABLE
        public bool Enabled;// { get; set; }
        public float Layer;// { get; set; }
        public int Duration; // if duration is negative, the state will remain running instead of finished when it's through
        public int FrameCount;
        public WarpMode WarpMode;
        public float Speed;// { get; set; }
        public string Name;// { get; set; }
        public AnimationBlending Blending;// { get; set; }
        public States State;// { get; set; }//MUTABLE
        public Action FinishAction = () => { };
        public Action OnFadeOut = () => { };
        public Action OnFadeIn = () => { };

        public Dictionary<float, Action> Actions = new Dictionary<float, Action>();

        float UpdateInterval = 1, UpdateLeftOver = 0;

        public float Percentage
        {
            get
            {
                return MathHelper.Clamp(this.Frame / (float)this.FrameCount, 0, 1);
            }
        }

        public Animation(WarpMode loop, params Keyframe[] kfs)
            : base()
        {
            this.State = States.Running;
            //this.WeightFunc = () => this.WeightValue;
            this.Weight = 1;
            this.Enabled = true;
            this.Blending = AnimationBlending.Override;
            this.Layer = 0;
            this.Frame = 0;
            this.WeightChange = this.FadeLength = 0;
        //    this.Speed = 1;
            this.Speed = 1f;
            this.WarpMode = loop;
            foreach (Keyframe kf in kfs)
                this.AddFrame(kf);
            this.Duration = FrameCount;
        }

        public void Update()
        {
            if (!Enabled)
                return;

                if (this.FadeValue < this.FadeLength)
                {
                    this.FadeValue++;
                    this.Weight = this.FadeInterpolation(0, 1, this.Fade);
                    if (this.Fade >= 1)
                        this.OnFadeIn();

                    if (this.PreFade)
                        return;
                }
                var prevframe = this.Frame;
                this.Frame += this.Speed;
                this.PerformActions(prevframe, this.Frame);
                if (this.Frame >= this.FrameCount)
                {
                    switch (this.WarpMode)
                    {
                        case Graphics.WarpMode.Loop:
                            //this.Frame = 0;
                            this.Frame -= this.FrameCount;
                            break;

                        case Graphics.WarpMode.Once:
                        case Graphics.WarpMode.Clamp:
                            this.Frame = this.FrameCount;
                            break;

                        default:
                            break;
                    }
                }

                //if (this.Fade)
                //    Weight -= 0.1f;
                this.Weight += this.WeightChange;
        }

        private void PerformActions(float prevframe, float nextframe)
        {
            foreach(var action in this.Actions)
            {
                if(prevframe < action.Key && action.Key <= nextframe)
                {
                    action.Value();
                }
            }
        }
        public void AddAction(float frame, Action action)
        {
            this.Actions.Add(frame, action);
        }

        public void UpdateFixed()
        {
            if (!Enabled)
                return;
            float iterations = UpdateLeftOver + this.Speed;
            while (iterations >= UpdateInterval)
            {
                iterations--;
                if (this.FadeValue < this.FadeLength)
                {
                    this.FadeValue++;
                    this.Weight = this.FadeInterpolation(0, 1, this.Fade);
                    if (this.Fade >= 1)
                        this.OnFadeIn();
                    //this.FadeValue = this.FadeInterpolation(0, this.FadeLength, this.fa)
                    if (this.PreFade)
                        continue;
                }
                this.Frame += 1;
                if (this.Frame >= this.FrameCount)
                {
                    switch (this.WarpMode)
                    {
                        case Graphics.WarpMode.Loop:
                            //this.Frame = 0;
                            this.Frame -= this.FrameCount;
                            break;

                        case Graphics.WarpMode.Once:
                        case Graphics.WarpMode.Clamp:
                            this.Frame = this.FrameCount;
                            break;

                        default:
                            break;
                    }
                }
                
                //if (this.Fade)
                //    Weight -= 0.1f;
                this.Weight += this.WeightChange;
            }
            UpdateLeftOver = iterations;
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
        //public override string ToString()
        //{
        //    string text = Name + ": ";
        //    foreach (Keyframe kf in this)
        //    {
        //        text += kf.ToString() + "\n";
        //    }
        //    return text.TrimEnd('\n'); ;
        //}

        public override string ToString()
        {
            string text = Name + " t:" + this.Frame + " w:" + Weight
                + " f: " + (this.FadeLength > 0 ? this.Fade : (float)1);
            return text;
        }

        public void Update(Bone bone)
        {
        }

        static public void Start(GameObject obj, AnimationCollection animation, float speed = 1, Func<Bone, bool> condition = null, Dictionary<Bone.Types, Action> onFinish = null)
        {
            //Start(obj, animation, speed, condition ?? (b => true), onFinish ?? new Dictionary<string, Action>());
            SpriteComponent sprite;
            if (!obj.TryGetComponent<SpriteComponent>("Sprite", out sprite))
                return;
            Start(sprite.Body, animation, speed, condition ?? (b => true), onFinish ?? new Dictionary<Bone.Types, Action>());
        }
        /// <summary>
        /// TODO: Make fadelength somehow correlated to keyframes or animation length
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="animation"></param>
        /// <param name="fadeLength"></param>
        /// <param name="speed"></param>
        /// <param name="condition"></param>
        /// <param name="onFinish"></param>
        static public void CrossFade(GameObject obj, AnimationCollection animation, float fadeLength = 0.1f, float speed = 1, Func<Bone, bool> condition = null, Dictionary<Bone.Types, Action> onFinish = null)
        {
            CrossFade(obj.Body, animation, fadeLength, speed, condition, onFinish);
        }
        static public void CrossFade(Bone root, AnimationCollection animation, float fadeLength = 0.1f, float speed = 1, Func<Bone, bool> condition = null, Dictionary<Bone.Types, Action> onFinish = null)
        {
            root.FadeOut(animation.Layer, fadeLength);
            foreach (var ani in animation.Inner.Values)
            {
                ani.Weight = 0;
                ani.WeightChange = fadeLength;
            }
            Start(root, animation, speed, condition ?? (b => true), onFinish ?? new Dictionary<Bone.Types, Action>());
        }
        static public void Start(Bone body, AnimationCollection animation, float speed = 1)
        {
            Start(body, animation, speed, bone => true, new Dictionary<Bone.Types, Action>());
        }

        static public void Start(Bone body, AnimationCollection animation, float speed = 1, Func<Bone, bool> condition = null, Dictionary<Bone.Types, Action> onFinish = null)
        {
            condition = condition ?? (b => true);
            onFinish = onFinish ?? new Dictionary<Bone.Types, Action>();
            Queue<Bone> toHandle = new Queue<Bone>();
            toHandle.Enqueue(body);
            while (toHandle.Count > 0)
            {
                Bone bone = toHandle.Dequeue();
                Animation ani;
                if (animation.TryGetValue(bone.Type, out ani))
                {
                    if (condition(bone))//bone.Animation.Name == "Idle")
                    {
                        //ani[0] = new Keyframe(0, bone.Offset, bone.Angle);
                    
                        List<Animation> list;
                        if (!bone.Layers.TryGetValue(ani.Layer, out list))
                        {
                            list = new List<Animation>();
                            bone.Layers[ani.Layer] = list;
                        }
                        list.Add(ani);

                        //bone.Animation.Speed = speed;
                        ani.Speed = speed;
                        bone.State = Bone.States.Stopped;
                        //bone.FinishAction = onFinish.GetValueOrDefault(bone.Name);
                        ani.FinishAction = onFinish.GetValueOrDefault(bone.Type);
                        bone.Restart(false);
                    }
                }
                //foreach (var sub in bone.Children)
                //    toHandle.Enqueue(sub.Value);
                foreach (var joint in bone.Joints.Values)
                    if (joint.Bone != null)
                        toHandle.Enqueue(joint.Bone);
            }
        }

        public void GetValue(out Vector2 offset, out float angle)
        {
            // if fading in, return a percentage of the first frame
            if (this.FadeValue < this.FadeLength)
            {
                Keyframe first = this[0];
                var t = this.FadeInterpolation(0, 1, this.Fade);
                angle = first.Angle * t;
                offset = first.Offset * t;
                return;
            }

            float f;
            if (this.FrameCount == 0)
                f = 0;
            else
                f = this.WarpMode == WarpMode.Loop ? this.Frame % this.FrameCount : this.Frame;

            for (int i = 0; i < this.Count - 1; i++)
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
            if (this.FinishAction != null)
            {
                FinishAction();
                FinishAction = null;
            }
            angle = this.WarpMode == WarpMode.Loop ? this[0].Angle : this[this.Count - 1].Angle;
            offset = this.WarpMode == WarpMode.Loop ? this[0].Offset : this[this.Count - 1].Offset;
        }


        internal void Stop()
        {
            this.State = States.Finished;
            this.Weight = 0;
            this.WeightChange = 0;
            this.FadeValue = 0;
            this.FadeLength = 0;
        }
        internal void FadeIn(bool preFade, int fadeLength, Func<float, float, float, float> interpolation)
        {
            this.PreFade = preFade;
            //this.Weight = 0;
            //this.WeightChange = 0.1f;
            this.FadeLength = fadeLength;
            this.FadeValue = 0;
            this.FadeInterpolation = interpolation;
            this.Weight = 0;
        }
        internal void FadeOut()
        {
            this.State = States.Finished;
            this.Weight = 1;
            this.WeightChange = -0.1f;
        }

        public int Count { get { return this.Keyframes.Count; } }
    }
}
