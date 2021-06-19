using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.Windows.Forms;
using System.Diagnostics;
using Start_a_Town_.GameModes;


namespace Start_a_Town_
{
    public class BlockBorderToken
    {
        [Flags]
        public enum Sides { None, Left, Right, Bottom }
        public Vector3 Global;
        public Block Block;
        public Cell Cell;
        public Sides Side;
        public BlockBorderToken(Vector3 global, Block block, Cell cell, Sides side)
        {
            this.Global = global;
            this.Block = block;
            this.Cell = cell;
            this.Side = side;
        }
    }
    public class LightToken
    {
        public Vector3 Global; //{ get; set; }
        public Color Sun; //{ get; set; }
        public Vector4 Block;// { get; set; }
    }


    public class Camera : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Camera"; }
        }
        //public const int FogZDistance = 8, FogFadeLength = 16;
        public const int FogZOffset = 2, FogFadeLength = 8;

        Vector4 FogColor = Color.SteelBlue.ToVector4();// Color.Goldenrod.ToVector4();
        float FollowSpeed = 1;
        GameObject Following;
        float DragDelay = 0, DragDelayMax = Engine.TicksPerSecond / 4;
        bool Dragging;
        Vector2 DragOrigin, DragVector;
        Vector2 _Coordinates;
        bool _HideUnknownBlocks = true;//true;
        public bool HideUnknownBlocks // TODO: make it static
        {
            get { return _HideUnknownBlocks; }
            set
            {
                _HideUnknownBlocks = value;
                Rooms.Ingame.CurrentMap.InvalidateChunks();
                this.TopSliceChanged = true;
            }
        }
        //public bool DrawTopSlice = true;
        bool _DrawTopSlice = true;//true;
        public bool DrawTopSlice
        {
            get { return _DrawTopSlice; }
            set
            {
                _DrawTopSlice = value;
                Rooms.Ingame.CurrentMap.InvalidateChunks();
            }
        }

        internal float GetDrawDepth(GameObject o)
        {
            return o.Global.GetDrawDepth(o.Map, this);
        }
        internal float GetDrawDepth(IMap map, Vector3 global)
        {
            return global.GetDrawDepth(map, this);
        }
        public bool DrawZones = true;
        static public bool HideCeiling;
        public Vector2 Location;
        public bool HideTerrainAbovePlayer;
        public int HideTerrainAbovePlayerOffset;
        public float ZoomMax = 8;// 16;
        public float ZoomMin = 0.125f;
        public int Width, Height;
        public Vector3 Global = Vector3.Zero;
        int _DrawLevel = Map.MaxHeight - 1;
        public int DrawLevel
        {
            get { return this._DrawLevel; }
            set
            {
                var oldvalue = this._DrawLevel;
                this._DrawLevel = value;
                if (oldvalue != value)
                    this.TopSliceChanged = true;
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
                    this.Move(this.Coordinates - new Vector2(0, Block.BlockHeight * (value - oldvalue)));
            }
        }
        public float ZoomNext;
        public float Zoom = 2;//1;
        public override string ToString()
        {
            string text = Location.ToString();
            text += "\nZoom: " + Zoom +
                "\nRotation: " + Rotation;
            return text;
        }
        public void HandleInput(InputState i)
        {


        }
        public static Rectangle CellIntersectBox = new Rectangle();
        static public bool BlockTargeting = true;

        public Vector2 Coordinates
        {
            get { return _Coordinates; }
            set
            {
                _Coordinates = value;
                this.Location = Coordinates - new Vector2((int)((Width / 2) / Zoom), (int)((Height / 2) / Zoom));
                //this.Location = Coordinates - new Vector2(((Width / 2) / Zoom), ((Height / 2) / Zoom));
            }
        }
        Map _Map;
        public Map Map
        {
            get { return this._Map; }
            set
            {
                this._Map = value;
                // TODO: clear light cache
            }
        }

        public event EventHandler<EventArgs> ZoomChanged;
        protected void OnZoomChanged()
        {
            if (ZoomChanged != null)
                ZoomChanged(this, EventArgs.Empty);
        }
        public static event EventHandler<EventArgs> LocationChanged;
        protected void OnLocationChanged()
        {
            if (LocationChanged != null)
                LocationChanged(this, EventArgs.Empty);
        }
        public static event EventHandler<EventArgs> RotationChanged;
        protected void OnRotationChanged()
        {
            foreach (var chunk in Rooms.Ingame.CurrentMap.GetActiveChunks())// Engine.Map.ActiveChunks)
            {
                chunk.Value.OnCameraRotated(this);
                //chunk.Value.Valid = false;
                chunk.Value.Invalidate();
            }
            CenterOn(Global);
            if (RotationChanged != null)
                RotationChanged(this, EventArgs.Empty);
        }

        public void Initialize()
        {

        }

        void gfx_DeviceReset(object sender, EventArgs e)
        {
            //Width = (sender as GraphicsDeviceManager).PreferredBackBufferWidth;
            //Height = (sender as GraphicsDeviceManager).PreferredBackBufferHeight;
            //Width = Game1.Instance.Window.ClientBounds.Width;
            //Height = Game1.Instance.Window.ClientBounds.Height;
            Width = (sender as GraphicsDeviceManager).PreferredBackBufferWidth;
            Height = (sender as GraphicsDeviceManager).PreferredBackBufferHeight;
            this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            this.RenderTargetsInvalid = true;
            //ResetRenderTargets(gd);
        }
        bool RenderTargetsInvalid = true;

        public override void Tick(IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (parent == null)
                return;

            Global = parent.Transform.Global + Vector3.UnitZ;

            this.Follow(Global);
        }
        public void Update(IMap map, Vector3 global)
        {
            this.Global = global + Vector3.UnitZ;
            this.Follow(Global);

            this.UpdateFog(map);
        }
        public void Update(IMap map, Vector2 coords)
        {
            this.Coordinates = coords;
            this.UpdateFog(map);
        }
        public void Update(IMap map)
        {
            this.Follow();
            this.SmoothZoom(ZoomNext);
            //this.SmoothRotate(RotationNext);
            this.UpdateFog(map);
        }

        void UpdateFog(IMap map)
        {
            this.FogT = (this.FogT + 0.05f * map.Net.Speed) % 100;
        }
        public void Move(Vector2 coords)
        {
            this.Following = null;
            this.Coordinates = coords;
        }
        public void Update(GameTime gt)
        {
            this.SmoothZoom(ZoomNext);
            if (!this.Dragging)
                return;

            if (DragDelay < DragDelayMax)
            {
                DragDelay += 1;// GlobalVars.DeltaTime;
                return;
            }
            //this.Location += GlobalVars.DeltaTime * DragVector / (10f);
            this.Coordinates += DragVector / (Zoom * 10f);//GlobalVars.DeltaTime * 

        }

        void SetZoom(float value)
        {
            this.Zoom = value;

            //this.Zoom *= 100;
            //this.Zoom = (float)Math.Round(this.Zoom);
            //this.Zoom /= 100;

            var offset = new Vector2(this.Width / 2, this.Height / 2);
            offset /= this.Zoom;
            this.Location = this.Coordinates - offset;
            //this.Location = Coordinates - new Vector2(Width / (2 * Zoom), Height / (2 * Zoom));
            //this.Location = Coordinates - new Vector2((int)((Width / 2) / Zoom), (int)((Height / 2) / Zoom));
            //this.Location = Coordinates - new Vector2((float)Math.Round((Width / 2) / Zoom), (float)Math.Round((Height / 2) / Zoom));

            //this.Location = this.Location;
        }
        public void SmoothZoom(float next)
        {
            float diff = next - this.Zoom;
            //diff *= 1000;
            //diff = (float)Math.Round(diff);
            //diff /= 1000;
            var zoomSpeed = 0.1f; //0.33f
            var n = zoomSpeed * diff;

            //n *= 1000f;
            //n = (float)Math.Round(n);
            //n /= 1000f;

            //var nextZoom = this.Zoom + n;
            //nextZoom *= 100f;
            //nextZoom = (float)Math.Round(nextZoom);
            //nextZoom /= 100f;
            //this.SetZoom(nextZoom);
            //return;
            if (Math.Abs(n) < 0.001f)
            {
                //this.Zoom = next;
                this.SetZoom(next);
                //next.ToConsole();
            }
            else
                this.SetZoom(this.Zoom + n);
            //this.Zoom += n;
        }

        public void CenterOn(Vector3 global)
        {
            Global = global;
            int xx, yy;
            Coords.Iso(this, global.X, global.Y, global.Z, out xx, out yy);
            //this.Location = new Vector2(xx - (Width / 2) / Zoom, yy - (Height / 2) / Zoom);
            this.Coordinates = new Vector2(xx, yy);
            this.DrawLevel = (int)Math.Max(this.DrawLevel, global.Z + 1);
        }
        public void Follow()
        {
            if (this.Following == null)
                return;
            if (this.Following.IsIndoors())
            {
                this.DrawLevel = (int)(this.Following.Global.CeilingZ().Z + this.Following.Physics.Height - 1);
            }
            else
            {
                this.DrawLevel = this.Following.Map.GetMaxHeight();
            }
            this.Follow(this.Following.Global);
        }
        public void Follow(Vector3 global)
        {
            Global = global;
            //int xx, yy;
            float xx, yy;
            Coords.Iso(this, global.X, global.Y, global.Z, out xx, out yy);

            Vector2 currentLoc = this.Coordinates//this.Location
                , nextLoc = new Vector2(xx, yy);
            Vector2 diff = nextLoc - currentLoc;

            diff *= 100;
            diff = diff.Round();
            diff /= 100;

            //this.Coordinates = currentLoc + 0.05f * diff;
            //var nextCoords = currentLoc + 0.05f * diff;
            var nextCoords = currentLoc + 0.05f * diff;

            // TODO: find a way to make it smooth without seaming between sprites
            //nextCoords *= 10;



            /// uncomment this to make camera movement rigid instead of smooth
            nextCoords = nextCoords.Round(); // must round to prevent seaming between blocks when moving camera
            ///


            //nextCoords /= 10;

            //this.Coordinates *= 10;
            this.Coordinates = nextCoords;//.Round();
            //this.Coordinates /= 10;
            //this.Coordinates = this.Coordinates.Round();
        }

        public void UpdateBbox()
        {
            CellIntersectBox.X = (int)((Width / 2) / Zoom);// -Bbox.Width / 2;
            CellIntersectBox.Y = (int)((Height / 2) / Zoom);// -Bbox.Height / 2;
            CellIntersectBox.Width = (int)((Width / 2) / Zoom); // (int)(Bbox.Width * 2);
            CellIntersectBox.Height = (int)((Height / 2) / Zoom); // (int)(Bbox.Height * 2);
        }
        public Rectangle ViewPort;

        public void GetEverything(IMap map, Vector3 global, Rectangle spriteRect, out float depth, out Rectangle screenBounds, out Vector2 screenLoc)
        {
            //depth = global.GetDrawDepth(Engine.Map, this); ;// global.GetDepth(Engine.Map, this);
            depth = global.GetDrawDepth(map, this); ;// global.GetDepth(Engine.Map, this);
            screenBounds = GetScreenBounds(global, spriteRect);
            screenLoc = new Vector2(screenBounds.X, screenBounds.Y);
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle)
        {
            return this.GetScreenBounds(x, y, z, spriteRectangle, 0, 0);
            //return this.GetScreenBounds(x, y, z, spriteRectangle, Vector2.Zero);
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin)
        {
            //float xx, yy;
            //Coords.Iso(this, x, y, z, out xx, out yy);
            ////return new Rectangle((int)(Zoom * (xx + spriteRectangle.X - X)), (int)(Zoom * (yy + spriteRectangle.Y - Y)), (int)(Zoom * spriteRectangle.Width), (int)(Zoom * spriteRectangle.Height));
            //int xxx = (int)Math.Round(Zoom * (xx + spriteRectangle.X - this.Location.X - origin.X));
            //int yyy = (int)Math.Round(Zoom * (yy + spriteRectangle.Y - this.Location.Y - origin.Y));
            //int w = (int)Math.Ceiling(Zoom * spriteRectangle.Width);
            //int h = (int)Math.Ceiling(Zoom * spriteRectangle.Height);
            //return new Rectangle(xxx, yyy, w, h);

            int xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            //return new Rectangle((int)(Zoom * (xx + spriteRectangle.X - X)), (int)(Zoom * (yy + spriteRectangle.Y - Y)), (int)(Zoom * spriteRectangle.Width), (int)(Zoom * spriteRectangle.Height));
            return new Rectangle((int)(Zoom * (xx + spriteRectangle.X - this.Location.X - origin.X)), (int)(Zoom * (yy + spriteRectangle.Y - this.Location.Y - origin.Y)), (int)(Zoom * spriteRectangle.Width), (int)(Zoom * spriteRectangle.Height));
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy)
        {
            int xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            return new Rectangle(
                (int)(Zoom * (xx + spriteRectangle.X - this.Location.X - originx)), 
                (int)(Zoom * (yy + spriteRectangle.Y - this.Location.Y - originy)), 
                (int)(Zoom * spriteRectangle.Width), 
                (int)(Zoom * spriteRectangle.Height));
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy, float scale)
        {
            int xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            var scalezoom = scale * Zoom;
            return new Rectangle(
                (int)(Zoom * (xx + scale * spriteRectangle.X - this.Location.X - originx)), 
                (int)(Zoom * (yy + scale * spriteRectangle.Y - this.Location.Y - originy)), 
                (int)(scalezoom * spriteRectangle.Width), 
                (int)(scalezoom * spriteRectangle.Height));
        }
        public Vector4 GetScreenBoundsVector4(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin, float scale = 1)
        {
            float xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            //float xxx = (float)(Zoom * (xx + spriteRectangle.X - this.Location.X - origin.X));
            //float yyy = (float)(Zoom * (yy + spriteRectangle.Y - this.Location.Y - origin.Y));
            //float w = (float)(Zoom * spriteRectangle.Width);
            //float h = (float)(Zoom * spriteRectangle.Height);
            var loc = this.Location;//.Round(); //Floor();
            //loc = loc.Round();
            float xxx = (float)((xx + scale * spriteRectangle.X - loc.X - origin.X));
            float yyy = (float)((yy + scale * spriteRectangle.Y - loc.Y - origin.Y));
            float w = scale * (float)(spriteRectangle.Width);
            float h = scale * (float)(spriteRectangle.Height);
            //xxx = (float)Math.Round(xxx);
            //yyy = (float)Math.Round(yyy);
            var vector = new Vector4(xxx, yyy, w, h);

            vector *= this.Zoom;
            return vector;
            //return new Vector4(xxx, yyy, w, h);

        }
        public Vector4 GetScreenBoundsVector4NoOffset(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin)
        {
            float xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            float xxx = (float)((xx + spriteRectangle.X - origin.X));
            float yyy = (float)((yy + spriteRectangle.Y - origin.Y));
            float w = (float)(spriteRectangle.Width);
            float h = (float)(spriteRectangle.Height);
            var vector = new Vector4(xxx, yyy, w, h);
            return vector;
        }
        public void GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, out Vector2 pos, out Vector2 size)
        {
            float xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            pos = new Vector2(xx + spriteRectangle.X - Location.X, yy + spriteRectangle.Y - Location.Y) * this.Zoom;
            size = new Vector2(Zoom * spriteRectangle.Width, this.Zoom * spriteRectangle.Height);
        }
        public Vector2 GetScreenBounds(float x, float y, float z)
        {
            int xx, yy;
            Coords.Iso(this, x, y, z, out xx, out yy);
            // return new Rectangle((int)(Zoom * (xx + spriteRectangle.X - X)), (int)(Zoom * (yy + spriteRectangle.Y - Y)), (int)(Zoom * spriteRectangle.Width), (int)(Zoom * spriteRectangle.Height));
            return new Vector2(Zoom * (xx - Location.X), Zoom * (yy - Location.Y));
        }
        public Vector2 GetScreenPosition(TargetArgs target)
        {
            var t = target;
            var fx = t.Face.X * .5f;
            var fy = t.Face.Y * .5f;
            var yx = fx + fy;
            var fz = yx == 0 ? (t.Face.Z == 1 ? 1 : 0) : .5f;
            return this.GetScreenPosition(t.Global + new Vector3(fx, fy, fz));
        }
        public Vector2 GetScreenPosition(Vector3 pos)
        {
            int xx, yy;
            Coords.Iso(this, pos.X, pos.Y, pos.Z, out xx, out yy);

            //float xx, yy;
            //Coords.Iso(this, pos.X, pos.Y, pos.Z, out xx, out yy);

            return new Vector2(Zoom * (xx - Location.X), Zoom * (yy - Location.Y));
        }
        public Vector2 GetScreenPositionFloat(Vector3 pos)
        {
            float xx, yy;
            Coords.Iso(this, pos.X, pos.Y, pos.Z, out xx, out yy);

            //float xx, yy;
            //Coords.Iso(this, pos.X, pos.Y, pos.Z, out xx, out yy);
            var loc = this.Location;//.Round(); //Floor();//.Round();
            var screenpos = new Vector2(Zoom * (xx - loc.X), Zoom * (yy - loc.Y));
            //screenpos = screenpos.Round();
            return screenpos;
        }
        //public void GetScreenBounds(Vector3 pos, out Vector2 screenpos)
        //{
        //    int xx, yy;
        //    Coords.Iso(this, pos.X, pos.Y, pos.Z, out xx, out yy);

        //    screenpos = new Vector2((int)(Zoom * (xx - X)), (int)(Zoom * (yy - Y)));
        //}
        //public Rectangle GetScreenBounds2(float x, float y, float z, Rectangle spriteRectangle)
        //{
        //    int xx, yy;
        //    Coords.iso(this, x, y, z, out xx, out yy);
        //    int _x = -Chunk.Width / 2, _y = 0;// Chunk.Height / 2;
        //    double xr = _x * RotCos - _y * RotSin;
        //    double yr = _x * RotSin + _y * RotCos;
        //    return new Rectangle((int)(Zoom * (xx + spriteRectangle.X)), (int)(Zoom * (yy + spriteRectangle.Y)), (int)(Zoom * spriteRectangle.Width), (int)(Zoom * spriteRectangle.Height));
        //}

        public Rectangle GetScreenBounds(Vector3 global, Rectangle spriteRectangle)
        {
            return GetScreenBounds(global.X, global.Y, global.Z, spriteRectangle);
        }
        public Rectangle GetScreenBounds(Vector3 global, Rectangle spriteRectangle, Vector2 origin)
        {
            return GetScreenBounds(global.X, global.Y, global.Z, spriteRectangle, origin);
        }

        public bool CullingCheck(float x, float y, float z, Rectangle sourceBounds, out Rectangle screenBounds)
        {
            screenBounds = GetScreenBounds(x, y, z, sourceBounds);
            return ViewPort.Intersects(screenBounds);
        }

        public override object Clone()
        {
            //return new Camera(this.Map, Width, Height);
            return new Camera(Width, Height);
        }

        double _Rotation;
        public double RotCos, RotSin;

        //public Camera(Map map) : this(map, Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height) { }
        public Camera()
            : this(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height)
        {
            this.WaterSpriteBatch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.SpriteBatch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
        }

        //public Camera(Map map, int width, int height, float x = 0, float y = 0, float z = 0, float zoom = 1, int rotation = 0)
        public Camera(int width, int height, float x = 0, float y = 0, float z = 0, float zoom = 2, int rotation = 0)
        {
            // TODO: Complete member initialization
            //this.Map = map;
            this.Width = width;
            this.Height = height;
            this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
            //this._Zoom = zoom;
            this.Zoom = zoom;
            this.ZoomNext = zoom;
            this.Rotation = rotation;
            //this.RotationNext = rotation;
            CenterOn(new Vector3(x, y, z));

            Game1.Instance.graphics.DeviceReset += new EventHandler<EventArgs>(gfx_DeviceReset);
        }

        //[System.ComponentModel.DefaultValue(Matrix.CreateRotationZ(0))]
        //Matrix _RotationMatrix = Matrix.CreateRotationZ(0);
        //public Matrix RotationMatrix { get { return this._RotationMatrix; } protected set { this._RotationMatrix = value; } }
        public void Rotate(Vector3 global, out int rx, out int ry)
        {
            Coords.Rotate((int)this.Rotation, global.X, global.Y, out rx, out ry);
        }
        /// <summary>
        /// TODO: make rotation a field for speed and calculate the shits in some other way
        /// </summary>
        public double Rotation
        {
            get { return _Rotation; }
            set
            {
                double oldRot = _Rotation;
                _Rotation = value % 4;

                //this.RotationMatrix = Matrix.CreateRotationZ((float)(_Rotation * (Math.PI / 2f)));
                //this.RotationMatrix = Matrix.CreateRotationZ((float)(_Rotation * ((float)Math.PI / 2f)));
                if (_Rotation < 0)
                    _Rotation = 4 + value;

                RotCos = Math.Cos((Math.PI / 2f) * _Rotation);
                RotSin = Math.Sin((Math.PI / 2f) * _Rotation);

                RotCos = Math.Round(RotCos + RotCos) / 2f;
                RotSin = Math.Round(RotSin + RotSin) / 2f;

                if (_Rotation != oldRot)
                    OnRotationChanged();


            }
        }
        //float _RotationNext;
        //float RotationNext
        //{
        //    get { return this._RotationNext; }
        //    set
        //    {
        //        value = value % 4;
        //        this._RotationNext = value;
        //        if (_RotationNext < 0)
        //            _RotationNext = 4 + _RotationNext;
        //    }
        //}
        public void RotateClockwise()
        {
            this.Rotation++;

            //this.RotationNext++;
        }
        public void RotateCounterClockwise()
        {
            this.Rotation--;

            //this.RotationNext--;
        }
        public void RotationReset()
        {
            this.Rotation = 0;
        }
        public bool DrawCell(Canvas canvas, IMap map, Vector3 global)
        {
            Chunk chunk;
            Cell cell;
            map.TryGetAll(global, out chunk, out cell);
            int z = cell.Z;
            Block block = cell.Block;// Block.Registry[cell.Type];
            if (block == BlockDefOf.Air)
            {
                chunk.InvalidateCell(cell);
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }


            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            int rgx, rgy;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out rgx, out rgy);

            Vector3 globalRotated = new Vector3(rgx, rgy, z);
            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(gx, gy, z, Block.Bounds, Vector2.Zero);// GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            int rlx, rly;
            Coords.Rotate(this, lx, ly, out rlx, out rly);
            //var depth = rlx + rly;
            var depth = rgx + rgy;

            Color finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on

            if ((cell.AllEdges == 0 && HideUnknownBlocks))
                //|| map.IsUndiscovered(global))
                Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth);
            else
                block.Draw(canvas, chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);
            return true;
        }
        public bool DrawCell(Canvas canvas, IMap map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;
            bool isAir = false;
            Block.Types cellTile = cell.Block.Type;
            if (cellTile == Block.Types.Air)
            {
                //isAir = true;
                chunk.InvalidateCell(cell);
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }

            Block block = cell.Block;

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            int rgx, rgy;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out rgx, out rgy);

            Vector3 globalRotated = new Vector3(rgx, rgy, z);
            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);
            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            int rlx, rly;
            Coords.Rotate(this, lx, ly, out rlx, out rly);
            var depth = rlx + rly;

            Color finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
            var isDiscovered = !map.IsUndiscovered(new Vector3(gx, gy, z));
            /// DONT ERASE
            ///if (cell.AllEdges == 0 && HideUnknownBlocks)  // do i want cells that have already been discoverd, to remain visible even if they become obstructed again?
            if (!isDiscovered && HideUnknownBlocks)// && isAir) // do i want cells that have already been discoverd, to remain visible even if they become obstructed again?
                Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth);
            else// if(!isAir)
                //Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth);
                block.Draw(canvas, chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);
            //else
            //{

            //}
            return true;
        }
        public bool DrawBlockGlobal(MySpriteBatch sb, IMap map, Vector3 global)
        {
            int z = (int)global.Z;
            int gx = (int)global.X;
            int gy = (int)global.Y;
            //Block.Types cellTile = cell.Block.Type;
            //if (cellTile == Block.Types.Air)
            //{
            //    chunk.InvalidateCell(cell);
            //    ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
            //    return false;
            //}


            //int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            //LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            //Coords.Rotate(this, gx, gy, out int rgx, out int rgy);
            //Vector3 globalRotated = new Vector3(rgx, rgy, z);
            //float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);
            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(gx, gy, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, gx, gy, out int rlx, out int rly);
            var depth = rlx + rly;


            //block.Draw(canvas, chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);
            sb.DrawBlock(Block.Atlas.Texture, screenBoundsVector4,
                Block.BlockBlueprint,
                this.Zoom, Color.Transparent, Color.White*.5f, Color.White, Color.White, Vector4.One, Vector4.Zero, depth, null, global);
            return true;
        }
        public bool DrawUnknown(Canvas canvas, IMap map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            int rgx, rgy;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out rgx, out rgy);
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z, false);

            Vector3 globalRotated = new Vector3(rgx, rgy, z);
            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);
            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            int rlx, rly;
            Coords.Rotate(this, lx, ly, out rlx, out rly);
            var depth = rlx + rly;

            //if(this.HideUnknownBlocks)
                Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

            return true;
        }
        public bool DrawUnknown(MySpriteBatch sb, IMap map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            int rgx, rgy;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out rgx, out rgy);
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z, false);

            Vector3 globalRotated = new Vector3(rgx, rgy, z);
            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);
            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            int rlx, rly;
            Coords.Rotate(this, lx, ly, out rlx, out rly);
            var depth = rlx + rly;

            Block.DrawUnknown(sb, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

            return true;
        }
        //Dictionary<Cell, MyVertex[]> VertexBuffer = new Dictionary<Cell, MyVertex[]>();
        /// <summary>
        /// TODO: OPTIMIZE: 
        /// convert lightcache to dictionary,
        /// reduce vector3.rotate calls,
        /// pass controller instance instead of accessing singleton getter,
        /// remove vector3 ctor from getdrawdepth,
        /// create vertexes at getscreenbounds to reduce rect ctors,
        /// create field for color.white instead of getting it from getter each time,
        /// make gameobject.global a field
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="map"></param>
        /// <param name="chunk"></param>
        /// <param name="cell"></param>
        /// <param name="playerGlobal"></param>
        /// <param name="hiddenRects"></param>
        /// <param name="a"></param>
        public bool DrawCell(MySpriteBatch sb, IMap map, Chunk chunk, Cell cell, Vector3? playerGlobal, List<Rectangle> hiddenRects, EngineArgs a)
        {
            int z = cell.Z;

            if (z > this.MaxDrawZ)//.GetMaxDrawLevel()) // i'm cacheing that in the beginning of the drawchunk method
                return false;

            Block.Types cellTile = cell.Block.Type;
            if (cellTile == Block.Types.Air)
            {
                chunk.InvalidateCell(cell);
                //chunk.VisibleOutdoorCells.Remove(Chunk.FindIndex(cell.LocalCoords));
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }

            Block block = cell.Block;// Block.Registry[cell.Type];

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            //if (Engine.HideOccludedBlocks)
            //    if (light.Sun.Add(light.Block) == Color.Transparent)
            //        return false;

            int rgx, rgy;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out rgx, out rgy);

            Vector3 globalRotated = new Vector3(rgx, rgy, z);
            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);

            DepthFar = Math.Min(DepthFar, cd);
            DepthNear = Math.Max(DepthNear, cd);

            //Color finalFogColor = playerGlobal != null ? this.GetFogColor(z) : Color.Transparent;

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);

            var depth = lx + ly;

            maxd = Math.Max(maxd, depth);
            mind = Math.Min(mind, depth);
            Color finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
            //block.Draw(chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);
            block.Draw(chunk.Canvas, chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);

            return true;
        }
        float maxd = float.MinValue, mind = float.MaxValue;
        /// <summary>
        /// TODO: OPTIMIZE: 
        /// convert lightcache to dictionary,
        /// reduce vector3.rotate calls,
        /// pass controller instance instead of accessing singleton getter,
        /// remove vector3 ctor from getdrawdepth,
        /// create vertexes at getscreenbounds to reduce rect ctors,
        /// create field for color.white instead of getting it from getter each time,
        /// make gameobject.global a field
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="map"></param>
        /// <param name="chunk"></param>
        /// <param name="cell"></param>
        /// <param name="playerGlobal"></param>
        /// <param name="hiddenRects"></param>
        /// <param name="a"></param>
        public bool DrawCellOld(MySpriteBatch sb, IMap map, Chunk chunk, Cell cell, Vector3? playerGlobal, List<Rectangle> hiddenRects, EngineArgs a)
        {
            //MyVertex[] cachedVertices;
            //if (this.VertexBuffer.TryGetValue(cell, out cachedVertices))
            //{

            //    sb.Draw(Block.Atlas.Texture, cachedVertices);
            //    return true;
            //}

            int z = cell.Z;

            //if (z > map.DrawLevel)
            //if (z > this.DrawLevel)
            //    return;

            //if (a.HideTerrain)
            if (z > this.MaxDrawZ)//.GetMaxDrawLevel()) // i'm cacheing that in the beginning of the drawchunk method
                return false;
            //if (this.HideTerrainAbovePlayer)
            //{
            //    //if (playerGlobal.HasValue ? z >= playerGlobal.Value.Z + 2 : z >= this.DrawLevel)// map.DrawLevel)
            //    if (playerGlobal.HasValue ? z > playerGlobal.Value.Z + 2 : z >= this.DrawLevel)// map.DrawLevel)
            //        return;
            //}

            Block.Types cellTile = cell.Block.Type;
            if (cellTile == Block.Types.Air)
            {
                chunk.InvalidateCell(cell);
                //chunk.VisibleOutdoorCells.Remove(Chunk.FindIndex(cell.LocalCoords));
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }

            Block block = cell.Block;// Block.Registry[cell.Type];

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            //// uncomment to draw only block in a sphere around the player
            //if (playerGlobal.HasValue)
            //{
            //    float dist = Vector3.DistanceSquared(playerGlobal.Value, new Vector3(gx, gy, z));
            //    if (dist > 64)
            //        return false;
            //}

            Rectangle spriteBounds = Block.Bounds;
            //Rectangle screenBounds = GetScreenBounds(gx, gy, z, spriteBounds);
            var screenBoundsVector4 = GetScreenBoundsVector4(gx, gy, z, spriteBounds, Vector2.Zero);
            Rectangle screenBounds = screenBoundsVector4.ToRectangle();

            if (!ViewPort.Intersects(screenBounds))
                return false;

            //Color light = GetFinalLight(this, map, chunk, cell, gx, gy, z);
            //Color sunlight, blocklight;
            //GetFinalLight(this, map, chunk, cell, gx, gy, z, out sunlight, out blocklight);
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            // hide block based on underground/overground
            if (playerGlobal.HasValue)
            {
                var heightmap = chunk.GetHeightMapValue(cell.LocalCoords);

                var playerisunderground = playerGlobal.Value.Z < this.PlayerHeightMapValue;
                if (playerisunderground)
                {
                    if (z >= heightmap) //&&
                        //       light.Sun.R == 255)
                        //&&
                        //light.Sun.G == 255 &&
                        //light.Sun.B == 255)
                        return false;
                }
                else
                    if (z < heightmap &&
                        light.Sun.R == 16 &&
                        light.Sun.G == 16 &&
                        light.Sun.B == 16)
                        return false;
            }
            if (Engine.HideOccludedBlocks)
                //if (light == Color.Black)
                //if(sunlight.Add(blocklight) == Color.Transparent)
                if (light.Sun.Add(light.Block) == Color.Transparent)
                    return false;

            int rgx, rgy;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out rgx, out rgy);

            //Coords.Rotate(this, gx - map.Global.X, gy - map.Global.Y, out rgx, out rgy);
            Vector3 globalRotated = new Vector3(rgx, rgy, z);
            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);

            DepthFar = Math.Min(DepthFar, cd);
            DepthNear = Math.Max(DepthNear, cd);


            Color finalFogColor = playerGlobal != null ? this.GetFogColor(z) : Color.Transparent;
            //Color finalFogColor = playerGlobal != null ? this.GetFogColorSphere(new Vector3(gx, gy, z)) : Color.Transparent;

            if (finalFogColor.A == 255)
                return false; // if fully obstructed by fog, don't draw nor check for mouseover
            MyVertex[] vertices;
            //var finalsb = this.SpriteBatch;
            //var finalalpha = 1f;
            if (a.HideWalls)
                if (HidesPlayer(playerGlobal, hiddenRects, cell, ref screenBounds, rgx, rgy))
                {
                    //finalsb = this.WaterSpriteBatch;
                    //finalalpha = .33f;
                    // draw a transparent block here and return?
                    // do i need to pass the fog? 

                    //if (cell.CachedVertices != null)
                    //{
                    //    cell.CachedVertices[0].Position = new Vector3(screenBoundsVector4.X, screenBoundsVector4.Y, cd);
                    //    cell.CachedVertices[1].Position = new Vector3(screenBoundsVector4.X + screenBoundsVector4.Z, screenBoundsVector4.Y, cd);
                    //    cell.CachedVertices[2].Position = new Vector3(screenBoundsVector4.X + screenBoundsVector4.Z, screenBoundsVector4.Y + screenBoundsVector4.W, cd);
                    //    cell.CachedVertices[3].Position = new Vector3(screenBoundsVector4.X, screenBoundsVector4.Y + screenBoundsVector4.W, cd);
                    //    sb.Draw(Block.Atlas.Texture, cell.CachedVertices);
                    //    return true;
                    //}

                    vertices = block.Draw(chunk, cell.LocalCoords, this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, cd, cell.Variation, cell.Orientation, cell.BlockData);

                    //if (vertices != null)
                    //    cell.CachedVertices = vertices;
                    return false;
                }


            //if (cell.CachedVertices != null)
            //{
            //    cell.CachedVertices[0].Position = new Vector3(screenBoundsVector4.X, screenBoundsVector4.Y, cd);
            //    cell.CachedVertices[1].Position = new Vector3(screenBoundsVector4.X + screenBoundsVector4.Z, screenBoundsVector4.Y, cd);
            //    cell.CachedVertices[2].Position = new Vector3(screenBoundsVector4.X + screenBoundsVector4.Z, screenBoundsVector4.Y + screenBoundsVector4.W, cd);
            //    cell.CachedVertices[3].Position = new Vector3(screenBoundsVector4.X, screenBoundsVector4.Y + screenBoundsVector4.W, cd);

            //sb.Draw(Block.Atlas.Texture, cell.CachedVertices);

            //}
            //else
            //{
            vertices = block.Draw(chunk, cell.LocalCoords, this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, cd, cell.Variation, cell.Orientation, cell.BlockData);
            //    //block.Draw(finalsb, this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White * finalalpha, cd, cell.Variation, cell.Orientation, cell.BlockData);
            //    if (vertices != null)
            //        cell.CachedVertices = vertices;
            //}

            //map.TilesDrawn++;
            if (block != BlockDefOf.Water || (ToolManager.Instance.ActiveTool is PlayerControl.BlockPainter)) // TODO: tidy this up
                CheckMouseover(map, chunk, cell, ref screenBounds, cd);
            return true;

        }

        public float FogLevel = 0;
        public Color GetFogColor(int z)
        {
            if (!Fog)
                return Color.Transparent;
            if (PlayerOld.Actor == null)
                return Color.Transparent;
            //var z = global.Z;
            var playerGlobal = PlayerOld.Actor.Transform.Global;// Player.Actor.Global;
            if (playerGlobal.Z > 1)
                //if (z < playerGlobal.Z)
                if (z < playerGlobal.Z + this.FogLevel)
                {
                    //var d = Math.Abs(z - playerGlobal.Z + 1);
                    var d = Math.Abs(z - playerGlobal.Z + 1 - this.FogLevel);
                    float fogDistance = 8;// 16; //5;
                    d = MathHelper.Clamp(d, 0, fogDistance) / fogDistance;
                    //if (d == 1)
                    //    return Color.Transparent;
                    var fog = Color.Lerp(Color.White, Color.DarkSlateBlue, d);
                    var val = (byte)(d * 255);
                    var finalFogColor = new Color(fog.R, fog.G, fog.B, val);
                    return finalFogColor;
                }
            return Color.Transparent;
        }
        public Color GetFogColorNew(int z)
        {
            if (!Fog)
                return Color.Transparent;
            if (this.LastZTarget > 1)
                if (z < this.LastZTarget - FogZOffset)
                {
                    //var d = Math.Abs(z - playerGlobal.Z + 1);
                    var d = Math.Abs(z - this.LastZTarget + FogZOffset);
                    d = MathHelper.Clamp(d, 0, FogFadeLength) / FogFadeLength;
                    var fog = Color.Lerp(Color.White, Color.DarkSlateBlue, d);
                    var val = (byte)(d * 255);
                    var finalFogColor = new Color(fog.R, fog.G, fog.B, val);
                    return finalFogColor;
                }
            return Color.Transparent;
        }

        //public Color GetFogColorSphere(Vector3 global)
        //{
        //    if (!Fog)
        //        return Color.Transparent;
        //    if (Player.Actor == null)
        //        return Color.Transparent;
        //    var playerGlobal = Player.Actor.Transform.Global;
        //    var d = Vector3.Distance(global, playerGlobal);

        //    float fogDistance = 8;// 16; //5;
        //    d = MathHelper.Clamp(d, 0, fogDistance) / fogDistance;
        //    var fog = Color.Lerp(Color.White, Color.DarkSlateBlue, d);
        //    var val = (byte)(d * 255);
        //    var finalFogColor = new Color(fog.R, fog.G, fog.B, val);
        //    return finalFogColor;
        //}
        private void CheckMouseover(IMap map, Chunk chunk, Cell cell, ref Rectangle screenBounds, float mouseoverDepthToCheck)
        {
            if (!screenBounds.Intersects(Controller.Instance.MouseRect))
                return;

            int xx = (int)((Controller.Instance.msCurrent.X - screenBounds.X) / (float)Zoom);
            int yy = (int)((Controller.Instance.msCurrent.Y - screenBounds.Y) / (float)Zoom);

            Vector3 face;
            if (!Block.BlockMouseMap.HitTest(xx, yy, out face))
                return;
            if (map.GetMouseover() == null)
            {
                map.SetMouseover(new WorldPosition(map, new Vector3(chunk.Start.X + cell.X, chunk.Start.Y + cell.Y, cell.Z)));
            }
            else
            {
                float existingMouseoverDepth = map.GetMouseover().Global.GetDrawDepth(map, this);
                if (existingMouseoverDepth > mouseoverDepthToCheck)
                    return;
                else
                    map.SetMouseover(new WorldPosition(map, new Vector3(chunk.Start.X + cell.X, chunk.Start.Y + cell.Y, cell.Z)));
            }
            //if (map.Mouseover == null)
            //{
            //    map.Mouseover = new Position(map, new Vector3(chunk.Start.X + cell.X, chunk.Start.Y + cell.Y, cell.Z));
            //}
            //else
            //{
            //    float existingMouseoverDepth = map.Mouseover.Global.GetDrawDepth(map, this);
            //    if (existingMouseoverDepth > mouseoverDepthToCheck)
            //        return;
            //    else
            //        map.Mouseover = new Position(map, new Vector3(chunk.Start.X + cell.X, chunk.Start.Y + cell.Y, cell.Z));
            //}
        }

        public int MaxDrawZ;
        int PlayerHeightMapValue;
        Vector2 CameraOffset;

        internal void DrawChunk(MySpriteBatch sb, IMap map, Chunk chunk, Vector3? playerGlobal, List<Rectangle> hiddenRects, EngineArgs a)
        {
            throw new Exception();

            //if (playerGlobal.HasValue)
            //    PlayerHeightMapValue = map.GetHeightmapValue(playerGlobal.Value);
            //this.MaxDrawZ = GetMaxDrawLevel();
            
            //foreach (var cell in chunk.VisibleOutdoorCells)
            //    DrawCell(sb, map, chunk, cell.Value, playerGlobal, hiddenRects, a);
      

            //foreach (var blockentity in chunk.BlockEntities)
            //    blockentity.Value.Draw(this, map, blockentity.Key.ToGlobal(chunk));

            //// draw full topmost slice
            //for (int i = 0; i < Chunk.Size; i++)
            //{
            //    for (int j = 0; j < Chunk.Size; j++)
            //    {
            //        Cell cell;
            //        var toplevel = MaxDrawZ;// this.GetMaxDrawLevel();
            //        var local = new Vector3(i, j, toplevel);//this.DrawLevel);
            //        if (!chunk.TryGetCell(local, out cell))
            //            continue;
            //        if (cell.Block.Type == Block.Types.Air)
            //            continue;
            //        if (chunk.VisibleOutdoorCells.ContainsKey(Chunk.GetCellIndex(local))) // VERY SLOW
            //            continue;
            //        var vertices = DrawCell(sb, map, chunk, cell, playerGlobal, hiddenRects, a);
            //    }
            //}

            //// draw front of frontmost chunks here?

        }

        private void DrawChunkMesh(IMap map, Chunk chunk, MySpriteBatch buffer)
        {
            float x, y;
            Coords.Iso(this, chunk.MapCoords.X * Chunk.Size, chunk.MapCoords.Y * Chunk.Size, 0, out x, out y);

            var world = Matrix.CreateTranslation(new Vector3(x, y, ((chunk.MapCoords.X + chunk.MapCoords.Y) * Chunk.Size)));
            this.Effect.Parameters["World"].SetValue(world);

            //this.PrepareShader(map);

            this.Effect.CurrentTechnique.Passes["Pass1"].Apply();

            buffer.Draw();
        }

        public void PrepareShader(IMap map)
        {
            var view =// Matrix.Identity;
           new Matrix(
              1.0f, 0.0f, 0.0f, 0.0f,
              0.0f, -1.0f, 0.0f, 0.0f,
              0.0f, 0.0f, 1.0f, 0.0f,
              0.0f, 0.0f, 0.0f, 1.0f);
            float camerax = this.Coordinates.X;
            float cameray = this.Coordinates.Y;
            view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            var near = 0f;//Chunk.Size * map.GetSizeInChunks() * 2 * this.Zoom;
            var far = -Chunk.Size * map.GetSizeInChunks() * 2 * this.Zoom;

            //var near = 0f;
            //var s = map.GetSizeInChunks();
            //int xx, yy;
            //Coords.Rotate(this, s, s, out xx, out yy);
            //var far = -Chunk.Size * (xx + yy) * this.Zoom;

            near = -GetFarDepth(map) * this.Zoom;
            far = -GetNearDepth(map) * this.Zoom;
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, near, far);
            this.Effect.CurrentTechnique = this.Effect.Techniques["Chunks"];
            this.Effect.Parameters["View"].SetValue(view);
            this.Effect.Parameters["Projection"].SetValue(projection);

            DepthNear = this.GetNearDepth(map);
            DepthFar = this.GetFarDepth(map);
            this.Effect.Parameters["FarDepth"].SetValue(DepthFar);
            this.Effect.Parameters["NearDepth"].SetValue(DepthNear);
            this.Effect.Parameters["DepthResolution"].SetValue((2) / (DepthNear - DepthFar));
            this.Effect.Parameters["OutlineThreshold"].SetValue((1) / (DepthNear - DepthFar));
        }

        private void PrepareShaderTransparent(IMap map)
        {
            var view =// Matrix.Identity;
            new Matrix(
               1.0f, 0.0f, 0.0f, 0.0f,
               0.0f, -1.0f, 0.0f, 0.0f,
               0.0f, 0.0f, 1.0f, 0.0f,
               0.0f, 0.0f, 0.0f, 1.0f);
            float camerax = this.Coordinates.X;
            float cameray = this.Coordinates.Y;
            view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            //var near = 0;
            //var far = -Chunk.Size * map.GetSizeInChunks() * 2 * this.Zoom;
            var near = -GetFarDepth(map) * this.Zoom;
            var far = -GetNearDepth(map) * this.Zoom;
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, near, far);//0, 1);
            this.Effect.CurrentTechnique = this.Effect.Techniques["CombinedWater"];
            this.Effect.Parameters["View"].SetValue(view);
            this.Effect.Parameters["Projection"].SetValue(projection);

            DepthNear = this.GetNearDepth(map);
            DepthFar = this.GetFarDepth(map);
            this.Effect.Parameters["FarDepth"].SetValue(DepthFar);
            this.Effect.Parameters["NearDepth"].SetValue(DepthNear);
            this.Effect.Parameters["DepthResolution"].SetValue((2) / (DepthNear - DepthFar));
            this.Effect.Parameters["OutlineThreshold"].SetValue((1) / (DepthNear - DepthFar));

        }

        private void DrawChunkTransparentMesh(IMap map, Chunk chunk, MySpriteBatch buffer)
        {
            float x, y;
            Coords.Iso(this, chunk.MapCoords.X * Chunk.Size, chunk.MapCoords.Y * Chunk.Size, 0, out x, out y);

            var world = Matrix.CreateTranslation(new Vector3(x, y, ((chunk.MapCoords.X + chunk.MapCoords.Y) * Chunk.Size)));
            this.Effect.Parameters["World"].SetValue(world);

            //PrepareShaderTransparent(map);



            this.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            //chunk.VertexBuffer.Draw();
            buffer.Draw();
        }



        private static bool PlayerIsUnderground(Map map, Chunk chunk, Vector3? playerGlobal, Cell cell)
        {
            Vector3 playerLocal = playerGlobal.Value.ToLocal();// Position.ToLocal(playerGlobal.Value);
            Chunk pChunk = Position.GetChunk(map, playerGlobal.Value);
            byte playerSun = pChunk.GetSunlight(playerLocal);
            byte cellSun = chunk.GetSunlight(cell.X, cell.Y, cell.Z + 1);
            if (playerSun < 15)
                if (cellSun == 15)
                    return true;
            return false;
        }

        private bool HidesPlayer(Vector3? playerGlobal, List<Rectangle> hiddenRects, Cell cell, ref Rectangle tileBounds, int rgx, int rgy)
        {
            //if (Engine.HideWalls)
            //    return false;
            // don't draw tile if it hides the player character

            if (!cell.Opaque)
                return false;

            if (!playerGlobal.HasValue)
                return false;
            if (cell.Z < playerGlobal.Value.Z)
                return false;

            float prgx, prgy;
            Vector3 round = playerGlobal.Value.RoundXY();//.Round();
            Coords.Rotate(this, round.X, round.Y, out prgx, out prgy);
            if ((rgx >= prgx) && (rgy >= prgy))
            //if ((rgx >= prgx - 1) && (rgy >= prgy - 1))
            {
                bool hide = false;
                foreach (Rectangle hiddenRect in hiddenRects)
                    if (tileBounds.Intersects(hiddenRect))
                    {
                        hide = true;
                        continue;
                    }
                if (hide)
                    return true;
            }


            return false;
        }

        //public static float GetCellDepth(Camera camera, Map map, Vector3 global)
        //{
        //    Chunk chunk; Cell cell;
        //    Position.TryGet(map, global, out cell, out chunk);
        //    float localdFar, localdNear;
        //    chunk.GetLocalDepthRange(camera, out localdNear, out localdFar);
        //    float cd = Position.GetDepth(global) / Chunk.Dmax;
        //    cd = 1 - (localdFar + cd * (localdNear - localdFar));
        //    return cd;
        //}

        //public static float GetCellDepth(float localdNear, float localdFar, ref Vector3 localRotated)
        //{
        //    float cd = Position.GetDepth(localRotated) / Chunk.Dmax;
        //    cd = 1 - (localdFar + cd * (localdNear - localdFar));
        //    return cd;
        //}

        //private float GetCellDepth(float localdNear, float localdFar, int z, int x, int y)
        //{
        //    int rx, ry;
        //    Coords.Rotate(this, x, y, out rx, out ry);
        //    //float cd = (rx + ry + cellZ) / (float)(Chunk.Size + Chunk.Size - 3 + Map.MaxHeight);
        //    //float dmax = Chunk.DepthMax();
        //    Vector3 rotated = new Vector3(rx, ry, z);
        //    float cd = Position.GetDepth(rotated) / Chunk.Dmax;
        //    cd = 1 - (localdFar + cd * (localdNear - localdFar));
        //    Vector3 local = new Vector3(x, y, z);
        //    return cd;
        //}

        private static Color GetLight(Camera camera, Map map, Chunk chunk, int localx, int localy, int gx, int gy, int z)
        {
            byte sunsouth, suneast, suntop;
            int rightx, righty, leftx, lefty;
            Coords.Rotate(camera, 1, 0, out rightx, out righty);
            Coords.Rotate(camera, 0, 1, out leftx, out lefty);

            //int
            //    leastx = localx + rightx,
            //    leasty = localy - righty,
            //    lsouthx = localx - leftx,
            //    lsouthy = localy + lefty;

            //if (leastx < 0 || leastx > Chunk.Size ||
            //    leasty < 0 || leasty > Chunk.Size)
            //    Chunk.TryGetSunlight(gx + rightx, gy - righty, z, out suneast);
            //else
            //    suneast = chunk.GetSunlight(localx + rightx, localy - righty, z);

            //if (lsouthx < 0 || lsouthx > Chunk.Size ||
            //    lsouthy < 0 || lsouthy > Chunk.Size)
            //    Chunk.TryGetSunlight(gx - leftx, gy + lefty, z, out sunsouth);
            //else
            //    sunsouth = chunk.GetSunlight(localx - leftx, localy + lefty, z);
            //  Color sky = Color.HotPink;/// Color.Multiply(Color.HotPink, 0.5f);
            // float sky = 0.5f;
            Chunk.TryGetSunlight(map, gx + rightx, gy - righty, z, out suneast);
            Chunk.TryGetSunlight(map, gx - leftx, gy + lefty, z, out sunsouth);
            suntop = z < Map.MaxHeight - 1 ? chunk.GetSunlight(localx, localy, z + 1) : (byte)15;
            // Color color = Color.Multiply(new Color(suneast / 15f, sunsouth / 15f, suntop / 15f), Map.Instance.DayTime);//, 255);
            Color color = new Color(suneast / 15f, sunsouth / 15f, suntop / 15f);
            Engine.LightQueries++;
            return color;
        }

        private static Color GetLight(Map map, Chunk chunk, int localx, int localy, int gx, int gy, int z)
        {
            byte sunsouth, suneast, suntop;
            // int gx, gy;// = chunk.X * Chunk.Size + x, gy = chunk.Y * Chunk.Size + y;
            // Position.GetGlobal(chunk, localx, localy, out gx, out gy);
            if (localy > 14)
                Chunk.TryGetSunlight(map, gx, gy + 1, z, out sunsouth);
            else
                sunsouth = chunk.GetSunlight(localx, localy + 1, z);
            if (localx > 14)
                Chunk.TryGetSunlight(map, gx + 1, gy, z, out suneast);
            else
                suneast = chunk.GetSunlight(localx + 1, localy, z);
            suntop = z < Map.MaxHeight - 1 ? chunk.GetSunlight(localx, localy, z + 1) : (byte)15;
            Color color = new Color(suneast / 16f, sunsouth / 16f, suntop / 16f);//, 255);
            Engine.LightQueries++;
            return color;
        }


        public static LightToken GetFinalLight(Camera camera, IMap map, Chunk chunk, Cell cell, int gx, int gy, int z, bool updateblockfaces = true)
        {
            // UNCOMMENT THIS FOR THE LOVE OF GOD
            //if (chunk.LightCache.TryGetValue(new Vector3(gx, gy, z), out color))
            //    return color;
            //if (cell.Light != null)
            //    return cell.Light;
            LightToken cached;
            Vector3 global = new Vector3(gx, gy, z);
            if (gx == 28 && gy == 0 && z == 64)
            {

            }
            if (chunk.LightCache2.TryGetValue(global, out cached))
            {
                //sun = cached.Sun;
                //block = cached.Block;
                return cached;
            }

            // update block exposed faces too here?
            // TESTING IF REMOVING THIS BREAKS ANYTHING
            //if (updateblockfaces)
            //    chunk.UpdateBlockFaces(cell); // COMMENT if i want to see visible horizontal slices of the map


            //byte
            //    sunsouth, suneast, suntop,
            //    blocksouth, blockeast, blocktop;

            //int rightx, righty, leftx, lefty;
            Coords.Rotate(camera, 1, 0, out int rightx, out int righty);
            Coords.Rotate(camera, 0, 1, out int leftx, out int lefty);

            Chunk.TryGetFinalLight(map, gx + rightx, gy - righty, z, out byte suneast, out byte blockeast);
            Chunk.TryGetFinalLight(map, gx - leftx, gy + lefty, z, out byte sunsouth, out byte blocksouth);
            byte suntop, blocktop;
            if (z + 1 < map.MaxHeight)
            {
                suntop = Math.Max((byte)0, chunk.GetSunlight(cell.X, cell.Y, z + 1));
                blocktop = chunk.GetBlockLight(cell.X, cell.Y, z + 1);
            }
            else
            {
                suntop = 15;
                blocktop = 15;
            }
            //// if block at top of map, light it up
            //if (z == Map.MaxHeight - 1)
            //    finalsuntop = 15;

            //  color = new Color(suneast / 15f, sunsouth / 15f, suntop / 15f);
            //color = new Color((suneast + 1) / 16f, (sunsouth + 1) / 16f, (suntop + 1) / 16f);

            //Color sun = new Color((suneast) / 15f, (sunsouth) / 15f, (suntop) / 15f);
            //Vector4 block = new Vector4((blockeast) / 15f, (blocksouth) / 15f, (blocktop) / 15f, 1f);


            ////

            Color sun = new((suneast + 1) / 16f, (sunsouth + 1) / 16f, (suntop + 1) / 16f);
            Vector4 block = new((blockeast + 1) / 16f, (blocksouth + 1) / 16f, (blocktop + 1) / 16f, 1f);

            //Color sun = new Color(
            //    (Cell.CheckFace(camera, cell, Vector3.UnitX) ? 1f : 0) * (suneast + 1) / 16f,
            //    (Cell.CheckFace(camera, cell, Vector3.UnitY) ? 1f : 0) * (sunsouth + 1) / 16f,
            //    (Cell.CheckFace(camera, cell, Vector3.UnitZ) ? 1f : 0) * (suntop + 1) / 16f);
            //Vector4 block = new Vector4(
            //    (Cell.CheckFace(camera, cell, Vector3.UnitX) ? 1f : 0) * (blockeast + 1) / 16f,
            //    (Cell.CheckFace(camera, cell, Vector3.UnitY) ? 1f : 0) * (blocksouth + 1) / 16f,
            //    (Cell.CheckFace(camera, cell, Vector3.UnitZ) ? 1f : 0) * (blocktop + 1) / 16f, 1f);

            Engine.LightQueries++;
            // AND THIS
            //chunk.CacheLight(new Vector3(gx, gy, z), color);
            LightToken light = new LightToken() { Global = global, Sun = sun, Block = block };
            //chunk.LightCache2.AddOrUpdate(global, light, (pos, existing) => existing);
            chunk.LightCache2[global] = light;
            //cell.Light = light;
            return light;
        }



        //private static void GetFinalLight(Camera camera, Map map, Chunk chunk, int localx, int localy, int gx, int gy, int z, out Color south, out Color east, out Color top)
        //{
        //    byte sunsouth, suneast, suntop;
        //    int rightx, righty, leftx, lefty;
        //    Coords.Rotate(camera, 1, 0, out rightx, out righty);
        //    Coords.Rotate(camera, 0, 1, out leftx, out lefty);

        //    Chunk.TryGetFinalLight(map, gx + rightx, gy - righty, z, out suneast);
        //    Chunk.TryGetFinalLight(map, gx - leftx, gy + lefty, z, out sunsouth);
        //    byte finalsuntop = (byte)Math.Max(0, chunk.GetSunlight(localx, localy, z + 1) - Map.Instance.SkyDarkness);
        //    suntop = z < Map.MaxHeight - 1 ? Math.Max(chunk.GetCellLight(localx, localy, z + 1), finalsuntop) : (byte)15;

        //    south = new Color(sunsouth / 15f, sunsouth / 15f, sunsouth / 15f);
        //    east = new Color(suneast / 15f, suneast / 15f, suneast / 15f);
        //    top = new Color(suntop / 15f, suntop / 15f, suntop / 15f);
        //    Engine.LightQueries++;
        //}

        private bool HideWalls(Chunk chunk, Cell cell, Edges edges, Vector3? playerGlobal)
        {
            if (!Engine.HideWalls)
                return false;
            if (!playerGlobal.HasValue)
                return false;
            if (PlayerOld.Actor == null)
                return false;

            Vector3 cellGlobal = cell.GetGlobalCoords(chunk);


            // TODO: LOL
            bool inFront = false;
            switch ((int)this.Rotation)
            {
                case 0:
                    if (cellGlobal.X >= playerGlobal.Value.X - 1)
                        if (cellGlobal.Y >= playerGlobal.Value.Y - 1)
                            inFront = true;
                    break;
                case 1:
                    if (cellGlobal.X >= playerGlobal.Value.X - 1)
                        if (cellGlobal.Y <= playerGlobal.Value.Y + 1)
                            inFront = true;
                    break;
                case 2:
                    if (cellGlobal.X <= playerGlobal.Value.X + 1)
                        if (cellGlobal.Y <= playerGlobal.Value.Y + 1)
                            inFront = true;
                    break;
                case 3:
                    if (cellGlobal.X <= playerGlobal.Value.X + 1)
                        if (cellGlobal.Y >= playerGlobal.Value.Y - 1)
                            inFront = true;
                    break;
                default:
                    break;
            }
            if (!inFront)
                return false;
            if ((cell.VerticalEdges & VerticalEdges.Top) != VerticalEdges.Top)
                if ((edges & Edges.West) == Edges.West || (edges & Edges.North) == Edges.North)
                    return true;

            //if (cellGlobal.X >= playerGlobal.X - 1)
            //    if (cellGlobal.Y >= playerGlobal.Y - 1)
            //    {
            //        if ((cell.VerticalEdges & VerticalEdges.Top) != VerticalEdges.Top)
            //            if ((edges & Edges.West) == Edges.West || (edges & Edges.North) == Edges.North)
            //                return true;
            //    }
            return false;
        }

        //private void HideUndergroundDo(SpriteBatch sb, Map map, Chunk chunk, float localdNear, float localdFar)
        //{
        //    throw new Exception();
        //    if (!this.HideUnderground)
        //        return;
        //    for (int j = 0; j < Chunk.Size; j++)
        //        for (int i = 0; i < Chunk.Size; i++)
        //        {
        //            //Cell cell = chunk.CellGrid2[Chunk.FindIndex(new Vector3(i, j, map.DrawLevel - 1))];// Cell.GetUniqueIndex(new Vector3(i, j, map.DrawLevel))];
        //            Cell cell = chunk.CellGrid2[Chunk.GetCellIndex(new Vector3(i, j, this.DrawLevel - 1))];// Cell.GetUniqueIndex(new Vector3(i, j, map.DrawLevel))];

        //            // TODO: optimize this
        //            //if(chunk.VisibleOutdoorCells.ContainsKey(Cell.GetUniqueIndex(new Vector3(i, j, map.DrawLevel - 1))))
        //            //if (chunk.VisibleOutdoorCells.ContainsKey(Chunk.FindIndex(new Vector3(i, j, map.DrawLevel - 1))))
        //            if (chunk.VisibleOutdoorCells.ContainsKey(Chunk.GetCellIndex(new Vector3(i, j, this.DrawLevel - 1))))
        //            {
        //                continue;
        //            }
        //            if (cell.Block.Type == Block.Types.Air)
        //                continue;
        //            int x = cell.X, y = cell.Y, z = cell.Z;
        //            int rx, ry;
        //            Coords.Rotate(this, x, y, out rx, out ry);
        //            //float cd = (rx + ry + cellZ) / (float)(Chunk.Size + Chunk.Size - 3 + Map.MaxHeight);
        //            //float dmax = Chunk.DepthMax();
        //            Vector3 rotated = new Vector3(rx, ry, z);
        //            //float cd = Position.GetDepth(rotated) / Chunk.Dmax;
        //            //cd = 1 - (localdFar + cd * (localdNear - localdFar));
        //            float cd = new Vector3(chunk.Start.X + x, chunk.Start.Y + y, z).GetDrawDepth(map, this);

        //            Color color = Color.Black;
        //            //Sprite tileSprite = Block.TileSprites[Block.Types.Sand];
        //            //Rectangle spriteBounds = tileSprite.GetBounds();
        //            Rectangle spriteBounds = Block.Bounds;// Block.Sand.Variations.First().Texture.Bounds;
        //            Rectangle tileBounds = GetScreenBounds(chunk.Start.X + x, chunk.Start.Y + y, z, spriteBounds);
        //            if (!ViewPort.Intersects(tileBounds))
        //                continue;
        //            Vector2 screenLoc = new Vector2(tileBounds.X, tileBounds.Y);
        //            //Rectangle sourceRect = tileSprite.SourceRects[0][0];
        //            Rectangle sourceRect = Block.Sand.Variations.First().Rectangle;
        //            sb.Draw(Map.TerrainSprites, screenLoc, sourceRect, color, 0, Vector2.Zero, Zoom, SpriteEffects.None, cd);
        //            map.TilesDrawn++;
        //        }
        //}

        //private void DrawOutlines(SpriteBatch sb, Map map, Edges hEdges, VerticalEdges vEdges, float depth, ref Rectangle tileBounds, ref Vector2 screenLoc, ref Rectangle sourceRect, ref Color color)
        //{
        //    //Color c = color * .5f;
        //    Color c = new Color(new Vector4(color.ToVector3() * 0.33f, 1)); //Color.Black;// 
        //    // Edges edges = (Edges)cell.GetOutlines(this);


        //    //Cell n;
        //    //if (Position.TryGetCell(new Vector3(chunk.Start.X + cell.X - 1, chunk.Start.Y + cell.Y, cell.Z), out n))
        //    //    if (n.Tile == TileBase.Types.Air)
        //    //Vector2 origin = new Vector2(sourceRect.Width / 2, sourceRect.Height - Tile.Depth / 2); //Tile.OriginCenter;// 
        //    if ((hEdges & Edges.West) == Edges.West)
        //    {
        //        sb.Draw(Map.TerrainSprites, screenLoc - new Vector2(0, Zoom + Zoom), new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width / 2, sourceRect.Height), c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //        sb.Draw(Map.TerrainSprites, screenLoc - new Vector2(Zoom + Zoom, 0), new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width / 2, sourceRect.Height - Block.Depth / 2), c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //        //sb.Draw(Map.TerrainSprites, screenLoc - new Vector2(Zoom + Zoom, Zoom + Zoom), new Rectangle(sourceRect.X, sourceRect.Y, sourceRect.Width / 2, sourceRect.Height), c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //        map.TileOutlinesDrawn++;
        //    }
        //    //if (Position.TryGetCell(new Vector3(chunk.Start.X + cell.X, chunk.Start.Y + cell.Y - 1, cell.Z), out n))
        //    //    if (n.Tile == TileBase.Types.Air)
        //    if ((hEdges & Edges.North) == Edges.North)
        //    {
        //        sb.Draw(Map.TerrainSprites, screenLoc + new Vector2(tileBounds.Width / 2, -Zoom - Zoom), new Rectangle(sourceRect.X + sourceRect.Width / 2, sourceRect.Y, sourceRect.Width / 2, sourceRect.Height), c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //        sb.Draw(Map.TerrainSprites, screenLoc + new Vector2(tileBounds.Width / 2 + Zoom + Zoom, 0), new Rectangle(sourceRect.X + sourceRect.Width / 2, sourceRect.Y, sourceRect.Width / 2, sourceRect.Height - Block.Depth / 2), c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //        //sb.Draw(Map.TerrainSprites, screenLoc + new Vector2(tileBounds.Width / 2 + Zoom + Zoom, -Zoom - Zoom), new Rectangle(sourceRect.X + sourceRect.Width / 2, sourceRect.Y, sourceRect.Width / 2, sourceRect.Height - Tile.Depth / 2), c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //        map.TileOutlinesDrawn++;
        //    }
        //    //if (Position.TryGetCell(new Vector3(chunk.Start.X + cell.X, chunk.Start.Y + cell.Y, cell.Z - 1), out n))
        //    //    if (n.Tile == TileBase.Types.Air)
        //    if ((vEdges & VerticalEdges.Bottom) == VerticalEdges.Bottom)
        //    {
        //        sb.Draw(Map.TerrainSprites, screenLoc + new Vector2(0, Zoom + Zoom), sourceRect, c, 0, Vector2.Zero, Zoom, SpriteEffects.None, depth);
        //       // sb.Draw(Map.TerrainSprites, screenLoc + Zoom * (origin + new Vector2(0, sourceRect.Height)), new Rectangle(sourceRect.X, sourceRect.Y + sourceRect.Height - Tile.Depth / 2, sourceRect.Width, Tile.Depth / 2), c, 0, origin, Zoom * 1.1f, SpriteEffects.None, depth);
        //        map.TileOutlinesDrawn++;
        //    }

        //    //  if (screenLoc.X > 0 && screenLoc.Y > 0)
        //    //      Console.WriteLine(screenLoc);
        //}
        //private BlockBorderToken.Sides DrawBorder(Edges hEdges, VerticalEdges vEdges)
        //{
        //    BlockBorderToken.Sides side = BlockBorderToken.Sides.None;
        //    if ((hEdges & Edges.West) == Edges.West)
        //    {
        //        side |= BlockBorderToken.Sides.Left;
        //    }
        //    if ((hEdges & Edges.North) == Edges.North)
        //    {
        //        side |= BlockBorderToken.Sides.Right;// true;
        //    }
        //    if ((vEdges & VerticalEdges.Bottom) == VerticalEdges.Bottom)
        //    {
        //        side |= BlockBorderToken.Sides.Bottom;// true; true;
        //    }
        //    return side;
        //}
        Vector3 LastMouseover = new Vector3(float.MinValue);
        Block LastMouseoverBlock;

        public void CreateMouseover(IMap map, Vector3 global)
        {
            //  Console.WriteLine(global);
            //Console.WriteLine(Controller.Instance.MouseoverNext.Object);
            if (Controller.Instance.MouseoverBlockNext.Object != null)
            {
                return;
            }
            Cell cell;
            Chunk chunk;
            //if (!Position.TryGet(map, global, out cell, out chunk))
            if (!map.TryGetAll(global, out chunk, out cell))
                return;
            GameObject mouseoverLast;

            //BlockComponent tileCompLast = null;
            //// WARNING! potential problems here
            //if (Controller.Instance.Mouseover.TryGet<GameObject>(out mouseoverLast))
            //{
            //    if (!mouseoverLast.TryGetComponent<BlockComponent>(out tileCompLast))
            //    {
            //        return;
            //    }
            //}



            //check if mouseover cell is the same as the last one
            //bool same = false;
            //if (tileCompLast != null)
            //    if (map.GetCell(mouseoverLast.Global) == cell)
            //    {
            //        //if (cell.Block == this.LastMouseoverBlock)
            //        same = true;
            //    }


            //bool same = false;
            //var prevtarget = Controller.Instance.Mouseover.Object as TargetArgs;
            //if (prevtarget != null)
            //    if (prevtarget.Global == global)
            //        same = true;


            GameObject tar = null;
            //BlockComponent tile;
            //PositionComponent posComp;
            //Position pos;
            //if (same)
            //{
            //    tar = mouseoverLast;
            //    //tile = tileCompLast;
            //    posComp = mouseoverLast.Transform;
            //    pos = posComp.Position;
            //}
            //else
            //    Cell.TryGetObject(map, global, out tar);

            //Sprite tileSprite = Block.TileSprites[Block.Types.Sand];
            Rectangle texbounds = Block.Bounds;// Block.Sand.Variations.First().Texture.Bounds;

            Rectangle cellScreenBounds = GetScreenBounds(global, texbounds);//tileSprite.GetBounds());
            Vector2 uvCoords = new Vector2((Controller.Instance.msCurrent.X - cellScreenBounds.X) / (float)Zoom, (Controller.Instance.msCurrent.Y - cellScreenBounds.Y) / (float)Zoom);
            //int faceIndex = (int)uvCoords.Y * Block.MouseMapSprite.Width + (int)uvCoords.X;
            int faceIndex = (int)uvCoords.Y * Block.MouseMapSprite.Width + (int)uvCoords.X;

            // find block coordinates
            Color sample = Block.BlockCoordinatesFull[faceIndex];
            float u = sample.R / 255f;
            float v = sample.G / 255f;
            float w = sample.B / 255f;
            //Vector2 coords = new Vector2(sample.R / 255f, sample.G / 255f);
            Vector3 precise = new Vector3(u, v, w);// Vector3.Zero;
            precise.X -= 0.5f;
            precise.Y -= 0.5f; // compensate for (0,0) being at the center of the block


            //Color faceColor = Block.TileSprites[cell.Type].MouseMap.Map[0][0][faceIndex];
            Color faceColor = Block.BlockMouseMap.Map[0][0][faceIndex];

            Vector3 vec, rotVec;
            //Block.TileSprites[cell.Type].MouseMap.HitTest((int)uvCoords.X, (int)uvCoords.Y, out vec);
            Block.BlockMouseMap.HitTest((int)uvCoords.X, (int)uvCoords.Y, out vec);


            // comment these lines if i want to select blocks even if mouseover face is inaccessible
            //if (!Cell.CheckFace(this, cell, vec))
            //    return;


            Coords.Rotate((int)this.Rotation, vec, out rotVec);
            precise = precise.Rotate(-this.Rotation);
            //precise = precise - precise * rotVec;
            // TODO: find more elegant way to do this
            if (rotVec == Vector3.UnitX || rotVec == -Vector3.UnitX)
                precise.X = 0;// = new Vector3(0, u, v);
            else if (rotVec == Vector3.UnitY || rotVec == -Vector3.UnitY)
                precise.Y = 0;// = new Vector3(u, 0, v);
            else if (rotVec == Vector3.UnitZ || rotVec == -Vector3.UnitZ)
                precise.Z = 0;// = new Vector3(u, v, 0);




            //byte light;
            //Color finalLight = GetFinalLight(this, map, chunk, cell.X, cell.Y, (int)global.X, (int)global.Y, cell.Z);

            ////int rx, ry;
            ////Coords.Rotate(this, faceColor.R, faceColor.G, out rx, out ry);
            ////Chunk.TryGetSunlight(global + new Vector3(rx, ry, faceColor.B), out light);

            //// we have the face vector from the faceColor, so multiply it with the final light to determine wether the mouseover face is lit
            //Color checkLight = new Color(finalLight.R * faceColor.R, finalLight.G * faceColor.G, finalLight.B * faceColor.B);




            //Chunk.TryGetSunlight(global + new Vector3(faceColor.R, faceColor.G, faceColor.B), out light);
            //if (light > 0)

            //if (Cell.CheckFace(this, cell, vec))
            //{
            var target = new TargetArgs(map, global, rotVec, precise);// { Network = map.Net };
            if (global != this.LastMouseover)
                Controller.Instance.MouseoverBlockNext.Object = target;// tar;
            else
                Controller.Instance.MouseoverBlockNext.Object = Controller.Instance.MouseoverBlock.Object;
            Controller.Instance.MouseoverBlockNext.Face = rotVec;// faceColor;
            Controller.Instance.MouseoverBlockNext.Precise = precise;
            Controller.Instance.MouseoverBlockNext.Target = target;// new TargetArgs(global, rotVec, precise);
            Controller.Instance.MouseoverBlockNext.Depth = global.GetMouseoverDepth(map, this);
            //}
            this.LastMouseover = global;
        }
        public void CreateMouseover(IMap map, Vector3 global, Rectangle rect, Vector2 point, bool behind)
        {
            if (Controller.Instance.MouseoverBlockNext.Object != null)
                return;
            Cell cell;
            Chunk chunk;
            if (!map.TryGetAll(global, out chunk, out cell))
                return;

            //BlockComponent tileCompLast = null;
            //// WARNING! potential problems here
            //if (Controller.Instance.Mouseover.TryGet<GameObject>(out GameObject mouseoverLast))
            //{
            //    if (!mouseoverLast.TryGetComponent<BlockComponent>(out tileCompLast))
            //    {
            //        return;
            //    }
            //}
            //check if mouseover cell is the same as the last one
            //bool same = false;
            //if (tileCompLast != null)
            //    if (map.GetCell(mouseoverLast.Global) == cell)
            //    {
            //        same = true;
            //    }



            Rectangle texbounds = Block.Bounds;
            Vector2 uvCoords = new Vector2((point.X - rect.X) / (float)Zoom, (point.Y - rect.Y) / (float)Zoom);
            int faceIndex = (int)uvCoords.Y * cell.Block.MouseMap.Texture.Width + (int)uvCoords.X;

            // find block coordinates
            //Color sample = Block.BlockCoordinatesFull[faceIndex];
            Color sample = cell.Block.UV[faceIndex];
            float u = sample.R / 255f;
            float v = sample.G / 255f;
            float w = sample.B / 255f;
            //Vector2 coords = new Vector2(sample.R / 255f, sample.G / 255f);
            Vector3 precise = new Vector3(u, v, w);// Vector3.Zero;
            precise.X -= 0.5f;
            precise.Y -= 0.5f; // compensate for (0,0) being at the center of the block

            //Color faceColor = Block.BlockMouseMap.Map[0][0][faceIndex];
            Color faceColor = cell.Block.MouseMap.Map[0][0][faceIndex];

            Vector3 vec, rotVec;
            //Block.BlockMouseMap.HitTest((int)uvCoords.X, (int)uvCoords.Y, out vec);
            cell.Block.MouseMap.HitTest(behind, (int)uvCoords.X, (int)uvCoords.Y, out vec);


            // comment these lines if i want to select blocks even if mouseover face is inaccessible
            //if (!Cell.CheckFace(this, cell, vec))
            //    return;


            Coords.Rotate((int)this.Rotation, vec, out rotVec);
            precise = precise.Rotate(-this.Rotation);
            //precise = precise - precise * rotVec;
            // TODO: find more elegant way to do this
            if (rotVec == Vector3.UnitX || rotVec == -Vector3.UnitX)
                precise.X = 0;// = new Vector3(0, u, v);
            else if (rotVec == Vector3.UnitY || rotVec == -Vector3.UnitY)
                precise.Y = 0;// = new Vector3(u, 0, v);
            else if (rotVec == Vector3.UnitZ || rotVec == -Vector3.UnitZ)
                precise.Z = 0;// = new Vector3(u, v, 0);


            Controller.SetMouseoverBlock(this, map, global, rotVec, precise);
            //return;
            //var target = new TargetArgs(global, rotVec, precise) { Network = map.Net };
            //if (global != this.LastMouseover)
            //    Controller.Instance.MouseoverNext.Object = target;// tar;
            //else
            //    Controller.Instance.MouseoverNext.Object = Controller.Instance.Mouseover.Object;
            //Controller.Instance.MouseoverNext.Face = rotVec;// faceColor;
            //Controller.Instance.MouseoverNext.Precise = precise;
            //Controller.Instance.MouseoverNext.Target = target;// new TargetArgs(global, rotVec, precise);
            //Controller.Instance.MouseoverNext.Depth = global.GetMouseoverDepth(map, this);
            //this.LastMouseover = global;
        }

       
        public int RenderIndex = 0;
        public RenderTarget2D MapRender,
            WaterRender, WaterDepth, WaterLight, WaterFog,
            WaterComposite,
            MapDepth, MapLight, TextureFogWater, MapComposite,
            RenderBeforeFog, LightBeforeFog, DepthBeforeFog, FogBeforeFog,
            FinalScene;
        public RenderTarget2D[] RenderTargets = new RenderTarget2D[5];//[4];//3];
        float b = 0;
        public void DrawMap(SpriteBatch sb, IMap map, ToolManager toolManager, UIManager ui, SceneState scene)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            //  Console.WriteLine("begin  draw map");
            if (map == null)
                return;


            int
                w = Width,// Game1.Instance.graphics.PreferredBackBufferWidth, 
                h = Height;// Game1.Instance.graphics.PreferredBackBufferHeight;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            if (this.RenderTargetsInvalid)
            {
                this.OnDeviceLost();
                this.RenderTargetsInvalid = false;
            }
            //if (MapRender == null)
            //    MapRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//,DiscardContents);//PreserveContents);//, false, SurfaceFormat.Vector4, DepthFormat.Depth16);
            ////if (WaterRender == null)
            ////    WaterRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//,DiscardContents);//PreserveContents);//, false, SurfaceFormat.Vector4, DepthFormat.Depth16);
            //if (MapDepth == null)
            //    MapDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents); //SurfaceFormat.Rg32 //Depth16
            ////DepthMap = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);//PreserveContents); //SurfaceFormat.Rg32 //Depth16
            //if (MapLight == null)
            //    MapLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);
            //if (this.TextureFogWater == null)
            //    this.TextureFogWater = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);
            //if (this.MapComposite == null)
            //    this.MapComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);

            //if (this.RenderBeforeFog == null)
            //    this.RenderBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);
            //if (this.LightBeforeFog == null)
            //    this.LightBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);   
            //if (this.DepthBeforeFog == null)
            //    this.DepthBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);   
            //if (this.FogBeforeFog == null)
            //    this.FogBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);

            //if (this.FinalScene == null)
            //    this.FinalScene = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);//PreserveContents);


            //if (WaterRender == null)
            //    WaterRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            //if (WaterDepth == null)
            //    WaterDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            //if (WaterLight == null)
            //    WaterLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            //if (WaterFog == null)
            //    WaterFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            //if (WaterComposite == null)
            //    WaterComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            RenderTargets[0] = MapRender;
            RenderTargets[1] = MapDepth;
            RenderTargets[2] = MapLight;
            this.RenderTargets[3] = this.TextureFogWater;

            //gd.Textures[1] = DepthMap;

            //gd.SamplerStates[1] = SamplerState.PointWrap;

            //Game1.Instance.Effect.Parameters["HasFog"].SetValue(map.Fog);
            //Game1.Instance.Effect.Parameters["Viewport"].SetValue(new Vector2(Game1.Instance.GraphicsDevice.Viewport.Width, Game1.Instance.GraphicsDevice.Viewport.Height));
            //Game1.Instance.Effect.Parameters["ChunkTex"].SetValue(new Vector2(Chunk.Width, Chunk.Height)); //TileBase.Height));

            //Vector4 ambient = Color.Lerp(Color.White, map.AmbientColor, (float)map.DayTimeNormal).ToVector4();
            //Game1.Instance.Effect.Parameters["AmbientLight"].SetValue(map.AmbientColor.ToVector4()); //TileBase.Height));

            gd.SetRenderTargets(MapRender, MapDepth, MapLight, TextureFogWater);

            //Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["RealTime"];
            //Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();

            //SamplerState sampler = this.Zoom > 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;

            //DepthStencilState tileStencil = new DepthStencilState();
            //tileStencil.DepthBufferFunction = CompareFunction.LessEqual;

            var a = EngineArgs.Default;

            //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, sampler, tileStencil, RasterizerState.CullCounterClockwise, Game1.Instance.Effect); //CullCounterClockwise
            //gd.Textures[2] = Map.ShaderMouseMap;// Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap - Cube"); //Halfblock");//

            //Game1.Instance.Effect.Parameters["SpritesheetWidth"].SetValue(Block.Atlas.Texture.Width);
            //Game1.Instance.Effect.Parameters["SpritesheetHeight"].SetValue(Block.Atlas.Texture.Height);
            //Game1.Instance.Effect.Parameters["TileWidth"].SetValue(Block.Width + 2 * Graphics.Borders.Thickness);
            //Game1.Instance.Effect.Parameters["TileHeight"].SetValue(Block.Height + 2 * Graphics.Borders.Thickness);

            //Game1.Instance.Effect.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);

            //gd.SamplerStates[2] = SamplerState.PointClamp;

            //b += 1;// GlobalVars.DeltaTime;
            //if (b >= Engine.TargetFps)
            //{
            //    b = 0;
            //    Engine.Average = Engine.TileDrawTime.Elapsed;
            //    Engine.TileDrawTime.Restart();
            //}
            //else
            //    Engine.TileDrawTime.Start();


            //Engine.TileDrawTime.Stop();
            //sb.End();

            //Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["TechniqueObjects"];
            //Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();

            //DepthStencilState stencil = new DepthStencilState();
            //stencil.StencilEnable = true;
            //stencil.StencilFunction = CompareFunction.Always;
            //stencil.StencilPass = StencilOperation.Replace;
            //stencil.ReferenceStencil = 1;
            //stencil.DepthBufferEnable = true;
            //stencil.DepthBufferFunction = CompareFunction.Less;

            //Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

            //sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, stencil, RasterizerState.CullNone, Game1.Instance.Effect);

            //Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            //sb.End();

            //Vector4 shaderRect = new Vector4(0, 0, 1, 1);
            //Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(shaderRect);

            //stencil = new DepthStencilState();
            //stencil.StencilEnable = true;
            //stencil.StencilFunction = CompareFunction.NotEqual;
            //stencil.ReferenceStencil = 1;
            //stencil.DepthBufferEnable = true;

            gd.SetRenderTargets(null);
            //gd.Clear(Color.DarkSlateBlue);
            //gd.Clear(Color.Black);
            gd.Clear(Color.Transparent);
            //sb.Begin();
            //sb.Draw(RenderTargets[RenderIndex], new Vector2(0, 0), Color.White);
            //sb.End();

            gd.RasterizerState = RasterizerState.CullNone;
            NewDraw(map, gd, a, scene, toolManager, ui);

            //sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, sampler, DepthStencilState.DepthRead, RasterizerState.CullNone);
            //GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
            //sb.End();
        }
        float DepthFar, DepthNear;
        public MySpriteBatch SpriteBatch;// = new MySpriteBatch(gd);
        public MySpriteBatch WaterSpriteBatch, ParticlesSpriteBatch, BlockParticlesSpriteBatch,
            TransparentBlocksSpriteBatch;
        float FogT = 0;
        public Effect Effect;
        static public bool DrawnOnce = false;
        private void NewDraw(IMap map, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager, UIManager ui)
        {
            DrawnOnce = true;
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            this.Effect = fx;
            var xx = this.CameraOffset.X / this.Width; //*this.Zoom ;
            var yy = this.CameraOffset.Y / this.Height; //*this.Zoom ;

            var world = Matrix.Identity;
            var view =// Matrix.Identity;
                new Matrix(
                   1.0f, 0.0f, 0.0f, 0.0f,
                   0.0f, -1.0f, 0.0f, 0.0f,
                   0.0f, 0.0f, -1.0f, 0.0f,
                   0.0f, 0.0f, 0.0f, 1.0f);
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, 0, 1);//0, 1);

            //fx.Parameters["World"].SetValue(Matrix.CreateTranslation(-1f, 1f, 0));
            fx.Parameters["World"].SetValue(Matrix.Identity);

            //fx.Parameters["World"].SetValue(world);
            //fx.Parameters["View"].SetValue(view);
            //fx.Parameters["Projection"].SetValue(projection);

            //fx.Parameters["World"].SetValue(Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(xx, yy, 0));// + 2 * Graphics.Borders.Thickness);
            //fx.Parameters["World"].SetValue(Matrix.CreateTranslation(.1f, .1f, .1f));// + 2 * Graphics.Borders.Thickness);
            //fx.Parameters["World"].SetValue(Matrix.Identity);// + 2 * Graphics.Borders.Thickness);

            fx.Parameters["BlockWidth"].SetValue(Block.Width);// + 2 * Graphics.Borders.Thickness);
            fx.Parameters["BlockHeight"].SetValue(Block.Height);// + 2 * Graphics.Borders.Thickness);
            fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
            fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
            fx.Parameters["Viewport"].SetValue(new Vector2(gd.Viewport.Width, gd.Viewport.Height));
            fx.Parameters["ViewportW"].SetValue(new Vector2(1, gd.Viewport.Width / (float)gd.Viewport.Height));
            fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
            //fx.Parameters["DepthResolution"].SetValue(2f / (Chunk.Size * 2));
            fx.Parameters["Zoom"].SetValue(this.Zoom);
            float borderPx = 1;
            fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / gd.Viewport.Width, borderPx / gd.Viewport.Height) * this.Zoom);
            //fx.Parameters["OutlineThreshold"].SetValue(0.02f);
            fx.Parameters["CullDark"].SetValue(Engine.CullDarkFaces);
            //Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), 0);//(float)map.DayTimeNormal);
            var nightAmount = (float)map.GetDayTimeNormal();
            Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), nightAmount);// (float)map.DayTimeNormal);
            ambientColor = map.GetAmbientColor();

            Vector4 ambient = ambientColor.ToVector4();// (float)map.DayTimeNormal).ToVector4(); //(float)map.DayTimeNormal //1f).ToVector4(); //
            fx.Parameters["AmbientLight"].SetValue(ambient);// new Vector4(1,1,1,0.5f)); 
            //var fogColor = Color.DarkSlateBlue.ToVector4();
            var fogColor = this.FogColor; //Color.SteelBlue.ToVector4();// this.FogColor;
            // choose between ambient or black background color
            //fogColor = Color.Lerp(new Color(fogColor), new Color(ambient), nightAmount).ToVector4();
            fogColor = Color.Lerp(new Color(fogColor), Color.Black, nightAmount).ToVector4();
            //fogColor = ambientColor.ToVector4();
            fx.Parameters["FogColor"].SetValue(fogColor);
            fx.Parameters["FogDistance"].SetValue(FogFadeLength);// 16); //8

            //fx.Parameters["FogOffset"].SetValue(this.FogT / 100f - this.Coordinates.X / (1000f));
            var fogoffset = new Vector2(this.FogT / 100f, 0);
            fx.Parameters["FogOffset"].SetValue(fogoffset - this.Coordinates / 1000f);
            if (toolManager.ActiveTool.Target != null && toolManager.ActiveTool.Target.Type != TargetType.Null)
            {
                this.LastZTarget = toolManager.ActiveTool.Target.Global.Z;
                fx.Parameters["FogZ"].SetValue(toolManager.ActiveTool.Target.Global.Z - FogZOffset);// 8);// - 16);
            }
            fx.Parameters["FogEnabled"].SetValue(Fog);

            //fx.Parameters["PlayerOcclusion"].SetValue(Player.Actor != null);
            //fx.Parameters["PlayerGlobal"].SetValue(Player.Actor != null ? Player.Actor.Global : Vector3.Zero);
            //if (Player.Actor != null)
            //{
            //    //fx.Parameters["PlayerRotX"].SetValue(Player.Actor.Global.X * this.RotCos - Player.Actor.Global.Y * this.RotSin);
            //    //fx.Parameters["PlayerRotY"].SetValue(Player.Actor.Global.X * this.RotSin + Player.Actor.Global.Y * this.RotCos);
            //    fx.Parameters["PlayerRotXY"].SetValue((float)(
            //        Player.Actor.Global.X * this.RotCos - Player.Actor.Global.Y * this.RotSin +
            //        Player.Actor.Global.X * this.RotSin + Player.Actor.Global.Y * this.RotCos));
            //    fx.Parameters["PlayerCenterOffset"].SetValue(this.GetScreenPositionFloat(Player.Actor.Global + Player.Actor.Physics.Height * Vector3.UnitZ / 2) / new Vector2(ViewPort.Width, ViewPort.Height) - Vector2.One * .5f);
            //}
            fx.Parameters["PlayerOcclusion"].SetValue(this.Following != null);
            fx.Parameters["PlayerGlobal"].SetValue(this.Following != null ? this.Following.Global : Vector3.Zero);
            if (this.Following != null)
            {
                //fx.Parameters["PlayerRotX"].SetValue(Player.Actor.Global.X * this.RotCos - Player.Actor.Global.Y * this.RotSin);
                //fx.Parameters["PlayerRotY"].SetValue(Player.Actor.Global.X * this.RotSin + Player.Actor.Global.Y * this.RotCos);
                fx.Parameters["PlayerRotXY"].SetValue((float)(
                    this.Following.Global.X * this.RotCos - this.Following.Global.Y * this.RotSin +
                    this.Following.Global.X * this.RotSin + this.Following.Global.Y * this.RotCos));
                fx.Parameters["PlayerCenterOffset"].SetValue(this.GetScreenPositionFloat(this.Following.Global + this.Following.Physics.Height * Vector3.UnitZ / 2) / new Vector2(ViewPort.Width, ViewPort.Height) - Vector2.One * .5f);
            }

            fx.Parameters["FogLevel"].SetValue(FogLevel);
            this.MaxDrawZ = GetMaxDrawLevel();
            this.Effect.Parameters["FogEnabled"].SetValue(Fog);
            //this.Effect.Parameters["PlayerZ"].SetValue(playerGlobal.HasValue ? playerGlobal.Value.Z + 2 : 0);
            this.Effect.Parameters["MaxDrawLevel"].SetValue(this.MaxDrawZ);
            this.Effect.Parameters["HideWalls"].SetValue(Engine.HideWalls);
            this.Effect.Parameters["OcclusionRadius"].SetValue(.01f * this.Zoom * this.Zoom);


            //this.FogT = (this.FogT + 0.05f) % 100; //update this on update method that is fixed step
            //gd.BlendState = BlendState.Additive;
            gd.DepthStencilState = DepthStencilState.Default;
            //gd.SamplerStates[0] = SamplerState.LinearWrap;
            //gd.SamplerStates[0] = this.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
            //gd.SamplerStates[1] = this.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp; //SamplerState.PointWrap;
            //gd.SamplerStates[2] = this.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp; //SamplerState.PointClamp;
            //gd.SamplerStates[3] = this.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;

            gd.SamplerStates[0] = SamplerState.PointClamp;// PointClamp;
            gd.SamplerStates[1] = SamplerState.PointClamp;
            gd.SamplerStates[2] = SamplerState.PointClamp;
            gd.SamplerStates[3] = SamplerState.PointClamp;

           

            //MySpriteBatch mySB = new MySpriteBatch(gd);
            if (this.SpriteBatch == null)
                SpriteBatch = new MySpriteBatch(gd);
            if (this.WaterSpriteBatch == null)
                this.WaterSpriteBatch = new MySpriteBatch(gd);
            if (this.ParticlesSpriteBatch == null)
                this.ParticlesSpriteBatch = new MySpriteBatch(gd);
            if (this.TransparentBlocksSpriteBatch == null)
                this.TransparentBlocksSpriteBatch = new MySpriteBatch(gd);
            //BlockParticlesSpriteBatch = ParticlesSpriteBatch; //why?
            if (this.BlockParticlesSpriteBatch == null)
                this.BlockParticlesSpriteBatch = new MySpriteBatch(gd);


            var clearcol = new Color(1f, 1f, 1f, 0); // if i put 1 for the alpha than tsansparent blocks will be shaded white  // (old comment) i put 1 again because i dont draw water on the fog texture after all
            //var clearcol = new Color(1f, 1f, 1f, 1f); // causes unhandled white background

            //gd.SetRenderTarget(this.WaterRender);
            //gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1); //Color.White  Color.Transparent, 1, 1);//

            gd.SetRenderTargets(this.TextureFogWater);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 1);
            // PROBLEM clearing the texturefogwater with the same parameters as the other rendertargets, causes the problem with the background being drawn over the toolmanager preview blocks
            gd.SetRenderTargets(MapRender, MapLight, MapDepth);//, this.TextureFogWater); //gd.Clear(Color.DarkSlateBlue);
            //var clearcol = new Color(1f, 1f, 0, 0); // 3rd component is 0 in order to not draw water on background
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1); //Color.White  Color.Transparent, 1, 1);//

            // after clearing each target with the appropriate parameters for each one, set them all together
            gd.SetRenderTargets(MapRender, MapLight, MapDepth, this.TextureFogWater);

            //gd.Clear(ClearOptions.DepthBuffer, new Color(1f, 1f, 1f, 0), 1, 1); //Color.White  Color.Transparent, 1, 1);//
            //gd.Clear(Color.Lime);
            // use new technique to draw both color and light in one pass in multiple rendertargets
            fx.CurrentTechnique = fx.Techniques["Combined"];
            //fx.CurrentTechnique = fx.Techniques["Default"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();


            gd.Textures[0] = Block.Atlas.Texture;
            //gd.Textures[2] = Map.ShaderMouseMap;
            //gd.Textures[3] = Map.BlockDepthMap;
            gd.Textures[2] = Block.Atlas.NormalTexture;
            gd.Textures[3] = Block.Atlas.DepthTexture;

            //gd.Textures[3] = Block.Atlas.DepthTexture;// Map.BlockDepthMap;
            //gd.Clear(Color.Transparent);
            //gd.Clear(ClearOptions.DepthBuffer, Color.White, 1, 0);

            //gd.DepthStencilState = new DepthStencilState() { DepthBufferFunction = CompareFunction.LessEqual };
            //far = near = 0;
            DepthNear = float.MinValue;
            DepthFar = float.MaxValue;
            //gd.SetRenderTarget(null);
            //gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 1); //Color.White  Color.Transparent, 1, 1);//
            //map.DrawBlocks(SpriteBatch, this, a);
            //EffectParameter effectMaxDrawZ = this.Effect.Parameters["MaxDrawLevel"];
            this.Effect.Parameters["RotCos"].SetValue((float)this.RotCos);
            this.Effect.Parameters["RotSin"].SetValue((float)this.RotSin);


            if (PlayerOld.Actor != null)
            {
                if (PlayerOld.Actor.IsSpawned)
                {
                    Sprite sprite = PlayerOld.Actor.GetSprite();// (Sprite)Player.Actor["Sprite"]["Sprite"];
                    Rectangle spriteBounds = sprite.GetBounds(); // make bounds a field
                    Rectangle screenBounds = this.GetScreenBounds(PlayerOld.Actor.Global, spriteBounds);//, spriteBounds.Center.ToVector());
                    //var xxx = screenBounds.X / (float)this.Width-.5f;
                    //var yyy = screenBounds.Y / (float)this.Height -.5f;
                    var xxx = screenBounds.X / (float)this.Width - .5f;
                    var yyy = screenBounds.Y / (float)this.Height - .5f;
                    var www = (screenBounds.X + screenBounds.Width) / (float)this.Width - .5f;
                    var hhh = (screenBounds.Y + screenBounds.Height) / (float)this.Height - .5f;
                    xxx = -.1f * this.Zoom;
                    yyy = -.15f * this.Zoom;
                    www = .1f * this.Zoom;
                    hhh = .15f * this.Zoom;
                    var box = new Vector4(xxx, yyy, www, hhh);

                    //var box = new Vector4(screenBounds.X / (float)this.Width, screenBounds.Y / (float)this.Height, (screenBounds.X + screenBounds.Width) / (float)this.Width, (screenBounds.Y + screenBounds.Height) / (float)this.Height);
                    this.Effect.Parameters["PlayerBoundingBox"].SetValue(box);
                    this.Effect.Parameters["PlayerDepth"].SetValue(PlayerOld.Actor.Global.GetDrawDepth(map, this));

                }
            }

            this.PrepareShader(map);
            Towns.Housing.House house = PlayerOld.Actor != null ? map.Town.GetHouseAt(PlayerOld.Actor.Global) : null;

            var visibleChunks = (from ch in map.GetActiveChunks().Values where this.ViewPort.Intersects(ch.GetScreenBounds(this)) select ch);//.OrderByDescending(c=>c.MapCoords.X + c.MapCoords.Y).ToList();
            //foreach (var chunk in visibleChunks)
            //    chunk.HitTestEntities(this);

            if (house != null)
            {
                house.Draw(this, this.Effect);
            }
            else
            {
                //var ws = Stopwatch.StartNew();

                foreach (var chunk in visibleChunks)//map.GetActiveChunks())
                {
                    //if (TopSliceChanged)
                    //{
                    //    chunk.BuildSlice(this, map, this.MaxDrawZ);
                    //}

                    // TODO: DONT BUILD TOP SLICE TWICE!
                    ///if (chunk.Value.VertexBuffer != null)

                    if (!chunk.Valid)
                        chunk.Build(this);



                    //{
                    chunk.DrawOpaqueLayers(this, this.Effect); // TODO: is it faster to pass only the effectparameters?
                    //vertices += chunk.Canvas.Opaque.vertices.Length;
                    continue;

                    //effectMaxDrawZ.SetValue(this.MaxDrawZ);
                    //DrawChunkMesh(map, chunk, chunk.VertexBuffer);
                    //effectMaxDrawZ.SetValue(this.MaxDrawZ + 1);
                    //DrawChunkMesh(map, chunk, chunk.TopSliceMesh);
                }
                //ws.Stop();
                //Console.WriteLine("chunk meshes built in {0}", ws.Elapsed);
            }
            //chunk meshes built in 00:00:01.0184017
            this.TopSliceChanged = false;
            //return;
            DepthFar--;
            DepthNear++;

            // TODO: these temporarily only work with static maps
            DepthNear = this.GetNearDepth(map);
            DepthFar = this.GetFarDepth(map);

            
            fx.Parameters["FarDepth"].SetValue(DepthFar);
            fx.Parameters["NearDepth"].SetValue(DepthNear);

            fx.Parameters["DepthResolution"].SetValue((2) / (DepthNear - DepthFar));
            fx.Parameters["OutlineThreshold"].SetValue((1) / (DepthNear - DepthFar));

            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();

            var objs = map.GetObjects();

            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // AUTO EFTAIGE POU DEN LEITOURGOUSE TO DEPTH STO BLOCK HIGHLIGHT
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            toolManager.DrawBeforeWorld(SpriteBatch, map, this);

            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            map.DrawBeforeWorld(SpriteBatch, this);
            ui.DrawWorld(SpriteBatch, this);
            foreach (var entity in objs)
                entity.DrawAfter(SpriteBatch, this); // cull non visible entities
            SpriteBatch.Flush();

            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[2] = Block.Atlas.NormalTexture;
            gd.Textures[3] = Block.Atlas.DepthTexture;
            fx.CurrentTechnique = fx.Techniques["CombinedWater"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            gd.SetRenderTargets(WaterRender, WaterLight, WaterDepth, WaterFog);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);


            PrepareShaderTransparent(map);

            foreach (var chunk in visibleChunks)
            {
                if (!chunk.Valid)
                    continue;
                chunk.DrawTransparentLayers(this, this.Effect);
            }
         
            // combine scenes and apply ambient light
            //gd.SetRenderTarget(null);
            gd.SetRenderTarget(this.MapComposite);
            gd.Clear(new Color(fogColor));//.Transparent);


            //gd.Clear(new Color(fogColor));
            gd.Textures[0] = this.MapRender;
            gd.Textures[1] = this.MapLight;
            gd.Textures[2] = this.MapDepth;
            gd.Textures[3] = this.TextureFogWater;
            var watertxt = Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");
            gd.Textures[4] = watertxt;
            gd.SamplerStates[4] = SamplerState.PointWrap;
            fx.Parameters["WaterTextureSize"].SetValue(new Vector2(watertxt.Width, watertxt.Height));

            var offset2 = new Vector2(0, .5f + this.FogT / 100f);

            var wateroffset = (this.Coordinates / (watertxt.Width)).Floor() * (watertxt.Width);
            wateroffset = (this.Coordinates - wateroffset) / (watertxt.Width);
            fx.Parameters["WaterOffset"].SetValue(fogoffset - wateroffset);

            var wateroffset2 = (this.Coordinates / (watertxt.Height)).Floor() * (watertxt.Height);
            wateroffset = (this.Coordinates - wateroffset) / (watertxt.Height);
            fx.Parameters["WaterOffset2"].SetValue(offset2 - wateroffset);

            SpriteBatch.Draw(MapRender, MapRender.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["FinalInsideBorders"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();



            gd.SetRenderTarget(this.WaterComposite);
            gd.Clear(Color.Transparent);
            gd.Textures[0] = this.WaterRender;
            gd.Textures[1] = this.WaterLight;
            gd.Textures[2] = this.WaterDepth;
            //gd.Textures[3] = this.TextureFogWater;// WaterFog; //it's pink/purple before in the shader i write both red values for the fog and blue values for the water
            gd.Textures[3] = this.WaterFog; //it's pink/purple before in the shader i write both red values for the fog and blue values for the water
            //var watertxt = Game1.Instance.Content.Load<Texture2D>("Graphics/watersmallpixely");
            gd.Textures[4] = watertxt;
            gd.SamplerStates[4] = SamplerState.PointWrap;
            fx.Parameters["WaterTextureSize"].SetValue(new Vector2(watertxt.Width, watertxt.Height));
            //var wateroffset = (this.Coordinates / (watertxt.Width)).Floor() * (watertxt.Width);
            //wateroffset = (this.Coordinates - wateroffset) / (watertxt.Width);
            //fx.Parameters["WaterOffset"].SetValue(fogoffset - wateroffset);
            SpriteBatch.Draw(WaterRender, WaterRender.Bounds, gd.Viewport.Bounds, Color.White);
            // TODO: Must draw entities before final composition, so fog is applied over them accordingly
            fx.CurrentTechnique = fx.Techniques["CompositeWater"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();



            // edw eixa ta entities kai shadows
            //sort objects back to front for fucking proper semitraspanrent rendering
            // TODO: culling
            SortEntities(map, objs);
            IEnterior enterior = null;
            if (PlayerOld.Actor != null)
                enterior = GameMode.Current.GetEnterior(map, PlayerOld.Actor.Global);
            // TODO: have the particle manager set textures because different emitters might use different atlases (blocks vs entities)
            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            DrawEntities(scene, objs, enterior);
            map.DrawParticles(this.SpriteBatch, this);

            //  // draw entity shadows
            MySpriteBatch shadowsSB = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntityShadows"];
            //  gd.BlendState = BlendState.NonPremultiplied;
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteComponent.DrawShadows(shadowsSB, map, this);
            gd.SetRenderTarget(this.MapComposite); // to evala auto edw epeidh to allaksa prin
            shadowsSB.Flush();

            //if (Controller.Instance.Mouseover == null)
            //{

            //}

            // flush entity spritebatch after shadows so they get drawn above them
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //gd.SetRenderTargets(this.MapComposite, this.TextureFogWater);
            gd.SetRenderTargets(this.MapComposite, this.TextureFogWater, this.MapDepth);
            SpriteBatch.Flush();


            //  draw particles drawn by entities
            fx.CurrentTechnique = fx.Techniques["Particles"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            gd.Textures[0] = Block.Atlas.Texture;// 
            BlockParticlesSpriteBatch.Flush();
            gd.Textures[0] = Sprite.Atlas.Texture;// 
            ParticlesSpriteBatch.Flush();

            // draw block mouseover highlight, here or after fog?
            // set textures here or in tool draw method?
            // DRAW here things such as entity previews for debug spawning
            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            // gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // AUTO EFTAIGE POU DEN LEITOURGOUSE TO DEPTH STO BLOCK HIGHLIGHT GAMW TH PANAGIA
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            toolManager.DrawAfterWorld(SpriteBatch, map);
            SpriteBatch.Flush();




            // draw entity mouseover highlight
            fx.CurrentTechnique = fx.Techniques["EntityMouseover"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            //GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
            if (toolManager.ActiveTool != null)
                if (toolManager.ActiveTool.Target != null)
                {
                    GameObject mouseover = toolManager.ActiveTool.Target.Object as GameObject;
                    if (mouseover != null)
                        if (mouseover.IsSpawned)
                            mouseover.DrawMouseover(SpriteBatch, this);
                }


            SpriteBatch.Flush();

            // draw non-water on pre-final texture
            gd.SetRenderTargets(this.RenderBeforeFog, this.FogBeforeFog);//this.DepthBeforeFog, this.LightBeforeFog, this.FogBeforeFog);
            gd.Clear(new Color(fogColor));
            gd.Clear(ClearOptions.DepthBuffer, Color.White, 1, 1);
            fx.CurrentTechnique = fx.Techniques["RenderMapWithoutFog"];
            var fogtxt = Game1.Instance.Content.Load<Texture2D>("Graphics/Fog04");
            fx.Parameters["FogTextureSize"].SetValue(new Vector2(fogtxt.Width, fogtxt.Height));
            gd.SamplerStates[2] = SamplerState.PointWrap;
            gd.Textures[0] = this.MapComposite;
            gd.Textures[1] = this.TextureFogWater;
            gd.Textures[2] = fogtxt;
            gd.Textures[3] = this.MapDepth;
            SpriteBatch.Draw(this.MapComposite, this.MapComposite.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();



            // draw water on pre-final texture
            gd.Textures[0] = this.WaterComposite;
            gd.Textures[1] = this.WaterFog;
            gd.Textures[2] = fogtxt;
            gd.Textures[3] = this.WaterDepth;
            SpriteBatch.Draw(this.WaterComposite, this.WaterComposite.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["Water"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();


            // draw block highlight now so it's correctly placed over water according to depth
            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            // which textures to use???
            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[1] = Block.Atlas.DepthTexture;


            // apply fog to the pre-final texture render(that contains map + water)
            gd.SetRenderTargets(this.FinalScene);
            gd.Clear(new Color(fogColor));

            gd.Textures[0] = this.RenderBeforeFog;
            gd.Textures[1] = this.FogBeforeFog;
            gd.Textures[2] = fogtxt;
            gd.Textures[3] = this.MapDepth; // i added this here because i'm using s3 in the shader to read depth and this index was set to waterdepth from the previous draw operation

            SpriteBatch.Draw(this.RenderBeforeFog, this.RenderBeforeFog.Bounds, gd.Viewport.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["ApplyFog"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();


            ///test
            ///i moved this here from ingame.cs's draw method
            var sb = new SpriteBatch(gd);
            gd.SetRenderTarget(this.FinalScene);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            map.DrawInterface(sb, this);
            sb.End();
            ///
           
            // draw final scene to backbuffer
            RenderTarget2D[] targets = new RenderTarget2D[] { this.FinalScene, this.RenderBeforeFog, FogBeforeFog,
                this.WaterRender, this.WaterDepth, this.WaterLight, this.WaterFog,
                this.WaterComposite,
                this.MapRender, 
                
                this.MapDepth, this.MapLight, this.TextureFogWater };
            this.RenderTargets = targets.ToArray();
            gd.SetRenderTarget(null);
            //gd.Textures[0] = this.FinalScene;
            gd.Textures[0] = this.RenderTargets[this.RenderIndex];
            fx.CurrentTechnique = fx.Techniques["Normal"];
         
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Draw(this.FinalScene, this.FinalScene.Bounds, gd.Viewport.Bounds, Color.White);

            SpriteBatch.Flush();



            // draw ui and other elements
            //ui.DrawWorld(SpriteBatch, this);
            map.DrawWorld(SpriteBatch, this);

            SpriteBatch.Flush();



            //fx.CurrentTechnique = fx.Techniques["Normal"];
            //fx.CurrentTechnique.Passes.First().Apply();
            //GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
            //if (!mouseover.IsNull())
            //    mouseover.DrawMouseover(mySB, this);
            //mySB.Flush();
        }

        private void DrawEntities(SceneState scene, List<GameObject> objs, IEnterior enterior)
        {
            foreach (var obj in objs)
            {
                if (obj.Global.Z > this.MaxDrawZ + 1)// this.GetMaxDrawLevel() + 1) // i'm cacheing this at the start of the drawchunk method
                    continue;
                if (enterior != null)
                    if (!enterior.Contains(obj.Global))
                        continue;
                //if (this.HideTerrainAbovePlayer && Player.Actor != null)
                //{
                //    var h = Player.Actor.Transform.Global.Z + Player.Actor.GetPhysics().Height;
                //    if (obj.Transform.Global.Z > h)
                //        continue;
                //}

                // TODO: check bounding box intersection instead of single point to avoid entity pop-in
                //var pos = this.GetScreenPosition(obj.Global);
                //var bounds = new Rectangle((int)pos.X, (int)pos.Y, 1, 1);
                var bounds = obj.GetScreenBounds(this); // TODO: cache bounds?
                if (!this.ViewPort.Intersects(bounds))
                    continue;
                obj.Draw(SpriteBatch, this);
                scene.ObjectsDrawn.Add(obj);
            }
        }

        private void SortEntities(IMap map, List<GameObject> objs)
        {
            objs.Sort((o1, o2) =>
            {
                float d1 = o1.Global.GetDrawDepth(map, this);
                float d2 = o2.Global.GetDrawDepth(map, this);
                if (d1 < d2) return -1;
                else if (d1 == d2) return 0;
                else return 1;
            });
        }
        public void NewDraw(RenderTarget2D target, IMap map, GraphicsDevice gd, EngineArgs a, SceneState scene, ToolManager toolManager)
        {
            if (MapRender == null)
                MapRender = new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);//,DiscardContents);//PreserveContents);//, false, SurfaceFormat.Vector4, DepthFormat.Depth16);
            if (MapDepth == null)
                MapDepth = new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);//PreserveContents); //SurfaceFormat.Rg32 //Depth16
            if (MapLight == null)
                MapLight = new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);//PreserveContents);

            Effect fx = Game1.Instance.Content.Load<Effect>("blur");

            fx.Parameters["BlockWidth"].SetValue(Block.Width + 2 * Graphics.Borders.Thickness);
            fx.Parameters["BlockHeight"].SetValue(Block.Height + 2 * Graphics.Borders.Thickness);
            fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
            fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
            fx.Parameters["Viewport"].SetValue(new Vector2(target.Width, target.Height));
            fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
            //fx.Parameters["DepthResolution"].SetValue(2f / (Chunk.Size * 2));
            fx.Parameters["Zoom"].SetValue(this.Zoom);
            float borderPx = 1;
            fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / target.Width, borderPx / target.Height) * this.Zoom);
            //fx.Parameters["OutlineThreshold"].SetValue(5f);//0.02f);
            fx.Parameters["CullDark"].SetValue(Engine.CullDarkFaces);
            Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), 0);//(float)map.DayTimeNormal);
            Vector4 ambient = ambientColor.ToVector4();// (float)map.DayTimeNormal).ToVector4(); //(float)map.DayTimeNormal //1f).ToVector4(); //
            fx.Parameters["AmbientLight"].SetValue(ambient);// new Vector4(1,1,1,0.5f));    

            gd.DepthStencilState = DepthStencilState.Default;
            //gd.SamplerStates[0] = this.Zoom > 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
            //gd.SamplerStates[1] = SamplerState.PointWrap;
            //gd.SamplerStates[2] = SamplerState.PointClamp;
            //gd.SamplerStates[3] = this.Zoom > 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;

            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.SamplerStates[1] = SamplerState.PointClamp;
            gd.SamplerStates[2] = SamplerState.PointClamp;
            gd.SamplerStates[3] = SamplerState.PointClamp;

            MySpriteBatch mySB = new MySpriteBatch(gd);


            //gd.Clear(ClearOptions.DepthBuffer, new Color(1f, 1f, 1f, 0), 1, 1); //Color.White  Color.Transparent, 1, 1);//

            // use new technique to draw both color and light in one pass in multiple rendertargets
            DrawBlocks(map, gd, a, fx, mySB);

            // combine scenes
            gd.SetRenderTarget(target);
            DrawScene(target, gd, fx, mySB);

            // draw objects
            DrawEntities(map, gd, scene, fx, mySB);

            // draw entity shadows
            DrawEntityShadows(map, gd, fx, mySB);

            // draw block selection, using shadow shader for projected textures
            DrawBlockSelection(map, toolManager, fx, mySB);

            DrawMouseoverEntity(fx, mySB);
        }

        private void DrawBlocks(IMap map, GraphicsDevice gd, EngineArgs a, Effect fx, MySpriteBatch mySB)
        {
            gd.SetRenderTargets(MapRender, MapLight, MapDepth);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(1f, 1f, 1f, 0), 1, 1); //Color.White  Color.Transparent, 1, 1);//
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[2] = Map.ShaderMouseMap;
            gd.Textures[3] = Map.BlockDepthMap;
            DepthNear = float.MinValue;
            DepthFar = float.MaxValue;
            map.DrawBlocks(mySB, this, a);
            fx.Parameters["FarDepth"].SetValue(DepthFar);
            fx.Parameters["NearDepth"].SetValue(DepthNear);
            fx.Parameters["DepthResolution"].SetValue((2) / (DepthNear - DepthFar));
            fx.Parameters["OutlineThreshold"].SetValue((1) / (DepthNear - DepthFar));
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            mySB.Flush();
        }

        private void DrawScene(RenderTarget2D target, GraphicsDevice gd, Effect fx, MySpriteBatch mySB)
        {
            gd.Clear(Color.Transparent);
            gd.Textures[0] = MapRender;
            gd.Textures[1] = MapLight;
            gd.Textures[2] = MapDepth;
            mySB.Draw(MapRender, MapRender.Bounds, target.Bounds, Color.White);
            fx.CurrentTechnique = fx.Techniques["FinalInsideBorders"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            mySB.Flush();
        }

        private void DrawEntities(IMap map, GraphicsDevice gd, SceneState scene, Effect fx, MySpriteBatch mySB)
        {
            fx.CurrentTechnique = fx.Techniques["Entities"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            gd.Textures[0] = Sprite.Atlas.Texture;
            //gd.Textures[1] = Sprite.DepthAtlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            map.DrawObjects(mySB, this, scene);
            mySB.Flush();
        }

        private void DrawMouseoverEntity(Effect fx, MySpriteBatch mySB)
        {
            GameObject mouseover = Controller.Instance.MouseoverBlock.Object as GameObject;
            fx.CurrentTechnique = fx.Techniques["Default"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            if (!mouseover.IsNull())
                mouseover.DrawMouseover(mySB, this);
            mySB.Flush();
        }

        private void DrawBlockSelection(IMap map, ToolManager toolManager, Effect fx, MySpriteBatch mySB)
        {
            fx.CurrentTechnique = fx.Techniques["BlockHighlight"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            toolManager.DrawBeforeWorld(mySB, map, this);
            mySB.Flush();
        }

        private void DrawEntityShadows(IMap map, GraphicsDevice gd, Effect fx, MySpriteBatch mySB)
        {
            fx.CurrentTechnique = fx.Techniques["EntityShadows"];
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteComponent.DrawShadows(mySB, map, this);
            mySB.Flush();
        }

        //public void Draw(SpriteBatch sb, World world)
        //{
        //    foreach (KeyValuePair<Vector2, Map> map in world.Maps)//.OrderBy(m=>-m.Value.Global.X - m.Value.Global.Y))
        //    {
        //        map.Value.Thumb.Draw(sb, this);
        //    }
        //}

        bool HitTest(Rectangle bounds, Texture2D tex)
        {
            if (!bounds.Intersects(Controller.Instance.MouseRect))
                return false;

            int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / (float)Zoom);
            int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / (float)Zoom);

            // TODO: fix face detection
            //if (Sprite.MouseMap.HitTest(src, xx, yy, out face, 0, Orientation))
            //{
            // TODO: have the color array generated on creation of sprite and cache it
            Color[] spriteMap = new Color[tex.Width * tex.Height];
            tex.GetData(0, null, spriteMap, 0, tex.Width * tex.Height);
            Color c = spriteMap[yy * tex.Width + xx];
            if (c.A == 0)
                return false;
            return true;
        }

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e) { }
        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            //Controller.Input.UpdateKeyStates();

            //List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            //if (pressed.Contains(GlobalVars.KeyBindings.RotateMapLeft))
            if (e.KeyValue == (int)GlobalVars.KeyBindings.RotateMapLeft)
            {
                this.Rotation += 1;
                //foreach (var chunk in Engine.Map.ActiveChunks)
                //    chunk.Value.UpdateDrawList(this);
            }
            //if (pressed.Contains(GlobalVars.KeyBindings.RotateMapRight))
            if (e.KeyValue == (int)GlobalVars.KeyBindings.RotateMapRight)
            {
                this.Rotation -= 1;
                //foreach (var chunk in Engine.Map.ActiveChunks)
                //    chunk.Value.UpdateDrawList(this);
            }
            if (e.KeyValue == (int)System.Windows.Forms.Keys.W)
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
                    Engine.HideWalls = !Engine.HideWalls;

            //if (e.KeyValue == (int)System.Windows.Forms.Keys.V)
            //{
            //    BlockTargeting = !BlockTargeting;
            //    Client.Console.Write("Block targeting " + (BlockTargeting ? "on" : "off"));
            //}
        }
        public void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (e.KeyValue == (int)System.Windows.Forms.Keys.F4)
            {
                var max = this.RenderTargets.GetUpperBound(0) + 1;
                this.RenderIndex = (RenderIndex + 1) % max;// 3;
                e.Handled = true;
            }
        }
        //public void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (!Dragging)
        //        return;
        //    this.DragVector = new Vector2(e.X, e.Y) - DragOrigin;
        //}
        //public void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (e.Handled)
        //        return;
        //    this.Dragging = true;
        //    this.DragOrigin = new Vector2(e.X, e.Y);
        //    this.DragVector = Vector2.Zero;

        //}
        //public void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (DragDelay > DragDelayMax)
        //        e.Handled = true;
        //    this.Dragging = false;
        //    this.DragDelay = 0;
        //}
        //public void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        //public void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        //public void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e) { }
        //public void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e) { }
        //public void HandleMouseWheel(HandledMouseEventArgs e)
        //{
        //    return;
        //    if (e.Handled)
        //        return;
        //    e.Handled = true;
        //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
        //    {
        //        this.AdjustDrawLevel(InputState.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) ? e.Delta * 16 : e.Delta);
        //        return;
        //    }
        //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
        //    {
        //        this.Rotation += e.Delta;
        //        return;
        //    }
        //    if (e.Delta < 0)
        //        ZoomDecrease();
        //    else
        //        ZoomIncrease();
        //    //this.ZoomNext = MathHelper.Clamp(this.ZoomNext, ZoomMin, ZoomMax);
        //}

        public void ZoomIncrease()
        {
            this.ZoomNext *= 2;
            this.ZoomNext = MathHelper.Clamp(this.ZoomNext, ZoomMin, ZoomMax);

        }

        public void ZoomDecrease()
        {
            this.ZoomNext /= 2;
            this.ZoomNext = MathHelper.Clamp(this.ZoomNext, ZoomMin, ZoomMax);

        }

        const float InitialZoom = 2;//1
        public void ZoomReset()
        {
            this.ZoomNext = InitialZoom;// 1;
        }
        public void HandleLButtonDblClk(HandledMouseEventArgs e) { }

        static public bool Fog = true;// { get; set; }
        public bool HideUnderground { get; set; }
        public bool BorderShading { get; set; }

        public float GetFarDepth(IMap map)
        {
            //var size = (int)Math.Sqrt(map.GetActiveChunks().Count) * Chunk.Size;// -1;
            var size = map.GetSizeInChunks() * Chunk.Size;// -1;

            switch ((int)this.Rotation)
            {
                case 0:
                    return Vector3.Zero.GetDrawDepth(map, this);

                case 1:
                    return new Vector3(0, size, 0).GetDrawDepth(map, this);

                case 2:
                    return new Vector3(size, size, 0).GetDrawDepth(map, this);

                case 3:
                    return new Vector3(size, 0, 0).GetDrawDepth(map, this);

                default: return 0;
            }
        }
        public float GetNearDepth(IMap map)
        {
            //var size = (int)Math.Sqrt(map.GetActiveChunks().Count) * Chunk.Size;// -1;
            var size = map.GetSizeInChunks() * Chunk.Size;// -1;

            switch ((int)this.Rotation)
            {
                case 0:
                    return new Vector3(size, size, 0).GetDrawDepth(map, this);
                case 1:
                    return new Vector3(size, 0, 0).GetDrawDepth(map, this);
                case 2:
                    return Vector3.Zero.GetDrawDepth(map, this);
                case 3:
                    return new Vector3(0, size, 0).GetDrawDepth(map, this);
                default: return 0;
            }
        }

        public void UpdateMaxDrawLevel()
        {
            this.MaxDrawZ = this.GetMaxDrawLevel();
        }
        public int GetMaxDrawLevel()
        {
            var value = (this.HideTerrainAbovePlayer && (PlayerOld.Actor != null)) ? (int)PlayerOld.Actor.Transform.Global.RoundXY().Z + 2 + this.HideTerrainAbovePlayerOffset : this.DrawLevel;
            value = Math.Min(Map.MaxHeight - 1, Math.Max(0, value));
            return value;
        }
        public bool TopSliceChanged = true;
        internal void ToggleHideBlocksAbove()
        {
            this.HideTerrainAbovePlayer = !this.HideTerrainAbovePlayer;
            if (this.HideTerrainAbovePlayer)
                this.HideTerrainAbovePlayerOffset = 0;
        }

        internal void AdjustDrawLevel(int p)
        {
            if (!this.HideTerrainAbovePlayer)
                this.DrawLevel = Math.Min(Map.MaxHeight - 1, Math.Max(0, this.DrawLevel + p));
            else
                this.HideTerrainAbovePlayerOffset += p;

            //this.TopSliceChanged = true;
        }

        public void OnDeviceLost()
        {
            int w = this.Width, h = this.Height;
            var gfx = Game1.Instance.GraphicsDevice;

            this.MapRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//,DiscardContents);//PreserveContents);//, false, SurfaceFormat.Vector4, DepthFormat.Depth16);
            this.MapDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents); //SurfaceFormat.Rg32 //Depth16
            this.MapLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);
            this.TextureFogWater = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);
            this.MapComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);

            this.RenderBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);
            this.LightBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);   
            this.DepthBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);   
            this.FogBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);//PreserveContents);

            this.FinalScene = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);//PreserveContents);


            this.WaterRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        }

        public void BuildChunk(IMap map, Chunk chunk, MySpriteBatch sb, EngineArgs a, int rotation)
        {
            Vector3? playerGlobal = null;// = new Nullable<Vector3>(Player.Actor != null ? Player.Actor.Global : null);
            List<Rectangle> hiddenRects = new List<Rectangle>();
            if (PlayerOld.Actor != null)
            {
                if (PlayerOld.Actor.IsSpawned)
                {
                    playerGlobal = new Nullable<Vector3>(PlayerOld.Actor.Global.RoundXY());
                    Sprite sprite = PlayerOld.Actor.GetSprite();// (Sprite)Player.Actor["Sprite"]["Sprite"];
                    Rectangle spriteBounds = sprite.GetBounds(); // make bounds a field
                    //spriteBounds.Inflate(spriteBounds.Width, spriteBounds.Height);
                    Rectangle screenBounds = this.GetScreenBounds(playerGlobal.Value, spriteBounds);//, spriteBounds.Center.ToVector());
                    hiddenRects.Add(screenBounds);
                    //var playerpos = camera.GetScreenPositionFloat(playerGlobal.Value);
                    //var screenBounds = new Rectangle((int)(playerpos.X - 32 * camera.Zoom), (int)(playerpos.Y - 32 * camera.Zoom), (int)(64 * camera.Zoom), (int)(64 * camera.Zoom));
                    //hiddenRects.Add(screenBounds);
                }
            }
            this.DrawChunk(sb, map, chunk, playerGlobal, hiddenRects, a);
            chunk.BuildFrontmostBlocks(this);
            chunk.Valid = true;
        }
        public void MousePicking(IMap map)
        {
            //return;
            foreach (var chunk in (from ch in map.GetActiveChunks().Values where this.ViewPort.Intersects(ch.GetScreenBounds(this)) select ch))
                chunk.HitTestEntities(this);
            if (Controller.Instance.MouseoverBlockNext.Object != null)
                return;
            if (!BlockTargeting)
                return;
            var controller = Controller.Instance;
            var hidewalls = Engine.HideWalls;
            bool playerExists = PlayerOld.Actor != null;
            Vector3 playerGlobal = playerExists ? PlayerOld.Actor.Global : default(Vector3);
            float radius = .01f * this.Zoom * this.Zoom; //occlusion radius
            bool found = false;
            var foundDepth = float.MinValue;
            Vector3 foundGlobal = Vector3.Zero;
            //Vector3 foundFace = Vector3.UnitZ;
            Vector2 foundMouse = Vector2.Zero;
            Block foundBlock;
            Rectangle foundRect = Rectangle.Empty;
            var camx = this.Coordinates.X - (this.Width / 2f) / this.Zoom;
            var camy = this.Coordinates.Y - (this.Height / 2f) / this.Zoom;
            var mouse = UIManager.Mouse;
            var mousex = (int)mouse.X;// / this.Zoom);
            var mousey = (int)mouse.Y;// / this.Zoom);
            bool behind = InputState.IsKeyDown(System.Windows.Forms.Keys.Menu);
            var visibleChunks = (from ch in map.GetActiveChunks().Values.Reverse() where this.ViewPort.Intersects(ch.GetScreenBounds(this)) select ch).ToList();//.OrderByDescending(c=>c.MapCoords.X + c.MapCoords.Y).ToList();
            
            int rectw = (int)(32 * this.Zoom);
            int recth = (int)(40 * this.Zoom);
            foreach (var chunk in visibleChunks)// map.GetActiveChunks().Reverse())
            {
                //if (!chunk.Valid)
                //    continue;
                var chunkBounds = chunk.GetScreenBounds(this);
                //if (!chunkBounds.Intersects(controller.MouseRect))
                //    continue;
                if (!chunkBounds.Contains(mousex, mousey))
                    continue;
                float chunkx, chunky;
                Coords.Iso(this, chunk.X * Chunk.Size, chunk.Y * Chunk.Size, 0, out chunkx, out chunky);
                chunkx -= camx;
                chunky -= camy;
                //foreach (var array in new MyVertex[][] { chunk.Value.VertexBuffer.vertices, chunk.Value.NonOpaqueBuffer.vertices, chunk.Value.TopSliceMesh.vertices
                //,chunk.Value.TransparentBlocksVertexBuffer.vertices

                //foreach (var array in new MyVertex[][] {
                //    chunk.Canvas.Opaque.vertices,
                //    chunk.Canvas.NonOpaque.vertices,
                //    chunk.CanvasTopSlice.Opaque.vertices,
                //    chunk.Canvas.Transparent.vertices,
                //    chunk.Canvas.Designations.vertices
                //})

                var foglvl = GetFogLevel();
                for (int j = this.MaxDrawZ; j >= foglvl; j--)
                //for (int j = this.MaxDrawZ; j >= 0; j--)
                {
                    var slice = chunk.Slices[j];
                    if (slice == null)
                        continue;
                    if (!slice.Valid)
                        continue;
                    var arrays = new List<MyVertex[]>(3) { 
                        slice.Canvas.Opaque.vertices,
                        slice.Canvas.NonOpaque.vertices,
                        slice.Canvas.Designations.vertices };
                    if (j == this.MaxDrawZ)
                        arrays.Add(slice.Unknown.vertices);
                    foreach (var array in arrays)
                    {
                        var count = array.Length;
                        //for (int i = 0; i < count; i += 4)
                        for (int i = count - 4; i >= 0; i -= 4)
                        {
                            //var v = array[i];
                            //if (v.BlockCoords.Z > this.MaxDrawZ)
                            //    continue;
                            //var tl = v.Position;

                            //var xxx = tl.X + chunkx;
                            //var rectx = (int)(xxx * this.Zoom);
                            //if (mousex < rectx)
                            //    continue;

                            //var yyy = tl.Y + chunky;
                            //var recty = (int)(yyy * this.Zoom);
                            //if (mousey < recty)
                            //    continue;

                            //var br = array[i + 2].Position;

                            //var www = (br.X - tl.X);
                            //var hhh = (br.Y - tl.Y);

                            //var rectww = (int)(www * this.Zoom);
                            //if (mousex >= rectx + rectw)
                            //    continue;
                            //var recthh = (int)(hhh * this.Zoom);
                            //if (mousey >= recty + recth)
                            //    continue;

                            if (!EarlyOutMousePicking(array, i, mousex, mousey, chunkx, chunky, rectw, recth, out int rectx, out int recty, out Vector3 global))
                                continue;

                            // TODO: check intersection in previous stages
                            //if (rectx <= mousex && mousex < rectx + rectw && recty <= mousey && mousey < recty + recth)
                            //{

                            //var global = v.BlockCoords;
                            var block = chunk.GetBlockFromGlobal(global.X, global.Y, global.Z);

                            if (!block.IsTargetable(global))
                                continue;
                            if (hidewalls)
                                if (playerExists)
                                {
                                    if (global.Z >= playerGlobal.Z)
                                        if (global.X + global.Y > playerGlobal.X + playerGlobal.Y)
                                            if (block.Opaque)
                                            {
                                                //distance between mouse and center of screen normalized between -1,1
                                                var dx = mousex - this.Width / 2f;
                                                var dy = mousey - this.Height / 2f;
                                                var d = new Vector2(dx, dy);
                                                d.Y /= this.Width / (float)this.Height;
                                                d /= new Vector2(this.Width / 2f, this.Height / 2f);
                                                var l = d.LengthSquared();
                                                if (l < radius)
                                                    continue;
                                            }
                                }
                            var xx = (int)((mousex - rectx) / Zoom);
                            var yy = (int)((mousey - recty) / Zoom);
                            //Vector3 face;
                            if (!block.MouseMap.HitTestEarly(xx, yy))
                                continue;
                            int rx, ry;
                            Coords.Rotate(this, global.X, global.Y, out rx, out ry);
                            var currentDepth = rx + ry + global.Z;

                            if (currentDepth > foundDepth)
                            {
                                foundDepth = currentDepth;
                                foundGlobal = global;
                                //foundFace = face;
                                foundMouse = mouse;// new Vector2(mousex, mousey);
                                foundRect = new Rectangle((int)rectx, (int)recty, (int)rectw, (int)recth);
                                foundBlock = block;
                                found = true;
                                //CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
                                //return;
                            }
                            //}
                        }
                    }
                }

            }
            if (found)
            {
                // create mouseover anyway even if air in case of undiscovered area? or check drawunknownblocks?
                CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
                //return;

                //if (foundBlock != Block.Air) // if block was removed during this frame
                //    // TODO: if i already have the block why dont i pass it in the createmousover instead of getting it in there too?
                //    CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
            }
        }
        public bool EarlyOutMousePicking(MyVertex[] array, int i, float mousex, float mousey, float chunkx, float chunky, int rectw, int recth, out int rectx, out int recty, out Vector3 global)
        {
            rectx = recty = 0;
            var v = array[i];
            global = v.BlockCoords;
            var tl = v.Position;

            var br = array[i + 2].Position;
            if (br.X - tl.X == 0)
                return false;

            var xxx = tl.X + chunkx;
            rectx = (int)(xxx * this.Zoom);
            if (mousex < rectx)
                return false;

            var yyy = tl.Y + chunky;
            recty = (int)(yyy * this.Zoom);
            if (mousey < recty)
                return false;

            if (mousex >= rectx + rectw)
                return false;
            if (mousey >= recty + recth)
                return false;

            return true;
        }
        //public void MousePickingOld(IMap map)
        //{
        //    if (!BlockTargeting)
        //        return;
        //    var controller = Controller.Instance;
        //    var hidewalls = Engine.HideWalls;
        //    bool playerExists = Player.Actor != null;
        //    Vector3 playerGlobal = playerExists ? Player.Actor.Global : default(Vector3);
        //    float radius = .01f * this.Zoom * this.Zoom; //occlusion radius
        //    bool found = false;
        //    var foundDepth = float.MinValue;
        //    Vector3 foundGlobal = Vector3.Zero;
        //    //Vector3 foundFace = Vector3.UnitZ;
        //    Vector2 foundMouse = Vector2.Zero;
        //    Block foundBlock;
        //    Rectangle foundRect = Rectangle.Empty;
        //    var camx = this.Coordinates.X - (this.Width / 2f) / this.Zoom;
        //    var camy = this.Coordinates.Y - (this.Height / 2f) / this.Zoom;
        //    var mousex = (int)(controller.MouseRect.X);// / this.Zoom);
        //    var mousey = (int)(controller.MouseRect.Y);// / this.Zoom);
        //    bool behind = InputState.IsKeyDown(System.Windows.Forms.Keys.Menu);
        //    foreach (var chunk in map.GetActiveChunks().Reverse())
        //    {
        //        if (!chunk.Value.Valid)
        //            continue;
        //        var chunkBounds = chunk.Value.GetScreenBounds(this);
        //        if (!chunkBounds.Intersects(controller.MouseRect))
        //            continue;
        //        float x, y;
        //        Coords.Iso(this, chunk.Value.X * Chunk.Size, chunk.Value.Y * Chunk.Size, 0, out x, out y);

        //        //foreach (var array in new MyVertex[][] { chunk.Value.VertexBuffer.vertices, chunk.Value.NonOpaqueBuffer.vertices, chunk.Value.TopSliceMesh.vertices
        //        //,chunk.Value.TransparentBlocksVertexBuffer.vertices
        //        foreach (var array in new MyVertex[][] { chunk.Value.Canvas.Opaque.vertices, chunk.Value.Canvas.NonOpaque.vertices, chunk.Value.CanvasTopSlice.Opaque.vertices
        //        ,chunk.Value.Canvas.Transparent.vertices, chunk.Value.Canvas.Designations.vertices
        //        })
        //        {
        //            var count = array.Length;
        //            for (int i = 0; i < count; i += 4)
        //            {
        //                var v = array[i];
        //                if (v.BlockCoords.Z > this.MaxDrawZ)
        //                    continue;
        //                var tl = v.Position;
        //                var tr = array[i + 1].Position;
        //                var br = array[i + 2].Position;
        //                var bl = array[i + 3].Position;

        //                var xxx = (tl.X + x) - camx;
        //                var yyy = (tl.Y + y) - camy;
        //                var www = (tr.X - tl.X);
        //                var hhh = (bl.Y - tl.Y);

        //                var rectx = (int)(((tl.X + x) - camx) * this.Zoom);
        //                var recty = (int)(((tl.Y + y) - camy) * this.Zoom);
        //                var rectw = (int)(www * this.Zoom);
        //                var recth = (int)(hhh * this.Zoom);
        //                // TODO: check intersection in previous stages
        //                if (rectx <= mousex && mousex < rectx + rectw && recty <= mousey && mousey < recty + recth)
        //                {
        //                    int xx = (int)((mousex - rectx) / (float)Zoom);
        //                    int yy = (int)((mousey - recty) / (float)Zoom);
        //                    var global = v.BlockCoords;
        //                    //if (global == Vector3.Zero)
        //                    //    "mh me treleneis 2".ToConsole();
        //                    var block = chunk.Value.GetBlockFromGlobal((int)global.X, (int)global.Y, (int)global.Z);

        //                    if (!block.IsTargetable(global))
        //                        continue;
        //                    if (hidewalls)
        //                        if (playerExists)
        //                        {
        //                            if (global.Z >= playerGlobal.Z)
        //                                if (global.X + global.Y > playerGlobal.X + playerGlobal.Y)
        //                                    if (block.Opaque)
        //                                    {
        //                                        //distance between mouse and center of screen normalized between -1,1
        //                                        var dx = mousex - this.Width / 2f;
        //                                        var dy = mousey - this.Height / 2f;
        //                                        var d = new Vector2(dx, dy);
        //                                        d.Y /= this.Width / (float)this.Height;
        //                                        d /= new Vector2(this.Width / 2f, this.Height / 2f);
        //                                        var l = d.LengthSquared();
        //                                        if (l < radius)
        //                                            continue;
        //                                    }
        //                        }

        //                    //Vector3 face;
        //                    if (!block.MouseMap.HitTestEarly(xx, yy))
        //                        continue;
        //                    int rx, ry;
        //                    Coords.Rotate(this, global.X, global.Y, out rx, out ry);
        //                    var currentDepth = rx + ry + global.Z;

        //                    if (currentDepth > foundDepth)
        //                    {
        //                        foundDepth = currentDepth;
        //                        foundGlobal = global;
        //                        //foundFace = face;
        //                        foundMouse = new Vector2(mousex, mousey);
        //                        foundRect = new Rectangle((int)rectx, (int)recty, (int)rectw, (int)recth);
        //                        foundBlock = block;
        //                        found = true;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (found)
        //    {
        //        // create mouseover anyway even if air in case of undiscovered area? or check drawunknownblocks?
        //        CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
        //        //return;

        //        //if (foundBlock != Block.Air) // if block was removed during this frame
        //        //    // TODO: if i already have the block why dont i pass it in the createmousover instead of getting it in there too?
        //        //    CreateMouseover(map, foundGlobal, foundRect, foundMouse, behind);
        //    }
        //}

        public int GetFogLevel()
        {
            return (int)Math.Max(0, this.LastZTarget - FogZOffset - FogFadeLength);
        }

        public void DrawGrid(MySpriteBatch sb, IMap map, IEnumerable<IntVec3> positions)
        {
            this.DrawGrid(sb, map, positions, Color.Yellow * .5f);
        }
        public void DrawGrid(MySpriteBatch sb, IMap map, IEnumerable<IntVec3> positions, Color col)
        {
            var gridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
            Sprite.Atlas.Begin(sb);

            foreach (var pos in positions)
                this.DrawGridCell(sb, col, pos);
            sb.Flush();
        }
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        public float LastZTarget;
        public void DrawGridCells(MySpriteBatch sb, Color col, IEnumerable<Vector3> globals)
        {
            GridSprite.AtlasToken.Atlas.Begin(sb);
            foreach (var pos in globals)
                this.DrawGridCell(sb, col, pos);
            sb.Flush();
        }
        public void DrawGridCell(MySpriteBatch sb, Color col, Vector3 global)
        {
            if (global.Z > this.DrawLevel + 1)
                return;
            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);

            // is this slow?
            //GridSprite.AtlasToken.Atlas.Begin(sb);

            sb.Draw(GridSprite.AtlasToken.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, this.Zoom, col, SpriteEffects.None, depth);
        }
        public void DrawGridBlock(MySpriteBatch sb, Color col, Vector3 global)
        {
            if (global.Z > this.DrawLevel)
                return;
            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);
            //sb.Draw(Block.Atlas.Texture, pos, Block.BlockBlueprint.Rectangle, 0, Vector2.Zero, this.Zoom, col * .5f, SpriteEffects.None, depth);
            //return;
            sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, this.Zoom, col * .5f, SpriteEffects.None, depth);
        }
        public void DrawGridBlock(MySpriteBatch sb, Graphics.AtlasDepthNormals.Node.Token sprite, Color col, Vector3 global)
        {
            if (global.Z > this.DrawLevel)
                return;
            sprite.Atlas.Begin(sb);
            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);
            sb.Draw(Sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, this.Zoom, col * .5f, SpriteEffects.None, depth);
        }
        public void DrawGridBlockNoFlush(MySpriteBatch sb, Graphics.AtlasDepthNormals.Node.Token sprite, Color col, Vector3 global)
        {
            if (global.Z > this.DrawLevel)
                return;
            var bounds = this.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, this);
            sb.Draw(Sprite.Atlas.Texture, pos, sprite.Rectangle, 0, Vector2.Zero, this.Zoom, col * .5f, SpriteEffects.None, depth);
        }
        public void DrawGridBlocks(MySpriteBatch sb, IEnumerable<Vector3> positions, Color col)
        {
            Sprite.Atlas.Begin(sb);
            foreach (var pos in positions)
                this.DrawGridBlock(sb, col, pos);
            sb.Flush();
        }
        public void DrawGridBlocks(MySpriteBatch sb, Graphics.AtlasDepthNormals.Node.Token sprite, IEnumerable<Vector3> positions, Color col)
        {
            sb.Flush();
            //Block.Atlas.Begin(sb);
            sprite.Atlas.Begin(sb);
            foreach (var pos in positions)
                this.DrawGridBlock(sb, sprite, col, pos);
            sb.Flush();
        }
        public void DrawBlockMouseover(MySpriteBatch sb, IMap map, Vector3 global, Color color)
        {
            //var color = Color.White;
            if (global.Z > this.DrawLevel)
                return;
            Rectangle bounds = Block.Bounds;
            float cd;
            Rectangle screenBounds;
            Vector2 screenLoc;
            this.GetEverything(map, global, bounds, out cd, out screenBounds, out screenLoc);
            var scrbnds = this.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = global.GetDrawDepth(map, this);
            var cdback = cd - 2;// (this.Target.Global - Vector3.One).GetDrawDepth(map, camera);
            var highlight = Sprite.BlockHighlight;
            Sprite.Atlas.Begin(Game1.Instance.GraphicsDevice);
            var c = color * .5f;
            sb.Draw(Sprite.BlockHightlightBack.AtlasToken.Atlas.Texture, screenLoc, Sprite.BlockHightlightBack.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(this.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cdback);
            sb.Draw(highlight.AtlasToken.Atlas.Texture, screenLoc, highlight.AtlasToken.Rectangle, 0, Vector2.Zero, new Vector2(this.Zoom),
                Color.White, Color.White, c, Color.Transparent, SpriteEffects.None, cd);
            sb.Flush(); // flush here because i might have to switch textures in an overriden tool draw call
        }
        public void Transform(int x, int y, out double rx, out double ry)
        {
            double cos = Math.Cos((-this.Rotation) * Math.PI / 2f);
            double sin = Math.Sin((-this.Rotation) * Math.PI / 2f);
            rx = (x * cos - y * sin);
            ry = (x * sin + y * cos);
        }


        internal bool IsDrawable(Vector3 global)
        {
            return global.Z <= this.GetMaxDrawLevel() + 1;
        }


        internal void ToggleFollowing(GameObject gameObject)
        {
            this.Following = this.Following == gameObject ? null : gameObject;
        }

        internal void CenterOn(GameModes.StaticMaps.StaticMap map)
        {
            var x = map.Size.Blocks / 2;
            var y = x;
            var z = map.GetHeightmapValue(x, y);

            this.CenterOn(new Vector3(x, y, z));
        }


        internal bool IsCompletelyHiddenByFog(float z)
        {
            return z < this.LastZTarget - FogZOffset - FogFadeLength + 1;
        }
    }
}
