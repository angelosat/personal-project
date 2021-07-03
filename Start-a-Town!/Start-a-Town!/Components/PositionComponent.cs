using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.IO;

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
        public Position Position;
        public Vector2 Direction;
        public bool Exists;
        
        public override void MakeChildOf(GameObject parent)
        {
            parent.Transform = this;
        }

        // TODO: MAKE FIELD FOR SPEED! OPTIMIZE!
        public Vector3 Global
        {
            get {
                return Parent == null ? this.Position.Global : Parent.Global; 
            }
            set { this.Position.Global = value; }
        }

        public PositionComponent()
            : base()
        {
            this.Position = new Start_a_Town_.Position();
            this.Direction = Vector2.Zero;
            this.Exists = false;
            this.Parent = null;
        }
        public PositionComponent(Position pos)
            : this()
        {
            this.Position = pos;
        }
        public PositionComponent(Map map, Vector3 global)
            : this()
        {
            this.Position = new Position(map, global);
        }
        public PositionComponent(Map map, Vector3 global, Vector3 velocity, Vector2 direction)
            : this()
        {
            this.Position = new Position(map, global);
            this.Direction = direction;
        }
        public override string ToString()
        {
            return this.Global.ToString() + "\n" +
                "Velocity: " + this.Position.Velocity.ToString() + "\n" +
                "Direction: " + this.Direction.ToString() + "\n" +
                base.ToString();
        }
        
        public override object Clone()
        {
            PositionComponent mov = new PositionComponent(new Position(Position));
            return mov;
        }

        static public Rectangle GetScreenBounds(Camera camera, SpriteComponent sprComp, Vector3 global)
        {
            Rectangle bounds;
            camera.CullingCheck(global.X, global.Y, global.Z, sprComp.Sprite.GetBounds(), out bounds);
            return bounds;
        }

        public Rectangle GetScreenBounds(Camera camera, SpriteComponent sprComp)
        {
            Position CurrentPosition = this.Position;
            Chunk chunk = CurrentPosition.GetChunk();
            Cell cell = CurrentPosition.GetCell();
            Rectangle bounds;
            camera.CullingCheck(CurrentPosition.Global.X, CurrentPosition.Global.Y, CurrentPosition.Global.Z, sprComp.Sprite.GetBounds(), out bounds);
            return bounds;
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();

            data.Add(this.Global.SaveOld("Global"));
            data.Add(this.Position.Velocity.SaveOld("Velocity"));
            data.Add(this.Direction.Save("Direction"));

            return data;
        }

        internal override void Load(SaveTag data)
        {
            data.TryGetTag("Global", t => this.Global = t.LoadVector3());
            data.TryGetTag("Velocity", t => this.Position.Velocity = t.LoadVector3());
            data.TryGetTag("Direction", t => this.Direction = t.LoadVector2());
        }
        
        public override void Write(BinaryWriter w)
        {
            w.Write(this.Exists);
            w.Write(this.Position.Global);
            w.Write(this.Position.Velocity);
            w.Write(this.Direction);
        }

        public override void Read(BinaryReader r)
        {
            this.Exists = r.ReadBoolean();
            this.Global = r.ReadVector3();
            this.Position.Velocity = r.ReadVector3();
            this.Direction = r.ReadVector2();
        }
    }
}
