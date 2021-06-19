using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics
{
    public struct Keyframe
    {
        public int Time;
        public float Angle;
        public Vector2 Offset;
        public Func<float, float, float, float> Interpolation;
       // public Animation.Types Type;
        public Keyframe(int time, Vector2 offset, float angle, Func<float, float, float, float> interpolation)//, Animation.Types type = Animation.Types.Absolute)
        {
            this.Time = time;
            this.Offset = offset;
            this.Angle = angle;
            this.Interpolation = interpolation;
      //      this.Type = type;
        }
        public Keyframe(int time, Vector2 offset, float angle)
        {
            this.Time = time;
            this.Offset = offset;
            this.Angle = angle;
            this.Interpolation = Start_a_Town_.Interpolation.Lerp;
         //   this.Type = Animation.Types.Absolute;
        }
        public override string ToString()
        {
            return Time.ToString() + " " + Offset.ToString() + " " + this.Angle.ToString();// +" " + Interpolation;
        }
    }
}
