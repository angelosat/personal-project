using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics.Animations
{
    class AnimationTool : AnimationCollection
    {
        public Action Contact = () => { };

        public AnimationTool()
            : this(() => { })
        {

        }
        public AnimationTool(Action contact)
            : base("WorkingNew")
        {
            this.Contact = contact;
            this.Layer = 2f;
            //AnimationCollection ani = new AnimationCollection("Working", true) { Layer = 2 };
            //var handani = new Animation(WarpMode.Loop,
            //    new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
            //    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),// -(float)Math.PI / 2f, Interpolation.Sine),
            //    new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp));
            //handani.AddAction(25, () => Contact());
            //this.Add(Bone.Types.RightHand, handani);
            //ani.Add("Left Hand",
            //    new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
            //    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
            //    new Keyframe(25, Vector2.Zero, -(float)Math.PI, Interpolation.Exp)
            //    );


            this.Add(Bone.Types.RightHand,
                    new Animation(WarpMode.Loop,
                        new Keyframe(0, Vector2.Zero, -4 * (float)Math.PI / 3f, Interpolation.Exp),
                        //new Keyframe(10, Vector2.Zero, -5 * (float)Math.PI / 8f, Interpolation.Exp),//(a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t))),//(a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t))),
                        //new Keyframe(20, Vector2.Zero, -5 * (float)Math.PI / 8f),
                        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Exp),//(a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t))),//(a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t))),
                        new Keyframe(20, Vector2.Zero, -(float)Math.PI / 2f),
                        new Keyframe(60, Vector2.Zero, -4 * (float)Math.PI / 3f, Interpolation.Exp)//-5 * (float)Math.PI / 8f
                        ));
            this[Bone.Types.RightHand].AddAction(10, ()=>this.Contact());

            // TODO: blend body leaning with up&down from running using weights
            this.Add(Bone.Types.Torso,
                new Animation(WarpMode.Loop,
                //new Keyframe(0, Vector2.Zero, 0),
                //new Keyframe(80, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Sine)
                    new Keyframe(0, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Exp),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 8f, Interpolation.Exp),//(a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t))),
                    new Keyframe(20, Vector2.Zero, (float)Math.PI / 8f),
                    new Keyframe(60, Vector2.Zero, -(float)Math.PI / 8f, Interpolation.Exp)
                    ));

            this.Add(Bone.Types.Mainhand,
                new Animation(WarpMode.Loop,
                    new Keyframe(0, Vector2.Zero, 0, Interpolation.Exp),
                    new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Exp),//(a, b, t) => Interpolation.Sine(a, b, (float)Math.Sqrt(t))),
                    new Keyframe(20, Vector2.Zero, (float)Math.PI / 2f),
                    new Keyframe(60, Vector2.Zero, 0, Interpolation.Exp)
                // new Keyframe(10, Vector2.Zero, 0, Interpolation.Exp)
                ));

            //this.Add(Bone.Types.Hips,
            //    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
            //    new Keyframe(15, new Vector2(0, -8), 0, Interpolation.Sine),//(float)Math.PI / 4f, Interpolation.Sine),
            //    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
            //    );
            //this.Add(Bone.Types.RightFoot,
            //    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
            //    new Keyframe(15, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine),
            //    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
            //    );
            //this.Add(Bone.Types.LeftFoot,
            //    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
            //    new Keyframe(15, Vector2.Zero, -(float)Math.PI / 4f, Interpolation.Sine),
            //    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
            //    );
        }
    }
}
