using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static class VectorHelper
    {
        
        static readonly List<Vector3> AdjacentRadial = Vector3.Zero.GetRadial(2);
        static readonly IntVec3[] AdjacentRadialLarge = IntVec3.Zero.GetRadial(Chunk.Size);
        static public IntVec3[] GetRadial(this IntVec3 center, int radius)
        {
            var r = Vector3.One * radius;
            var box = new BoundingBox((Vector3)center - r, (Vector3)center + r).GetBox();
            box.Sort((a, b) =>
            {
                float aa = a.LengthSquared();
                float bb = b.LengthSquared();
                if (aa < bb)
                    return -1;
                else if (aa == bb)
                    return 0;
                else
                    return 1;
            });
            return box.Select(v => (IntVec3)v).ToArray();
        }
        static public List<Vector3> GetRadial(this Vector3 center, int radius)
        {
            var r = Vector3.One * radius;
            var box = new BoundingBox(center - r, center + r).GetBox();
            box.Sort((a, b) =>
            {
                float aa = a.LengthSquared();
                float bb = b.LengthSquared();
                if (aa < bb)
                    return -1;
                else if (aa == bb)
                    return 0;
                else
                    return 1;
            });
            return box;
        }
        static public IEnumerable<Vector3> GetRadial(this Vector3 center)
        {
            foreach (var n in AdjacentRadial)
                yield return center + n;
        }
        static public IEnumerable<IntVec3> GetRadialLarge(this IntVec3 center)
        {
            foreach (var n in AdjacentRadialLarge)
                yield return center + n;
        }
        static public void GetMinMaxVector3(Vector3 vec1, Vector3 vec2, out Vector3 min, out Vector3 max)
        {
            var xmin = Math.Min(vec1.X, vec2.X);
            var ymin = Math.Min(vec1.Y, vec2.Y);
            var zmin = Math.Min(vec1.Z, vec2.Z);
            var xmax = vec1.X + vec2.X - xmin;
            var ymax = vec1.Y + vec2.Y - ymin;
            var zmax = vec1.Z + vec2.Z - zmin;
            min = new Vector3(xmin, ymin, zmin);
            max = new Vector3(xmax, ymax, zmax);
        }
        static public void GetMinMaxVector2(Vector2 vec1, Vector2 vec2, out Vector2 min, out Vector2 max)
        {
            var xmin = Math.Min(vec1.X, vec2.X);
            var ymin = Math.Min(vec1.Y, vec2.Y);
            var xmax = vec1.X + vec2.X - xmin;
            var ymax = vec1.Y + vec2.Y - ymin;
            min = new Vector2(xmin, ymin);
            max = new Vector2(xmax, ymax);
        }

        static public readonly Vector3[] Adjacent = new Vector3[]{
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };
        static public readonly IntVec3[] AdjacentIntVec3 = new IntVec3[]{
            new IntVec3(1, 0, 0),
            new IntVec3(-1, 0, 0),
            new IntVec3(0, 1, 0),
            new IntVec3(0, -1, 0),
            new IntVec3(0, 0, 1),
            new IntVec3(0, 0, -1)
        };
        static public readonly Vector3[] AdjacentXY = new Vector3[]{
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0)
        };
        static public readonly IntVec3[] AdjacentXYIntVec3 = new IntVec3[]{
            new IntVec3(1, 0, 0),
            new IntVec3(-1, 0, 0),
            new IntVec3(0, 1, 0),
            new IntVec3(0, -1, 0)
        };
        static public readonly Vector3[] Column3 = new Vector3[]{
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(0, 0, -1)
        };
        static public Vector3[] GetAdjacent(this Vector3 center)
        {
            var array = new Vector3[6];
            for (int i = 0; i < Adjacent.Length; i++)
            {
                array[i] = center + Adjacent[i];
            }
            return array;
        }
        static public IEnumerable<Vector3> GetAdjacentLazy(this Vector3 global)
        {
            yield return (global + new Vector3(1, 0, 0));
            yield return (global - new Vector3(1, 0, 0));
            yield return (global + new Vector3(0, 1, 0));
            yield return (global - new Vector3(0, 1, 0));
            yield return (global + new Vector3(0, 0, 1));
            yield return (global - new Vector3(0, 0, 1));
        }
        static public IEnumerable<IntVec3> GetAdjacentLazy(this IntVec3 global)
        {
            for (int i = 0; i < 6; i++)
                yield return global + AdjacentIntVec3[i];
            //yield return (global + new IntVec3(1, 0, 0));
            //yield return (global - new IntVec3(1, 0, 0));
            //yield return (global + new IntVec3(0, 1, 0));
            //yield return (global - new IntVec3(0, 1, 0));
            //yield return (global + new IntVec3(0, 0, 1));
            //yield return (global - new IntVec3(0, 0, 1));
        }
        static public IEnumerable<IntVec3> GetAdjacentHorLazy(this IntVec3 global)
        {
            for (int i = 0; i < 4; i++)
                yield return global + AdjacentXYIntVec3[i];
        }
    }
}
