using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class Line
    {
        static public bool LineOfSight(int x0, int y0, int x1, int y1, int z, Predicate<Vector3> check)//, Vector3 b)
        {
            //var a = new Vector2(x0, y0);
            //var b = new Vector2(x1, y1);
            var points = Plot(x0, y0, x1, y1);
            foreach (var p in points)
            {
                // if blocked return false
                var pp = new Vector3(p, z);
                if (check(pp))
                    return false;
            }
            return true;

            ////var x0 = a.X;
            ////var y0 = a.Y;
            ////var x1 = b.X;
            ////var y1 = b.Y;
            //var dx = x1 - x0;
            //var dy = y1 - y0;
            ////var adx = Math.Abs(dx);
            ////var ady = Math.Abs(dy);
            //float f = 0;
            //int sx, sy; //steps

            //if (dy < 0)
            //{
            //    dy = -dy;
            //    sy = -1;
            //}
            //else
            //    sy = 1;

            //if (dx < 0)
            //{
            //    dx = -dx;
            //    sx = -1;
            //}
            //else
            //    sx = 1;

            //if (dx >= dy)
            //{

            //}
            //return true;
        }
        static public bool LineOfSight(int x0, int y0, int z0, int x1, int y1, int z1, Predicate<Vector3> check)//, Vector3 b)
        {
            //var points = Plot(x0, y0, z0, x1, y1, z1);
            //var points = Plot3dOrtho(x0, y0, z0, x1, y1, z1);
            var points = Plot3dThick(x0, y0, z0, x1, y1, z1);

            foreach (var p in points)
            {
                // if blocked return false
                if (check(p))
                    return false;
            }
            return true;
        }

        static public List<Vector2> Plot(int x0, int y0, int x1, int y1)
        {
            var points = new List<Vector2>();

            var dx = x1 - x0;
            var dy = y1 - y0;
            int sx, sy; //steps

            if (dy < 0)
            {
                dy = -dy;
                sy = -1;
            }
            else
                sy = 1;

            if (dx < 0)
            {
                dx = -dx;
                sx = -1;
            }
            else
                sx = 1;

            int f = 0;
            int x = x0, y = y0;
            if (dx >= dy)
            {
                while (x != x1) // doesn't add last point?
                {
                    var p = new Vector2(x, y);
                    points.Add(p);

                    f += dy;
                    if (f > dx)
                    {
                        y += sy;
                        f -= dx;
                    }
                    x += sx;
                }
            }
            else
            {
                while (y != y1)
                {
                    var p = new Vector2(x, y);
                    points.Add(p);

                    f += dx;
                    if (f > dy)
                    {
                        x += sx;
                        f -= dy;
                    }
                    y += sy;
                }
            }

            //var dx = b.X - a.X;
            //var dy = b.Y - a.Y;
            //var adx = Math.Abs(dx);
            //var ady = Math.Abs(dy);
            //float error = 0;
            //if(adx > ady)
            //{

            //}

            return points;
        }
        static public List<Vector3> Plot(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            var points = new List<Vector3>();

            var dx = x1 - x0;
            var dy = y1 - y0;
            var dz = z1 - z0;
            int sx, sy, sz; //steps

            if (dy < 0) { dy = -dy; sy = -1; }
            else sy = 1;

            if (dx < 0) { dx = -dx; sx = -1; }
            else sx = 1;

            if (dz < 0) { dz = -dz; sz = -1; }
            else sz = 1;

            int f = 0, g = 0;
            int x = x0, y = y0, z = z0;
            var max = Math.Max(dx, Math.Max(dy, dz));

            points.Add(new Vector3(x0, y0, z0));

            if (max == dx)
            {
                while (x != x1)
                {
                    f += dy;
                    if (f > dx)
                    {
                        y += sy;
                        f -= dx;
                        points.Add(new Vector3(x, y, z));
                    }

                    g += dz;
                    if (g > dx)
                    {
                        z += sz;
                        g -= dx;
                        points.Add(new Vector3(x, y, z));
                    }

                    x += sx;
                    points.Add(new Vector3(x, y, z));

                }
            }
            else if (max == dy)
            {
                while (y != y1)
                {
                    f += dx;
                    if (f > dy)
                    {
                        x += sx;
                        f -= dy;
                        points.Add(new Vector3(x, y, z));
                    }

                    g += dz;
                    if (g > dy)
                    {
                        z += sz;
                        g -= dy;
                        points.Add(new Vector3(x, y, z));
                    }

                    y += sy;
                    points.Add(new Vector3(x, y, z));
                }
            }
            else if (max == dz)
            {
                while (z != z1)
                {
                    f += dx;
                    if (f > dz)
                    {
                        x += sx;
                        f -= dz;
                        points.Add(new Vector3(x, y, z));
                    }

                    g += dy;
                    if (g > dz)
                    {
                        y += sy;
                        g -= dz;
                        points.Add(new Vector3(x, y, z));
                    }

                    z += sz;
                    points.Add(new Vector3(x, y, z));
                }
            }

            return points;
        }

        static public List<Vector3> PlotOld(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            var points = new List<Vector3>();

            var dx = x1 - x0;
            var dy = y1 - y0;
            var dz = z1 - z0;
            int sx, sy, sz; //steps

            if (dy < 0)
            {
                dy = -dy;
                sy = -1;
            }
            else
                sy = 1;

            if (dx < 0)
            {
                dx = -dx;
                sx = -1;
            }
            else
                sx = 1;

            if (dz < 0)
            {
                dz = -dz;
                sz = -1;
            }
            else
                sz = 1;

            //int f = 0;
            //int x = x0, y = y0;
            //if (dx >= dy)
            //{
            //    while (x != x1) // doesn't add last point?
            //    {
            //        var p = new Vector2(x, y);
            //        points.Add(p);

            //        f += dy;
            //        if (f > dx)
            //        {
            //            y += sy;
            //            f -= dx;
            //        }
            //        x += sx;
            //    }
            //}
            //else
            //{
            //    while (y != y1)
            //    {
            //        var p = new Vector2(x, y);
            //        points.Add(p);

            //        f += dx;
            //        if (f > dy)
            //        {
            //            x += sx;
            //            f -= dy;
            //        }
            //        y += sy;
            //    }
            //}

            int f = 0, g = 0;
            int x = x0, y = y0, z = z0;
            var max = Math.Max(dx, Math.Max(dy, dz));
            if (max == dx)
            {
                while (x != x1) // doesn't add last point?
                {
                    var p = new Vector3(x, y, z);
                    points.Add(p);

                    f += dy;
                    if (f > dx)
                    {
                        y += sy;
                        f -= dx;
                        //points.Add(new Vector3(x, y, z));
                    }

                    g += dz;
                    if (g > dx)
                    {
                        z += sz;
                        g -= dx;
                        //points.Add(new Vector3(x, y, z));
                    }

                    x += sx;
                }
            }
            else if (max == dy)
            {
                while (y != y1)
                {
                    var p = new Vector3(x, y, z);
                    points.Add(p);

                    f += dx;
                    if (f > dy)
                    {
                        x += sx;
                        f -= dy;
                        //points.Add(new Vector3(x, y, z));
                    }

                    g += dz;
                    if (g > dy)
                    {
                        z += sz;
                        g -= dy;
                        //points.Add(new Vector3(x, y, z));
                    }

                    y += sy;
                }
            }
            else if (max == dz)
            {
                while (z != z1)
                {
                    var p = new Vector3(x, y, z);
                    points.Add(p);

                    f += dx;
                    if (f > dz)
                    {
                        x += sx;
                        f -= dz;
                        //points.Add(new Vector3(x, y, z));
                    }

                    g += dy;
                    if (g > dz)
                    {
                        y += sy;
                        g -= dz;
                        //points.Add(new Vector3(x, y, z));
                    }

                    z += sz;
                }
            }

            return points;
        }

        public void line(int x0, int y0, int x1, int y1, int value)
        {
            var points = new List<Vector2>();

            int xDist = Math.Abs(x1 - x0);
            int yDist = -Math.Abs(y1 - y0);
            int xStep = (x0 < x1 ? +1 : -1);
            int yStep = (y0 < y1 ? +1 : -1);
            int error = xDist + yDist;

            //plot(x0, y0, value);
            points.Add(new Vector2(x0, y0));

            while (x0 != x1 || y0 != y1)
            {
                if (2 * error > yDist)
                {
                    // horizontal step
                    error += yDist;
                    x0 += xStep;
                }

                if (2 * error < xDist)
                {
                    // vertical step
                    error += xDist;
                    y0 += yStep;
                }

                //plot(x0, y0, value);
                points.Add(new Vector2(x0, y0));

            }
        }

        public void lineNoDiag(int x0, int y0, int x1, int y1, int value)
        {
            var points = new List<Vector2>();

            int xDist = Math.Abs(x1 - x0);
            int yDist = -Math.Abs(y1 - y0);
            int xStep = (x0 < x1 ? +1 : -1);
            int yStep = (y0 < y1 ? +1 : -1);
            int error = xDist + yDist;

            //plot(x0, y0, value);
            points.Add(new Vector2(x0, y0));

            while (x0 != x1 || y0 != y1)
            {
                if (2 * error - yDist > xDist - 2 * error)
                {
                    // horizontal step
                    error += yDist;
                    x0 += xStep;
                }
                else
                {
                    // vertical step
                    error += xDist;
                    y0 += yStep;
                }

                //plot(x0, y0, value);
                points.Add(new Vector2(x0, y0));

            }
        }

        static List<Vector3> plotLine3d(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            var points = new List<Vector3>();
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int dz = Math.Abs(z1 - z0), sz = z0 < z1 ? 1 : -1;
            int dm = Math.Max(dx, Math.Max(dy, dz)), i = dm; /* maximum difference */
            x1 = y1 = z1 = dm / 2; /* error offset */
            for (; ; )
            {  /* loop */
                //setPixel(x0, y0, z0);
                points.Add(new Vector3(x0, y0, z0));

                if (i-- == 0) break;
                x1 -= dx; if (x1 < 0) { x1 += dm; x0 += sx; }
                y1 -= dy; if (y1 < 0) { y1 += dm; y0 += sy; }
                z1 -= dz; if (z1 < 0) { z1 += dm; z0 += sz; }
            }


            return points;
        }

        static List<Vector3> Plot3dOrtho(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            var points = new List<Vector3>();
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int dz = Math.Abs(z1 - z0), sz = z0 < z1 ? 1 : -1;
            float ex, ey, ez;
            int x = x0, y = y0, z = z0;
            points.Add(new Vector3(x, y, z));

            for (int ix = 0, iy = 0, iz = 0; ix < dx || iy < dy || iz < dz; )
            {
                ex = dx > 0 ? (.5f + ix) / dx : int.MaxValue;
                ey = dy > 0 ? (.5f + iy) / dy : int.MaxValue;
                ez = dz > 0 ? (.5f + iz) / dz : int.MaxValue;

                var emin = Math.Min(ex, Math.Min(ey, ez));
                if (emin == ex && emin == ey)
                {
                    points.Add(new Vector3(x + sx, y, z));
                    points.Add(new Vector3(x, y + sy, z));
                    points.Add(new Vector3(x + sx, y + sy, z));
                    x += sx;
                    ix++;
                    y += sy;
                    iy++;
                    continue;
                }
                else if (emin == ex)
                {
                    x += sx;
                    ix++;
                }
                else if (emin == ey)
                {
                    y += sy;
                    iy++;
                }
                else
                {
                    z += sz;
                    iz++;
                }
                points.Add(new Vector3(x, y, z));
            }
            return points;
        }
        static List<Vector3> Plot3dThick(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            var points = new List<Vector3>();

            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int dz = Math.Abs(z1 - z0), sz = z0 < z1 ? 1 : -1;
            int dm = Math.Max(dx, Math.Max(dy, dz)), i = dm; /* maximum difference */
            x1 = y1 = z1 = dm / 2; /* error offset */

            for (; ; )
            {  /* loop */
                points.Add(new Vector3(x0, y0, z0));
                if (i-- == 0) break;
                x1 -= dx;
                y1 -= dy;
                z1 -= dz;
                if (x1 < 0 && y1 < 0)
                {
                    if (dy > 0)
                        points.Add(new Vector3(x0, y0 + sy, z0));
                    if (dx > 0)
                        points.Add(new Vector3(x0 + sx, y0, z0));
                    x1 += dm;
                    x0 += sx;
                    y1 += dm;
                    y0 += sy;
                }
                else
                {
                    if (x1 < 0)
                    {
                        if (dy > 0)
                            points.Add(new Vector3(x0, y0 + sy, z0));
                        x1 += dm;
                        x0 += sx;
                    }
                    if (y1 < 0)
                    {
                        if (dx > 0)
                            points.Add(new Vector3(x0 + sx, y0, z0));
                        y1 += dm;
                        y0 += sy;
                    }
                }
                if (z1 < 0)
                {
                    z1 += dm;
                    z0 += sz;
                }
            }

            return points;
        }
        //static List<Vector3> Plot3dThickOld(int x0, int y0, int z0, int x1, int y1, int z1)
        //{
        //    var points = new List<Vector3>();

        //    int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        //    int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        //    int dz = Math.Abs(z1 - z0), sz = z0 < z1 ? 1 : -1;
        //    int dm = Math.Max(dx, Math.Max(dy, dz)), i = dm; /* maximum difference */
        //    x1 = y1 = z1 = dm / 2; /* error offset */

        //    for (; ; )
        //    {  /* loop */
        //        points.Add(new Vector3(x0, y0, z0));
        //        if (i-- == 0) break;
        //        x1 -= dx;
        //        if (x1 < 0)
        //        {
        //            if (dy > 0)
        //                points.Add(new Vector3(x0, y0 + sy, z0));
        //            x1 += dm;
        //            x0 += sx;
        //        }
        //        y1 -= dy;
        //        if (y1 < 0)
        //        {
        //            if (dx > 0)
        //                points.Add(new Vector3(x0 + sx, y0, z0));
        //            y1 += dm;
        //            y0 += sy;
        //        }
        //        z1 -= dz;
        //        if (z1 < 0)
        //        {
        //            z1 += dm;
        //            z0 += sz;
        //        }
        //    }

        //    return points;
        //}
    }
}
