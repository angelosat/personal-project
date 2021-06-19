using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics.Animations
{
    class AnimationWalk : AnimationCollection
    {
        public Action OnFootDown = () => { };


        public AnimationWalk()
            : base("Walking")
        {
            this.Layer = 1;
            //AnimationCollection ani = new AnimationCollection("Walking", true) { Layer = 1 };
            
            var hipsani = new Animation(WarpMode.Loop, new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
                new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
            hipsani.AddAction(20, () => OnFootDown());
            this.Add(Bone.Types.Hips, hipsani);

            this.Add(Bone.Types.RightHand,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
            this.Add(Bone.Types.LeftHand,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
            this.Add(Bone.Types.RightFoot,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
            this.Add(Bone.Types.LeftFoot,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
                new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
            this.Add(Bone.Types.Head,
                new Keyframe(0, Vector2.Zero, 0),
                new Keyframe(5, new Vector2(0, 2), 0, Interpolation.Sine),
                new Keyframe(10, new Vector2(0, 0), 0, Interpolation.Sine),
                new Keyframe(15, new Vector2(0, -2), 0, Interpolation.Sine),
                new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
        }
    }
}
