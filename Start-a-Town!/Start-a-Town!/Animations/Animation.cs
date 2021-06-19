using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Animations
{
    public sealed class Animation// : Dictionary<BoneDef, Animation>
       // : System.Collections.Generic.IEnumerable<KeyValuePair<BoneDef, Animation>>
    {
        public AnimationDef Def { get; private set; }
        //public Dictionary<float, Action<GameObject>> Events = new Dictionary<float, Action<GameObject>>();

        //public Dictionary<BoneDef, AnimationClip> Inner = new Dictionary<BoneDef, AnimationClip>();
        public GameObject Entity;
        public bool Enabled;
        //{
        //    set
        //    {
        //        foreach (var item in this.Inner.Values)
        //            item.Enabled = value;
        //    }
        //}
        public float Weight = 1;
        //{
        //    set
        //    {
        //        foreach (var item in this.Inner.Values)
        //            item.Weight = value;
        //    }
        //}
        public float WeightChange;
        //{
        //    set
        //    {
        //        foreach (var item in this.Inner.Values)
        //            item.WeightChange = value;
        //    }
        //}
        public float Speed = 1;

        internal Animation SetWeight(int v)
        {
            this.Weight = v;
            return this;
        }

        //{
        //    set
        //    {
        //        foreach (var item in this.Inner.Values)
        //            item.Speed = value;
        //    }
        //}
        public float Frame;

        //public void Foreach(Action<AnimationClip> action)
        //{
        //    foreach (var item in this.Inner.Values)
        //        action(item);
        //}
        public Animation(AnimationDef def)
        {
            this.Def = def;
        }
        //public AnimationClip this[BoneDef i]
        //{
        //    get { return this.Inner[i]; }
        //    set { this.Inner[i] = value; }
        //}

        public float Layer => this.Def.Layer;
        //public bool Loop;
        public float Fade { get { return this.FadeValue / (float)this.FadeLength; } }
        public string Name;
        private bool PreFade;
        private int FadeLength;
        private int FadeValue;
        private Func<float, float, float, float> FadeInterpolation;
        public AnimationStates State;
        public Action FinishAction = () => { };
        public Action OnFadeOut = () => { };
        public Action OnFadeIn = () => { };
        public Animation(GameObject entity, string name, bool loop = false)
            : base()
        {
            this.Entity = entity;
            //this.FrameCount = 0;
            //this.Layer = 1;// 0;
            this.Name = name;
            //this.Loop = loop;
        }

        public override string ToString()
        {
            return string.Format("{0} f: {1} w: {2}", this.Def.Name, this.Frame, this.Weight);
        }

        public void Restart()
        {
            this.Frame = 0;
            this.Weight = 1;
            this.WeightChange = 0;
            this.State = AnimationStates.Running;
            //foreach (var inn in this.Inner)
            //    inn.Value.Frame = 0;
        }

        internal void FadeOutAndRemove()
        {
            this.WeightChange = -0.1f;// true;
            this.State = AnimationStates.Removed;
        }
        internal void FadeOut(float perTick)
        {
            this.WeightChange = -perTick;
            this.State = AnimationStates.Finished;
        }
        internal void FadeOut()
        {
            this.State = AnimationStates.Finished;
            //this.Weight = 1;
            this.WeightChange = -0.1f;
        }
        internal void FadeOut(int seconds)
        {
            float frames = Engine.TicksPerSecond * seconds;
            float dw = 1 / frames;
            this.WeightChange = -dw;// true;
            this.State = AnimationStates.Finished;
        }
        internal void Stop()
        {
            this.State = AnimationStates.Finished;
            this.Weight = 0;
            this.WeightChange = 0;
            this.FadeValue = 0;
            this.FadeLength = 0;
        }
        internal void FadeIn(bool preFade, int fadeLength, Func<float, float, float, float> interpolation)
        {
            this.PreFade = preFade;
            this.FadeLength = fadeLength;
            this.FadeValue = 0;
            this.FadeInterpolation = interpolation;
            this.Weight = 0;
        }
        
        //public void Add(BoneDef type, params Keyframe[] keyframes)
        //{
        //    this.Inner.Add(type, new AnimationClip(WarpMode.Loop, keyframes) {  Name = this.Name, Layer = this.Layer });
        //}

        public void Add(BoneDef type, AnimationClip animation)
        {
            throw new Exception();
            //this.Inner.Add(type, animation);
            //animation.Name = this.Name;
            //animation.Layer = this.Layer;
        }
        public bool TryGetValue(BoneDef type, out AnimationClip ani)
        {
            return this.Def.KeyFrames.TryGetValue(type, out ani);
            //return this.Inner.TryGetValue(type, out ani);
        }

        //internal void Tick()
        //{
        //    var nextlayers = new SortedDictionary<float, List<AnimationClip>>();
        //    List<AnimationClip> nextAnimations;
        //    foreach (var layer in this.Layers)
        //    {
        //        nextAnimations = new List<AnimationClip>();
        //        foreach (var ani in layer.Value)
        //        {
        //            ani.Update(parent);
        //            if (!(ani.State == AnimationStates.Removed && ani.Weight <= 0))
        //                nextAnimations.Add(ani);
        //        }
        //        if (nextAnimations.Any())
        //            nextlayers[layer.Key] = nextAnimations;
        //    }
        //    this.Layers = nextlayers;
        //    foreach (var node in this.Joints)
        //        if (node.Value.Bone != null)
        //            node.Value.Bone.UpdateAnimations(parent);
        //}
        public void Update(GameObject entity)
        {
            if (this.FadeValue < this.FadeLength)
            {
                this.FadeValue++;
                this.Weight = this.FadeInterpolation(0, 1, this.Fade);
                if (this.Fade >= 1)
                    this.OnFadeIn();

                if (this.PreFade)
                    return;
            }
            if (this.Weight > 0)
            {
                var prevframe = this.Frame;
                this.Frame += this.Speed;
                this.PerformActions(prevframe, this.Frame, entity);
                if (this.Frame >= this.Def.FrameCount)
                {
                    switch (this.Def.WarpMode)
                    {
                        case WarpMode.Loop:
                            //this.Frame = 0;
                            this.Frame -= this.Def.FrameCount;
                            break;

                        case WarpMode.Once:
                        case WarpMode.Clamp:
                            this.Frame = this.Def.FrameCount;
                            break;

                        default:
                            break;
                    }
                }
            }
            //if (this.Fade)
            //    Weight -= 0.1f;
            this.Weight += this.Def.WeightChangeFunc?.Invoke(entity) ?? this.WeightChange;

            //this.Weight += this.WeightChange;
            this.Weight = MathHelper.Clamp(this.Weight, 0, 1);
        }
        private void PerformActions(float prevframe, float nextframe, GameObject entity)
        {
            if (this.State == AnimationStates.Removed)
                return;
            foreach (var action in this.Def.Events)
            {
                if (prevframe < action.Key && action.Key <= nextframe)
                {
                    action.Value(entity);
                }
            }

            //foreach(var clip in this.Def.KeyFrames.Values)
            //{
            //    foreach(var kf in clip.Keyframes)
            //    if (prevframe < kf.Key && action.Key <= nextframe)
            //    {
            //        action.Value(entity);
            //    }
            //}
        }

        internal void GetValue(BoneDef boneType, ref Vector2 doff, ref float dang)
        {
            //var clip = this.Def.KeyFrames[boneType];
            if (this.Def.KeyFrames.TryGetValue(boneType, out var clip))
                clip.GetValue(this.Frame, this.Fade, out doff, out dang);
        }
        internal bool TryGetValue(BoneDef boneType, ref Vector2 doff, ref float dang)
        {
            //var clip = this.Def.KeyFrames[boneType];
            if (this.Def.KeyFrames.TryGetValue(boneType, out var clip))
            {
                clip.GetValue(this.Frame, this.Fade, out doff, out dang);
                return true;
            }
            return false;
        }
        //public Animation SetWarpMode(WarpMode mode)
        //{
        //    foreach (var ani in this.Inner)
        //        ani.Value.WarpMode = mode;
        //    return this;
        //}

        //static public AnimationCollection Empty
        //{
        //    get { return new AnimationCollection("Empty"); }
        //}

        //static public AnimationCollection Walking
        //{
        //    get
        //    {
        //        AnimationCollection ani = new AnimationCollection("Walking", true) { Layer = 1 };
        //        //ani.Add("Body",
        //        //    new Keyframe(0, Vector2.Zero, 0),
        //        //    new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
        //        //    new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));

        //        var hipsani = new Animation(WarpMode.Loop, new Keyframe(0, Vector2.Zero, 0),
        //            new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
        //            new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));

        //        ani.Add(BoneDef.Hips, hipsani);
        //            //new Keyframe(0, Vector2.Zero, 0),
        //            //new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
        //            //new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
        //        ani.Add(BoneDef.RightHand,
        //            new Keyframe(0, Vector2.Zero, 0),
        //            new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //        ani.Add(BoneDef.LeftHand,
        //            new Keyframe(0, Vector2.Zero, 0),
        //            new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //        ani.Add(BoneDef.RightFoot,
        //            new Keyframe(0, Vector2.Zero, 0),
        //            new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //        ani.Add(BoneDef.LeftFoot,
        //            new Keyframe(0, Vector2.Zero, 0),
        //            new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //        ani.Add(BoneDef.Head,
        //            new Keyframe(0, Vector2.Zero, 0),
        //            new Keyframe(5, new Vector2(0, 2), 0, Interpolation.Sine),
        //            new Keyframe(10, new Vector2(0, 0), 0, Interpolation.Sine),
        //            new Keyframe(15, new Vector2(0, -2), 0, Interpolation.Sine),
        //            new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
        //        return ani;
        //    }
        //}

        //static public AnimationCollection Idle
        //{
        //    get
        //    {
        //        AnimationCollection ani = new AnimationCollection("Idle", true);
        //        ani.Add(BoneDef.Torso,
        //            //  new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine));
        //          new Keyframe(10, Vector2.UnitY, 0, Interpolation.Sine),
        //          new Keyframe(20, -Vector2.UnitY, 0, Interpolation.Sine)// Animation.Types.Relative));
        //          );
        //        return ani;
        //    }
        //}

        //static public AnimationCollection Jumping
        //{
        //    get
        //    {
        //        AnimationCollection ani = new AnimationCollection("Jumping") { Layer = 2f };//0.5f }; //
        //        ani.Add(BoneDef.RightHand, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
        //        ani.Add(BoneDef.LeftHand, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
        //        ani.Add(BoneDef.RightFoot, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
        //        ani.Add(BoneDef.LeftFoot, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
        //        ani.Add(BoneDef.Torso, new Keyframe(0, Vector2.Zero, 0));
        //        ani.Add(BoneDef.Head, new Keyframe(0, Vector2.Zero, 0));
        //        ani.Speed = 0;
        //        //AnimationCollection ani = new AnimationCollection("Jumping", true) { Layer = 1 };
        //        //ani.Add("Body",
        //        //  new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine)//,
        //        //  //new Keyframe(10, Vector2.UnitY, 0, Interpolation.Sine),
        //        //  //new Keyframe(20, -Vector2.UnitY, 0, Interpolation.Sine)// Animation.Types.Relative));
        //        //  );
        //        return ani;
        //    }
        //}

        //static public AnimationCollection Working
        //{
        //    get
        //    {
        //        AnimationCollection ani = new AnimationCollection("Working", true) { Layer = 2 };
        //        ani.Add(BoneDef.RightHand,
        //            new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
        //            new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),// -(float)Math.PI / 2f, Interpolation.Sine),
        //            new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp)
        //            );
        //        //ani.Add("Left Hand",
        //        //    new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
        //        //    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //        //    new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp)
        //        //    );
        //        ani.Add(BoneDef.Hips,
        //            new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(15, new Vector2(0, -8), 0, Interpolation.Sine),//(float)Math.PI / 4f, Interpolation.Sine),
        //            new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
        //            );
        //        ani.Add(BoneDef.RightFoot,
        //            new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(15, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine),
        //            new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
        //            );
        //        ani.Add(BoneDef.LeftFoot,
        //            new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
        //            new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),
        //            new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
        //            );
        //        return ani;

        //        //AnimationCollection ani = new AnimationCollection("Working", true) { Layer = 2 };
        //        //ani.Add("Right Hand",
        //        //    //new Keyframe(30, Vector2.Zero, -(float)Math.PI, Interpolation.Sine),
        //        //    //new Keyframe(50, Vector2.Zero, 0, Interpolation.Exp)
        //        //    new Keyframe(0, Vector2.Zero, 0),
        //        //    new Keyframe(15, Vector2.Zero, -(float)Math.PI, Interpolation.Sine),
        //        //    new Keyframe(25, Vector2.Zero, 0, Interpolation.Exp)// Animation.Types.Relative)
        //        //    //new Keyframe(50, Vector2.Zero, (float)Math.PI, Interpolation.Exp)// Animation.Types.Relative)
        //        //    );
        //        //return ani;
        //    }
        //}

        static public Animation Block
        {
            get
            {
                throw new Exception();
                //AnimationCollection ani = new AnimationCollection("Block") { Layer = 2 };
                //ani.Add(BoneDef.LeftHand,
                //    new Animation(WarpMode.Clamp,
                //        new Keyframe(0, Vector2.Zero, 0),
                //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
                //        ));
                //ani[BoneDef.LeftHand].Duration = -1;
                //return ani;
            }
        }

        static public Animation RaiseRHand(GameObject entity)
        {
            throw new Exception();

            //Animation ani = new Animation(entity, "RaiseRHand") { Layer = 3 };
            //ani.Add(BoneDef.RightHand,
            //    new AnimationClip(WarpMode.Clamp,
            //        //new Keyframe(0, Vector2.Zero, 0),
            //        new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
            //        //new Keyframe(80, Vector2.Zero, -4 * (float)Math.PI / 3f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
            //        ));
            //ani[BoneDef.RightHand].Duration = -1;

            //// TODO: blend body leaning with up&down from running using weights
            //ani.Add(BoneDef.Torso,
            //    new AnimationClip(WarpMode.Clamp,
            //        //new Keyframe(0, Vector2.Zero, 0),
            //        //new Keyframe(80, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Sine)
            //        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Sine)
            //        ));



            //return ani;
        }
        static public Animation DeliverAttack(GameObject entity)
        {
            Animation ani = new Animation(entity, "DropRHand");// { Layer = 4};//3 };//DropRHand");
                ani.Add(BoneDef.RightHand,
                    new AnimationClip(WarpMode.Once,
                    //new Keyframe(0, Vector2.Zero, -(float)Math.PI),
                    //new Keyframe(100, Vector2.Zero, 0, Interpolation.Exp)

                        new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
                        //new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),
                        //new Keyframe(20, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                         new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp)
                    ));
                ani.Add(BoneDef.Mainhand,//Mainhand,
                    new AnimationClip(WarpMode.Once,
                        new Keyframe(0, Vector2.Zero, 0),
                        new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                        new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp)
                       // new Keyframe(10, Vector2.Zero, 0, Interpolation.Exp)
                    ));
                ani.Add(BoneDef.Torso,
                    new AnimationClip(WarpMode.Clamp,
                        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f),
                        new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f),
                        new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f)

                   //     new Keyframe(10, Vector2.Zero, 0)
                        ));
                return ani;
        }

        //static public AnimationCollection ManipulateItem
        //{
        //    get
        //    {
        //        AnimationCollection ani = new AnimationCollection("ManipulateItem") { Layer = 2 };//DropRHand");
        //        ani.Add(BoneDef.RightHand,
        //            new Animation(WarpMode.Once,
        //                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
        //            ));
        //        ani.Add(BoneDef.LeftHand,
        //            new Animation(WarpMode.Once,
        //                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
        //            ));
        //        //ani.Add("Right Hand",
        //        //    new Animation(WarpMode.Once,
        //        //        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),// Vector2.Zero, -4 * (float)Math.PI / 3f),
        //        //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
        //        //    ));
        //        //ani.Add("Left Hand",
        //        //    new Animation(WarpMode.Once,
        //        //        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),//Vector2.Zero, -4 * (float)Math.PI / 3f),
        //        //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
        //        //    ));
        //        return ani;
        //    }
        //}

        //static public Animation Hauling(GameObject entity)
        //{
        //        Animation ani = new Animation(entity, "Hauling") { Layer = 3 };
        //        ani.Add(BoneDef.RightHand, new AnimationClip(WarpMode.Once,
        //            new Keyframe(0, Vector2.Zero, -(float)Math.PI)
        //            ));
        //        ani.Add(BoneDef.LeftHand, new AnimationClip(WarpMode.Once,
        //            new Keyframe(0, Vector2.Zero, -(float)Math.PI)
        //            ));
        //        ani.Add(BoneDef.Torso, new AnimationClip(WarpMode.Once,
        //            new Keyframe(0, Vector2.Zero, 0)
        //            ));
        //        return ani;

        //        //ani.Add(BoneDef.RightHand, new Animation(WarpMode.Once,
        //        //    new Keyframe(0, Vector2.Zero, 0),
        //        //    new Keyframe(10, Vector2.Zero, -(float)Math.PI)
        //        //    ));
        //        //ani.Add(BoneDef.LeftHand, new Animation(WarpMode.Once,
        //        //    new Keyframe(0, Vector2.Zero, 0),
        //        //    new Keyframe(10, Vector2.Zero, -(float)Math.PI)
        //        //    ));
        //        //ani.Add(BoneDef.Torso, new Animation(WarpMode.Once,
        //        //    new Keyframe(0, Vector2.Zero, 0),
        //        //    new Keyframe(10, Vector2.Zero, 0)
        //        //    ));
        //        //return ani;
        //}

        //static public AnimationCollection DropHands
        //{
        //    get
        //    {
        //        AnimationCollection ani = new AnimationCollection("DropHands") { Layer = 2 };
        //        ani.Add(BoneDef.RightHand,
        //            new Animation(WarpMode.Clamp,
        //                new Keyframe(10, Vector2.Zero, 0)
        //                ));
        //        ani.Add(BoneDef.LeftHand,
        //            new Animation(WarpMode.Clamp,
        //                new Keyframe(10, Vector2.Zero, 0)
        //                ));
        //        return ani;
        //    }
        //}

        public void ExportToXml()
        {
            var doc = new XDocument();
            var root = new XElement("Animation");
            //foreach (var value in this.Inner)
            //{
            //    var bone =
            //       new XElement("Bone", new XAttribute("Type", value.Key.ToString()), new XAttribute("WarpMode", value.Value.WarpMode));//,
            //    foreach (var ani in value.Value.Keyframes) // write keyframes
            //    {
            //        bone.Add(
            //            new XElement("KeyFrame",
            //                new XElement("Time", ani.Time.ToString()),
            //                new XElement("Offset", ani.Offset.ToString()),
            //                new XElement("Angle", Math.Round(ani.Angle / Math.PI, 3).ToString())
            //                ));
            //    }
            //    root.Add(bone);

            //}
            doc.Add(root);

            doc.Save(this.Name + ".xml");
        }

        static public void Export()
        {
            //var list = new List<AnimationCollection>() { new Animations.AnimationDeliverAttack() };
            //foreach (var item in list)
            //    item.ExportToXml();
        }

        internal SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.Frame.Save("Frame"));
            tag.Add(this.FadeValue.Save("FadeValue"));
            tag.Add(this.FadeLength.Save("FadeLength"));
            tag.Add(this.Weight.Save("Weight"));
            tag.Add(this.WeightChange.Save("WeightChange"));
            tag.Add(((int)this.State).Save("State"));

            return tag;

            //var tag = new SaveTag(SaveTag.Types.Compound, name);
            //var animationsTags = new SaveTag(SaveTag.Types.List, "Animations", SaveTag.Types.Compound);
            //foreach(var ani in this.Inner)
            //{
            //    var anitag = new SaveTag(SaveTag.Types.Compound);
            //    anitag.Add(new SaveTag(SaveTag.Types.Int, "BoneType", (int)ani.Key));
            //    anitag.Add(ani.Value.Save("Animation"));
            //    animationsTags.Add(anitag);
            //}
            //tag.Add(animationsTags);
            //return tag;
        }
        public void Load(SaveTag tag)
        {
            tag.TryGetTagValue<string>("Def", t => Start_a_Town_.Def.GetDef<AnimationDef>(t));
            tag.TryGetTagValue("Frame", out this.Frame);
            tag.TryGetTagValue("FadeValue", out this.FadeValue);
            tag.TryGetTagValue("FadeLength", out this.FadeLength);
            tag.TryGetTagValue("Weight", out this.Weight);
            tag.TryGetTagValue("WeightChange", out this.WeightChange);
            tag.TryGetTagValue<int>("State", t => this.State = (AnimationStates)t);

            //tag.TryGetTag("Animations", v =>
            //    {
            //        var anilist = v.Value as List<SaveTag>;
            //        foreach (var aniTag in anilist)
            //        {
            //            BoneDef t = (BoneDef)(int)aniTag["BoneType"].Value;
            //            if(!this.Inner.ContainsKey(t))
            //            {
            //                string.Format("{0} missing", t).ToConsole();
            //                continue;
            //            }
            //            this.Inner[t].Load(aniTag["Animation"]);
            //        }
            //    });
        }

        //public void Write(BinaryWriter w)
        //{
        //    foreach(var ani in this.Inner)
        //        ani.Value.Write(w);
        //}
        //public void Read(BinaryReader r)
        //{
        //    foreach (var ani in this.Inner)
        //        ani.Value.Read(r);
        //}
        internal void Write(System.IO.BinaryWriter w)
        {
            //w.Write(this.Enabled);
            w.Write(this.Def.Name);
            w.Write(this.Frame);
            w.Write(this.FadeLength);
            w.Write(this.FadeValue);
            w.Write(this.Weight);
            w.Write(this.WeightChange);
            w.Write((int)this.State);
        }
        internal void Read(System.IO.BinaryReader r)
        {
            //this.Enabled = r.ReadBoolean();
            this.Def = Start_a_Town_.Def.GetDef<AnimationDef>(r.ReadString());
            this.Frame = r.ReadSingle();
            this.FadeLength = r.ReadInt32();
            this.FadeValue = r.ReadInt32();
            this.Weight = r.ReadSingle();
            this.WeightChange = r.ReadSingle();
            this.State = (AnimationStates)r.ReadInt32();
        }
        
    }
}
