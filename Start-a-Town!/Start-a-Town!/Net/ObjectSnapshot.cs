using System;
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

        static public void Write(GameObject obj, BinaryWriter writer)
        {
            writer.Write(obj.Global);
            writer.Write(obj.Velocity);
            writer.Write(obj.Direction);
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
    }
}
