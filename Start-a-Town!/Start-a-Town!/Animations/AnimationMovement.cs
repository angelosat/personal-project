using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics.Animations
{
    class AnimationsMovement
    {
        //static public Animation Walking(GameObject entity)
        //{
        //    var ani = new Animation(null, "Walking");
        //    ani.Layer = 1;
        //    //AnimationCollection ani = new AnimationCollection("Walking", true) { Layer = 1 };

        //    var hipsani = new AnimationClip(WarpMode.Loop,
        //        new Keyframe(0, Vector2.Zero, 0),
        //        new Keyframe(10, new Vector2(0, -8), 0, Interpolation.Sine),
        //        new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
        //    //hipsani.AddAction(20, () => Components.MobileComponent.OnFootDown(ani.Entity));
        //    hipsani.AddEvent(20, (e) => Components.MobileComponent.OnFootDown(e));
        //    ani.Add(BoneDef.Hips, hipsani);

        //    ani.Add(BoneDef.RightHand,
        //        new Keyframe(0, Vector2.Zero, 0),
        //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //        new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //    ani.Add(BoneDef.LeftHand,
        //        new Keyframe(0, Vector2.Zero, 0),
        //        new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //        new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //    ani.Add(BoneDef.RightFoot,
        //        new Keyframe(0, Vector2.Zero, 0),
        //        new Keyframe(10, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //        new Keyframe(30, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //    ani.Add(BoneDef.LeftFoot,
        //        new Keyframe(0, Vector2.Zero, 0),
        //        new Keyframe(10, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(20, Vector2.Zero, 0, Interpolation.Sine),
        //        new Keyframe(30, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine),
        //        new Keyframe(40, Vector2.Zero, 0, Interpolation.Sine));
        //    ani.Add(BoneDef.Head,
        //        new Keyframe(0, Vector2.Zero, 0),
        //        new Keyframe(5, new Vector2(0, 2), 0, Interpolation.Sine),
        //        new Keyframe(10, new Vector2(0, 0), 0, Interpolation.Sine),
        //        new Keyframe(15, new Vector2(0, -2), 0, Interpolation.Sine),
        //        new Keyframe(20, new Vector2(0, 0), 0, Interpolation.Sine));
        //    return ani;
        //}

        //static public Animation Jumping(GameObject parent)
        //{
        //    var ani = new Animation(parent, "Jumping");

        //    ani.Layer = 2f;
        //    ani.Add(BoneDef.RightHand, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
        //    ani.Add(BoneDef.LeftHand, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
        //    ani.Add(BoneDef.RightFoot, new Keyframe(0, Vector2.Zero, -(float)Math.PI / 2f, Interpolation.Sine));
        //    ani.Add(BoneDef.LeftFoot, new Keyframe(0, Vector2.Zero, (float)Math.PI / 2f, Interpolation.Sine));
        //    ani.Add(BoneDef.Torso, new Keyframe(0, Vector2.Zero, 0));
        //    ani.Add(BoneDef.Hips, new Keyframe(0, Vector2.Zero, 0));
        //    ani.Add(BoneDef.Head, new Keyframe(0, Vector2.Zero, 0));
        //    ani.Speed = 0;
        //    return ani;
        //}
    }
    
}
