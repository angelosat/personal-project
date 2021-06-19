using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics.Animations
{
    class AnimationJump : AnimationCollection
    {
        public AnimationJump()
            : base("Jumping")
        {
            this.Layer = 2f;
            this.Add(Bone.Types.RightHand, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
            this.Add(Bone.Types.LeftHand, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
            this.Add(Bone.Types.RightFoot, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
            this.Add(Bone.Types.LeftFoot, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
            this.Add(Bone.Types.Torso, new Keyframe(0, Vector2.Zero, 0));
            this.Add(Bone.Types.Hips, new Keyframe(0, Vector2.Zero, 0));
            this.Add(Bone.Types.Head, new Keyframe(0, Vector2.Zero, 0));
            this.Speed = 0;
        }
    }
}
