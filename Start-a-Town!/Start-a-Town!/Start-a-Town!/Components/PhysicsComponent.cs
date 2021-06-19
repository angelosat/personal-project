using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    public class PhysicsComponent : Component
    {
        public enum ObjectSize { Immovable = -1, Inventoryable, Haulable }

        ObjectSize Size { get { return (ObjectSize)this["Size"]; } set { this["Size"] = value; } }
        protected bool Solid { get { return (bool)this["Solid"]; } set { this["Solid"] = value; } }

        static public float Gravity = -0.03f;//-0.04f;// -0.05f; //35f;
        
        //public int Size { get; private set; }
        //public float WalkSpeed;
        //public bool Solid;
        //public int Height;

        public override void Initialize(GameObject parent)
        {
            //   MakeSolid(parent, true);
        }

        private void MakeSolid(GameObject parent, Map map, bool solid)
        {
            if (!(bool)this["Solid"])
                return;

            Vector3 global = parent["Position"].GetProperty<Position>("Position").Global;
            float z = 0;
            while (z < Map.MaxHeight && z < (int)this["Height"])
            {
                Cell cell;
                if (Position.TryGetCell(map, global + new Vector3(0, 0, z), out cell))
                    cell.Solid = solid;
                z++;
            }
        }

        PhysicsComponent()
            : base()
        {
            Properties.Add("Weight");
            Properties.Add("Solid");
            Properties.Add("Height");
            Properties.Add("Size");
            //   this["Sticky"] = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">-1: can't pick up, 0: inventory, 1: haulable</param>
        /// <param name="solid"></param>
        /// <param name="height"></param>
        /// <param name="weight"></param>
        public PhysicsComponent(int size = 0, bool solid = false, int height = 1, float weight = 1)//, bool sticky = false)
            : this()
        {
            //WalkSpeed = walkspeed;
            //Size = size;
            //Solid = solid;
            //Height = height;

            // TODO: make height a float
            Properties["Weight"] = weight;
            Properties["Height"] = height;
            Properties["Solid"] = solid;
            Properties["Size"] = size;
            //  this["Sticky"] = sticky;
        }

        //public void ApplyForce(Vector3 force)
        //{
        //    Speed += force;
        //}

        float Friction = 0.01f;// 0.005f;
        float terminalVelocity = 0.1f;
        public override void Update(GameObject parent, Map map, Chunk chunk)
        {
            //if (Sticky)
            //    return;
            ///TODO
            ///instead of checking the tile below every update, make the cell below send a message to the entities above when it's removed
            ///and when walking into a new cell
            //Console.WriteLine("phys");
            MovementComponent positionComp;
            if (!parent.TryGetComponent<MovementComponent>("Position", out positionComp))
                return;
            Position posNow = positionComp.GetProperty<Position>("Position");
            Vector3 next = Vector3.Zero, global = posNow.Global, speed = positionComp.GetProperty<Vector3>("Speed");
            Cell nextCell;
            Chunk nextChunk;
            float thisDensity = 0f;
            Cell thisCell;

            if (!posNow.TryGetCell(out thisCell))
                return;

            TileComponent tileComp = TileComponent.TileMapping[thisCell.TileType].GetComponent<TileComponent>("Physics");

            thisDensity = tileComp.GetProperty<float>(Stat.Density.Name);

            speed.X *= (1 - thisDensity);
            speed.Y *= (1 - thisDensity);

            if (speed.Z == 0)
            {
                if (speed.X > 0)
                    speed.X = Math.Max(0, speed.X - Friction);
                else
                    speed.X = Math.Min(0, speed.X + Friction);
                if (speed.Y > 0)
                    speed.Y = Math.Max(0, speed.Y - Friction);
                else
                    speed.Y = Math.Min(0, speed.Y + Friction);

                Cell cellBelow;
                if (Position.TryGetCell(map, global + speed + new Vector3(0, 0, Gravity * (1 - thisDensity)), out cellBelow))
                    if (!cellBelow.Solid)
                        speed.Z = Gravity;
            }
            else
                // speed.Z = Math.Max(-GlobalVars.DeltaTime * (1 - thisDensity), speed.Z + Gravity * GlobalVars.DeltaTime * (1 - thisDensity));
                speed.Z = Math.Max(-GlobalVars.DeltaTime * (1 - thisDensity), speed.Z + Gravity * GlobalVars.DeltaTime * (1 - thisDensity));
            next = global + speed * GlobalVars.DeltaTime;

            //check if can climb
            Vector3 nextRounded = next.Round();// Position.Round(next);

            if (!Position.TryGet(map, nextRounded, out nextCell, out nextChunk)) //if the next cell is out of bounds, return without updating position
            {
                speed = Vector3.Zero;
                positionComp["Speed"] = speed;
                return;
            }
            float oldspeedz = speed.Z;
            if (nextCell.Solid)
            {
                //if (speed.Z > 0)
                //if (parent.ID == GameObject.Types.Twig || parent.ID == GameObject.Types.Cobble || parent.ID == GameObject.Types.Soil)
                //    Console.WriteLine("");
                if (speed.Z == 0)
                    GameObject.PostMessage(parent, Message.Types.CollisionCell, parent, nextCell.GetGlobalCoords(nextChunk));
                if (speed.Z < 0)
                    VerticalCompensate(map, ref next, ref nextRounded, ref global, ref speed);
                if (nextRounded.Z < Math.Floor(global.Z))
                    speed.Z = 0; // comment to prevent jumping while pushing against obstacle
                ResolveCollision(map, ref next, ref global, ref speed, ref nextCell, ref nextRounded);

                if (oldspeedz < 0)
                    next.Z = nextRounded.Z + 1f;
                //else if(oldspeedz>0 &&
                //    speed.Z <= 0)

                positionComp.Update3(parent, next);
                positionComp["Speed"] = speed;
                return;

            }
            else //if the next cell is not solid, do collision detection with other objects
            {
                VerticalCompensate(map, ref next, ref nextRounded, ref global, ref speed);
                if (speed != Vector3.Zero)
                {
                    GameObject collided;
                    if (TryCollision(map, nextRounded, parent, out collided))
                    {
                        ResolveCollision(map, ref next, ref global, ref speed, ref nextCell, ref nextRounded);
                        positionComp.Update3(parent, next);
                        positionComp["Speed"] = speed;
                        return;
                    }
                }
            }

            ///check if it fits height wise
            if (!CanFit(map, next))
            {
                speed = Vector3.Zero;
                positionComp["Speed"] = speed;
                return;
            }

            positionComp.Update3(parent, next);
            //if (Position.TryGetCell(map, next, out nextCell))
            //    if (nextCell.Solid)
            //        Console.WriteLine("WTF2");
            positionComp["Speed"] = speed;
            //if (speed.Z != 0)
            //    positionComp.Spin += 0.1f;
            //else positionComp.Spin = 0;
        }

        private static void VerticalCompensate(Map map, ref Vector3 next, ref Vector3 nextRounded, ref Vector3 global, ref Vector3 speed)
        {
            ////check if the block difference is more than one while falling, check intermediary blocks
            ////float hdif = (float)Math.Abs(global.Z - next.Z);
            //float hdif = next.Z - global.Z;
            //if (hdif < -1)// > 1)
            //{
            //    for (int i = 0; i < (int)Math.Ceiling(hdif); i++)
            //    {
            //        Vector3 check = global - new Vector3(0, 0, 1 + i);
            //        if (Position.GetCell(map, check).Solid)
            //        {
            //            next.Z = (float)Math.Floor(global.Z - i);
            //            nextRounded.Z = next.Z;
            //            speed.Z = 0;
            //            return;
            //        }
            //    }
            //}

            float hdif = global.Z - next.Z;
            if (hdif > 1)
            {
                for (int i = 0; i < (int)Math.Ceiling(hdif); i++)
                {
                    Vector3 check = global - new Vector3(0, 0, 1 + i);
                    if (Position.GetCell(map, check).Solid)
                    {
                        next.Z = (float)Math.Floor(global.Z - i);
                        nextRounded.Z = next.Z;
                        speed.Z = 0;
                        return;
                    }
                }
            }
        }

        private static void ResolveCollision(Map map, ref Vector3 next, ref Vector3 global, ref Vector3 speed, ref Cell nextCell, ref Vector3 nextRounded)
        {
            //  // TODO: UPDATE POSITION ONLY ON THE AXIS THAT IT CAN MOVE
            //float distX = Math.Abs(next.X - nextRounded.X);
            //float distY = Math.Abs(next.Y - nextRounded.Y);
            float distX = Math.Abs(global.X - nextRounded.X);
            float distY = Math.Abs(global.Y - nextRounded.Y);
            // speed = new Vector3(0, 0, speed.Z);
            //   Vector2 colNorm;
            Cell currentCell;
            Vector3 nnext;
            if (distX == distY)
            {
                next = global;
                //  speed = new Vector3(0, 0, speed.Z);// Vector3.Zero;
                //next.Z = nextRounded.Z;
                if (Position.TryGetCell(map, next, out nextCell))
                    if (Position.TryGetCell(map, global, out currentCell))
                        if (!currentCell.Solid && nextCell.Solid)
                            Console.WriteLine("WTF");
            }
            //else if (distX > distY)
            else if (distY > distX)
            {
                //  colNorm = new Vector2(next.X < nextRounded.X ? -1f : 1f, 0);
                //float nx = next.X < nextRounded.X ? -0.5f : 0.5f;
                //next = new Vector3(nextRounded.X + nx, next.Y, global.Z);
                //float nx = next.X < nextRounded.X ? -0.5f : 0.5f;
                //next = new Vector3(next.X, global.Y, global.Z);
                //if (Position.TryGetCell(next, out nextCell))
                //    if (Position.TryGetCell(global, out currentCell))
                //        if (!currentCell.Solid && nextCell.Solid)
                //            Console.WriteLine("WTF");
                nnext = new Vector3(next.X, global.Y, next.Z);//, global.Z);
                if (Position.TryGetCell(map, nnext, out nextCell))
                {
                    if (!nextCell.Solid)
                        next = nnext;
                    else
                    {
                        nnext = new Vector3(global.X, next.Y, global.Z);
                        if (Position.TryGetCell(map, nnext, out nextCell))
                        {
                            if (!nextCell.Solid)
                                next = nnext;
                            else
                                next = global + new Vector3(0, 0, speed.Z);
                        }
                    }
                }

            }
            else
            {
                //float ny = next.Y < nextRounded.Y ? -0.5f : 0.5f;
                //next = new Vector3(global.X, nextRounded.Y + ny, global.Z);
                //next = new Vector3(global.X, next.Y, global.Z);
                //if (Position.TryGetCell(next, out nextCell))
                //    if (Position.TryGetCell(global, out currentCell))
                //        if (!currentCell.Solid && nextCell.Solid)
                //            Console.WriteLine("WTF");
                //    speed = new Vector3(0, 0, speed.Z);
                //if (speed.Z > 0)
                //    Console.WriteLine("ASDASFASEASEG");
                nnext = new Vector3(global.X, next.Y, next.Z);// global.Z);
                if (Position.TryGetCell(map, nnext, out nextCell))
                {
                    if (!nextCell.Solid)
                        next = nnext;
                    else
                    {
                        nnext = new Vector3(next.X, global.Y, global.Z);
                        if (Position.TryGetCell(map, nnext, out nextCell))
                        {
                            if (!nextCell.Solid)
                                next = nnext;
                            else
                                next = global + new Vector3(0, 0, speed.Z);
                        }
                    }
                }
            }
            //  //Rectangle tileRect = new Rectangle(nextRounded.X, nextRounded.Y, 1, 1);
            //  //tileRect.ori
            ////  Vector2 tileCenter = new Vector2(nextRounded.X + 0.5f, nextRounded.Y + 0.5f);
            //  float distX = Math.Abs(next.X - nextRounded.X);
            //  float distY = Math.Abs(next.Y - nextRounded.Y);
            //  //float nx = next.X < nextRounded.X ? -1f : 1f;
            //  //float ny = next.Y < nextRounded.Y ? -1f : 1f;
            //  Vector2 colNorm;
            //  if (distX > distY)
            //      colNorm = new Vector2(next.X < nextRounded.X ? -1f : 1f, 0);
            //  else
            //      colNorm = new Vector2(0, next.Y < nextRounded.Y ? -1f : 1f);

            //  speed = new Vector3(colNorm.X, colNorm.Y, speed.Z); 
            //     speed = Vector3.Zero;
        }
        bool CanFit(Map map, Vector3 next)
        {
            for (int h = 0; h < GetProperty<int>("Height"); h++)
            {
                Cell fitCell;
                if (Position.TryGetCell(map, next + new Vector3(0, 0, h), out fitCell))
                    if (fitCell.Solid) // if cell exists and is solid, return without updating positiong
                    {

                        return false;
                    }
            }
            return true;
        }
        void Fall(MovementComponent posComp)//, out Vector3 next)
        {
            Vector3 speed = posComp.GetProperty<Vector3>("Speed");
            if (speed.Z == 0)
            {
                Cell cell = posComp.GetProperty<Position>("Position").GetCell();
                if (!cell.Solid)
                    posComp["Speed"] = new Vector3(speed.X, speed.Y, Gravity);
            }
            else
                posComp["Speed"] = new Vector3(speed.X, speed.Y, speed.Z + Gravity);
            //posComp.GetProperty<Vector3>("Speed").Z += Gravity;
        }
        //void Friction(MovementComponent posComp)
        //{
        //    Vector3 speed = posComp.GetProperty<Vector3>("Speed");
        //    if (speed.X > 0)
        //        speed.X = Math.Max(0, speed.X - friction);
        //    else
        //        speed.X = Math.Min(0, speed.X + friction);
        //    if (speed.Y > 0)
        //        speed.Y = Math.Max(0, speed.Y - friction);
        //    else
        //        speed.Y = Math.Min(0, speed.Y + friction);
        //    posComp["Speed"] = speed;
        //}

        bool TryCollision(Map map, Vector3 global, GameObject parent, out GameObject collided)
        {
            collided = null;
            Chunk nextChunk;
            Cell nextCell;
            if (!Position.TryGet(map, global, out nextCell, out nextChunk))
                return false;

            List<GameObject> objects = nextChunk.GetObjects();
            foreach (GameObject objCheck in objects)
            {
                if (objCheck == parent)
                    continue;

                PhysicsComponent objPhys;
                if (!objCheck.TryGetComponent<PhysicsComponent>("Physics", out objPhys))
                    continue;
                //bool objSolid = (bool)objPhys["Solid"];
                //if (!objSolid)
                //    continue;
                if (!(bool)objPhys["Solid"])
                    //if (!(bool)objCheck["Physics"]["Solid"])
                    continue;

                MovementComponent objPos;
                if (!objCheck.TryGetComponent<MovementComponent>("Position", out objPos))
                    continue;
                Vector3 objGlobal = objPos.GetProperty<Position>("Position").Global;
                Cell objCell = Position.GetCell(map, objGlobal);
                if (objCell == nextCell)
                {
                    collided = objCheck;
                    return true;
                }
            }
            return false;
        }


        void StepUp(MovementComponent posComp, Cell cell)//Now, Cell cellNext)
        {
            // if (cellNow != cellNext)
            if (cell.Solid) //Next.Solid)
            {
                Position CurrentPosition = posComp.GetProperty<Position>("Position");
                CurrentPosition.Global = new Vector3(CurrentPosition.Global.X, CurrentPosition.Global.Y, cell.Z + 1);//Next.Z + 1);
            }
        }

        public override bool HandleMessage(GameObject parent, GameObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.PickUp:
                    if (parent == sender)
                    {
                        Log.Enqueue(Log.EntryTypes.Default, sender.Name + " tried to pick themselves up.");
                        return true;
                    }

                    //GameObject.PostMessage(e.Sender, Size == 0 ? Message.Types.Receive : Message.Types.Hold, parent, parent.ToSlot(), parent);//
                    GameObject.PostMessage(e.Sender, Message.Types.Receive, parent, parent.ToSlot(), parent);//
                    //if (GetProperty<int>("Size") < 0)
                    //    return true;
                    //MovementComponent posComp;
                    //if (parent.TryGetComponent<MovementComponent>("Position", out posComp))
                    //{
                    //    if (sender["Stats"].GetProperty<float>(Stat.Strength.Name) < parent["Physics"].GetProperty<float>("Weight"))
                    //    {
                    //        Log.Enqueue(Log.EntryTypes.TooHeavy, Stat.Strength);
                    //        return true;
                    //    }
                    //    //if 
                    //    InventoryComponent.GiveObject(sender, parent);
                    //     //   Map.RemoveObject(parent);
                    //    return true;
                    //}
                    return true;
                case Message.Types.Carry:
                    e.Sender.PostMessage(Message.Types.Hold, parent, parent.ToSlot(), parent);
                    return true;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                default:
                    return false;
            }
        }

        public override void Query(GameObject parent, List<Interaction> list)//GameObjectEventArgs e)
        {
            // if (this.Size == 0)
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.PickUp, source: parent, name: "Pick up",//(Size == 0 ? "Pick up" : "Carry"),
                actorCond: new InteractionConditionCollection(
                    new InteractionCondition(actor => InventoryComponent.CheckWeight(actor, parent), "Too heavy")),//"I can't pick this up!")),
                effect: new NeedEffectCollection() { new NeedEffect("Holding") }));// InteractionEffect("Holding")));//, cond: new InteractionCondition("InRange", true, planType: AI.PlanType.FindNearest, parameters: agent=>(agent.Global - parent.Global).Length()));

            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Carry, source: parent, name: "Carry",
                actorCond:
                new InteractionConditionCollection(
                    new InteractionCondition(actor => InventoryComponent.CheckWeight(actor, parent), "I can't pick this up!")),
                effect: new NeedEffectCollection() { new NeedEffect("Carrying") }));// InteractionEffect("Carrying")));
        }

        //public override void Update(GameObject entity)
        //{
        //    ///TODO
        //    ///instead of checking the tile blow every update, make the cell below send a message to the entities above when it's removed

        //    MovementComponent positionComp = entity.GetComponent<MovementComponent>("Position");

        //    Position posNow = positionComp.Position;
        //    Position posBelow = new Position(posNow.Global - new Vector3(0, 0, 1));

        //    if (posBelow.GetCell().IsSolid)
        //    {
        //        FallSpeed = 0;
        //        return;
        //    }

        //    FallSpeed += Gravity;

        //    float heightDif = FallSpeed * GlobalVars.DeltaTime;
        //    if (heightDif > 1)
        //    {
        //        int heightRound = (int)Math.Round(heightDif);
        //        for (int i = 1; i < heightRound; i++)
        //        {
        //            Position check = new Position(posNow.Rounded - new Vector3(0, 0, i));
        //            if (check.GetCell().IsSolid)
        //                heightDif = check.Global.Z - (posNow.Rounded.Z - i);
        //        }
        //    }

        //    positionComp.SetPosition(entity, posNow.Global - new Vector3(0, 0, heightDif));
        //}

        //public override string ToString()
        //{
        //    //if(GlobalVars.DebugMode)
        //    //    return base.ToString();
        //    return "Walkspeed: " + WalkSpeed +
        //       // "\nSpeed: " + Speed +
        //        "\nSize: " + Size;
        //}

        public override object Clone()
        {
            //return new PhysicsComponent(WalkSpeed, Size, Solid, Height);
            PhysicsComponent phys = new PhysicsComponent();
            foreach (KeyValuePair<string, object> property in Properties)
            {
                phys.Properties[property.Key] = property.Value;
            }
            return phys;
        }

        //public Vector3 GetSpeed()
        //{
        //    return new Vector3(Speed.X, Speed.Y, Speed.Z);
        //}

        //public override string GetWorldText(GameObject actor)
        //{
        //    int size = GetProperty<int>("Size");
        //    return size >= 0 ? ("[" + GlobalVars.KeyBindings.PickUp + "]: " + (size == 0 ? "Pick up" : "Carry")) : "";
        //}
    }
}
