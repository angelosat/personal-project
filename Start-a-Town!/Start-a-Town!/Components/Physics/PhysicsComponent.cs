using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components
{
    public enum ObjectSize { Immovable = -1, Inventoryable, Haulable }
    public class PhysicsComponent : EntityComponent
    {
        public override string ComponentName { get; } = "Physics";
        public ObjectSize Size;
        public bool Solid;
        public float Height;
        public float Weight;
        public bool Enabled = true;
        const float FrictionFactor = .5f;
        public static float Jump = 0.2f;//-0.04f;// -0.05f; //35f;
        public static float Friction = 0.02f;// 0.005f; // TODO move this to blocks? 
        public bool MidAir { get; private set; } // => this.Parent.Velocity.Z != 0;// HACK because checking velocity.z == 0 returns true at the peak of the jump 
        public const int KnockbackMagnitude = 3;

        public PhysicsComponent()
            : base()
        {
            this.Initialize();
        }
        public PhysicsComponent(PhysicsComponent toCopy)
        {
            this.Height = toCopy.Height;
        }
       
        public PhysicsComponent Initialize(int size = 0, bool solid = false, float height = 1, float weight = 1)
        {
            this.Size = (ObjectSize)size;
            this.Solid = false;
            this.Height = height;
            this.Weight = weight;
            return this;
        }

        public int Reach => (int)Math.Ceiling(this.Height) + 2;

        /// <summary>
        /// make positioncomponent, global, velocity, height, map, fields instead of properties
        /// </summary>
        /// <param name="net"></param>
        /// <param name="parent"></param>
        /// <param name="chunk"></param>
        public override void Tick()
        {
            if (!this.Enabled)
                return;

            var parent = this.Parent;
            var map = parent.Map;
            var net = parent.Net;
            Vector3 next;
            Vector3 lastGlobal = parent.Transform.Global;
            if (!map.TryGetCell(lastGlobal, out var thisCell))
                return;

            Vector3 velocity = parent.Velocity;

            var feetposition = lastGlobal + Vector3.UnitZ * .1f;
            var feetcell = map.GetCell(feetposition);
            Block feetblock = feetcell.Block;
            if (!this.Enabled)
                return;

            // TODO: make getdensity method on block that takes into account its height
            float density = feetblock.GetDensity(feetcell.BlockData, feetposition);
            velocity *= (1 - density);

            float nx, ny, nz;
            BoundingBox box = this.GetBoundingBox(parent, lastGlobal);

            nz = this.ResolveVertical(parent, map, box, ref velocity, density);

            velocity = new Vector3(velocity.X * FrictionFactor, velocity.Y * FrictionFactor, velocity.Z);
            if (velocity.LengthSquared() < .0001f)
                velocity = Vector3.Zero;

            Vector3 blocktransform = this.GetStandingBlockTransform(map, lastGlobal);
            var origin = parent.Global + blocktransform;

            if (velocity.X != 0 || blocktransform.X != 0)
                nx = ResolveHorizontalCorners(net, parent, origin, map, box, ref velocity, Vector2.UnitX, nz, out nz).X;
            else
                nx = lastGlobal.X;

            if (velocity.Y != 0 || blocktransform.Y != 0)
                ny = ResolveHorizontalCorners(net, parent, origin, map, box, ref velocity, Vector2.UnitY, nz, out nz).Y;
            else
                ny = lastGlobal.Y;

            next = new Vector3(nx, ny, nz);

            if (lastGlobal != next)
                parent.SetPosition(next);
            else if (velocity == Vector3.Zero)
                this.Enabled = false;
            this.DetectEntityCollisions(parent, lastGlobal, next);
            // reset speed according to new position to prevent it from accumulating
            parent.Velocity = velocity;

            // log state change 
            if (parent.Exists) // must check because entity might have despawned itself during it's update 
            {
                if (lastGlobal != parent.Global)
                {
                    // send a "step on" message on next block
                    net.LogStateChange(parent.RefID);
                    Vector3 nextRounded = next.RoundXY();
                    if (nextRounded != lastGlobal.RoundXY())
                    {
                        Vector3 blockGlobal = nextRounded - Vector3.UnitZ;
                        var bl = parent.Map.GetBlock(blockGlobal);
                        bl.OnSteppedOn(parent, blockGlobal);
                    }
                }
            }
        }
        public BoundingBox GetBoundingBox(Vector3 global)
        {
            return new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, this.Height));
        }
        public BoundingBox GetBoundingBox(GameObject parent, Vector3 global)
        {
            return new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, parent.Height));
        }
        public BoundingBox GetBoundingBox(Vector3 global, float height)
        {
            return new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, height));
        }
        private Vector3 GetStandingBlockTransform(MapBase map, Vector3 lastGlobal)
        {
            Vector3 blocktransform = Vector3.Zero;

            // if feet on contact with non-air block, get velocity transform of block directly below

            var underfeet = lastGlobal + new Vector3(0, 0, map.Gravity);
            var underfeetBlockCoords = underfeet.ToBlock();
            var underfeetCell = map.GetCell(underfeet);
            var underfeetBlock = underfeetCell.Block;
            if (underfeetBlock != BlockDefOf.Air)
            {
                blocktransform = underfeetCell.Block.GetVelocityTransform(underfeetCell.BlockData, underfeetBlockCoords);
            }
            else
            {
                var tocheck = new Vector3[3];
                if (underfeetBlockCoords.X < .5f && underfeetBlockCoords.Y < .5f)
                {
                    if (underfeetBlockCoords.X < underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(-1, 0, 0);
                        tocheck[1] = new Vector3(0, -1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, -1, 0);
                        tocheck[1] = new Vector3(-1, 0, 0);
                    }
                    tocheck[2] = new Vector3(-1, -1, 0);
                }
                else if (underfeetBlockCoords.X > .5f && underfeetBlockCoords.Y < .5f)
                {
                    if (underfeetBlockCoords.X > underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(1, 0, 0);
                        tocheck[1] = new Vector3(0, -1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, -1, 0);
                        tocheck[1] = new Vector3(1, 0, 0);
                    }
                    tocheck[2] = new Vector3(1, -1, 0);
                }
                else if (underfeetBlockCoords.X > .5f && underfeetBlockCoords.Y > .5f)
                {
                    if (underfeetBlockCoords.X > underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(1, 0, 0);
                        tocheck[1] = new Vector3(0, 1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, 1, 0);
                        tocheck[1] = new Vector3(1, 0, 0);
                    }
                    tocheck[2] = new Vector3(1, 1, 0);
                }
                else
                {
                    if (underfeetBlockCoords.X < underfeetBlockCoords.Y)
                    {
                        tocheck[0] = new Vector3(-1, 0, 0);
                        tocheck[1] = new Vector3(0, 1, 0);
                    }
                    else
                    {
                        tocheck[0] = new Vector3(0, 1, 0);
                        tocheck[1] = new Vector3(-1, 0, 0);
                    }
                    tocheck[2] = new Vector3(-1, 1, 0);
                }

                Cell contactCell;
                foreach (var check in tocheck)
                {
                    var cellglobal = (underfeet + check).Round();
                    contactCell = map.GetCell(cellglobal);
                    if (contactCell == null || contactCell.Block == BlockDefOf.Air)
                    {
                        continue;
                    }

                    blocktransform = contactCell.Block.GetVelocityTransform(contactCell.BlockData, (underfeetBlockCoords + check * .25f).Round());
                }
            }
            return blocktransform;
        }

        private float ResolveVertical(GameObject parent, MapBase map, BoundingBox box, ref Vector3 speed, float density)
        {
            var grav = map.Gravity;
            var adjustedGravity = grav;
            var global = parent.Global;
            if (speed.Z == 0)
            {
                // TODO: maybe instead of checking the corners, i check if the box intersects with nearby block boxes?
                var corners = new Vector3[] {
                    box.Min,
                    new Vector3(box.Min.X, box.Max.Y, global.Z),
                    new Vector3(box.Max.X, box.Min.Y, global.Z),
                    new Vector3(box.Max.X, box.Max.Y, global.Z)
                };
                if (corners.All(c => !map.IsSolid(c + new Vector3(0, 0, adjustedGravity))))
                {
                    this.MidAir = true;
                    speed.Z = grav;
                    return global.Z + adjustedGravity;
                }

                return global.Z;
            }
            var offset = new Vector3(0, 0, speed.Z);
            var next = global + offset;
            if (speed.Z < 0)
            {
                float fallheight = global.Z - next.Z;
                if (fallheight > 1)
                {
                    for (int i = 0; i < (int)Math.Ceiling(fallheight); i++)
                    {
                        var check = global - new Vector3(0, 0, 1 + i);
                        if (IsStandable(map, check))
                        {
                            parent.Net.PostLocalEvent(parent, Message.Types.HitGround, Math.Abs(speed.Z));
                            speed.Z = 0;
                            // land
                            this.HitGround(parent, check);
                            return (int)check.Z + Block.GetBlockHeight(map, check);
                        }
                    }
                }
                else if (fallheight > 0)
                {
                    var nextbox = new BoundingBox(next - new Vector3(.25f, .25f, 0), next + new Vector3(.25f, .25f, this.Height));
                    var corners =
                        new Vector3[] {
                            nextbox.Min,
                            new Vector3(nextbox.Min.X, nextbox.Max.Y, next.Z),
                            new Vector3(nextbox.Max.X, nextbox.Min.Y, next.Z),
                            new Vector3(nextbox.Max.X, nextbox.Max.Y, next.Z)
                        };
                    if (corners.Any(c => GetDensity(map, c + new Vector3(0, 0, grav)) > 0))
                    {
                        var f = speed.Z;
                        speed.Z = 0;
                        // land
                        this.HitGround(parent, next);

                        float blockheightbelow = 0;
                        blockheightbelow = corners.Max(c => Block.GetBlockHeight(map, c));
                        return (int)next.Z + blockheightbelow;
                    }
                }
            }
            else
            {
                this.MidAir = true;
                // hit on cieling
                if (map.IsSolid(box.Max + offset))
                {
                    // dont set velocity z to 0, because later we check if velocity is zero and we disable physics. 
                    // reflect velocity instead
                    this.HitCeiling(parent, (box.Max + offset));
                    speed.Z = -speed.Z;
                    return box.Min.Z;
                }
            }
            speed.Z += adjustedGravity;
            return next.Z;
        }

        private static Vector3 ResolveHorizontalCorners(INetwork net, GameObject parent, Vector3 origin, MapBase map, BoundingBox boxGlobal, ref Vector3 velocity, Vector2 horAxis, float nz, out float zz)
        {
            zz = nz;
            Vector3 step = new Vector3(horAxis, 0) * velocity;
            Vector3 nextorigin = origin + step;
            var leadingCorners = new List<Vector3>()
            {
                nextorigin + new Vector3(-.25f, -.25f, 0),
                nextorigin + new Vector3(.25f, -.25f, 0),
                nextorigin + new Vector3(-.25f, .25f, 0),
                nextorigin + new Vector3(.25f, .25f, 0)
            };

            int minz = (int)boxGlobal.Min.Z, maxz = (int)boxGlobal.Max.Z;
            int dz = maxz - minz;

            foreach (var next in leadingCorners)
            {
                for (float z = 0; z < dz; z += 0.5f) // increment by 0.5f so we dont skip the solid part of a lower height block
                {
                    Vector3 check = next + new Vector3(0, 0, z);
                    if (map.IsSolid(check))
                    {
                        // check if can climb
                        if (velocity.Z == 0 && z == 0)
                        {
                            var stepUp = check + Vector3.UnitZ / 2f;
                            if (!map.IsSolid(stepUp))
                            {
                                var hbottom = map.GetSolidObjectHeight(check);
                                var htop = map.GetSolidObjectHeight(check + Vector3.UnitZ);
                                var h = hbottom + htop;
                                var floor = step + new Vector3(origin.X, origin.Y, (float)Math.Floor(check.Z));
                                var stepon = floor + Vector3.UnitZ * (h + .01f);
                                zz = stepon.Z;
                                return stepon;
                            }
                        }
                        var block = map.GetBlock(next);
                        return origin;
                    }
                }
            }
            velocity = velocity * (Vector3.One - step) + step * (step);
            return origin + step;
        }

        void DetectEntityCollisions(GameObject parent, Vector3 last, Vector3 next)
        {
            if (next == last)
            {
                return;
            }

            foreach (Chunk ch in parent.Map.GetChunks(parent.Map.GetChunk(last).MapCoords)) // TODO: optimize, search for entities on nearby chunks only if entity is on current chunk edge
            {
                foreach (GameObject obj in ch.GetObjects())
                {
                    if (obj == parent)
                    {
                        continue;
                    }

                    var lastDistance = Vector3.Distance(obj.Global, last);
                    var nextDistance = Vector3.Distance(obj.Global, next);
                    if (lastDistance >= 1 && nextDistance < 1) // changed the inequality so the item doesn't combine if freefalling on an adjacent block
                    {
                        // collision
                        obj.Net.PostLocalEvent(obj, ObjectEventArgs.Create(Message.Types.EntityCollision, new object[] { parent }));
                        //obj.Net.EventOccured(Message.Types.EntityCollision, parent, obj); //removing this because object gets disposed as a result of the above line
                    }
                    // TODO: combine items only when an item enters another stationary item's cell?
                }
            }
        }

        public override void OnSpawn()
        {
            this.Enabled = true;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            Message.Types msg = e.Type;
            switch (msg)
            {
                case Message.Types.Attacked:
                    GameObject attacker = e.Parameters[0] as GameObject;
                    Attack attack = e.Parameters[1] as Attack;

                    float knockResistance = 1;// - Math.Min(1, StatsComponent.GetStatOrDefault(parent, Stat.Types.KnockbackResistance, 0f));

                    Vector3 knockback = attack.GetMomentum() * knockResistance;
                    parent.Velocity += knockback * KnockbackMagnitude;
                    return true;

                case Message.Types.EntityCollision:
                    if (parent.Net is Server)
                    {
                        (e.Parameters[0] as GameObject).SyncAbsorb(parent);
                    }

                    return true;

                default:
                    return base.HandleMessage(parent, e);
            }
        }

        public override object Clone()
        {
            PhysicsComponent phys = new PhysicsComponent(this);
            phys.Size = this.Size;
            phys.Weight = this.Weight;
            phys.Height = this.Height;
            phys.Solid = this.Solid;
            return phys;
        }

        public override void OnTooltipCreated(GameObject parent, UI.Control tooltip)
        {
            if (this.Size == ObjectSize.Immovable)
            {
                return;
            }

            tooltip.AddControlsBottomLeft(
                new GroupBox().AddControlsHorizontally(
                    new Label("Weight: ") { TextColor = Color.LightGray },
                    new Label(() => $"{this.Weight * parent.StackSize} kg ({this.Weight} kg x{parent.StackSize})") { TextColor = Color.LightGray })
                );
        }
        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> list)
        {
            if (this.Size == ObjectSize.Immovable)
            {
                return;
            }

            list.Add(PlayerInput.RButton, new InteractionHaul());
            list.Add(PlayerInput.Activate, new InteractionHaul());
        }
        internal override ContextAction GetContextRB(GameObject parent, GameObject player)
        {
            if (this.Size == ObjectSize.Immovable)
            {
                return null;
            }

            return new ContextAction(new InteractionHaul()) { Shortcut = PlayerInput.RButton, Available = () => this.Size != ObjectSize.Immovable };
        }

        public override string ToString()
        {
            return "Enabled: " + this.Enabled.ToString() + "\n" + base.ToString();
        }

        public static void Enable(GameObject parent)
        {
            parent.TryGetComponent<PhysicsComponent>(f => f.Enabled = true);
        }

        /// <summary>
        /// TODO: pass cell or block in here since i fetch it (by checking if the position is solid) in the check before calling this method
        /// </summary>
        /// <param name="vector3"></param>
        private void HitGround(GameObject parent, Vector3 vector3)
        {
            this.MidAir = false;
            parent.Map.EventOccured(Message.Types.EntityHitGround, parent, vector3);
            //parent.PostMessage(new(Message.Types.EntityHitGround));
            parent.Net.PostLocalEvent(parent, Message.Types.HitGround, Math.Abs(vector3.Z));
        }
        private void HitCeiling(GameObject parent, Vector3 vector3)
        {
            parent.Map.EventOccured(Message.Types.EntityHitCeiling, parent, vector3);
        }

        public static bool IsStanding(GameObject parent)
        {
            var global = parent.Global;
            var height = parent.Physics.Height;
            var map = parent.Map;
            var gravity = map.Gravity;
            var box = new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, height));
            var corners = new Vector3[] {
                    box.Min,
                    new Vector3(box.Min.X, box.Max.Y, global.Z),
                    new Vector3(box.Max.X, box.Min.Y, global.Z),
                    new Vector3(box.Max.X, box.Max.Y, global.Z)
                };
            return corners.Any(c => map.GetBlock(c + new Vector3(0, 0, gravity)).Density > 0);
        }

        public static Vector3 Decelerate(Vector3 velocity)
        {
            return velocity *= FrictionFactor;// .5f;
        }

        public static float GetDensity(MapBase map, Vector3 global)
        {
            var cell = map.GetCell(global);
            return cell.Block.GetDensity(cell.BlockData, global);
        }
        public static bool IsStandable(MapBase map, Vector3 global)
        {
            var gravity = map.Gravity;
            var box = new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, 1));
            var corners = new Vector3[] {
                    box.Min,
                    new Vector3(box.Min.X, box.Max.Y, global.Z),
                    new Vector3(box.Max.X, box.Min.Y, global.Z),
                    new Vector3(box.Max.X, box.Max.Y, global.Z)
                };
            return corners.Any(c => map.GetBlock(c + new Vector3(0, 0, gravity)).Density > 0);
        }
    }
}
