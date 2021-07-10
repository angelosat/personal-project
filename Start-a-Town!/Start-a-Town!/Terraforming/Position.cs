using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [Obsolete]
    public class Position
    {
        public Vector3 Global;
        public Vector3 Velocity;

        static public Vector3 Floor(Vector3 global)
        {
            return new Vector3((int)Math.Floor(global.X), (int)Math.Floor(global.Y), (int)global.Z);
        }

        public Position()
        {

        }
        public Position(Vector3 global)
        {
            Global = global;
        }
        
        public Position(Position pos)
        {
            Global = pos.Global;
            Velocity = pos.Velocity;
        }

        public override string ToString()
        {
            return "Global: " + Global.ToString() +
                "\nVelocity: " + Velocity.ToString();
        }
    }
}
