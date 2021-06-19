using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    struct BoundingCylinder
    {
        public Vector3 Position;
        public float Radius;
        public float Height;

        public BoundingCylinder(Vector3 global, float radius, float height)
        {
            this.Position = global;
            this.Radius = radius;
            this.Height = height;
        }

        public bool Intersects(Ray ray)
        {
            //if (!ray.Intersects(new BoundingBox(this.Position + new Vector3(-Radius, -Radius, 0), this.Position + new Vector3(Radius, Radius, this.Height))).HasValue)
            //    return false;
            var checkbox = new BoundingBox(this.Position + new Vector3(-Radius, -Radius, 0), this.Position + new Vector3(Radius, Radius, this.Height));
            var intersection = ray.Intersects(checkbox);
            if (!intersection.HasValue)
                return false;

            //Vector3 distance = this.Position - ray.Position; // just check their XY coordinates, the z one has been covered  by checking the boundingbox above
            Vector3 distance = new Vector3(this.Position.XY() - ray.Position.XY(), 0); //this.Position - ray.Position; // just check their XY coordinates, the z one has been covered  by checking the boundingbox above

            float scalar = Vector3.Dot(distance, ray.Direction);
            Vector3 ab = ray.Direction * Math.Abs(scalar);// length * cos * ray.Direction;
            Vector3 rejection = distance - ab;

            //Vector3 distance = this.Position - ray.Position;
            //Vector3 distanceNormal = Vector3.Normalize(distance);
            //float dot = Vector3.Dot(ray.Direction, distance);
            //double angle = Math.Acos(dot);
            //angle.ToConsole();
            //Vector3 normal = Vector3.Normalize(ray.Direction);            
            //float length = distance.Length();
            //float cos = (float)Math.Cos(angle);
            //Vector3 ab = length * cos * normal;

            // Vector3 projection = ray.Position + ab;
            float rejectionLength = rejection.Length();
            //float projectionDistance = Vector3.Distance(this.Position, ab);
            return rejectionLength <= this.Radius;
        }

        public static BoundingCylinder Create(GameObject obj)
        {
            // TODO: half the radius maybe
            //return new BoundingCylinder(obj.Global, 0.5f, (float)obj["Physics"]["Height"]);
            return new BoundingCylinder(obj.Global, 0.5f, obj.GetPhysics().Height);

        }
    }
}
