using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics
{
    class Transition
    {
        public enum States { Unstarted, Running, Finished }


        public States State { get; private set; }
        public Keyframe A, B;
        int Frames;
       // Func<float, float, float, float> Interpolate;

        static public Transition Create(Keyframe a, Keyframe b)
        {
            //Keyframe kfnext;
            //switch (b.Type)
            //{
            //    case Animation.Types.Relative:
            //        kfnext = new Keyframe(b.Time, a.Offset + b.Offset, a.Angle + b.Angle, b.Interpolation, b.Type);
            //        break;
            //    default:
            //        kfnext = b;
            //        break;
            //}
            return new Transition(a, b);//kfnext);
        }

        Transition(Keyframe a, Keyframe b)
        {
            this.A = a;
            this.B = b;
            this.Frames = b.Time - a.Time;
            //this.Interpolate = interpolation;
        }
        //public States Step(out float value)
        //{
        //    value = this.A + this.B * ((Current++) / (float)Frames);
        //    return Current > Frames ? States.Finished : States.Running;
        //}

        public float GetPercentage(int frame)
        {
            return 1 - ((this.B.Time - frame) / (float)(this.B.Time - this.A.Time));
            //return (frame - this.A.Time) / (float)(this.B.Time - this.A.Time);
        }
        public States Step(int frame, out Vector2 offset, out float angle)
        {
      //     float t = MathHelper.Clamp(GetPercentage(frame), 0, 1);
            if (A.Time == B.Time)
            {
                angle = B.Angle;
                offset = B.Offset;
                return States.Running;
            }
            float t = GetPercentage(frame);
            angle = this.B.Interpolation(this.A.Angle, this.B.Angle, t);// this.A.Angle + (this.B.Angle - this.A.Angle) * t;
            offset = new Vector2(this.B.Interpolation(this.A.Offset.X, this.B.Offset.X, t), this.B.Interpolation(this.A.Offset.Y, this.B.Offset.Y, t));
            State = frame >= (this.B.Time - 1) ? States.Finished : States.Running;
            return State;
        }


        //static float GetPercentage(float frame, Keyframe a, Keyframe b)
        //{
        //    return 1 - ((b.Time - frame) / (float)(b.Time - a.Time));
        //}
        //static public States Step(float frame, Keyframe a, Keyframe b, out Vector2 offset, out float angle)
        //{
        //    if (a.Time == b.Time)
        //    {
        //        angle = b.Angle;
        //        offset = b.Offset;
        //        return States.Finished;
        //    }
        //    float t = GetPercentage(frame, a, b);
        //    angle = b.Interpolation(a.Angle, b.Angle, t);// this.A.Angle + (this.B.Angle - this.A.Angle) * t;
        //    offset = new Vector2(b.Interpolation(a.Offset.X, b.Offset.X, t), b.Interpolation(a.Offset.Y, b.Offset.Y, t));
        //    return (frame >= b.Time) ? States.Finished : States.Running;
        //}

        public override string ToString()
        {
            return this.A.ToString() + " -> " + this.B.ToString();
        }
    }
}
