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
        public byte[] Data;

        public override string ToString()
        {
            return this.Time.ToString();//.Name.ToString() + " Position: " + this.Position + " Velocity: " + this.Velocity;
        }

        public ObjectSnapshot()
        {

        }
        //public ObjectSnapshot(GameObject obj)
        //{
        //    this.Object = obj;
        //    this.Position = obj.Global;
        //    this.Velocity = obj.Velocity;
        //}

        public ObjectSnapshot Interpolate(ObjectSnapshot next, TimeSpan now)
        {
            return Interpolate(this, next, now);
        }
        //public static ObjectSnapshot Interpolate(ObjectSnapshot prev, ObjectSnapshot next, TimeSpan now)
        //{
        //    float t = (float)((now.TotalMilliseconds - prev.Time.TotalMilliseconds) / (next.Time.TotalMilliseconds - prev.Time.TotalMilliseconds));
        //    Vector3 global = Interpolation.Lerp(prev.Position, next.Position, t);
        //    Vector3 velocity = Interpolation.Lerp(prev.Velocity, next.Velocity, t);
        //    return new ObjectSnapshot() { Time = now, Position = global, Velocity = velocity };
        //}
        public GameObject Interpolate(ObjectSnapshot next, TimeSpan now)
        {
            float t = (float)((now.TotalMilliseconds - this.Time.TotalMilliseconds) / (next.Time.TotalMilliseconds - this.Time.TotalMilliseconds));
            GameObject prevObj = GameObject.Create(this.Data);
            GameObject nextObj = GameObject.Create(next.Data);
            Vector3 global = Interpolation.Lerp(prev.Position, next.Position, t);
            Vector3 velocity = Interpolation.Lerp(prev.Velocity, next.Velocity, t);
            return new ObjectSnapshot() { Time = now, Position = global, Velocity = velocity };
        }

        static public void Write(GameObject obj, BinaryWriter writer)
        {
            //writer.Write(obj.Global);
            //writer.Write(obj.Velocity);

            obj.Write(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">The time of the snapshot</param>
        /// <param name="obj">The game object the state corresponds to</param>
        /// <param name="reader">The reader to read values from</param>
        /// <returns></returns>
        static public ObjectSnapshot FromReader(TimeSpan time, BinaryReader reader)
        {
            return new ObjectSnapshot()
            {
                Time = time,
                Data = (reader.BaseStream as MemoryStream).ToArray()
            };
        }

        static public ObjectSnapshot FromObject(TimeSpan time, GameObject obj)
        {
            return new ObjectSnapshot()
            {
                Time = time,
                Data = obj.GetSnapshotData()
            };
        }

        public GameObject GetObject()
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(this.Data)))
                return GameObject.Create(r);
        }
    }
}
