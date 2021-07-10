using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class PositionComponent : EntityComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Position";
            }
        }

        public float Spin;
        public GameObject ParentEntity;
        public Vector2 Direction;
        public bool Exists;
        public Vector3 Velocity;
        public Vector3 Global;
       

        public override void MakeChildOf(GameObject parent)
        {
            parent.Transform = this;
        }
        public override string ToString()
        {
            return this.Global.ToString() + "\n" +
                "Velocity: " + this.Velocity.ToString() + "\n" +
                "Direction: " + this.Direction.ToString() + "\n" +
                base.ToString();
        }
        
        public override object Clone()
        {
            PositionComponent mov = new();
            return mov;
        }

        static public Rectangle GetScreenBounds(Camera camera, SpriteComponent sprComp, Vector3 global)
        {
            Rectangle bounds;
            camera.CullingCheck(global.X, global.Y, global.Z, sprComp.Sprite.GetBounds(), out bounds);
            return bounds;
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();

            data.Add(this.Global.SaveOld("Global"));
            data.Add(this.Velocity.SaveOld("Velocity"));
            data.Add(this.Direction.Save("Direction"));

            return data;
        }

        internal override void Load(SaveTag data)
        {
            data.TryGetTag("Global", t => this.Global = t.LoadVector3());
            data.TryGetTag("Velocity", t => this.Velocity = t.LoadVector3());
            data.TryGetTag("Direction", t => this.Direction = t.LoadVector2());
        }
        
        public override void Write(BinaryWriter w)
        {
            w.Write(this.Exists);
            w.Write(this.Global);
            w.Write(this.Velocity);
            w.Write(this.Direction);
        }

        public override void Read(BinaryReader r)
        {
            this.Exists = r.ReadBoolean();
            this.Global = r.ReadVector3();
            this.Velocity = r.ReadVector3();
            this.Direction = r.ReadVector2();
        }
    }
}
