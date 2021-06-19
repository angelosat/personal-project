using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Pathfinding;

namespace Start_a_Town_
{
    public static class Pathfinder
    {
        private const int straight_cost = 10, diagonal_cost = 14;
        //private static int h, g;

        private static int heuristic(Vector2 start, Vector2 end)
        {
            int h_diagonal = Math.Min(Math.Abs((int)(start.X-end.X)), Math.Abs((int)(start.Y-end.Y)));
            int h_straight = Math.Abs((int)(start.X-end.X)) + Math.Abs((int)(start.Y-end.Y));
            return (diagonal_cost * h_diagonal + straight_cost * (h_straight - 2 * h_diagonal));
        }
        private static int cost(Vector2 start, Vector2 end)
        {
            if ((start.X - end.X) * (start.Y - end.Y) == 0)
                return straight_cost;

            return diagonal_cost;
        }
        //private static PFTile check;

        private static Dictionary<Vector2, Vector2> parent;
        private static Dictionary<Vector2, float> g;
        private static Dictionary<Vector2, float> h;

        public static Stack<Vector2> getPath(Vector2 start, Vector2 goal)
        {
            //Console.WriteLine("start of path calculation");
            
            // cPFTileurrent = start;
            Vector2 current = start, check;
            double d;
            float check_g;
            bool better;

            List<Vector2> closedSet = new List<Vector2>();
            List<Vector2> openSet = new List<Vector2>();

            parent = new Dictionary<Vector2, Vector2>();
            g = new Dictionary<Vector2, float>();
            h = new Dictionary<Vector2, float>();
            PriorityQueue<double, Vector2> f = new PriorityQueue<double, Vector2>();

            //PFTile current = new PFTile(start);
            openSet.Add(start);
            //openSet.Add(current);
            g.Add(start, 0);
            h.Add(start, heuristic(start, goal));
            //current.g = 0;
            //current.h = heuristic(start, goal);
            f.Enqueue(heuristic(start, goal), start);

            while (openSet.Count > 0)
            {
                current = f.Dequeue();
                if (current == goal)
                    return constructPath(current, start);

                //Console.WriteLine("current != goal, continuing");
                openSet.Remove(current);
                closedSet.Add(current);

                //check surrounding nodes
                for (int dir = 0; dir < 8; dir++)
                {
                    d = dir * (Math.PI / 4);
                    check = new Vector2(current.X + (int)Math.Round(Math.Cos(d)), current.Y - (int)Math.Round(Math.Sin(d)));
                    if (closedSet.Contains(check))
                        continue;

                    g.TryGetValue(check, out check_g);
                    check_g += cost(current, check);

                    if (!openSet.Contains(check))
                    {
                        openSet.Add(check);
                        better = true;
                    }
                    else if (check_g < g[check])
                    {
                        better = true;
                    }
                    else
                    {
                        better = false;
                    }

                    if (better)
                    {
                        parent[check] = current;
                        g[check] = check_g;
                        h[check] = heuristic(check, goal);
                        //f[check] = check_g + h[check];
                        f.Enqueue(check_g + h[check], check);
                    }
                }  
            }
            

            Console.WriteLine("no path");
            //return path;
            return null;// new Stack<Vector2>();
        }

        private static Stack<Vector2> constructPath(Vector2 current, Vector2 start)
        {
            Stack<Vector2> path = new Stack<Vector2>();
            //do
            //{
            //    //Console.WriteLine("constructing path");
            //    path.Push(current);
            //    current = parent[current];
            //} while (current != start);
            while (current != start)
            {
                //Console.WriteLine("constructing path");
                path.Push(current);
                current = parent[current];
            } 
            //Console.WriteLine("path length: " + path.Count.ToString());
            return path;
        }
    }
}
