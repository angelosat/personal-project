using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Physics
{
    class Physics
    {
        static public float Friction { get { return PhysicsComponent.Friction; } }
        static float Gravity { get { return PhysicsComponent.Gravity; } }

        static public void Update(float weight, float friction, IMap map, Vector3 global, Vector3 velocity, out Vector3 nextglobal, out Vector3 nextvelocity)
        {
          
            //Vector3 lastGlobal = parent.Transform.Global;
            Cell thisCell;// = map.GetCell(global);
            if (!map.TryGetCell(global, out thisCell))
            {
                nextglobal = global;
                nextvelocity = velocity;
                return;
            }
            //Vector3 velocity = parent.Transform.Position.Velocity;// positionComp.Position.Velocity;

            Block block = thisCell.Block;
            float density = 1 - (block.IsSolid(thisCell, global.ToBlock()) ? block.Density : 0);

            velocity = velocity * density;

            float nx, ny, nz;

            BoundingBox box = new BoundingBox(global, global);// + new Vector3(0, 0, Height));

            var fric = Math.Min(1, 2 * Friction * friction);// Friction* friction;// Friction / 2f;
            nz = ResolveVertical(map, box, weight, ref velocity, density);
            if (nz == global.Z)
            {
                if (velocity.X > 0)
                    velocity.X = Math.Max(0, velocity.X - fric);
                else
                    velocity.X = Math.Min(0, velocity.X + fric);
                if (velocity.Y > 0)
                    velocity.Y = Math.Max(0, velocity.Y - fric);
                else
                    velocity.Y = Math.Min(0, velocity.Y + fric);
            }


            if (velocity.X != 0)
                nx = ResolveHorizontal(fric, map, box, ref velocity, Vector2.UnitX, nz, out nz).X;
            else
                nx = global.X;

            if (velocity.Y != 0)
                ny = ResolveHorizontal(fric, map, box, ref velocity, Vector2.UnitY, nz, out nz).Y;
            else
                ny = global.Y;

            Vector3 next = ResolveCorner(map, global, new Vector3(nx, ny, nz));

            nextvelocity = velocity;
            nextglobal = next;

        }
        static private Vector3 ResolveCorner(IMap map, Vector3 current, Vector3 next)
        {
            if (next == current)
                return current;
            if (!Block.IsBlockSolid(map, next))
                return next;
            Vector3 newNext;
            Vector3 nextRound = next.RoundXY();
            // TODO: do something without square root for speed
            float dx = Math.Abs(next.X - nextRound.X);
            float dy = Math.Abs(next.Y - nextRound.Y);
            if (dx < dy)
                newNext = new Vector3(next.X, current.Y, next.Z);
            else
                newNext = new Vector3(current.X, next.Y, next.Z);
            return newNext;
        }
        static private float ResolveVertical(IMap map, BoundingBox box, float weight,  ref Vector3 speed, float density)
        {
            var grav = Gravity * weight;
            Vector3 global = box.Min;
            if (speed.Z == 0)
            {
                if (!Block.IsBlockSolid(map, global + new Vector3(0, 0, grav)))
                {
                    speed.Z = grav;
                    return global.Z + density * grav;
                }
                return global.Z;
            }
            Vector3 offset = new Vector3(0, 0, speed.Z);// * GlobalVars.DeltaTime);
            Vector3 next = global + offset;
            //Cell cell;
            if (speed.Z < 0)
            {
                float height = global.Z - next.Z;
                if (height > 1)
                {
                    for (int i = 0; i < (int)Math.Ceiling(height); i++)
                    {
                        Vector3 check = global - new Vector3(0, 0, 1 + i);
                        if (Block.IsBlockSolid(map, check))
                        {
                            //parent.Net.PostLocalEvent(parent, Message.Types.HitGround, Math.Abs(speed.Z));
                            speed.Z = 0;
                            return (int)check.Z + Block.GetBlockHeight(map, check);
                        }
                    }
                }
                else if (height > 0)
                {
                    //if (next.IsSolid(map))
                    if (Block.IsBlockSolid(map, next))
                    {
                        var f = speed.Z;
                        //parent.Net.SyncEvent(parent, Message.Types.HitGround, w => w.Write(f));
                        speed.Z = 0;
                        var blockheightbelow = Block.GetBlockHeight(map, next);
                        return (int)next.Z + blockheightbelow;
                    }
                }
            }
            else
            {
                // WARNING
                //if ((box.Max + offset).IsSolid(map))
                if (Block.IsBlockSolid(map, box.Max + offset))
                {
                    speed.Z = 0;
                    return box.Min.Z;
                }
            }
            speed.Z += grav;
            return next.Z;
        }
        private static Vector3 ResolveHorizontal(float friction, IMap map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis, float nz, out float zz)
        {
            zz = nz;
            Vector3 unit = new Vector3(horAxis, 0) * velocity;

            Vector3 next = boxGlobal.Min + unit;
            int min = (int)boxGlobal.Min.Z, max = (int)boxGlobal.Max.Z;
            int d = max - min;
            for (float z = 0; z < 1; z+=0.5f)//d; z += 0.5f) // increment by 0.5f so we dont miss the solid part of a less than full height block
            {
                Vector3 check = next;// +new Vector3(0, 0, z);
                //Cell cell;
                //if (!map.TryGetCell(check, out cell))
                //{
                //    net.EventOccured(Message.Types.EntityEnteringUnloadedChunk, parent);
                //    return boxGlobal.Min;
                //}
                // WARNING
                if(Block.IsBlockSolid(map, check))
                {
                    // check if can climb
                    if(false)
                    if (velocity.Z == 0 && z == 0)
                    {
                        var step = check + Vector3.UnitZ / 2f;// +Vector3.UnitZ * 0.01f;
                        if (!Block.IsBlockSolid(map, step))
                        {
                            //var hbottom = Block.GetBlockHeight(map, check);
                            //var htop = Block.GetBlockHeight(map, check + Vector3.UnitZ);
                            var hbottom = map.GetSolidObjectHeight(check);
                            var htop = map.GetSolidObjectHeight(check + Vector3.UnitZ);
                            var h = hbottom + htop;
                            var floor = new Vector3(check.X, check.Y, (float)Math.Floor(check.Z));
                            var stepon = floor + Vector3.UnitZ * (h + +0.01f);
                            zz = stepon.Z;
                            return stepon;
                        }
                    }
                    // if sliding against a wall, take friction into equation

                    //unit *= (1 - friction);
                    //zz = boxGlobal.Min.Z + (nz - boxGlobal.Min.Z) * (1 - friction);

                    //BlockCollision(net, parent, next);
                    //next = boxGlobal.Min;
                    return boxGlobal.Min;
                }
            }
            velocity = velocity * (Vector3.One - unit) + unit * (next - boxGlobal.Min);
            return next;
        }
        void DetectEntityCollisions(IMap map, Vector3 last, Vector3 next)
        {
            //foreach (var obj in parent.GetNearbyObjects(range: (r) => r <= 1))
            //{

            //}
            if (next == last)
                return;
            foreach (Chunk ch in map.GetChunks(map.GetChunk(last).MapCoords)) // TODO: optimize, search for entities on nearby chunks only if entity is on current chunk edge
                foreach (GameObject obj in ch.GetObjects())
                {
                    //if (obj == parent)
                    //    continue;
                    var lastDistance = Vector3.Distance(obj.Global, last);
                    var nextDistance = Vector3.Distance(obj.Global, next);
                    //if (lastDistance > 1 && nextDistance <= 1)
                    //{
                    //    // collision
                    //    obj.Net.PostLocalEvent(obj, ObjectEventArgs.Create(Message.Types.EntityCollision, new object[] { parent }));
                    //    obj.Net.EventOccured(Message.Types.EntityCollision, parent, obj);
                    //}
                }
        }
        //static void BlockCollision(IObjectProvider net, GameObject parent, Vector3 next)
        //{
        //    net.PostLocalEvent(parent, Message.Types.BlockCollision, next);
        //}
    }
}
