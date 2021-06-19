using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static public class FloodFill
    {
        static public IEnumerable<IntVec3> BeginIncludeEdges(IMap map, IntVec3 begin, Func<Cell, IntVec3, bool> condition)
        {
            var cell = map.GetCell(begin);
            if (!condition(cell, begin))
                throw new Exception();
            yield return begin;

            Queue<IntVec3> toHandle = new();
            HashSet<IntVec3> handled = new() { begin };
            toHandle.Enqueue(begin);
            while (toHandle.Any())
            {
                var current = toHandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.TryGetCell(n, out var ncell))
                        continue;
                    yield return n;
                    if (condition(ncell, n))
                        toHandle.Enqueue(n);
                }
            }
        }
        static public IEnumerable<IntVec3> BeginExcludeEdges(IMap map, IntVec3 begin, Func<Cell, IntVec3, bool> condition)
        {
            var cell = map.GetCell(begin);
            if (!condition(cell, begin))
                throw new Exception();
            yield return begin;

            Queue<IntVec3> toHandle = new();
            HashSet<IntVec3> handled = new() { begin };
            toHandle.Enqueue(begin);
            while(toHandle.Any())
            {
                var current = toHandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!map.TryGetCell(n, out var ncell))
                        continue;
                    if (condition(ncell, n))
                    {
                        yield return n;
                        toHandle.Enqueue(n);
                    }
                }
            }
        }
    }
}
