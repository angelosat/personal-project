using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.Windows.Forms;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public class Camera : EntityComponent
    {
        public override string ComponentName => "Camera";
        public const int FogZOffset = 2, FogFadeLength = 8;
        Vector4 FogColor = Color.SteelBlue.ToVector4();
        GameObject Following;
        float DragDelay = 0, DragDelayMax = Engine.TicksPerSecond / 4;
        bool Dragging;
        Vector2 DragVector;
        Vector2 _Coordinates;
        bool _HideUnknownBlocks = true;
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
        bool _DrawTopSlice = true;
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
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
        public static event EventHandler<EventArgs> LocationChanged;
        protected void OnLocationChanged()
        {
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }
        public static event EventHandler<EventArgs> RotationChanged;
        protected void OnRotationChanged()
        {
            foreach (var chunk in Rooms.Ingame.CurrentMap.GetActiveChunks())
            {
                chunk.Value.OnCameraRotated(this);
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
            Width = (sender as GraphicsDeviceManager).PreferredBackBufferWidth;
            Height = (sender as GraphicsDeviceManager).PreferredBackBufferHeight;
            this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            this.RenderTargetsInvalid = true;
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
                DragDelay += 1;
                return;
            }
            this.Coordinates += DragVector / (Zoom * 10f); 

        }

        void SetZoom(float value)
        {
            this.Zoom = value;

            var offset = new Vector2(this.Width / 2, this.Height / 2);
            offset /= this.Zoom;
            this.Location = this.Coordinates - offset;
        }
        public void SmoothZoom(float next)
        {
            float diff = next - this.Zoom;
            
            var zoomSpeed = 0.1f;
            var n = zoomSpeed * diff;

            if (Math.Abs(n) < 0.001f)
            {
                this.SetZoom(next);
            }
            else
                this.SetZoom(this.Zoom + n);
        }

        public void CenterOn(Vector3 global)
        {
            Global = global;
            Coords.Iso(this, global.X, global.Y, global.Z, out int xx, out int yy);
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
            Coords.Iso(this, global.X, global.Y, global.Z, out float xx, out float yy);

            Vector2 currentLoc = this.Coordinates
                , nextLoc = new Vector2(xx, yy);
            Vector2 diff = nextLoc - currentLoc;

            diff *= 100;
            diff = diff.Round();
            diff /= 100;

            var nextCoords = currentLoc + 0.05f * diff;

            // TODO: find a way to make it smooth without seaming between sprites

            /// uncomment this to make camera movement rigid instead of smooth
            nextCoords = nextCoords.Round(); // must round to prevent seaming between blocks when moving camera
            ///

            this.Coordinates = nextCoords;
        }

        public void UpdateBbox()
        {
            CellIntersectBox.X = (int)((Width / 2) / Zoom);
            CellIntersectBox.Y = (int)((Height / 2) / Zoom);
            CellIntersectBox.Width = (int)((Width / 2) / Zoom);
            CellIntersectBox.Height = (int)((Height / 2) / Zoom);
        }
        public Rectangle ViewPort;

        public void GetEverything(IMap map, Vector3 global, Rectangle spriteRect, out float depth, out Rectangle screenBounds, out Vector2 screenLoc)
        {
            depth = global.GetDrawDepth(map, this);
            screenBounds = GetScreenBounds(global, spriteRect);
            screenLoc = new Vector2(screenBounds.X, screenBounds.Y);
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle)
        {
            return this.GetScreenBounds(x, y, z, spriteRectangle, 0, 0);
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin)
        {
            Coords.Iso(this, x, y, z, out int xx, out int yy);
            return new Rectangle((int)(Zoom * (xx + spriteRectangle.X - this.Location.X - origin.X)), (int)(Zoom * (yy + spriteRectangle.Y - this.Location.Y - origin.Y)), (int)(Zoom * spriteRectangle.Width), (int)(Zoom * spriteRectangle.Height));
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy)
        {
            Coords.Iso(this, x, y, z, out int xx, out int yy);
            return new Rectangle(
                (int)(Zoom * (xx + spriteRectangle.X - this.Location.X - originx)), 
                (int)(Zoom * (yy + spriteRectangle.Y - this.Location.Y - originy)), 
                (int)(Zoom * spriteRectangle.Width), 
                (int)(Zoom * spriteRectangle.Height));
        }
        public Rectangle GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, int originx, int originy, float scale)
        {
            Coords.Iso(this, x, y, z, out int xx, out int yy);
            var scalezoom = scale * Zoom;
            return new Rectangle(
                (int)(Zoom * (xx + scale * spriteRectangle.X - this.Location.X - originx)), 
                (int)(Zoom * (yy + scale * spriteRectangle.Y - this.Location.Y - originy)), 
                (int)(scalezoom * spriteRectangle.Width), 
                (int)(scalezoom * spriteRectangle.Height));
        }
        public Vector4 GetScreenBoundsVector4(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin, float scale = 1)
        {
            Coords.Iso(this, x, y, z, out float xx, out float yy);
            var loc = this.Location;
            float xxx = (float)((xx + scale * spriteRectangle.X - loc.X - origin.X));
            float yyy = (float)((yy + scale * spriteRectangle.Y - loc.Y - origin.Y));
            float w = scale * (float)(spriteRectangle.Width);
            float h = scale * (float)(spriteRectangle.Height);
            var vector = new Vector4(xxx, yyy, w, h);
            vector *= this.Zoom;
            return vector;
        }
        public Vector4 GetScreenBoundsVector4NoOffset(float x, float y, float z, Rectangle spriteRectangle, Vector2 origin)
        {
            Coords.Iso(this, x, y, z, out float xx, out float yy);
            float xxx = (float)((xx + spriteRectangle.X - origin.X));
            float yyy = (float)((yy + spriteRectangle.Y - origin.Y));
            float w = (float)(spriteRectangle.Width);
            float h = (float)(spriteRectangle.Height);
            var vector = new Vector4(xxx, yyy, w, h);
            return vector;
        }
        public void GetScreenBounds(float x, float y, float z, Rectangle spriteRectangle, out Vector2 pos, out Vector2 size)
        {
            Coords.Iso(this, x, y, z, out float xx, out float yy);
            pos = new Vector2(xx + spriteRectangle.X - Location.X, yy + spriteRectangle.Y - Location.Y) * this.Zoom;
            size = new Vector2(Zoom * spriteRectangle.Width, this.Zoom * spriteRectangle.Height);
        }
        public Vector2 GetScreenBounds(float x, float y, float z)
        {
            Coords.Iso(this, x, y, z, out int xx, out int yy);
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
            Coords.Iso(this, pos.X, pos.Y, pos.Z, out int xx, out int yy);
            return new Vector2(Zoom * (xx - Location.X), Zoom * (yy - Location.Y));
        }
        public Vector2 GetScreenPositionFloat(Vector3 pos)
        {
            Coords.Iso(this, pos.X, pos.Y, pos.Z, out float xx, out float yy);
            var loc = this.Location;
            var screenpos = new Vector2(Zoom * (xx - loc.X), Zoom * (yy - loc.Y));
            return screenpos;
        }

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
            return new Camera(Width, Height);
        }

        double _Rotation;
        public double RotCos, RotSin;

        public Camera()
            : this(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height)
        {
            this.WaterSpriteBatch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.SpriteBatch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
        }

        public Camera(int width, int height, float x = 0, float y = 0, float z = 0, float zoom = 2, int rotation = 0)
        {
            this.Width = width;
            this.Height = height;
            this.ViewPort = new Rectangle(0, 0, this.Width, this.Height);
            this.Zoom = zoom;
            this.ZoomNext = zoom;
            this.Rotation = rotation;
            CenterOn(new Vector3(x, y, z));
            Game1.Instance.graphics.DeviceReset += new EventHandler<EventArgs>(gfx_DeviceReset);
        }
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
        
        public void RotateClockwise()
        {
            this.Rotation++;
        }
        public void RotateCounterClockwise()
        {
            this.Rotation--;
        }
        public void RotationReset()
        {
            this.Rotation = 0;
        }
        public bool DrawCell(Canvas canvas, IMap map, Vector3 global)
        {
            map.TryGetAll(global, out var chunk, out var cell);
            int z = cell.Z;
            Block block = cell.Block;
            if (block == BlockDefOf.Air)
            {
                chunk.InvalidateCell(cell);
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);

            //Vector3 globalRotated = new Vector3(rgx, rgy, z);
            //float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(gx, gy, z, Block.Bounds, Vector2.Zero);// GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            //Coords.Rotate(this, lx, ly, out int rlx, out int rly);
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
            Block.Types cellTile = cell.Block.Type;
            if (cellTile == Block.Types.Air)
            {
                chunk.InvalidateCell(cell);
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }

            Block block = cell.Block;

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, lx, ly, out int rlx, out int rly);
            var depth = rlx + rly;

            Color finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
            var isDiscovered = !map.IsUndiscovered(new Vector3(gx, gy, z));
            /// DONT ERASE
            ///if (cell.AllEdges == 0 && HideUnknownBlocks)  // do i want cells that have already been discoverd, to remain visible even if they become obstructed again?
            if (!isDiscovered && HideUnknownBlocks)// && isAir) // do i want cells that have already been discoverd, to remain visible even if they become obstructed again?
                Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth);
            else
                block.Draw(canvas, chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);
            return true;
        }
        public bool DrawBlockGlobal(MySpriteBatch sb, IMap map, Vector3 global)
        {
            int z = (int)global.Z;
            int gx = (int)global.X;
            int gy = (int)global.Y;
            
            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(gx, gy, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, gx, gy, out int rlx, out int rly);
            var depth = rlx + rly;

            sb.DrawBlock(Block.Atlas.Texture, screenBoundsVector4,
                Block.BlockBlueprint,
                this.Zoom, Color.Transparent, Color.White*.5f, Color.White, Color.White, Vector4.One, Vector4.Zero, depth, null, global);
            return true;
        }
        public bool DrawUnknown(Canvas canvas, IMap map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;
            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;
            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z, false);

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, lx, ly, out int rlx, out int rly);
            var depth = rlx + rly;

            Block.DrawUnknown(canvas.Opaque, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

            return true;
        }
        public bool DrawUnknown(MySpriteBatch sb, IMap map, Chunk chunk, Cell cell)
        {
            int z = cell.Z;
            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);
            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z, false);

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);
            Coords.Rotate(this, lx, ly, out int rlx, out int rly);
            var depth = rlx + rly;

            Block.DrawUnknown(sb, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, Color.Transparent, Color.White, depth);

            return true;
        }
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

            if (z > this.MaxDrawZ)// i'm cacheing that in the beginning of the drawchunk method
                return false;

            Block.Types cellTile = cell.Block.Type;
            if (cellTile == Block.Types.Air)
            {
                chunk.InvalidateCell(cell);
                ("tried to draw air at " + cell.GetGlobalCoords(chunk).ToString()).ToConsole();
                return false;
            }

            Block block = cell.Block;

            int lx = cell.X, ly = cell.Y, gx = (int)chunk.Start.X + lx, gy = (int)chunk.Start.Y + ly;

            LightToken light = GetFinalLight(this, map, chunk, cell, gx, gy, z);

            var mapOffset = map.GetOffset();
            Coords.Rotate(this, gx - mapOffset.X, gy - mapOffset.Y, out int rgx, out int rgy);

            float cd = new Vector3(gx, gy, z).GetDrawDepth(map, this);

            DepthFar = Math.Min(DepthFar, cd);
            DepthNear = Math.Max(DepthNear, cd);

            var screenBoundsVector4 = GetScreenBoundsVector4NoOffset(lx, ly, z, Block.Bounds, Vector2.Zero);

            var depth = lx + ly;

            maxd = Math.Max(maxd, depth);
            mind = Math.Min(mind, depth);
            Color finalFogColor = Color.Transparent; // i calculate fog inside the shader from now on
            block.Draw(chunk.Canvas, chunk, new Vector3(gx, gy, z), this, screenBoundsVector4, light.Sun, light.Block, finalFogColor, Color.White, depth, cell.Variation, cell.Orientation, cell.BlockData);

            return true;
        }
        float maxd = float.MinValue, mind = float.MaxValue;

        public float FogLevel = 0;
       
        public Color GetFogColorNew(int z)
        {
            if (!Fog)
                return Color.Transparent;
            if (this.LastZTarget > 1)
                if (z < this.LastZTarget - FogZOffset)
                {
                    var d = Math.Abs(z - this.LastZTarget + FogZOffset);
                    d = MathHelper.Clamp(d, 0, FogFadeLength) / FogFadeLength;
                    var fog = Color.Lerp(Color.White, Color.DarkSlateBlue, d);
                    var val = (byte)(d * 255);
                    var finalFogColor = new Color(fog.R, fog.G, fog.B, val);
                    return finalFogColor;
                }
            return Color.Transparent;
        }

        

        public int MaxDrawZ;
        Vector2 CameraOffset;

        internal void DrawChunk(MySpriteBatch sb, IMap map, Chunk chunk, Vector3? playerGlobal, List<Rectangle> hiddenRects, EngineArgs a)
        {
            throw new Exception();
        }

        public void PrepareShader(IMap map)
        {
            var view =
                new Matrix(
                  1.0f, 0.0f, 0.0f, 0.0f,
                  0.0f, -1.0f, 0.0f, 0.0f,
                  0.0f, 0.0f, 1.0f, 0.0f,
                  0.0f, 0.0f, 0.0f, 1.0f);
            float camerax = this.Coordinates.X;
            float cameray = this.Coordinates.Y;
            view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            var near = -GetFarDepth(map) * this.Zoom;
            var far = -GetNearDepth(map) * this.Zoom;
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
            var view =
                new Matrix(
                   1.0f, 0.0f, 0.0f, 0.0f,
                   0.0f, -1.0f, 0.0f, 0.0f,
                   0.0f, 0.0f, 1.0f, 0.0f,
                   0.0f, 0.0f, 0.0f, 1.0f);
            float camerax = this.Coordinates.X;
            float cameray = this.Coordinates.Y;
            view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(this.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            var near = -GetFarDepth(map) * this.Zoom;
            var far = -GetNearDepth(map) * this.Zoom;
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, near, far);
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

        public static LightToken GetFinalLight(Camera camera, IMap map, Chunk chunk, Cell cell, int gx, int gy, int z, bool updateblockfaces = true)
        {
            // UNCOMMENT THIS?
            //if (chunk.LightCache.TryGetValue(new Vector3(gx, gy, z), out color))
            //    return color;
            //if (cell.Light != null)
            //    return cell.Light;
            var global = new Vector3(gx, gy, z);

            if (chunk.LightCache2.TryGetValue(global, out LightToken cached))
            {
                return cached;
            }

            // update block exposed faces too here?
            // TESTING IF REMOVING THIS BREAKS ANYTHING
            //if (updateblockfaces)
            //    chunk.UpdateBlockFaces(cell); // COMMENT if i want to see visible horizontal slices of the map

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
            
            Color sun = new((suneast + 1) / 16f, (sunsouth + 1) / 16f, (suntop + 1) / 16f);
            Vector4 block = new((blockeast + 1) / 16f, (blocksouth + 1) / 16f, (blocktop + 1) / 16f, 1f);

            Engine.LightQueries++;
            LightToken light = new LightToken() { Global = global, Sun = sun, Block = block };
            chunk.LightCache2[global] = light;
            return light;
        }

        Vector3 LastMouseover = new Vector3(float.MinValue);

        public void CreateMouseover(IMap map, Vector3 global)
        {
            if (Controller.Instance.MouseoverBlockNext.Object != null)
            {
                return;
            }
            if (!map.TryGetAll(global, out var chunk, out var cell))
                return;

            Rectangle texbounds = Block.Bounds;

            Rectangle cellScreenBounds = GetScreenBounds(global, texbounds);
            Vector2 uvCoords = new Vector2((Controller.Instance.msCurrent.X - cellScreenBounds.X) / (float)Zoom, (Controller.Instance.msCurrent.Y - cellScreenBounds.Y) / (float)Zoom);
            int faceIndex = (int)uvCoords.Y * Block.MouseMapSprite.Width + (int)uvCoords.X;

            // find block coordinates
            Color sample = Block.BlockCoordinatesFull[faceIndex];
            float u = sample.R / 255f;
            float v = sample.G / 255f;
            float w = sample.B / 255f;
            Vector3 precise = new Vector3(u, v, w);
            precise.X -= 0.5f;
            precise.Y -= 0.5f; // compensate for (0,0) being at the center of the block

            Block.BlockMouseMap.HitTest((int)uvCoords.X, (int)uvCoords.Y, out Vector3 vec);

            // comment these lines if i want to select blocks even if mouseover face is inaccessible
            //if (!Cell.CheckFace(this, cell, vec))
            //    return;

            Coords.Rotate((int)this.Rotation, vec, out Vector3 rotVec);
            precise = precise.Rotate(-this.Rotation);
            // TODO: find more elegant way to do this
            if (rotVec == Vector3.UnitX || rotVec == -Vector3.UnitX)
                precise.X = 0;
            else if (rotVec == Vector3.UnitY || rotVec == -Vector3.UnitY)
                precise.Y = 0;
            else if (rotVec == Vector3.UnitZ || rotVec == -Vector3.UnitZ)
                precise.Z = 0;

            var target = new TargetArgs(map, global, rotVec, precise);
            if (global != this.LastMouseover)
                Controller.Instance.MouseoverBlockNext.Object = target;
            else
                Controller.Instance.MouseoverBlockNext.Object = Controller.Instance.MouseoverBlock.Object;
            Controller.Instance.MouseoverBlockNext.Face = rotVec;
            Controller.Instance.MouseoverBlockNext.Precise = precise;
            Controller.Instance.MouseoverBlockNext.Target = target;
            Controller.Instance.MouseoverBlockNext.Depth = global.GetMouseoverDepth(map, this);
            this.LastMouseover = global;
        }
        public void CreateMouseover(IMap map, Vector3 global, Rectangle rect, Vector2 point, bool behind)
        {
            if (Controller.Instance.MouseoverBlockNext.Object != null)
                return;
            if (!map.TryGetAll(global, out var chunk, out var cell))
                return;

            var uvCoords = new Vector2((point.X - rect.X) / (float)Zoom, (point.Y - rect.Y) / (float)Zoom);
            int faceIndex = (int)uvCoords.Y * cell.Block.MouseMap.Texture.Width + (int)uvCoords.X;

            // find block coordinates
            Color sample = cell.Block.UV[faceIndex];
            float u = sample.R / 255f;
            float v = sample.G / 255f;
            float w = sample.B / 255f;
            var precise = new Vector3(u, v, w);// Vector3.Zero;
            precise.X -= 0.5f;
            precise.Y -= 0.5f; // compensate for (0,0) being at the center of the block

            cell.Block.MouseMap.HitTest(behind, (int)uvCoords.X, (int)uvCoords.Y, out Vector3 vec);

            // comment these lines if i want to select blocks even if mouseover face is inaccessible
            //if (!Cell.CheckFace(this, cell, vec))
            //    return;

            Coords.Rotate((int)this.Rotation, vec, out Vector3 rotVec);
            precise = precise.Rotate(-this.Rotation);
            // TODO: find more elegant way to do this
            if (rotVec == Vector3.UnitX || rotVec == -Vector3.UnitX)
                precise.X = 0;
            else if (rotVec == Vector3.UnitY || rotVec == -Vector3.UnitY)
                precise.Y = 0;
            else if (rotVec == Vector3.UnitZ || rotVec == -Vector3.UnitZ)
                precise.Z = 0;

            Controller.SetMouseoverBlock(this, map, global, rotVec, precise);
        }
       
        public int RenderIndex = 0;
        public RenderTarget2D MapRender,
            WaterRender, WaterDepth, WaterLight, WaterFog,
            WaterComposite,
            MapDepth, MapLight, TextureFogWater, MapComposite,
            RenderBeforeFog, LightBeforeFog, DepthBeforeFog, FogBeforeFog,
            FinalScene;
        public RenderTarget2D[] RenderTargets = new RenderTarget2D[5];
        public void DrawMap(SpriteBatch sb, IMap map, ToolManager toolManager, UIManager ui, SceneState scene)
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            if (map == null)
                return;

            if (this.RenderTargetsInvalid)
            {
                this.OnDeviceLost();
                this.RenderTargetsInvalid = false;
            }
            
            RenderTargets[0] = MapRender;
            RenderTargets[1] = MapDepth;
            RenderTargets[2] = MapLight;
            this.RenderTargets[3] = this.TextureFogWater;

            gd.SetRenderTargets(MapRender, MapDepth, MapLight, TextureFogWater);

            var a = EngineArgs.Default;

            gd.SetRenderTargets(null);
            gd.Clear(Color.Transparent);
            gd.RasterizerState = RasterizerState.CullNone;
            NewDraw(map, gd, a, scene, toolManager, ui);
        }
        float DepthFar, DepthNear;
        public MySpriteBatch SpriteBatch;
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
            var xx = this.CameraOffset.X / this.Width;
            var yy = this.CameraOffset.Y / this.Height;

            var world = Matrix.Identity;
            var view =
                new Matrix(
                   1.0f, 0.0f, 0.0f, 0.0f,
                   0.0f, -1.0f, 0.0f, 0.0f,
                   0.0f, 0.0f, -1.0f, 0.0f,
                   0.0f, 0.0f, 0.0f, 1.0f);
            var projection = Matrix.CreateOrthographicOffCenter(
                0, this.Width, -this.Height, 0, 0, 1);

            fx.Parameters["World"].SetValue(Matrix.Identity);

            fx.Parameters["BlockWidth"].SetValue(Block.Width);
            fx.Parameters["BlockHeight"].SetValue(Block.Height);
            fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
            fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
            fx.Parameters["Viewport"].SetValue(new Vector2(gd.Viewport.Width, gd.Viewport.Height));
            fx.Parameters["ViewportW"].SetValue(new Vector2(1, gd.Viewport.Width / (float)gd.Viewport.Height));
            fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
            fx.Parameters["Zoom"].SetValue(this.Zoom);
            float borderPx = 1;
            fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / gd.Viewport.Width, borderPx / gd.Viewport.Height) * this.Zoom);
            fx.Parameters["CullDark"].SetValue(Engine.CullDarkFaces);
            var nightAmount = (float)map.GetDayTimeNormal();
            Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), nightAmount);
            ambientColor = map.GetAmbientColor();

            Vector4 ambient = ambientColor.ToVector4();
            fx.Parameters["AmbientLight"].SetValue(ambient);
            var fogColor = this.FogColor;
            // choose between ambient or black background color
            fogColor = Color.Lerp(new Color(fogColor), Color.Black, nightAmount).ToVector4();
            fx.Parameters["FogColor"].SetValue(fogColor);
            fx.Parameters["FogDistance"].SetValue(FogFadeLength);

            var fogoffset = new Vector2(this.FogT / 100f, 0);
            fx.Parameters["FogOffset"].SetValue(fogoffset - this.Coordinates / 1000f);
            if (toolManager.ActiveTool.Target != null && toolManager.ActiveTool.Target.Type != TargetType.Null)
            {
                this.LastZTarget = toolManager.ActiveTool.Target.Global.Z;
                fx.Parameters["FogZ"].SetValue(toolManager.ActiveTool.Target.Global.Z - FogZOffset);
            }
            fx.Parameters["FogEnabled"].SetValue(Fog);

            fx.Parameters["PlayerOcclusion"].SetValue(this.Following != null);
            fx.Parameters["PlayerGlobal"].SetValue(this.Following != null ? this.Following.Global : Vector3.Zero);
            if (this.Following != null)
            {
                fx.Parameters["PlayerRotXY"].SetValue((float)(
                    this.Following.Global.X * this.RotCos - this.Following.Global.Y * this.RotSin +
                    this.Following.Global.X * this.RotSin + this.Following.Global.Y * this.RotCos));
                fx.Parameters["PlayerCenterOffset"].SetValue(this.GetScreenPositionFloat(this.Following.Global + this.Following.Physics.Height * Vector3.UnitZ / 2) / new Vector2(ViewPort.Width, ViewPort.Height) - Vector2.One * .5f);
            }

            fx.Parameters["FogLevel"].SetValue(FogLevel);
            this.MaxDrawZ = GetMaxDrawLevel();
            this.Effect.Parameters["FogEnabled"].SetValue(Fog);
            this.Effect.Parameters["MaxDrawLevel"].SetValue(this.MaxDrawZ);
            this.Effect.Parameters["HideWalls"].SetValue(Engine.HideWalls);
            this.Effect.Parameters["OcclusionRadius"].SetValue(.01f * this.Zoom * this.Zoom);

            gd.DepthStencilState = DepthStencilState.Default;
            
            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.SamplerStates[1] = SamplerState.PointClamp;
            gd.SamplerStates[2] = SamplerState.PointClamp;
            gd.SamplerStates[3] = SamplerState.PointClamp;

            if (this.SpriteBatch == null)
                SpriteBatch = new MySpriteBatch(gd);
            if (this.WaterSpriteBatch == null)
                this.WaterSpriteBatch = new MySpriteBatch(gd);
            if (this.ParticlesSpriteBatch == null)
                this.ParticlesSpriteBatch = new MySpriteBatch(gd);
            if (this.TransparentBlocksSpriteBatch == null)
                this.TransparentBlocksSpriteBatch = new MySpriteBatch(gd);
            if (this.BlockParticlesSpriteBatch == null)
                this.BlockParticlesSpriteBatch = new MySpriteBatch(gd);


            var clearcol = new Color(1f, 1f, 1f, 0); // if i put 1 for the alpha than tsansparent blocks will be shaded white  // (old comment) i put 1 again because i dont draw water on the fog texture after all
            //var clearcol = new Color(1f, 1f, 1f, 1f); // causes unhandled white background

            gd.SetRenderTargets(this.TextureFogWater);
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Transparent, 1, 1);
            // PROBLEM clearing the texturefogwater with the same parameters as the other rendertargets, causes the problem with the background being drawn over the toolmanager preview blocks
            gd.SetRenderTargets(MapRender, MapLight, MapDepth);
            //var clearcol = new Color(1f, 1f, 0, 0); // 3rd component is 0 in order to not draw water on background
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, clearcol, 1, 1);

            // after clearing each target with the appropriate parameters for each one, set them all together
            gd.SetRenderTargets(MapRender, MapLight, MapDepth, this.TextureFogWater);

            // use new technique to draw both color and light in one pass in multiple rendertargets
            fx.CurrentTechnique = fx.Techniques["Combined"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[2] = Block.Atlas.NormalTexture;
            gd.Textures[3] = Block.Atlas.DepthTexture;

            DepthNear = float.MinValue;
            DepthFar = float.MaxValue;
            
            this.Effect.Parameters["RotCos"].SetValue((float)this.RotCos);
            this.Effect.Parameters["RotSin"].SetValue((float)this.RotSin);

            if (PlayerOld.Actor != null)
            {
                if (PlayerOld.Actor.IsSpawned)
                {
                    Sprite sprite = PlayerOld.Actor.GetSprite();
                    Rectangle spriteBounds = sprite.GetBounds(); 
                    Rectangle screenBounds = this.GetScreenBounds(PlayerOld.Actor.Global, spriteBounds);
                    var xxx = screenBounds.X / (float)this.Width - .5f;
                    var yyy = screenBounds.Y / (float)this.Height - .5f;
                    var www = (screenBounds.X + screenBounds.Width) / (float)this.Width - .5f;
                    var hhh = (screenBounds.Y + screenBounds.Height) / (float)this.Height - .5f;
                    xxx = -.1f * this.Zoom;
                    yyy = -.15f * this.Zoom;
                    www = .1f * this.Zoom;
                    hhh = .15f * this.Zoom;
                    var box = new Vector4(xxx, yyy, www, hhh);
                    this.Effect.Parameters["PlayerBoundingBox"].SetValue(box);
                    this.Effect.Parameters["PlayerDepth"].SetValue(PlayerOld.Actor.Global.GetDrawDepth(map, this));
                }
            }

            this.PrepareShader(map);
            Towns.Housing.House house = PlayerOld.Actor != null ? map.Town.GetHouseAt(PlayerOld.Actor.Global) : null;

            var visibleChunks = (from ch in map.GetActiveChunks().Values where this.ViewPort.Intersects(ch.GetScreenBounds(this)) select ch);

            if (house != null)
            {
                house.Draw(this, this.Effect);
            }
            else
            {
                foreach (var chunk in visibleChunks)
                {
                    // TODO: DONT BUILD TOP SLICE TWICE!

                    if (!chunk.Valid)
                        chunk.Build(this);

                    chunk.DrawOpaqueLayers(this, this.Effect); // TODO: is it faster to pass only the effectparameters?
                    continue;
                }
            }
            this.TopSliceChanged = false;
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
            //gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
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
            gd.SetRenderTarget(this.MapComposite);
            gd.Clear(new Color(fogColor));

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
            SpriteBatch.Draw(WaterRender, WaterRender.Bounds, gd.Viewport.Bounds, Color.White);
            // TODO: Must draw entities before final composition, so fog is applied over them accordingly
            fx.CurrentTechnique = fx.Techniques["CompositeWater"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch.Flush();

            //sort objects back to front for proper semitraspanrent rendering
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
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteComponent.DrawShadows(shadowsSB, map, this);
            gd.SetRenderTarget(this.MapComposite); 
            shadowsSB.Flush();

            // flush entity spritebatch after shadows so they get drawn above them
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"];
            gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true };
            fx.CurrentTechnique.Passes["Pass1"].Apply();
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
            // gd.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = true }; // this broke depth on block highlights
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            toolManager.DrawAfterWorld(SpriteBatch, map);
            SpriteBatch.Flush();

            // draw entity mouseover highlight
            fx.CurrentTechnique = fx.Techniques["EntityMouseover"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
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
            gd.SetRenderTargets(this.RenderBeforeFog, this.FogBeforeFog);
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
            map.DrawWorld(SpriteBatch, this);

            SpriteBatch.Flush();
        }

        private void DrawEntities(SceneState scene, List<GameObject> objs, IEnterior enterior)
        {
            foreach (var obj in objs)
            {
                if (obj.Global.Z > this.MaxDrawZ + 1)
                    continue;
                if (enterior != null)
                    if (!enterior.Contains(obj.Global))
                        continue;
                
                // TODO: check bounding box intersection instead of single point to avoid entity pop-in
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
                MapRender = new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
            if (MapDepth == null)
                MapDepth = new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);
            if (MapLight == null)
                MapLight = new RenderTarget2D(gd, target.Width, target.Height, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);

            Effect fx = Game1.Instance.Content.Load<Effect>("blur");

            fx.Parameters["BlockWidth"].SetValue(Block.Width + 2 * Graphics.Borders.Thickness);
            fx.Parameters["BlockHeight"].SetValue(Block.Height + 2 * Graphics.Borders.Thickness);
            fx.Parameters["AtlasWidth"].SetValue(Block.Atlas.Texture.Width);
            fx.Parameters["AtlasHeight"].SetValue(Block.Atlas.Texture.Height);
            fx.Parameters["Viewport"].SetValue(new Vector2(target.Width, target.Height));
            fx.Parameters["TileVertEnsureDraw"].SetValue(Block.Depth / (float)Block.Height);
            fx.Parameters["Zoom"].SetValue(this.Zoom);
            float borderPx = 1;
            fx.Parameters["BorderResolution"].SetValue(new Vector2(borderPx / target.Width, borderPx / target.Height) * this.Zoom);
            fx.Parameters["CullDark"].SetValue(Engine.CullDarkFaces);
            Color ambientColor = Color.Lerp(Color.White, map.GetAmbientColor(), 0);
            Vector4 ambient = ambientColor.ToVector4();
            fx.Parameters["AmbientLight"].SetValue(ambient);   

            gd.DepthStencilState = DepthStencilState.Default;
            
            gd.SamplerStates[0] = SamplerState.PointClamp;
            gd.SamplerStates[1] = SamplerState.PointClamp;
            gd.SamplerStates[2] = SamplerState.PointClamp;
            gd.SamplerStates[3] = SamplerState.PointClamp;

            MySpriteBatch mySB = new MySpriteBatch(gd);

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
            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, new Color(1f, 1f, 1f, 0), 1, 1);
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
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            map.DrawObjects(mySB, this, scene);
            mySB.Flush();
        }

        private void DrawMouseoverEntity(Effect fx, MySpriteBatch mySB)
        {
            GameObject mouseover = Controller.Instance.MouseoverBlock.Object as GameObject;
            fx.CurrentTechnique = fx.Techniques["Default"];
            fx.CurrentTechnique.Passes["Pass1"].Apply();
            if (mouseover is not null)
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

        public void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e) { }
        public void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            if (e.KeyValue == (int)GlobalVars.KeyBindings.RotateMapLeft)
            {
                this.Rotation += 1;
            }
            if (e.KeyValue == (int)GlobalVars.KeyBindings.RotateMapRight)
            {
                this.Rotation -= 1;
            }
            if (e.KeyValue == (int)System.Windows.Forms.Keys.W)
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu))
                    Engine.HideWalls = !Engine.HideWalls;
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

        const float InitialZoom = 2;
        public void ZoomReset()
        {
            this.ZoomNext = InitialZoom;
        }
        public void HandleLButtonDblClk(HandledMouseEventArgs e) { }

        static public bool Fog = true;
        public bool HideUnderground { get; set; }
        public bool BorderShading { get; set; }

        public float GetFarDepth(IMap map)
        {
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
        }

        public void OnDeviceLost()
        {
            int w = this.Width, h = this.Height;
            var gfx = Game1.Instance.GraphicsDevice;

            this.MapRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.MapDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.MapLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.TextureFogWater = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.MapComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            this.RenderBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.LightBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents); 
            this.DepthBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.FogBeforeFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            this.FinalScene = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.DiscardContents);


            this.WaterRender = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterDepth = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterLight = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterFog = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            this.WaterComposite = new RenderTarget2D(gfx, w, h, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
        }

        public void BuildChunk(IMap map, Chunk chunk, MySpriteBatch sb, EngineArgs a, int rotation)
        {
            Vector3? playerGlobal = null;
            var hiddenRects = new List<Rectangle>();
            if (PlayerOld.Actor != null)
            {
                if (PlayerOld.Actor.IsSpawned)
                {
                    playerGlobal = new Nullable<Vector3>(PlayerOld.Actor.Global.RoundXY());
                    Sprite sprite = PlayerOld.Actor.GetSprite();
                    Rectangle spriteBounds = sprite.GetBounds();
                    Rectangle screenBounds = this.GetScreenBounds(playerGlobal.Value, spriteBounds);
                    hiddenRects.Add(screenBounds);
                }
            }
            this.DrawChunk(sb, map, chunk, playerGlobal, hiddenRects, a);
            chunk.BuildFrontmostBlocks(this);
            chunk.Valid = true;
        }
        public void MousePicking(IMap map)
        {
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
            Vector2 foundMouse = Vector2.Zero;
            Block foundBlock;
            Rectangle foundRect = Rectangle.Empty;
            var camx = this.Coordinates.X - (this.Width / 2f) / this.Zoom;
            var camy = this.Coordinates.Y - (this.Height / 2f) / this.Zoom;
            var mouse = UIManager.Mouse;
            var mousex = (int)mouse.X;
            var mousey = (int)mouse.Y;
            bool behind = InputState.IsKeyDown(System.Windows.Forms.Keys.Menu);
            var visibleChunks = (from ch in map.GetActiveChunks().Values.Reverse() where this.ViewPort.Intersects(ch.GetScreenBounds(this)) select ch).ToList();
            
            int rectw = (int)(32 * this.Zoom);
            int recth = (int)(40 * this.Zoom);
            foreach (var chunk in visibleChunks)
            {
                var chunkBounds = chunk.GetScreenBounds(this);
                if (!chunkBounds.Contains(mousex, mousey))
                    continue;
                Coords.Iso(this, chunk.X * Chunk.Size, chunk.Y * Chunk.Size, 0, out float chunkx, out float chunky);
                chunkx -= camx;
                chunky -= camy;
                
                var foglvl = GetFogLevel();
                for (int j = this.MaxDrawZ; j >= foglvl; j--)
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
                        for (int i = count - 4; i >= 0; i -= 4)
                        {
                            if (!EarlyOutMousePicking(array, i, mousex, mousey, chunkx, chunky, rectw, recth, out int rectx, out int recty, out Vector3 global))
                                continue;

                            // TODO: check intersection in previous stages
                            //if (rectx <= mousex && mousex < rectx + rectw && recty <= mousey && mousey < recty + recth)
                            //{
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
                            if (!block.MouseMap.HitTestEarly(xx, yy))
                                continue;
                            Coords.Rotate(this, global.X, global.Y, out int rx, out int ry);
                            var currentDepth = rx + ry + global.Z;

                            if (currentDepth > foundDepth)
                            {
                                foundDepth = currentDepth;
                                foundGlobal = global;
                                foundMouse = mouse;
                                foundRect = new Rectangle((int)rectx, (int)recty, (int)rectw, (int)recth);
                                foundBlock = block;
                                found = true;
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
            sprite.Atlas.Begin(sb);
            foreach (var pos in positions)
                this.DrawGridBlock(sb, sprite, col, pos);
            sb.Flush();
        }
        public void DrawBlockMouseover(MySpriteBatch sb, IMap map, Vector3 global, Color color)
        {
            if (global.Z > this.DrawLevel)
                return;
            Rectangle bounds = Block.Bounds;
            this.GetEverything(map, global, bounds, out float cd, out Rectangle screenBounds, out Vector2 screenLoc);
            var scrbnds = this.GetScreenBoundsVector4(global.X, global.Y, global.Z, bounds, Vector2.Zero);
            screenLoc = new Vector2(scrbnds.X, scrbnds.Y);
            cd = global.GetDrawDepth(map, this);
            var cdback = cd - 2;
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
