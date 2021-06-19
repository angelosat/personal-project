using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class Coords
    {
        //private Vector2 vector;

        //public float X
        //{
        //    get { return vector.X; }
        //    set { vector.X = value; }
        //}
        //public float Y
        //{
        //    get { return vector.Y; }
        //    set { vector.Y = value; }
        //}

        //public Coords(int x, int y)
        //{
        //    vector = new Vector2(x, y);
        //}

        //public Vector2 ToIso()
        //{
        //    return iso(vector);
        //}
        //public Vector2 ToMap()
        //{
        //    return map(vector);
        //}

        //public static Vector2 iso(Vector2 coords)
        //{
        //    return new Vector2(MapGlobal.getWidth() * 16 + (coords.X - coords.Y) * 16, (coords.X + coords.Y) * 8);
        //}
        //public static Vector2 map(Vector2 coords)
        //{
        //    return new Vector2((int)((coords.X - 8) / 32 + coords.Y / 16 - MapGlobal.getWidth() / 2), (int)(-(coords.X - 8) / 32 + coords.Y / 16 + MapGlobal.getHeight() / 2));
        //}

        // --> 1.0, 0.0, -1.0, 0.0 // x
        // --> 0.5, 2.0, 0.5, 0.0 // y
        // --> 0.0, -0.05, 0.0, 0.0 // depth
        // --> 0.0, 0.0, 0.0, 1.0 // [nothing]

        static Matrix matrix = new Matrix(Block.Width / 2, Block.Depth / 2, 0, 0, -Block.Width / 2, Block.Depth / 2, 0, 0, 0, Block.BlockHeight, 0, 0, 0, 0, 0, 1);

        public static Vector2 Transform(int x, int y, int z)
        {
            Vector4 pos = new Vector4(x, y, Map.MaxHeight - z, 0);
            Vector4.Transform(ref pos, ref matrix, out pos);
            return new Vector2(pos.X, pos.Y);
        }



        //public static void iso(Camera camera, float x, float y, float z, out double xx, out double yy)
        //{
        //    double xr = (x * camera.RotCos - y * camera.RotSin);
        //    double yr = (x * camera.RotSin + y * camera.RotCos);
        //    xx = (TileBase.Width * (xr - yr) / 2); //x /2 - y/2
        //    yy = ((xr + yr) * TileBase.Length / 2 + (Map.MaxHeight - z) * TileBase.BlockHeight); // 
        //}

        public static Vector2 GetScreenCoords(Vector3 global, Camera camera, Vector3 unitDimensions)
        {
            float x = global.X, y = global.Y, z = global.Z;
            float w = unitDimensions.X, h = unitDimensions.Y;
            double xr = (x * camera.RotCos - y * camera.RotSin);
            double yr = (x * camera.RotSin + y * camera.RotCos);
            float xx = (int)(w * (xr - yr) / 2);
            float yy = (int)((xr + yr) * h / 2 + z * unitDimensions.Z);

            //return new Vector2((int)(xx - camera.Zoom * camera.X), (int)(yy - camera.Zoom * camera.Y));
            return new Vector2((int)(xx - camera.Zoom * camera.Location.X), (int)(yy - camera.Zoom * camera.Location.Y));
        }

        public static void Iso(Camera camera, float x, float y, float z, out int xx, out int yy)
        {
            double xr = (x * camera.RotCos - y * camera.RotSin);
            double yr = (x * camera.RotSin + y * camera.RotCos);
            xx = (int)(Block.Width * (xr - yr) / 2); //x /2 - y/2
            yy = (int)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);// + (Map.MaxHeight - z) * Tile.BlockHeight); // 
        }

        public static void Iso(Camera camera, float x, float y, float z, out float xx, out float yy)
        {
            double xr = (x * camera.RotCos - y * camera.RotSin);
            double yr = (x * camera.RotSin + y * camera.RotCos);
            xx = (float)(Block.Width * (xr - yr) / 2); //x /2 - y/2
            yy = (float)((xr + yr) * Block.Depth / 2 - z * Block.BlockHeight);// + (Map.MaxHeight - z) * Tile.BlockHeight); // 
        }

        static float CosIso = (float)Math.Cos(Math.PI / 4f);
        static float SinIso = (float)Math.Sin(Math.PI / 4f);
        static public Matrix IsoMatrix = new Matrix(
                CosIso, -SinIso, 0, 0,
                SinIso, CosIso, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
        public static void Ortho(int x, int y, out float xx, out float yy)
        {
            Vector2 xy = new Vector2(x, 2*y);
            xy = Vector2.Transform(xy, IsoMatrix);
           // xy.Y = xy.Y / 2f;
        //    xy.Y *= 2f;
            xx = xy.X;
            yy = xy.Y;
        }

        public static Vector2 Ortho(Vector3 global)
        {
            float x = global.X, y = global.Y;
            Vector2 xy = new Vector2(x, 2 * y);
            xy = Vector2.Transform(xy, IsoMatrix);
            float xr = xy.X, yr = xy.Y;
            float xx = (int)(Block.Width * (xr - yr) / 2); 
            float yy = (int)((xr + yr) * Block.Depth / 2 + (Map.MaxHeight - global.Z) * Block.BlockHeight); ;
            return new Vector2(xx, yy);
        }

        //public static void Ortho(float x, float y, out float xx, out float yy)
        //{
        //    Vector2 xy = new Vector2(x, 2 * y);
        //    xy = Vector2.Transform(xy, IsoMatrix);
        //    // xy.Y = xy.Y / 2f;
        //    //    xy.Y *= 2f;
        //    xx = xy.X;
        //    yy = xy.Y;
        //}
        //public static void Rotate(Camera camera, float x, float y, float z, out int xx, out int yy)
        //{
        //    //xr = (int)(x * camera.RotCos - y * camera.RotSin);
        //    //yr = (int)(x * camera.RotSin + y * camera.RotCos);
        //    double xr = (x * camera.RotCos - y * camera.RotSin);
        //    double yr = (x * camera.RotSin + y * camera.RotCos);
        //    xx = (int)((xr - yr) / 2); //x /2 - y/2
        //    yy = (int)((xr + yr) / 2 + Map.MaxHeight - z); // 
        //}
        public static void Rotate(double radians, float x, float y, out int xx, out int yy)
        {
            xx = (int)(x * Math.Cos(radians) - y * Math.Sin(radians));
            yy = (int)(x * Math.Sin(radians) + y * Math.Cos(radians));
        }
        
        //public static void Rotate(double radians, float x, float y, out float xx, out float yy)
        //{
        //    xx = (float)(x * Math.Cos(radians) - y * Math.Sin(radians));
        //    yy = (float)(x * Math.Sin(radians) + y * Math.Cos(radians));
        //}
        public static Vector2 Rotate(double radians, Vector2 vector)
        {
            float x = vector.X, y = vector.Y;
            return new Vector2((float)(x * Math.Cos(radians) - y * Math.Sin(radians)), (float)(x * Math.Sin(radians) + y * Math.Cos(radians)));
        }
        public static Vector2 Rotate(Camera camera, Vector2 vector)
        {
            float x = vector.X, y = vector.Y;
            return new Vector2((float)(x * camera.RotCos - y * camera.RotSin), (float)(x * camera.RotSin + y * camera.RotCos));
        }
        public static Vector2 Rotate(Camera camera, Vector3 vector)
        {
            float x = vector.X, y = vector.Y;
            return new Vector2((float)(x * camera.RotCos - y * camera.RotSin), (float)(x * camera.RotSin + y * camera.RotCos));
        }
        public static void Rotate(Camera camera, float x, float y, out int xx, out int yy)
        {
            xx = (int)(x * camera.RotCos - y * camera.RotSin);
            yy = (int)(x * camera.RotSin + y * camera.RotCos);
        }
        public static void Rotate(Camera camera, float x, float y, out float xx, out float yy)
        {
            xx = (float)(x * camera.RotCos - y * camera.RotSin);
            yy = (float)(x * camera.RotSin + y * camera.RotCos);
        }
        public static void Rotate(Camera camera, Vector3 global, out Vector3 rotatedGlobal)
        {
            int xx, yy;
            xx = (int)(global.X * camera.RotCos - global.Y * camera.RotSin);
            yy = (int)(global.X * camera.RotSin + global.Y * camera.RotCos);
            rotatedGlobal = new Vector3(xx, yy, global.Z);
        }

        public static void Rotate(int r, Vector3 global, out Vector3 rotatedGlobal)
        {
            int _r = -r;
            int _Rotation = _r % 4;
            if (_Rotation < 0)
                _Rotation = 4 + _r;

            double RotCos = Math.Cos((Math.PI / 2f) * _Rotation);
            double RotSin = Math.Sin((Math.PI / 2f) * _Rotation);

            RotCos = Math.Round(RotCos + RotCos) / 2f;
            RotSin = Math.Round(RotSin + RotSin) / 2f;

            int xx, yy;
            xx = (int)(global.X * RotCos - global.Y * RotSin);
            yy = (int)(global.X * RotSin + global.Y * RotCos);
            rotatedGlobal = new Vector3(xx, yy, global.Z);
        }
        public static void Rotate(int r, float x, float y, out int xx, out int yy)
        {
            int _r = -r;
            int _Rotation = _r % 4;
            if (_Rotation < 0)
                _Rotation = 4 + _r;

            double RotCos = Math.Cos((Math.PI / 2f) * _Rotation);
            double RotSin = Math.Sin((Math.PI / 2f) * _Rotation);

            RotCos = Math.Round(RotCos + RotCos) / 2f;
            RotSin = Math.Round(RotSin + RotSin) / 2f;

            xx = (int)(x * RotCos - y * RotSin);
            yy = (int)(x * RotSin + y * RotCos);
        }
        //static public int isoX(int x, int y)
        //{
        //    return TileBase.Width / 2 + (x - y) * TileBase.Width / 2;
        //}
        //static public int isoY(int x, int y, int z)
        //{
        //    return (x + y + 1) * TileBase.Length / 2 + (Map.MaxHeight - z) * TileBase.BlockHeight;
        //    //return (x + y + 1) * TileBase.Length / 2 + (Map.MaxHeight - z) * TileBase.BlockHeight;
        //}

        //public static Vector2 iso(int x, int y, int z)
        //{
        //    //return new Vector2(TileBase.Width / 2 + (x - y) * TileBase.Width / 2, (x + y) * TileBase.Length / 2 + TileBase.Length / 2 + (Map.MaxHeight - z) * TileBase.BlockHeight);

        //    return new Vector2(isoX(x, y), isoY(x, y, z));
        //}

        //public static Vector2 iso(Vector3 coords)
        //{

        //    return new Vector2(TileBase.Width / 2 + (coords.X - coords.Y) * TileBase.Width / 2, (coords.X + coords.Y) * TileBase.Length / 2 + TileBase.Length / 2 + (Map.MaxHeight - coords.Z) * TileBase.BlockHeight);

        //}

    }
}