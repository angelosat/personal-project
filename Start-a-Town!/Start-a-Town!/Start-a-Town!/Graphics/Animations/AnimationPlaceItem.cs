﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics.Animations
{
    class AnimationPlaceItem : AnimationCollection
    {
        public Action Finish = () => { };

        public AnimationPlaceItem()
            : base("Dropping")
        {
            this.Layer = 2f;
            //AnimationCollection ani = new AnimationCollection("Working", true) { Layer = 2 };
            var handani = new Animation(WarpMode.Once,
                //new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine)
                );
            handani.AddAction(25, () => Finish());
            this.Add(Bone.Types.RightHand, handani);
            this.Add(Bone.Types.LeftHand, new Animation(WarpMode.Once,
                //new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
                new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine)
                ));
            this.Add(Bone.Types.Torso, new Animation(WarpMode.Once,
                //new Keyframe(0, Vector2.Zero, -(float)Math.PI, Interpolation.Exp),
                new Keyframe(0, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine),
                new Keyframe(25, Vector2.Zero, (float)Math.PI / 4f, Interpolation.Sine)
                ));

            //this.Add(Bone.Types.Hips,
            //    new Keyframe(0, Vector2.Zero, 0, Interpolation.Sine),
            //    new Keyframe(15, new Vector2(0, -8), 0, Interpolation.Sine),//(float)Math.PI / 4f, Interpolation.Sine),
            //    new Keyframe(25, Vector2.Zero, 0, Interpolation.Sine)
            //    );

        }
    }
}
