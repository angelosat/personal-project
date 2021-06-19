using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    static public class MapHelper
    {
        static public IEnumerable<Vector3> FloodFill(this IMap map, Vector3 begin)
        {
            var discovered = new HashSet<Vector3>();
            var current = begin;
            var cell = map.GetCell(current);
            discovered.Add(current);
            if (cell.IsRoomBorder)
                throw new Exception(); //return;
            var tohandle = new Queue<Vector3>();
            tohandle.Enqueue(current);
            yield return current;
            while (tohandle.Any())
            {
                current = tohandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (discovered.Contains(n))
                        continue;
                    if (map.TryGetCell(n, out var ncell))
                    {
                        yield return n;
                        if (!ncell.IsRoomBorder)
                        {
                            tohandle.Enqueue(n);
                            discovered.Add(n);
                        }
                    }
                    else
                    {

                    }
                }
            }
        }
    }
}
