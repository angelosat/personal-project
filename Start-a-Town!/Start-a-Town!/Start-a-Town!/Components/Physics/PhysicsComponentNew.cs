using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components
{
    public enum ObjectSize { Immovable = -1, Inventoryable, Haulable }
    public class PhysicsComponent : Component
    {
        public override string ComponentName
        {
            get
            {
                return "Physics";
            }
        }

        public ObjectSize Size { get { return (ObjectSize)this["Size"]; } set { 
            this["Size"] = value; 
        } }
        public bool Solid { get { return (bool)this["Solid"]; } set { this["Solid"] = value; } }
        public float Height;// { get { return (float)this["Height"]; } set { this["Height"] = value; } }
        public float Weight { get { return (float)this["Weight"]; } set { this["Weight"] = value; } }
        //  bool Floating { get { return (bool)this["Floating"]; } set { this["Floating"] = value; } }
        public bool Enabled = true;
        //static public float Gravity = -0.03f;//-0.04f;// -0.05f; //35f;
        //static public float Jump = 0.3f;//-0.04f;// -0.05f; //35f;
        //static public float Walk = 0.1f;
        //static public float Friction = 0.01f;// 0.005f;

        static public float Gravity = -0.015f;//-0.04f;// -0.05f; //35f;
        static public float Jump = 0.2f;//-0.04f;// -0.05f; //35f;
        static public float Walk = 0.08f;
        static public float Friction = 0.02f;// 0.005f;

        public PhysicsComponent()
            : base()
        {
            this.Initialize();
        }
        public PhysicsComponent(PhysicsComponent toCopy)
        {
            this.Height = toCopy.Height;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">-1: can't pick up, 0: inventory, 1: haulable</param>
        /// <param name="solid"></param>
        /// <param name="height"></param>
        /// <param name="weight"></param>
        public PhysicsComponent(int size = 0, bool solid = false, float height = 1, float weight = 1)//, bool sticky = false)
            : this()
        {
            //WalkSpeed = walkspeed;
            //Size = size;
            //Solid = solid;
            //Height = height;

            // TODO: make height a float
            Properties["Weight"] = weight;
            //Properties["Height"] = height
            this.Height = height;
            Properties["Solid"] = solid;
            Properties["Size"] = size;
            // this.Floating = floating;
            //  this["Sticky"] = sticky;
        }

        public PhysicsComponent Initialize(int size = 0, bool solid = false, float height = 1, float weight = 1)
        {
            this.Size = (ObjectSize)size;
            this.Solid = false;
            this.Height = height;
            this.Weight = weight;
            return this;
        }


        float terminalVelocity = 0.1f;

        /// <summary>
        /// make positioncomponent, global, velocity, height, map, fields instead of properties
        /// </summary>
        /// <param name="net"></param>
        /// <param name="parent"></param>
        /// <param name="chunk"></param>
        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk)
        {
            UpdateParticles(parent);
            if (!this.Enabled)
                return;
            var map = parent.Net.Map;

            Vector3 lastGlobal = parent.Transform.Global;// posNow.Global;
            Cell thisCell;
            //if (!lastGlobal.TryGetCell(map, out thisCell))
            if (!map.TryGetCell(lastGlobal, out thisCell))
                return;
            Vector3 velocity = parent.Transform.Position.Velocity;// positionComp.Position.Velocity;

            //Block feetblock = thisCell.Block;// Block.Registry[thisCell.Block.Type];
            var feetposition = lastGlobal + Vector3.UnitZ * .1f;
            var feetcell = map.GetCell(feetposition);
            Block feetblock = feetcell.Block;// Block.Registry[thisCell.Block.Type];

            //float density = 1 - (block.IsSolid(map, lastGlobal) ? block.Density : 0); //net
            //float density = 1 - (!block.IsSolid(map, lastGlobal) ? block.Density : 0); //net

            // TODO: make get density method on block that takes into account its height
            //float density = (feetblock.IsSolid(map, feetposition) ? feetblock.Density : 0); //net
            //float density = feetblock.Density;//
            float density = feetblock.GetDensity(feetcell.BlockData, feetposition);
            velocity = velocity * (1-density);
            //velocity = velocity.Round(2);

            float nx, ny, nz;
            BoundingBox box = new BoundingBox(lastGlobal, lastGlobal + new Vector3(0, 0, Height));

            //nz = ResolveVertical(map, box, velocity, density);
            //nz = ResolveVertical(parent, box, velocity, density);
            nz = ResolveVertical(parent, map, box, ref velocity, density);
            //if (velocity == Vector3.Zero)
            //{
            //    this.Enabled = false;
            //    return; // don't perform any more calculations if entity is stationary
            //}
            //velocity.Z += Gravity;// *GlobalVars.DeltaTime;
            if (nz == lastGlobal.Z)
            {
                //velocity.Z = 0;
                if (velocity.X > 0)
                    velocity.X = Math.Max(0, velocity.X - Friction);
                else
                    velocity.X = Math.Min(0, velocity.X + Friction);
                if (velocity.Y > 0)
                    velocity.Y = Math.Max(0, velocity.Y - Friction);
                else
                    velocity.Y = Math.Min(0, velocity.Y + Friction);
            }

            //if (velocity.X != 0)
            //    nx = ResolveHorizontal(net, parent, map, box, ref velocity, Vector2.UnitX).X;
            //else
            //    nx = lastGlobal.X;

            //if (velocity.Y != 0)
            //    ny = ResolveHorizontal(net, parent, map, box, ref velocity, Vector2.UnitY).Y;
            //else
            //    ny = lastGlobal.Y;

            if (velocity.X != 0)
                nx = ResolveHorizontalAABB(net, parent, map, box, ref velocity, Vector2.UnitX, nz, out nz).X;
            else
                nx = lastGlobal.X;

            if (velocity.Y != 0)
                ny = ResolveHorizontalAABB(net, parent, map, box, ref velocity, Vector2.UnitY, nz, out nz).Y;
            else
                ny = lastGlobal.Y;

            Vector3 next = ResolveCorner(map, lastGlobal, new Vector3(nx, ny, nz));

            // positionComp.Update3(parent, next);
            if (lastGlobal != next)
                parent.ChangePosition(next);
            else if (velocity == Vector3.Zero)
            {
                this.Enabled = false;
                //return; // don't perform any more calculations if entity is stationary
            }
            this.DetectEntityCollisions(parent, lastGlobal, next);
            // reset speed according to new position to prevent it from accumulating
            //velocity.X = next.X - lastGlobal.X;
            //velocity.Y = next.Y - lastGlobal.Y;
            //velocity = next - lastGlobal;

            //positionComp.Position.Velocity = velocity;
            //parent.Velocity = velocity;
            parent.Transform.Position.Velocity = velocity;
            //if (velocity == Vector3.Zero)
            //{
            //    this.Enabled = false;
            //    //return; // don't perform any more calculations if entity is stationary
            //}
            
            // log state change 
            if(parent.Exists) // must check because entity might have despawned itself during it's update 
            if (lastGlobal != parent.Transform.Global)
            {
                // send a "step on" message on next block
                net.LogStateChange(parent.Network.ID);
                Vector3 nextRounded = next.RoundXY();
                if (nextRounded != lastGlobal.RoundXY())
                {
                    net.PostLocalEvent(parent, Message.Types.EntityMovedCell, lastGlobal.RoundXY(), nextRounded);
                    net.EventOccured(Message.Types.EntityMovedCell, parent);
                    Vector3 blockGlobal = nextRounded - Vector3.UnitZ;
                    //blockGlobal.GetBlock(map).HandleMessage(blockGlobal, ObjectEventArgs.Create(net, Message.Types.StepOn, parent));
                    //Block.HandleMessage(net, blockGlobal, ObjectEventArgs.Create(net, Message.Types.StepOn, parent));
                    parent.Map.GetBlock(blockGlobal).OnSteppedOn(parent, blockGlobal);

                    //GameObject blockObj;
                    //if (Cell.TryGetObject(map, nextRounded - Vector3.UnitZ, out blockObj))
                    //    net.PostLocalEvent(blockObj, Message.Types.StepOn, parent);
                }
                //if (global.Round() != parent.Global.Round())
                //{
                //    GameObject blockObj;
                //    if (Cell.TryGetObject(map, parent.Global - Vector3.UnitZ, out blockObj))
                //        net.PostLocalEvent(blockObj, Message.Types.StepOn, parent);
                //}
            }
        }

        static private Vector3 ResolveCorner(IMap map, Vector3 current, Vector3 next)
        {
            if (next == current)
                return current;
            //if (!next.IsSolid(map))
            if (!map.IsSolid(next))
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
            //if (map.IsSolid(newNext))
            //    " EKALA WTWAR".ToConsole();
            return newNext;
        }

        /// <summary>
        /// TODO: maybe make it static and do a callback or return something if the entity has landed?
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="map"></param>
        /// <param name="box"></param>
        /// <param name="speed"></param>
        /// <param name="density"></param>
        /// <returns></returns>
        private float ResolveVertical(GameObject parent, IMap map, BoundingBox box, ref Vector3 speed, float density)
        {
            //Map map = parent.Map;
            var adjustedGravity = Gravity;// *(1 - density);
            Vector3 global = box.Min;
            if (speed.Z == 0)
            {
                // if (CanFall(map, global, speed, density))
                //if (!(global + new Vector3(0, 0, Gravity)).IsSolid(map))

                if (!map.IsSolid(global + new Vector3(0, 0, adjustedGravity)))// Gravity)))
                {
                    speed.Z = Gravity;
                    //return global.Z + density * Gravity;
                    return global.Z + adjustedGravity;

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
                        if (map.IsSolid(check))
                        {
                            parent.Net.PostLocalEvent(parent, Message.Types.HitGround, Math.Abs(speed.Z));
                            speed.Z = 0;
                            // land
                            Land(parent, check);
                            return (int)check.Z + Block.GetBlockHeight(map, check);
                        }
                    }
                }
                else if (height > 0)
                {
                    //if (next.IsSolid(map))
                    if (map.IsSolid(next))
                    {
                        var f = speed.Z;
                        parent.Net.SyncEvent(parent, Message.Types.HitGround, w => w.Write(f));
                        speed.Z = 0;
                        // land
                        Land(parent, next);

                        var blockheightbelow = Block.GetBlockHeight(map, next);
                        return (int)next.Z + blockheightbelow;
                    }
                }
            }
            else
            {
                // hit on cieling
                if (map.IsSolid(box.Max + offset))
                {
                    //speed.Z = 0;
                    // dont set velocity z to 0, because later we check if velocity is zero and we disable physics. 
                    // reflect velocity instead
                    speed.Z = -speed.Z;
                    return box.Min.Z;
                }
            }
            speed.Z += adjustedGravity;//Gravity;
            return next.Z;
        }
        
       
        private static Vector3 ResolveHorizontal(IObjectProvider net, GameObject parent, IMap map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis, float nz, out float zz)
        {
            zz = nz;
            Vector3 unit = new Vector3(horAxis, 0) * velocity;// *GlobalVars.DeltaTime;
            Vector3 next = boxGlobal.Min + unit;
            int min = (int)boxGlobal.Min.Z, max = (int)boxGlobal.Max.Z;
            int d = max - min;
            //for (int z = 0; z < d; z++)
            for (float z = 0; z < d; z+=0.5f) // increment by 0.5f so we dont miss the solid part of a less than full height block
            {
                Vector3 check = next + new Vector3(0, 0, z);
                //Cell cell;
                //if (!map.TryGetCell(check, out cell))
                //{
                //    net.EventOccured(Message.Types.EntityEnteringUnloadedChunk, parent);
                //    return boxGlobal.Min;
                //}
                // WARNING
                if (map.IsSolid(check))
                {
                    // check if can climb
                    if (velocity.Z == 0 && z == 0)
                    {
                        var step = check + Vector3.UnitZ / 2f;// +Vector3.UnitZ * 0.01f;
                        if (!map.IsSolid(step))
                        {
                            //var hbottom = Block.GetBlockHeight(map, check);
                            //var htop = Block.GetBlockHeight(map, check + Vector3.UnitZ);
                            var hbottom = map.GetSolidObjectHeight(check);
                            var htop = map.GetSolidObjectHeight(check + Vector3.UnitZ);
                            var h = hbottom + htop;
                            var floor = new Vector3(check.X, check.Y, (float)Math.Floor(check.Z));
                            var stepon = floor + Vector3.UnitZ * (h + + 0.01f);
                            zz = stepon.Z;
                            return stepon;
                        }
                    }
                    //if (map.GetBlock(check) == Block.Water)
                    //    "asdasd".ToConsole();
                    //(map.GetBlock(check) + " " + boxGlobal.Min.ToString()).ToConsole();
                    BlockCollision(net, parent, next);
                    return boxGlobal.Min;
                }
            }
            velocity = velocity * (Vector3.One - unit) + unit * (next - boxGlobal.Min);
            return next;
        }

        private static Vector3 ResolveHorizontalAABB(IObjectProvider net, GameObject parent, IMap map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis, float nz, out float zz)
        {
            //var box = new BoundingBox(boxGlobal.Min - (Vector3.UnitX + Vector3.UnitY) * .5f, boxGlobal.Max + (Vector3.UnitX + Vector3.UnitY) * .5f);
            Vector3 origin = boxGlobal.Min;
            //if (velocity.X <= 0 && velocity.Y <= 0)
            //    origin = box.Min;
            //else if (velocity.X <= 0 && velocity.Y >= 0)
            //    origin = box.Min + Vector3.UnitY;
            //else if (velocity.X >= 0 && velocity.Y <= 0)
            //    origin = box.Min + Vector3.UnitX;
            //else if (velocity.X >= 0 && velocity.Y >= 0)
            //    origin = box.Min + Vector3.UnitX + Vector3.UnitY;

            zz = nz;
            Vector3 step = new Vector3(horAxis, 0) * velocity;// *GlobalVars.DeltaTime;
            var leadingFace = step;
            leadingFace.Normalize();
            leadingFace *= .25f; // half width of the entity's bounding box
            Vector3 next = origin + leadingFace + step;
            int minz = (int)boxGlobal.Min.Z, maxz = (int)boxGlobal.Max.Z;
            int dz = maxz - minz;

            for (float z = 0; z < dz; z += 0.5f) // increment by 0.5f so we dont miss the solid part of a less than full height block
            {
                Vector3 check = next + new Vector3(0, 0, z);
                if (map.IsSolid(check))
                {
                    // check if can climb
                    if (velocity.Z == 0 && z == 0)
                    {
                        var stepUp = check + Vector3.UnitZ / 2f;// +Vector3.UnitZ * 0.01f;
                        if (!map.IsSolid(stepUp))
                        {
                            var hbottom = map.GetSolidObjectHeight(check);
                            var htop = map.GetSolidObjectHeight(check + Vector3.UnitZ);
                            var h = hbottom + htop;
                            var floor = new Vector3(check.X, check.Y, (float)Math.Floor(check.Z));
                            var stepon = floor + Vector3.UnitZ * (h + +0.01f);
                            zz = stepon.Z;
                            return stepon;
                        }
                    }
                    BlockCollision(net, parent, next);
                    return boxGlobal.Min;
                }
            }
            velocity = velocity * (Vector3.One - step) + step * (origin + step - boxGlobal.Min);
            return boxGlobal.Min + step;// next;
        }


        private static Vector3 ResolveHorizontal(IObjectProvider net, GameObject parent, IMap map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis)
        {
            Vector3 unit = new Vector3(horAxis, 0) * velocity;// *GlobalVars.DeltaTime;
            Vector3 next = boxGlobal.Min + unit;
            int min = (int)boxGlobal.Min.Z, max = (int)boxGlobal.Max.Z;
            int d = max - min;
            for (int z = 0; z < d; z++)
            {
                Vector3 check = next + new Vector3(0, 0, z);
                Cell cell;
                if (!map.TryGetCell(check, out cell))
                {
                    net.EventOccured(Message.Types.EntityEnteringUnloadedChunk, parent);
                    return boxGlobal.Min;
                }
                // WARNING
                if (map.IsSolid(check))
                {
                    // check if can step over
                    var step = check + Vector3.UnitZ / 2f;
                    if (!map.IsSolid(step))
                    {
                        var h = Block.GetBlockHeight(map, step);
                        var stepon = check + Vector3.UnitZ * h;
                        return stepon;
                    }

                    BlockCollision(net, parent, next);
                    return boxGlobal.Min;
                }
            }
            velocity = velocity * (Vector3.One - unit) + unit * (next - boxGlobal.Min);
            return next;
        }

        static void BlockCollision(IObjectProvider net, GameObject parent, Vector3 next)
        {
            net.PostLocalEvent(parent, Message.Types.BlockCollision, next);
        }

        private void ApplyFriction(ref Vector3 speed)
        {
            if (speed.X > 0)
                speed.X = Math.Max(0, speed.X - Friction);
            else
                speed.X = Math.Min(0, speed.X + Friction);
            if (speed.Y > 0)
                speed.Y = Math.Max(0, speed.Y - Friction);
            else
                speed.Y = Math.Min(0, speed.Y + Friction);
        }
        private static bool CanFall(IMap map, Vector3 global, Vector3 speed, float density)
        {
            //return !(global + speed + new Vector3(0, 0, Gravity * density)).IsSolid(map);
            return !map.IsSolid(global + speed + new Vector3(0, 0, Gravity * density));

        }
        private static void ContinueFalling(ref Vector3 speed, float thisDensity)
        {
            //speed.Z = Math.Max(-GlobalVars.DeltaTime * thisDensity, speed.Z + Gravity * GlobalVars.DeltaTime * thisDensity);
            speed.Z = Math.Max(-thisDensity, speed.Z + Gravity * thisDensity);
        }

        void DetectEntityCollisions(GameObject parent, Vector3 last, Vector3 next)
        {
            //foreach (var obj in parent.GetNearbyObjects(range: (r) => r <= 1))
            //{

            //}
            if (next == last)
                return;
            foreach (Chunk ch in parent.Map.GetChunks(parent.Map.GetChunk(last).MapCoords)) // TODO: optimize, search for entities on nearby chunks only if entity is on current chunk edge
                foreach (GameObject obj in ch.GetObjects())
                {
                    if (obj == parent)
                        continue;
                    var lastDistance = Vector3.Distance(obj.Global, last);
                    var nextDistance = Vector3.Distance(obj.Global, next);
                    if(lastDistance > 1 && nextDistance <= 1)
                    {
                        // collision
                        obj.Net.PostLocalEvent(obj, ObjectEventArgs.Create(Message.Types.EntityCollision, new object[] { parent }));
                        obj.Net.EventOccured(Message.Types.EntityCollision, parent, obj);
                    }
                }
        }


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

                if (!(bool)objPhys["Solid"])
                    continue;

                Vector3 objGlobal = objCheck.Global;
                Cell objCell = Position.GetCell(map, objGlobal);
                //if (objCell == nextCell)
                if (objCell == nextCell)
                {
                    collided = objCheck;
                    return true;
                }
            }
            return false;
        }

        // WARNING! i moved this over to movementcomponent
        //public override void Spawn(Net.IObjectProvider net, GameObject parent)
        //{
        //    Chunk.AddObject(parent, parent.Map);
        //}

        //public override void Remove(GameObject parent)
        //{
        //    if (!Chunk.RemoveObject(parent))
        //        throw new Exception();
        //}

        public override void Instantiate(GameObject parent, Action<GameObject> instantiator)
        {
            //instantiator(parent);
        }

        public override void Spawn(Net.IObjectProvider net, GameObject parent)
        {
            //net.Instantiate(parent);
            Chunk chunk;
            //if (!parent.Global.TryGetChunk(net.Map, out chunk))
            if (!net.Map.TryGetChunk(parent.Global, out chunk))
            {
                net.EventOccured(Message.Types.SpawnChunkNotLoaded, parent.Global.GetChunkCoords());
                throw new Exception("Chunk not loaded");
                return;
            }
            //if (!parent.Global.GetChunk(net.Map).GetObjects().FirstOrDefault(o => o.Network.ID == parent.Network.ID).IsNull())
            if (!net.Map.GetChunk(parent.Global).GetObjects().FirstOrDefault(o => o.Network.ID == parent.Network.ID).IsNull())
            {
                throw new Exception("Tried to spawn already spawned object: " + parent.Name);
            }


            Chunk.AddObject(parent, net.Map);// parent.Map);
            parent.GetComponent<PositionComponent>().Exists = true;
            this.Enabled = true;
        }


        public override void Despawn(GameObject parent)
        {
            if (parent.Exists)
                if (!Chunk.RemoveObject(parent.Map, parent))// { }
                    throw new Exception();
            //parent.GetComponent<PositionComponent>().Exists = false;
            parent.Exists = false;
            //parent.TryRemoveComponent<Components.PositionComponent>(); // TODO: remove this component by reference
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {


                case Message.Types.PickUp:
                    // sender = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;
                    e.Data.Translate(e.Network, r => sender = TargetArgs.Read(e.Network, r).Object);
                    if (parent == sender)
                    {
                        Log.Enqueue(Log.EntryTypes.Default, sender.Name + " tried to pick themselves up.");
                        return true;
                    }
                    switch (this.Size)
                    {
                        case ObjectSize.Inventoryable:
                            e.Network.PostLocalEvent(sender, ObjectEventArgs.Create(Message.Types.Receive, new object[] { parent }));
                            //Interaction.StartNew(e.Network, sender,
                            //    new Interaction(
                            //        ()=>
                            break;

                        case ObjectSize.Haulable:
                            e.Network.PostLocalEvent(sender, ObjectEventArgs.Create(Message.Types.Hold, new object[] { parent }));
                            break;

                        default:
                            break;
                    }
                    //GameObject.PostMessage(e.Sender, Message.Types.Receive, parent, parent.ToSlot(), parent);//

                    return true;
                //case Message.Types.Carry:
                //    throw new NotImplementedException();
                //    //e.Sender.PostMessage(Message.Types.Hold, parent, parent.ToSlot(), parent);
                //    return true;

                case Message.Types.Attacked:
                    return true;
                    GameObject attacker = e.Parameters[0] as GameObject;
                    Attack attack = e.Parameters[1] as Attack;
                    //if (attack.Momentum == Vector3.Zero)
                    //   return true;

                    float knockResistance = 1 - Math.Min(1, StatsComponent.GetStatOrDefault(parent, Stat.Types.KnockbackResistance, 0f));

                    //e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.ApplyForce, new object[] { attack.GetMomentum() * knockResistance }));
                    Vector3 knockback = attack.GetMomentum() * knockResistance;
                    parent.Velocity += knockback;
                    return true;

                    //case Message.Types.ApplyForce:
                    //    this.ApplyForce((Vector3)e.Parameters[0]);
                    return true;


                case Message.Types.EntityCollision:
                    this.Collision(parent, e.Parameters[0] as GameObject);
                    return true;

                default:
                    return base.HandleMessage(parent, e);
            }
        }

        void Collision(GameObject parent, GameObject obj)
        {
            if (parent.Net is Net.Client)
                return; // disable temporarily until i make it server-side only
            //if (parent.StackSize == parent.StackMax)
            //    return;
            if (parent.ID != obj.ID)
                return;
            if (parent.StackSize + obj.StackSize >= parent.StackMax)
                return;
            obj.StackSize += parent.StackSize;
            parent.Despawn();
            parent.Dispose();
            //Net.Server.Instance.Enqueue(new Net.Packets.PacketMergeEntities(obj, parent), parent.Global, true);
            //Net.Server.Instance.Enqueue(new Net.Packets.PacketMergeEntities(obj, parent), parent.Global, true);
            Net.Server.Instance.Enqueue(PacketType.MergeEntities, Network.Serialize(w => Net.Packets.PacketMergeEntities.Write(w, obj, parent)), SendType.OrderedReliable, parent.Global, true);
        }

        //void ApplyForce(Vector3 force)
        //{
        //    this["Speed"] = GetProperty<Vector3>("Speed") + force;
        //}


        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            // if (this.Size == 0)
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.PickUp, source: parent, name: "Pick up",//(Size == 0 ? "Pick up" : "Carry"),
                cond: new ConditionCollection(
                    new Condition((actor, target) => InventoryComponent.CheckWeight(actor, parent), "Too heavy")),//"I can't pick this up!")),
                effect: new NeedEffectCollection() { new AIAdvertisement("Holding") }));// InteractionEffect("Holding")));//, cond: new InteractionCondition("InRange", true, planType: AI.PlanType.FindNearest, parameters: agent=>(agent.Global - parent.Global).Length()));

            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Carry, source: parent, name: "Carry",
                cond:
                new ConditionCollection(
                    new Condition((actor, target) => InventoryComponent.CheckWeight(actor, parent), "I can't pick this up!")),
                effect: new NeedEffectCollection() { new AIAdvertisement("Carrying") }));// InteractionEffect("Carrying")));
        }

        public override void GetInteractions(GameObject parent, List<Interaction> actions)
        {
            actions.Add(new PickUp());
            actions.Add(new DropCarriedSnap());
            actions.Add(new InteractionObserve());
            //actions.AddRange(Interactions);
        }

        //static List<Interaction> Interactions = new List<Interaction>(){
        //    new PickUp(), 
        //    new DropCarriedSnap(),
        //    new InteractionObserve()
        //};

        public override object Clone()
        {
            //return new PhysicsComponent(WalkSpeed, Size, Solid, Height);
            PhysicsComponent phys = new PhysicsComponent(this);
            foreach (KeyValuePair<string, object> property in Properties)
            {
                phys.Properties[property.Key] = property.Value;
            }
            return phys;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            //tooltip.Controls.Add(new Label("Weight: " + this.Weight) { Location = tooltip.Controls.BottomLeft });
            if (this.Size != ObjectSize.Immovable)
            {
                var lbl = new Label("Weight: ") { Location = tooltip.Controls.BottomLeft, TextColorFunc = () => Color.LightGray };
                Color color = Color.LightGray;
                if (Player.Actor != null)
                {
                    //color = Interactions.Lift.CheckWeight(Player.Actor, new TargetArgs(parent)) ? Color.Lime : Color.Red;
                    color = HaulComponent.CheckWeight(Player.Actor, parent) ? Color.Lime : Color.Red;

                }
                var lblvalue = new Label(this.Weight.ToString()) { Location = lbl.TopRight, TextColorFunc = () => color };
                tooltip.Controls.Add(lbl, lblvalue);
            }
        }
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interactions.Interaction> list)
        {
            //list.Add(new PlayerInput(PlayerActions.PickUp, true), new Interactions.Lift());
            if (this.Size == ObjectSize.Immovable)
                return;

            //list.Add(new PlayerInput(PlayerActions.PickUp), new Interactions.PickUp());
            //list.Add(new PlayerInput(PlayerActions.PickUp, true), new Interactions.Lift());

            list.Add(new PlayerInput(PlayerActions.Interact), new Interactions.PickUp());
            //list.Add(new PlayerInput(PlayerActions.Interact, true), new Interactions.Lift());

            //if (this.Size == ObjectSize.Inventoryable)
            //    list.Add(new PlayerInput(PlayerActions.PickUp), new Interactions.PickUp());
            //else if (this.Size == ObjectSize.Haulable)
            //    list.Add(new PlayerInput(PlayerActions.PickUp), new Interactions.Lift());
        }
        internal override ContextAction GetContextRB(GameObject parent, GameObject player)
        {
            if (this.Size == ObjectSize.Immovable)
                return null;
            return new ContextAction(new Interactions.PickUp()) { Shortcut = PlayerInput.RButton, Available = () => this.Size != ObjectSize.Immovable };
        }
        public override void GetHauledActions(GameObject parent, TargetArgs target, List<Interactions.Interaction> list)
        {
            //if (this.Size == ObjectSize.Immovable)
            //    return;
            //list.Add(new Interactions.PickUp());
        }
        public override string ToString()
        {
            return "Enabled: " + this.Enabled.ToString() + "\n" + base.ToString();
        }

        static public void Enable(GameObject parent)
        {
            parent.TryGetComponent<PhysicsComponent>(f => f.Enabled = true);
        }

        //public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        //{
        //    var list = new Dictionary<PlayerInput, Interactions.Interaction>();
        //    this.GetPlayerActionsWorld(parent, list);
        //    foreach (var i in list)
        //        actions.Add(new ContextAction(i.Key.ToString() + ": " + i.Value.Name, () => true));
        //}

        /// <summary>
        /// TODO: pass cell or block in here since i fetch it (by checking if the position is solid) in the check before calling this method
        /// </summary>
        /// <param name="vector3"></param>
        private void Land(GameObject parent, Vector3 vector3)
        {
            if (parent.Net is Server)
                return;
            var block = parent.Map.GetBlock(vector3);
            var emitter = block.GetEmitter();//.GetDustEmitter();
            emitter.Source = parent.Global;// +Vector3.UnitZ;
            //emitter.Lifetime = Engine.TargetFps * .1f;

            emitter.Emit(10);//, -parent.Velocity * .1f);
            this.Emitters.Add(emitter);
        }
        List<ParticleEmitter> Emitters = new List<ParticleEmitter>();

        public override void Draw(MySpriteBatch sb, GameObject parent, Camera camera)
        {
            foreach (var e in this.Emitters)
                e.Draw(camera, parent.Map, e.Source);
        }
        private void UpdateParticles(GameObject parent)
        {
            foreach (var e in this.Emitters.ToList())
            {
                e.Update(parent.Map, e.Source);
                if (e.Particles.Count == 0)
                    this.Emitters.Remove(e);
            }
        }
    }
}
