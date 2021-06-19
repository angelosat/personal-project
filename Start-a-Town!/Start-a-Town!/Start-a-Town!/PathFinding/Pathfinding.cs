using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Threading;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_
{
    public class PathRequest
    {
        public Position Start, Goal;
        public float MinRange, MaxRange;
        public bool IgnoreTargetFootprint, AllowDifferentAltitudes;
        public Path Path;

        public PathRequest(Position start, Position goal, float minrange, float maxrange, bool ignoreftprnt = false, bool allowDifAlt = false)
        {
            Start = start;
            Goal = goal;
            MinRange = minrange;
            MaxRange = maxrange;
            IgnoreTargetFootprint = ignoreftprnt;
            AllowDifferentAltitudes = allowDifAlt;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public delegate void PathNotify(PathRequest pathRequest);


    public class Pathfinding
    {
        static Pathfinding _Instance;
        static Pathfinding Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Pathfinding();
                return _Instance;
            }
        }

        static public string Status
        //{ get { return "ASD"; } }
        { get { return Instance.Thread.ThreadState.ToString() + (Instance.Thread.ThreadState == ThreadState.Running ? " (" + Instance.Requests.Count + " Tasks)" : ""); } }
        static public void Stop()
        {
            Instance.Running = false;
        }
        Dictionary<PathRequest, Queue<PathNotify>> Callbacks;
        Queue<PathRequest> Requests;

        Pathfinding()
        {
            Requests = new Queue<PathRequest>();
            Callbacks = new Dictionary<PathRequest, Queue<PathNotify>>();
            Thread = new Thread(CalculatePaths);
            Thread.Name = "Pathfinder";
        }

        static public void Begin()
        {
            Instance.Running = true;
            if (Instance.Thread.ThreadState == ThreadState.Unstarted)
                Instance.Thread.Start();
            else if (Instance.Thread.ThreadState == ThreadState.WaitSleepJoin)
                Instance.Thread.Interrupt();
        }
        static public void End()
        {
            Instance.Running = false;
            //Instance.Thread.Abort();
            Instance.Thread.Interrupt();
        }

        static public void Request(PathRequest request, PathNotify callBack = null)
        {
            Instance.Requests.Enqueue(request);
            if (callBack != null)
            {
                //TODO pathrequests are now classes so find another way to check if a same pathrequest already exists
                Queue<PathNotify> otherRequests;
                if (Instance.Callbacks.TryGetValue(request, out otherRequests))
                    otherRequests.Enqueue(callBack);
                else
                    Instance.Callbacks.Add(request, new Queue<PathNotify>(new PathNotify[] { callBack }));
            }
            Begin();
        }

        Thread Thread;
        bool Running;
        int heuristic(Vector3 start, Vector3 end)
        {
            int h_diagonal = Math.Min(Math.Abs((int)(start.X - end.X)), Math.Abs((int)(start.Y - end.Y)));
            int h_straight = Math.Abs((int)(start.X - end.X)) + Math.Abs((int)(start.Y - end.Y));

            return (diagonal_cost * h_diagonal + straight_cost * (h_straight - 2 * h_diagonal) + vertical_cost * Math.Abs((int)(start.Z - end.Z)));
        }
        int cost(Vector3 start, Vector3 end)
        {
            if ((start.X - end.X) * (start.Y - end.Y) == 0)
                return straight_cost;

            return diagonal_cost;
        }
        Dictionary<Position, Position> parent;
        Dictionary<Position, float> g;
        Dictionary<Position, float> h;


        void CalculatePaths()
        {
            Object thisLock = new Object();
            lock (thisLock)
            {
                do
                {
                    while (Requests.Count > 0)
                    {
                        PathRequest request = Requests.Dequeue();
                        Path path = GetPath(request);
                        //TODO instead of calling back with a bool result, 
                        //attach an empty path to the request and let caller handle it
                        request.Path = path;
                        Queue<PathNotify> callBacks;
                        if (Callbacks.TryGetValue(request, out callBacks))
                        {
                            callBacks.Dequeue()(request);
                            Callbacks.Remove(request);
                        }
                    }
                    try { Thread.Sleep(Timeout.Infinite); }
                    catch (ThreadInterruptedException e) { }
                } while (Running);
            }
        }
        private const int straight_cost = 10, diagonal_cost = 14, vertical_cost = 1;
        Path GetPath(PathRequest pathRequest)
        {
            double d;
            float check_g;
            bool better;

            int fail = 0;

            List<Position> closedSet = new List<Position>();
            List<Position> openSet = new List<Position>();

            parent = new Dictionary<Position, Position>();
            g = new Dictionary<Position, float>();
            h = new Dictionary<Position, float>();
            PriorityQueue<double, Position> f = new PriorityQueue<double, Position>();

            Position start = new Position(pathRequest.Start);
            Position goal = new Position(pathRequest.Goal);
            //start.Global = new Vector3((float)Math.Round(start.Global.X), (float)Math.Round(start.Global.Y), (float)Math.Round(start.Global.Z));
            start.Round();
            //start.Global.Z += 1;

            //goal.Global.Z += 1;
            goal.Global += Vector3.Backward;
            bool ignoreftprnt = pathRequest.IgnoreTargetFootprint, allowDifAlt = pathRequest.AllowDifferentAltitudes;

            Position current = start, check;

            openSet.Add(start);

            g.Add(start, 0);
            h.Add(start, heuristic(start.Global, goal.Global));

            f.Enqueue(heuristic(start.Global, goal.Global), start);
            Color random = new Color(Engine.Map.World.Random.Next(255), Engine.Map.World.Random.Next(255), Engine.Map.World.Random.Next(255));


            while (openSet.Count > 0)
            {
                current = f.Dequeue();

                //Cell cell = Map.Instance.GetCellAt(current);
                //Chunk chunk = Map.Instance.GetChunkAt(current);

                if (!ignoreftprnt)
                {
                    if (Vector2.Distance(new Vector2(current.Global.X, current.Global.Y), new Vector2(goal.Global.X, goal.Global.Y)) < 2)//maxrange)
                        if (allowDifAlt)
                        {
                            if (goal.Global.Z >= current.Global.Z)// - 1)

                                if (goal.Global.Z <= current.Global.Z + 5)//4) //6)
                                {
                                    if (current.IsWalkable)
                                        return constructPath(current, start);
                                }
                        }

                }


                if (current.Global == goal.Global)
                    return constructPath(current, start);

                openSet.Remove(current);
                closedSet.Add(current);

                //check surrounding nodes
                for (int z = (int)(current.Global.Z - 1); z <= current.Global.Z + 1; z++)
                //for (int z = current.Z - 1; z <= current.Z + 5; z++)
                {
                    if (z == Map.MaxHeight)
                        continue;
                    for (int dir = 0; dir < 8; dir++)
                    {
                        d = dir * (Math.PI / 4);

                        //check = Map.Instance.GetCellAt((int)current.X + (int)Math.Round(Math.Cos(d)), (int)current.Y - (int)Math.Round(Math.Sin(d)), z);

                        //check = new Vector3((int)current.X + (int)Math.Round(Math.Cos(d)), (int)current.Y - (int)Math.Round(Math.Sin(d)), z);
                        //cell = Map.Instance.GetCellAt(check);
                        check = new Position(Engine.Map, new Vector3(current.Global.X + (int)Math.Round(Math.Cos(d)), current.Global.Y - (int)Math.Round(Math.Sin(d)), z));

                        //if (check != null)
                        //{
                        //if (check.Tile != null)
                        //    check.Tile.Blend = random;
                        //if (cell != null)
                        //    cell.Skylight = 5;

                        

                        if (closedSet.Contains(check))
                        {
                            //Console.WriteLine("continuing");
                            continue;
                        }

                        //if (check.IsWalkable || check == goal)
                        if (check.IsWalkable)
                        {


                            g.TryGetValue(check, out check_g);
                            check_g += cost(current.Global, check.Global);

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
                                h[check] = heuristic(check.Global, goal.Global);
                                f.Enqueue(check_g + h[check], check);
                            }
                            //}
                            //else if (check == goal)
                            //    return constructPath(current, start);
                        }

                        fail++;
                        if (fail >= 5000)
                        {
                            Console.WriteLine(fail);
                            //return null;
                            return Path.Empty;
                        }//if (Vector3.Distance(check.MapCoords, goal.MapCoords) > minrange)
                    }
                }
            }
            return Path.Empty;
        }

       

        private Path constructPath(Position current, Position start)
        {
            Path path = new Path();
            //Console.WriteLine("PATH:");
            while (current != start)
            {
                //Console.WriteLine(current);
                path.Add(current);
                current = parent[current];
            }
            path.Add(start);
            //Console.WriteLine(start);
            //Console.WriteLine("END PATH");
            // calculate directions
            // to improve
            for (int i = 1; i < path.Positions.Count; i++)
            {
                Vector2 dir = new Vector2(path.Positions[i].Global.X - path.Positions[i - 1].Global.X, path.Positions[i].Global.Y - path.Positions[i - 1].Global.Y);
                path.Directions.Add(dir);
                //Console.WriteLine(dir);
            }

            return path;
        }


    }
}
