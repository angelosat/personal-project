using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    public class StickyComponent : Component
    {
        static float Gravity = -0.05f; //35f;

        //public int Size { get; private set; }
        //public float WalkSpeed;
        //public bool Solid;
        //public int Height;

        public override void Initialize(GameObject parent)
        {
         //   MakeSolid(parent, true);
        }

        //private void MakeSolid(GameObject parent, bool solid)
        //{
        //    if (!(bool)this["Solid"])
        //        return;

        //    Vector3 global = parent["Position"].GetProperty<Position>("Position").Global;
        //    float z = 0;
        //    while (z < Map.MaxHeight && z < (int)this["Height"])
        //    {
        //        Cell cell;
        //        if (Position.TryGetCell(parent.Map, global + new Vector3(0, 0, z), out cell))
        //            cell.Solid = solid;
        //        z++;
        //    }
        //}

        StickyComponent()
            : base()
        {
            Properties.Add("Weight");
            Properties.Add("Solid");
            Properties.Add("Height");
            Properties.Add("Size");
        }

        public StickyComponent(int size = 0, bool solid = false, int height = 1, float weight = 1)
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

        }

        //public void ApplyForce(Vector3 force)
        //{
        //    Speed += force;
        //}

        float Friction = 0.01f;// 0.005f;
        float terminalVelocity = 0.1f;

        //public override void Update(GameObject parent, Chunk chunk)
        //{
        //    ///TODO
        //    ///instead of checking the tile below every update, make the cell below send a message to the entities above when it's removed
        //    ///and when walking into a new cell
        //    //Console.WriteLine("phys");
        //    MovementComponent positionComp;
        //    if (!parent.TryGetComponent<MovementComponent>("Position", out positionComp))
        //        return;
        //    Position posNow = positionComp.GetProperty<Position>("Position");
        //    Vector3 next = Vector3.Zero, global = posNow.Global, speed = positionComp.GetProperty<Vector3>("Speed");
        //    Cell nextCell;
        //    Chunk nextChunk;
        //    float thisDensity = 0f;
        //    Cell thisCell;
        //    //if (!Position.TryGetCell(Position.Round(global), out thisCell))
        //    //    return;
        //    if (!posNow.TryGetCell(out thisCell))
        //        return;

        //    TileComponent tileComp = TileComponent.TileMapping[thisCell.TileType].GetComponent<TileComponent>("Physics");

        //    thisDensity = tileComp.GetProperty<float>(Stat.Density.Name);

        //    speed.X *= (1 - thisDensity);
        //    speed.Y *= (1 - thisDensity);
        //   // Console.WriteLine(speed);

        //    if (speed.Z == 0)
        //    {
        //        if (speed.X > 0)
        //            speed.X = Math.Max(0, speed.X - Friction);// * (float)GlobalVars.DeltaTime);
        //        else
        //            speed.X = Math.Min(0, speed.X + Friction);// * (float)GlobalVars.DeltaTime);
        //        if (speed.Y > 0)
        //            speed.Y = Math.Max(0, speed.Y - Friction);// * (float)GlobalVars.DeltaTime);
        //        else
        //            speed.Y = Math.Min(0, speed.Y + Friction);// * (float)GlobalVars.DeltaTime);

        //        Cell cellBelow;
        //        if (Position.TryGetCell(global + speed + new Vector3(0, 0, Gravity * (1 - thisDensity)), out cellBelow))
        //            if (!cellBelow.Solid)
        //                speed.Z = Gravity;// *(1 - thisDensity);
        //    }
        //    else
        //        speed.Z = Math.Max(-GlobalVars.DeltaTime * (1 - thisDensity), speed.Z + Gravity * GlobalVars.DeltaTime * (1 - thisDensity));
        //    //  speed *= (1-thisDensity);
        //    //positionComp["Speed"] = speed;// *(float)GlobalVars.DeltaTime;
        //    next = global + speed * GlobalVars.DeltaTime;// positionComp.GetProperty<Vector3>("Speed");// / (float)GlobalVars.DeltaTime;

        //    //check if can climb
        //    Vector3 nextRounded = Position.Round(next);
            
        //    if (!Position.TryGet(nextRounded, out nextCell, out nextChunk)) //if the next cell is out of bounds, return without updating position
        //    {
        //        speed = Vector3.Zero;
        //        positionComp["Speed"] = speed;
        //        return;
        //    }

        //    if (nextCell.Solid)
        //    {
        //        speed.Z = 0;
        //        next.Z = nextCell.Z + 1f;
        //        Cell tryMore;
        //        if (Position.TryGetCell(new Vector3(nextRounded.X, nextRounded.Y, nextCell.Z + 1f), out tryMore))
        //        {
        //            if (tryMore.Solid) //if can't climb, return without updating position
        //            {
        //                speed = Vector3.Zero;
        //                positionComp["Speed"] = speed;
        //               // Console.WriteLine(new Vector3(nextRounded.X, nextRounded.Y, nextCell.Z + 1f));
        //                return;
        //            }
        //            next.Z = nextCell.Z + 1f;
        //            nextCell = tryMore;
        //        }
        //    }
        //    else //if the next cell is not solid, do collision detection with other objects
        //    {
        //        if (speed != Vector3.Zero)
        //        {
        //            GameObject collided;
        //            if (TryCollision(nextRounded, parent, out collided))
        //            {
        //                speed = Vector3.Zero;
        //                positionComp["Speed"] = speed;
        //                return;
        //            }
        //        }
        //    }

        //    ///check if it fits height wise
        //    if (!CanFit(next))
        //    {
        //        speed = Vector3.Zero;
        //        positionComp["Speed"] = speed;
        //        return;
        //    }


        //    positionComp.Update3(parent, next);

        //    positionComp["Speed"] = speed;
        //    if (speed.Z != 0)
        //        positionComp.Spin += 0.1f;
        //    else positionComp.Spin = 0;
        //}

        bool CanFit(Map map, Vector3 next)
        {
            for (int h = 0; h < GetProperty<int>("Height"); h++)
            {
                //Cell fitCell;
                //if (Position.TryGetCell(map, next + new Vector3(0, 0, h), out fitCell))
                //    if (fitCell.Solid) // if cell exists and is solid, return without updating position
                //    {
                //        return false;
                //    }
                if ((next + new Vector3(0, 0, h)).IsSolid(map))
                    return false;
            }
            return true;
        }
        //void Fall(MovementComponent posComp)//, out Vector3 next)
        //{
        //    Vector3 speed = posComp.GetProperty<Vector3>("Speed");
        //    if (speed.Z == 0)
        //    {
        //        Cell cell = posComp.GetProperty<Position>("Position").GetCell();
        //        if (!cell.Solid)
        //            posComp["Speed"] = new Vector3(speed.X, speed.Y, Gravity);
        //    }
        //    else
        //        posComp["Speed"] = new Vector3(speed.X, speed.Y, speed.Z + Gravity);
        //        //posComp.GetProperty<Vector3>("Speed").Z += Gravity;
        //}
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
                bool objSolid = (bool)objPhys["Solid"];
                if (!objSolid)
                    continue;
                PositionComponent objPos;
                if (!objCheck.TryGetComponent<PositionComponent>("Position", out objPos))
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


        //void StepUp(MovementComponent posComp, Cell cell)//Now, Cell cellNext)
        //{
        //   // if (cellNow != cellNext)
        //        if (cell.Solid) //Next.Solid)
        //        {
        //            Position CurrentPosition = posComp.GetProperty<Position>("Position");
        //            CurrentPosition.Global = new Vector3(CurrentPosition.Global.X, CurrentPosition.Global.Y, cell.Z+1);//Next.Z + 1);
        //        }
        //}
        public override void Despawn(Net.IObjectProvider net, GameObject parent)
        {
            Chunk.RemoveObject(net.GetMap(), parent);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
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
                    if (GetProperty<int>("Size") < 0)
                        return true;
                    PositionComponent posComp;
                    if (parent.TryGetComponent<PositionComponent>("Position", out posComp))
                    {
                        if (sender["Stats"].GetProperty<float>(Stat.Strength.Name) < parent["Physics"].GetProperty<float>("Weight"))
                        {
                            Log.Enqueue(Log.EntryTypes.TooHeavy, Stat.Strength);
                            return true;
                        }
                        //if 
                        InventoryComponent.GiveObject(e.Network, sender, parent);
                         //   Map.RemoveObject(parent);
                        return true;
                    }
                    return true;
                default:
                    return base.HandleMessage(parent, e);
            }
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
            StickyComponent phys = new StickyComponent();
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
