using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    public class ObjectSnapshot
    {
        public TimeSpan Time;
        public GameObject Object;
        public Vector3 Position, Velocity, Orientation;
        public byte[] Data;

        public override string ToString()
        {
            return this.Object.Name.ToString() + " Position: " + this.Position + " Velocity: " + this.Velocity;
        }

        public ObjectSnapshot()
        {

        }
        public ObjectSnapshot(GameObject obj)
        {
            this.Object = obj;
            this.Position = obj.Global;
            this.Velocity = obj.Velocity;
            this.Orientation = obj.Direction;
            if (float.IsNaN(this.Orientation.X) || float.IsNaN(this.Orientation.Y))
                throw new Exception();
        }

        public ObjectSnapshot Interpolate(ObjectSnapshot next, TimeSpan now)
        {
            return Interpolate(this, next, now);
        }
        public static ObjectSnapshot Interpolate(ObjectSnapshot prev, ObjectSnapshot next, TimeSpan now)
        {
            float t = (float)((now.TotalMilliseconds - prev.Time.TotalMilliseconds) / (next.Time.TotalMilliseconds - prev.Time.TotalMilliseconds));
            Vector3 global = Interpolation.Lerp(prev.Position, next.Position, t);
            //global = next.Position;
            Vector3 velocity = Interpolation.Lerp(prev.Velocity, next.Velocity, t);
            //velocity = next.Velocity;// -prev.Velocity;
            Vector3 direction = Interpolation.Lerp(prev.Orientation, next.Orientation, t);
            if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
                throw new Exception();
            return new ObjectSnapshot() { Time = now, Position = global, Velocity = velocity, Orientation = direction };
        }

        static public void Write(GameObject obj, BinaryWriter writer)
        {
            writer.Write(obj.Global);
            writer.Write(obj.Velocity);
            writer.Write(obj.Direction);
            //obj.Write(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">The time of the snapshot</param>
        /// <param name="obj">The game object the state corresponds to</param>
        /// <param name="reader">The reader to read values from</param>
        /// <returns></returns>
        static public ObjectSnapshot Create(TimeSpan time, GameObject obj, BinaryReader reader)
        {
            return new ObjectSnapshot()
            {
                Time = time,
                Object = obj,
                Position = reader.ReadVector3(),
                Velocity = reader.ReadVector3(),
                Orientation = reader.ReadVector3()
            };
        }

        public GameObject GetObject()
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(this.Data)))
                return GameObject.CreateCustomObject(r);
        }
    }
}
