using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics
{
    public class AnimationCollection// : Dictionary<Bone.Types, Animation>
       // : System.Collections.Generic.IEnumerable<KeyValuePair<Bone.Types, Animation>>
    {
        public Dictionary<Bone.Types, Animation> Inner = new Dictionary<Bone.Types, Animation>();

        public float Weight
        {
            set
            {
                foreach (var item in this.Inner.Values)
                    item.Weight = value;
            }
        }
        public float Speed
        {
            set
            {
                foreach (var item in this.Inner.Values)
                    item.Speed = value;
            }
        }
        public void Foreach(Action<Animation> action)
        {
            foreach (var item in this.Inner.Values)
                action(item);
        }

        public Animation this[Bone.Types i]
        {
            get { return this.Inner[i]; }
            set { this.Inner[i] = value; }
        }
        //public System.Collections.Generic.IEnumerator<KeyValuePair<Bone.Types, Animation>> GetEnumerator()
        //{
        //    return this.Inner.GetEnumerator();
        //}
        public float Layer;// { get; set; }
        //public bool Loop;
        public string Name;
        //public int FrameCount;
        //protected AnimationCollection()
        //{

        //}

        public AnimationCollection(string name, bool loop = false)
            : base()
        {
            //this.FrameCount = 0;
            this.Layer = 1;// 0;
            this.Name = name;
            //this.Loop = loop;
        }

        public override string ToString()
        {
            return this.Name;
        }

        //public void Add(string name, params Keyframe[] keyframes)
        //{
        //    this.Add(name, new KeyframeCollection(keyframes));
        //}
        public void Add(Bone.Types type, params Keyframe[] keyframes)
        {
            this.Inner.Add(type, new Animation(WarpMode.Loop, keyframes) { Name = this.Name, Layer = this.Layer });
        }
        public void Add(Bone.Types type, Animation animation)
        {
            this.Inner.Add(type, animation);
            animation.Name = this.Name;
            animation.Layer = this.Layer;
        }
        public bool TryGetValue(Bone.Types type, out Animation ani)
        {
            return this.Inner.TryGetValue(type, out ani);
        }

        public AnimationCollection SetWarpMode(WarpMode mode)
        {
            foreach (var ani in this.Inner)
                ani.Value.WarpMode = mode;
            return this;
        }

        static public AnimationCollection Empty
        {
            get { return new AnimationCollection("Empty"); }
        }

        static public AnimationCollection Walking
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("Walking", true) { Layer = 1 };
                //ani.Add("Body",
                //    new Keyframe(0, Vector2.Zero, 0),
                //    new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
                //    new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));

                var hipsani = new Animation(WarpMode.Loop, new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
                    new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));

                ani.Add(Bone.Types.Hips, hipsani);
                    //new Keyframe(0, Vector2.Zero, 0),
                    //new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
                    //new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
                ani.Add(Bone.Types.RightHand,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
                ani.Add(Bone.Types.LeftHand,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
                ani.Add(Bone.Types.RightFoot,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
                ani.Add(Bone.Types.LeftFoot,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
                ani.Add(Bone.Types.Head,
                    new Keyframe(0, Vector2.Zero, 0),
                    new Keyframe(5, new Vector2(0, 2), 0, Interpolation.Sine),
                    new Keyframe(10, new Vector2(0, 0), 0, Interpolation.Sine),
                    new Keyframe(15, new Vector2(0, -2), 0, Interpolation.Sine),
                    new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
                return ani;
            }
        }

        static public AnimationCollection Idle
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("Idle", true);
                ani.Add(Bone.Types.Torso,
                    //  new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine));
                  new Keyframe(10, Vector2.UnitY, 0, Interpolation.Sine),
                  new Keyframe(20, -Vector2.UnitY, 0, Interpolation.Sine)// Animation.Types.Relative));
                  );
                return ani;
            }
        }

        static public AnimationCollection Jumping
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("Jumping") { Layer = 2f };//0.5f }; //
                ani.Add(Bone.Types.RightHand, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
                ani.Add(Bone.Types.LeftHand, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
                ani.Add(Bone.Types.RightFoot, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
                ani.Add(Bone.Types.LeftFoot, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
                ani.Add(Bone.Types.Torso, new Keyframe(0, Vector2.Zero, 0));
                ani.Add(Bone.Types.Head, new Keyframe(0, Vector2.Zero, 0));
                ani.Speed = 0;
                //AnimationCollection ani = new AnimationCollection("Jumping", true) { Layer = 1 };
                //ani.Add("Body",
                //  new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine)//,
                //  //new Keyframe(10, Vector2.UnitY, 0, Interpolation.Sine),
                //  //new Keyframe(20, -Vector2.UnitY, 0, Interpolation.Sine)// Animation.Types.Relative));
                //  );
                return ani;
            }
        }

        static public AnimationCollection Working
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("Working", true) { Layer = 2 };
                ani.Add(Bone.Types.RightHand,
                    new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
                    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),// -(float)Math.PI / 2f, Interpolation.Sine),
                    new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp)
                    );
                //ani.Add("Left Hand",
                //    new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
                //    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                //    new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp)
                //    );
                ani.Add(Bone.Types.Hips,
                    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(15, new Vector2(0, -8), 0, Interpolation.Sine),//(float)Math.PI / 4f, Interpolation.Sine),
                    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
                    );
                ani.Add(Bone.Types.RightFoot,
                    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(15, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine),
                    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
                    );
                ani.Add(Bone.Types.LeftFoot,
                    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
                    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),
                    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
                    );
                return ani;

                //AnimationCollection ani = new AnimationCollection("Working", true) { Layer = 2 };
                //ani.Add("Right Hand",
                //    //new Keyframe(30, Vector2.Zero, -(float)Math.PI, Interpolation.Sine),
                //    //new Keyframe(50, Vector2.Zero, 0, Interpolation.Exp)
                //    new Keyframe(0, Vector2.Zero, 0),
                //    new Keyframe(15, Vector2.Zero, -(float)Math.PI, Interpolation.Sine),
                //    new Keyframe(25, Vector2.Zero, 0, Interpolation.Exp)// Animation.Types.Relative)
                //    //new Keyframe(50, Vector2.Zero, (float)Math.PI, Interpolation.Exp)// Animation.Types.Relative)
                //    );
                //return ani;
            }
        }

        static public AnimationCollection Block
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("Block") { Layer = 2 };
                ani.Add(Bone.Types.LeftHand,
                    new Animation(WarpMode.Clamp,
                        new Keyframe(0, Vector2.Zero, 0),
                        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
                        ));
                ani[Bone.Types.LeftHand].Duration = -1;
                return ani;
            }
        }

        static public AnimationCollection RaiseRHand
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("RaiseRHand") { Layer = 3 };
                ani.Add(Bone.Types.RightHand,
                    new Animation(WarpMode.Clamp,
                        //new Keyframe(0, Vector2.Zero, 0),
                        new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
                        //new Keyframe(80, Vector2.Zero, -4 * (float)Math.PI / 3f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
                        ));
                ani[Bone.Types.RightHand].Duration = -1;

                // TODO: blend body leaning with up&down from running using weights
                ani.Add(Bone.Types.Torso,
                    new Animation(WarpMode.Clamp,
                        //new Keyframe(0, Vector2.Zero, 0),
                        //new Keyframe(80, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Sine)
                        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Sine)
                        ));


                //ani.Add("Right Foot",
                //    new Animation(WarpMode.Clamp,
                //        new Keyframe(0, Vector2.Zero, 0),
                //        new Keyframe(80, Vector2.Zero, (float)Math.PI / 8f, Interpolation.Sine)
                //        ));
                //ani.Add("Left Foot",
                //    new Animation(WarpMode.Clamp,
                //        new Keyframe(0, Vector2.Zero, 0),
                //        new Keyframe(80, Vector2.Zero, (float)Math.PI / 8f, Interpolation.Sine)
                //        ));
                ///

                //ani.Add("Right Hand",
                //    new Keyframe(0, Vector2.Zero, 0),
                //    //new Keyframe(20, Vector2.Zero, -(float)Math.PI, Interpolation.Sine)
                //    new Keyframe(80, Vector2.Zero, -4*(float)Math.PI/3f, (a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t)))
                //    );
                //ani["Right Hand"].Duration = -1;

                ///

                //ani.Add("Body", ani,
                //    new Keyframe(20, Vector2.Zero, -(float)Math.PI/4f, Interpolation.Sine)
                //    );
                //ani["Body"].Duration = -1;

                return ani;
            }
        }
        static public AnimationCollection DeliverAttack
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("DropRHand") { Layer = 4};//3 };//DropRHand");
                ani.Add(Bone.Types.RightHand,
                    new Animation(WarpMode.Once,
                    //new Keyframe(0, Vector2.Zero, -(float)Math.PI),
                    //new Keyframe(100, Vector2.Zero, 0, Interpolation.Exp)

                        new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f),
                        //new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),
                        //new Keyframe(20, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                         new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),
                        new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp)
                    ));
                ani.Add(Bone.Types.Mainhand,//Mainhand,
                    new Animation(WarpMode.Once,
                        new Keyframe(0, Vector2.Zero, 0),
                        new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),
                        new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp)
                       // new Keyframe(10, Vector2.Zero, 0, Interpolation.Exp)
                    ));
                ani.Add(Bone.Types.Torso,
                    new Animation(WarpMode.Clamp,
                        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f),
                        new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f),
                        new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f)

                   //     new Keyframe(10, Vector2.Zero, 0)
                        ));
                return ani;
            }
        }

        static public AnimationCollection ManipulateItem
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("ManipulateItem") { Layer = 2 };//DropRHand");
                ani.Add(Bone.Types.RightHand,
                    new Animation(WarpMode.Once,
                        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                    ));
                ani.Add(Bone.Types.LeftHand,
                    new Animation(WarpMode.Once,
                        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                    ));
                //ani.Add("Right Hand",
                //    new Animation(WarpMode.Once,
                //        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),// Vector2.Zero, -4 * (float)Math.PI / 3f),
                //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                //    ));
                //ani.Add("Left Hand",
                //    new Animation(WarpMode.Once,
                //        new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),//Vector2.Zero, -4 * (float)Math.PI / 3f),
                //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp)
                //    ));
                return ani;
            }
        }

        static public AnimationCollection Hauling
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("Hauling") { Layer = 3 };
                ani.Add(Bone.Types.RightHand, new Animation(WarpMode.Once,
                    new Keyframe(0, Vector2.Zero, -(float)Math.PI)
                    ));
                ani.Add(Bone.Types.LeftHand, new Animation(WarpMode.Once,
                    new Keyframe(0, Vector2.Zero, -(float)Math.PI)
                    ));
                ani.Add(Bone.Types.Torso, new Animation(WarpMode.Once,
                    new Keyframe(0, Vector2.Zero, 0)
                    ));
                return ani;

                //ani.Add(Bone.Types.RightHand, new Animation(WarpMode.Once,
                //    new Keyframe(0, Vector2.Zero, 0),
                //    new Keyframe(10, Vector2.Zero, -(float)Math.PI)
                //    ));
                //ani.Add(Bone.Types.LeftHand, new Animation(WarpMode.Once,
                //    new Keyframe(0, Vector2.Zero, 0),
                //    new Keyframe(10, Vector2.Zero, -(float)Math.PI)
                //    ));
                //ani.Add(Bone.Types.Torso, new Animation(WarpMode.Once,
                //    new Keyframe(0, Vector2.Zero, 0),
                //    new Keyframe(10, Vector2.Zero, 0)
                //    ));
                //return ani;
            }
        }

        static public AnimationCollection DropHands
        {
            get
            {
                AnimationCollection ani = new AnimationCollection("DropHands") { Layer = 2 };
                ani.Add(Bone.Types.RightHand,
                    new Animation(WarpMode.Clamp,
                        new Keyframe(10, Vector2.Zero, 0)
                        ));
                ani.Add(Bone.Types.LeftHand,
                    new Animation(WarpMode.Clamp,
                        new Keyframe(10, Vector2.Zero, 0)
                        ));
                return ani;
            }
        }

        public void ExportToXml()
        {
            var doc = new XDocument();
            var root = new XElement("Animation");
            foreach (var value in this.Inner)
            {
                //var element = new XElement("Animation", new XAttribute("Type", value.Key.ToString()));
                //var element = 
                //    new XElement("Type", new XAttribute("WarpMode", value.Value.WarpMode),
                //        value.Key.ToString());
                var bone =
                   new XElement("Bone", new XAttribute("Type", value.Key.ToString()), new XAttribute("WarpMode", value.Value.WarpMode));//,
                       //value.Key.ToString());
                //var keyframes = new XElement("KeyFrames");
                foreach (var ani in value.Value.Keyframes) // write keyframes
                {
                    bone.Add(
                        new XElement("KeyFrame",
                            new XElement("Time", ani.Time.ToString()),
                            new XElement("Offset", ani.Offset.ToString()),
                            new XElement("Angle", Math.Round(ani.Angle / Math.PI, 3).ToString())
                            ));
                }
                //bone.Add(keyframes);
                root.Add(bone);

            }
            doc.Add(root);

            doc.Save(this.Name + ".xml");
        }

        static public void Export()
        {
            var list = new List<AnimationCollection>() { new Animations.AnimationDeliverAttack() };
            foreach (var item in list)
                item.ExportToXml();
        }
    }
}
