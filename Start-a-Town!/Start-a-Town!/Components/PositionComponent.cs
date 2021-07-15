﻿using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class PositionComponent : EntityComponent
    {
        public override string ComponentName => "Position";

        public GameObject ParentEntity;
        public Vector2 Direction;
        public bool Exists;
        public Vector3 Velocity;

        Vector3 _Global;
        public Vector3 Global
        {
            get => this.ParentEntity?.Global ?? this._Global;
            set => this._Global = value;
        }

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
            camera.CullingCheck(global.X, global.Y, global.Z, sprComp.Sprite.GetBounds(), out Rectangle bounds);
            return bounds;
        }

        internal override void AddSaveData(SaveTag tag)
        {
            this.Global.Save(tag, "Global");
            this.Velocity.Save(tag, "Velocity");
            this.Direction.Save(tag, "Direction");
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
