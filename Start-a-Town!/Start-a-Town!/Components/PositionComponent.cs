using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Position Position;// { get { return (Position)this["Position"]; } set { this["Position"] = value; } }
       // public Vector3 Velocity { get { return (Vector3)this["Speed"]; } set { this["Speed"] = value; } }
        public Vector2 Direction;// { get { return (Vector2)this["Direction"]; } set { this["Direction"] = value; } }
        public bool Exists { get { return (bool)this["Exists"]; } set { this["Exists"] = value; } }
        public GameObject Parent;// { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }
        //public TargetArgs ParentTarget;
        //public GameObject Parent
        //{
        //    get { return this.ParentTarget == null ? null : this.ParentTarget.Object; }
        //    set
        //    {
        //        if (value == null)
        //            this.ParentTarget = null;
        //        else
        //            this.ParentTarget = new TargetArgs(value);
        //    }
        //}
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
            //this.Velocity = Vector3.Zero;
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
           // this.Velocity = velocity;
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
      //      mov["Speed"] = this["Speed"];
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

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                //case Message.Types.SetPosition:
                //    //Update3(parent, (Vector3)e.Parameters[0]);
                //    parent.ChangePosition((Vector3)e.Parameters[0]);
                //    return true;

                case Message.Types.ChangeDirection:
                    e.Data.Translate(e.Network, r =>
                    {
                        this.Direction = r.ReadVector2();
                    });
                    return true;

                case Message.Types.Move:
                    Vector3 dir =   e.Data.Translate<DirectionEventArgs>(e.Network).Direction;//(Vector3)e.Parameters[0];
                    //using (BinaryReader reader = new BinaryReader(new MemoryStream(e.Data)))
                    //{
                    //    Vector3 dir = reader.ReadVector3();
                    //    this.Direction = new Vector2(dir.X, dir.Y);
                    //    return true;
                    //}
                    
                    this.Direction = new Vector2(dir.X, dir.Y);
                    return true;
                //case Message.Types.ApplyForce:
                //    this.ApplyForce((Vector3)e.Parameters[0]);
                //    return true;
                default:
                    return base.HandleMessage(parent, e);
            }
        }

        //public override void Query(GameObject parent, List<Interaction> actions)
        //{
        //    actions.Add(
        //        new Interaction(
        //            TimeSpan.Zero,
        //            (actor, target) => { InventoryComponent.UseHeldObject(actor, target); },
        //            parent,
        //            "Use" + (InventoryComponent.GetHeldObject(Player.Actor).HasValue ? " " + InventoryComponent.GetHeldObject(Player.Actor).Object.Name : ""), // TODO: make name a func
        //            "Using",
        //            cond: 
        //                new InteractionConditionCollection(
        //                    new InteractionCondition((actor, target) => InventoryComponent.GetHeldObject(actor).HasValue, "No item equipped")
        //                    )

        //            ));
        //}



        //public override void GetTooltip(GameObject parent, UI.Control tooltip)
        //{
        //    if (!parent.Exists)
        //        return;
        //    //if (this.Position.IsNull())
        //    //    return;
        //    tooltip.Controls.Add(new UI.Label(tooltip.Controls.Last().BottomLeft, this.Position.ToString()));//Global.ToString()));

        //    //if(!ScreenManager.CurrentScreen.Camera.IsNull())
        //    //tooltip.Controls.Add(new UI.Label(tooltip.Controls.Last().BottomLeft, "Depth: " + parent.Global.GetDepth(ScreenManager.CurrentScreen.Camera).ToString()));//Global.ToString()));
        //}

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();

            //Position pos = this.Position;
            //    Vector3 global = pos.Global;
            //    data.Add(new SaveTag(SaveTag.Types.Float, "GlobalX", global.X));
            //    data.Add(new SaveTag(SaveTag.Types.Float, "GlobalY", global.Y));
            //    data.Add(new SaveTag(SaveTag.Types.Float, "GlobalZ", global.Z));
            //Vector3 speed = this.Position.Velocity;
            //data.Add(new SaveTag(SaveTag.Types.Float, "SpeedX", speed.X));
            //data.Add(new SaveTag(SaveTag.Types.Float, "SpeedY", speed.Y));
            //data.Add(new SaveTag(SaveTag.Types.Float, "SpeedZ", speed.Z));



            data.Add(this.Global.SaveOld("Global"));
            data.Add(this.Position.Velocity.SaveOld("Velocity"));
            data.Add(this.Direction.Save("Direction"));

            return data;
        }

        internal override void Load(SaveTag data)
        {

            //Dictionary<string, SaveTag> dic = data.Value as Dictionary<string, SaveTag>;//.ToDictionary();
            //if (dic.ContainsKey("GlobalX"))
            //{
            //    this.Global = new Vector3((float)data["GlobalX"].Value, (float)data["GlobalY"].Value, (float)data["GlobalZ"].Value);
            //    this.Position.Velocity = new Vector3((float)data["SpeedX"].Value, (float)data["SpeedY"].Value, (float)data["SpeedZ"].Value);
            //}

            data.TryGetTag("Global", t => this.Global = t.LoadVector3());
            data.TryGetTag("Velocity", t => this.Position.Velocity = t.LoadVector3());
            data.TryGetTag("Direction", t => this.Direction = t.LoadVector2());

        }

        
        public override void Write(BinaryWriter w)
        {
            //bool exists = !this.Position.IsNull();
            //writer.Write(exists);
            w.Write(this.Exists);
            //if (this.Exists)
                w.Write(this.Position.Global);
                w.Write(this.Position.Velocity);
                w.Write(this.Direction);
        }

        public override void Read(BinaryReader r)
        {
            this.Exists = r.ReadBoolean();
                //this.Position = new Position(reader.ReadVector3());
                this.Global = r.ReadVector3();
                this.Position.Velocity = r.ReadVector3();
                this.Direction = r.ReadVector2();
        }
    }
}
