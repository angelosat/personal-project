using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PathFinding
{
    public class PathingWORKING
    {
        public enum States { Stopped, Working, Finished }//, Failed }
        public States State;

        static Dictionary<HashSet<Vector3>, int> InaccessibleAreas = new Dictionary<HashSet<Vector3>, int>();
        readonly static int InaccessibleAreasForgetTimer = Engine.TargetFps * 60; //60 seconds

        const float CostStraight = 10, CostDiag = 14;
        //public class Path
        //{
        //    public Stack<Vector3> Stack = new Stack<Vector3>();
        //    public void Build(Node node)
        //    {
        //        this.Stack = new Stack<Vector3>();
        //        var current = node;
        //        while (current.Parent != null)
        //        {
        //            this.Stack.Push(current.Global);
        //            current = current.Parent;
        //        }
        //    }

        //    public override string ToString()
        //    {
        //        var text = "";
        //        foreach (var item in this.Stack)
        //            text += item.ToString() + "\n";
        //        return text.TrimEnd('\n');
        //    }
        //}

        //public class Node
        //{
        //    public IMap Map;
        //    public Vector3 Global;
        //    public Node Parent;
        //    //public float Cost;
        //    //bool CostCalculated = false;
        //    //float _Cost;
        //    public float CostToGoal, CostFromStart;
        //    //{ get{
        //    //    if(!this.CostCalculated
        //    //    this._Cost = Heuristic(Global}
        //    public Node(IMap map, Vector3 global, Vector3 goal)
        //    {
        //        this.Map = map;
        //        this.Global = global;
        //        this.CostToGoal = Heuristic(global, goal);
        //    }
        //    //public float GetHeuristic(Vector3 goal)
        //    //{
        //    //                    //return Heuristic(this.Global, goal);
        //    //    if(!this.CostCalculated;
        //    //    this.Cost = Heuristic(this.Global}

        //    //}
        //    public override string ToString()
        //    {
        //        return this.Global.ToString() + " from " + (this.Parent != null ? this.Parent.Global.ToString() : "null");
        //    }
        //}

        PriorityQueue<float, Node> Open = new PriorityQueue<float, Node>();
        //List<Vector3> Closed = new List<Vector3>();
        List<Node> Closed = new List<Node>();

        Vector3 Goal;
        Dictionary<Vector3, Node> Nodes = new Dictionary<Vector3, Node>();

        public Vector3 Start, Finish, FinishPrecise;
        Path PathInProgress;
        IMap Map;
        public Path GetPath()
        {
            if (this.State != States.Finished)
                throw new Exception();
            this.State = States.Stopped;
            return this.PathInProgress;
        }
        public void Begin(IMap map, Vector3 start, Vector3 finish)
        {
            this.Start = start.Round();


            this.FinishPrecise = finish;
            this.Finish = finish.Round();

            if (IsInaccessible(map, this.Finish))
            {
                Fail();
                return;
            }

            this.Goal = this.Finish;
            this.Map = map;

            this.State = PathingWORKING.States.Working;
            this.PathInProgress = new Path();
            var startNode = new Node(this.Map, this.Start, this.Finish, Heuristic);
            var finishNode = new Node(this.Map, this.Finish, this.Finish, Heuristic);
            this.Nodes.Clear();
            this.Open.Clear();
            this.Closed.Clear();
            this.Nodes.Add(this.Start, startNode);
            this.Nodes[this.Finish] = finishNode;
            this.Open.Enqueue(startNode.CostToGoal, startNode);
        }

        private void Fail()
        {
            this.State = States.Finished;
            this.PathInProgress = null;
        }
        public void Work()
        {
            if (this.State != States.Working)
                return;

            //if (this.State == PathingSync.States.Stopped)
            //{
            //    this.State = PathingSync.States.Working;
            //    this.PathInProgress = new Path();
            //    var startNode = new Node(this.Map, this.Start, this.Finish);
            //    var finishNode = new Node(this.Map, this.Finish, this.Finish);
            //    this.Nodes.Add(this.Start, startNode);
            //    this.Nodes.Add(this.Finish, finishNode);
            //    this.Open.Enqueue(startNode.CostToGoal, startNode);
            //}
            //var i = 0; 
            //while (this.Open.Count > 0 || i++ < Engine.TargetFps)
            //for (int i = 0; i < Engine.TargetFps; i++)
            //{

            if (this.Open.Count > 0)
            {
                var current = this.Open.Dequeue();

                // TODO: handle the case where the target block is solid in a better way than this. maybe receive required minimum target range as an argument when starting the pathfinding?
                if (Math.Abs(current.Global.X - this.Finish.X) <= 1 && Math.Abs(current.Global.Y - this.Finish.Y) <= 1)
                //if (current.Global == this.Finish)
                {
                    // TODO: maybe replace vector3 in each path step with a targetArg, so if the final target is an entity, 
                    // we add the entity as a target instead of its global at the time of path calculation
                    current.Global = this.FinishPrecise;
                    this.PathInProgress.Build(current);
                    // replace rounded finish node with original precise node
                    //if (this.PathInProgress.Stack.Count > 0) // if the target was right next to the start
                    //this.PathInProgress.Stack.Pop();
                    if (this.PathInProgress.Stack.Count == 0) // if the target was right next to the start
                        this.PathInProgress.Stack.Push(this.FinishPrecise);

                    //var steplist = this.PathInProgress.Stack.ToList();
                    //steplist[steplist.Count - 1] = this.FinishPrecise;
                    //steplist.Reverse();
                    //this.PathInProgress.Stack = new Stack<Vector3>(steplist);

                    this.State = States.Finished;
                    return;
                }

                this.Closed.Add(current);
                var neighbors = current.Global.GetNeighborsDiag();
                foreach (var n in neighbors) // put visibility check here
                {
                    Node nnode;
                    if (!this.Nodes.TryGetValue(n, out nnode))
                    {
                        nnode = new Node(this.Map, n, this.Finish, Heuristic);
                        this.Nodes[n] = nnode;
                    }
                    //if (!this.Closed.Contains(n))
                    if (!this.Closed.Any(nn => nn.Global == n))
                    {
                        //if(!open.Contains(n))
                        if (!this.Open.Any(node => node.Global == n))
                        {
                            nnode.CostFromStart = float.MaxValue;// -1;
                            nnode.Parent = null;
                        }
                        //Update(current, n);
                        Update(this.Map, current, nnode);
                    }
                }
            }

            else
            {
                // no path found
                HandleInaccessibleBlock(this.Map, this.Finish);
                Fail();
                return;
            }

        }

        public Path GetPath(IMap map, Vector3 start, Vector3 finish)
        {
            var watch = Stopwatch.StartNew();
            var finishPrecise = finish;
            Path path = new Path();
            this.Goal = finish;
            start = start.Round();
            finish = finish.Round();
            var startNode = new Node(map, start, finish, Heuristic) { };
            var finishNode = new Node(map, finish, finish, Heuristic);

            this.Nodes.Add(start, startNode);
            this.Nodes[finish] = finishNode;

            //var open = new PriorityQueue<float, Node>();
            ////var open = new PriorityQueue<float, Vector3>();
            //var closed = new List<Vector3>();

            this.Open.Enqueue(startNode.CostToGoal, startNode);
            //open.Enqueue(Heuristic(start, finish), start);
            while (this.Open.Count > 0)
            {
                var current = this.Open.Dequeue();
                if (current.Global == finish)
                //if (current == finish)
                {
                    path.Build(current);
                    watch.Stop();
                    Console.WriteLine("path found in " + watch.ElapsedMilliseconds.ToString() + "ms");

                    // replace rounded finish node with original precise node
                    throw new Exception();
                    // TODO: WRONG, I'm REPLACING THE FIRST STEP INSTEAD OF THE LAST
                    path.Stack.Pop();
                    path.Stack.Push(finishPrecise);

                    return path;
                }
                //this.Closed.Add(current.Global);
                this.Closed.Add(current);
                //if (!IsWalkable(map, current.Global))
                //    continue;
                var neighbors = current.Global.GetNeighborsDiag();
                foreach (var n in neighbors) // put visibility check here
                //foreach (var n in current.GetNeighborsDiag())
                {
                    Node nnode;
                    if (!this.Nodes.TryGetValue(n, out nnode))
                    {
                        nnode = new Node(map, n, finish, Heuristic);
                        this.Nodes[n] = nnode;
                    }
                    //if (!this.Closed.Contains(n))
                    if (!this.Closed.Any(nn => nn.Global == n))
                    {
                        //if(!open.Contains(n))
                        if (!this.Open.Any(node => node.Global == n))
                        {
                            nnode.CostFromStart = float.MaxValue;// -1;
                            nnode.Parent = null;
                        }
                        //Update(current, n);
                        Update(map, current, nnode);
                    }
                }
            }

            return null;
        }

        static float Heuristic(Vector3 start, Vector3 finish)
        {
            var dx = (float)Math.Abs(finish.X - start.X);
            var dy = (float)Math.Abs(finish.Y - start.Y);
            var dz = (float)Math.Abs(finish.Z - start.Z);

            //return dx * CostStraight + dy * CostDiag + dz * 100;
            if (dx > dy)
                return (dx - dy) * CostStraight + dy * CostDiag + dz * 100;
            else if (dy > dx)
                return (dy - dx) * CostStraight + dx * CostDiag + dz * 100;
            else
                return dx * CostDiag + dz * 100;
        }

        void Update(IMap map, Node current, Node next)
        {
            //if (!IsWalkable(map, current.Global))
            if (!IsValidMovement(map, current.Global, next.Global))
                return;
            var costOld = next.CostFromStart;
            //ComputeCost(current, next);
            ComputeCostTheta(current, next);

            if (next.CostFromStart < costOld)
            {
                //if (this.Open.Any(n => n.Global == next.Global))
                //{
                //}
                this.Open.Remove(next);
                this.Open.Enqueue(next.CostFromStart + Heuristic(next.Global, this.Goal), next);
            }
        }

        static void ComputeCost(Node current, Node next)
        {
            //float cost = 0;
            //if (current.Global.X == next.Global.X || current.Global.Y == next.Global.Y)
            //    cost = CostStraight;
            //else
            //    cost = CostDiag;
            var cost = GetCost(current, next);
            var costCheck = current.CostFromStart + cost;
            if (costCheck < next.CostFromStart)
            {
                next.Parent = current;
                next.CostFromStart = costCheck;
            }
        }

        static void ComputeCostTheta(NodeBase current, NodeBase next)
        {
            var los = current.Parent != null ? LineOfSight(current.Map, current.Parent.Global, next.Global) : false;
            float cost, costCheck;
            if (los)
            {
                cost = GetCost(current.Parent, next);
                costCheck = current.Parent.CostFromStart + cost;
                if (costCheck < next.CostFromStart)
                {
                    next.Parent = current.Parent;
                    next.CostFromStart = costCheck;
                }
            }
            else
            {
                cost = GetCost(current, next);
                costCheck = current.CostFromStart + cost;
                if (costCheck < next.CostFromStart)
                {
                    next.Parent = current;
                    next.CostFromStart = costCheck;
                }
            }
        }

        static float GetCost(NodeBase current, NodeBase next)
        {
            return GetCostEuclidean(current, next);
            float cost = 0;
            if (current.Global.X == next.Global.X || current.Global.Y == next.Global.Y)
                cost = CostStraight;
            else
                cost = CostDiag;
            return cost;
        }
        static float GetCostEuclidean(NodeBase current, NodeBase next)
        {
            return Vector3.DistanceSquared(current.Global, next.Global);
        }
        static bool IsWalkable(IMap map, Vector3 global)
        {
            if (map.IsSolid(global))
                return false;
            if (map.IsSolid(global + Vector3.UnitZ)) // replace this with height check
                return false;
            if (!map.IsSolid(global - Vector3.UnitZ))
                return false;
            return true;
        }
        static bool CanWalkOnBlock(IMap map, Vector3 global)
        {
            if (!map.IsSolid(global))
                return false;
            if (map.IsSolid(global + Vector3.UnitZ))
                return false;
            if (map.IsSolid(global + Vector3.UnitZ + Vector3.UnitZ)) // replace this with height check
                return false;

            return true;
        }
        static bool IsDiagonal(Vector3 a, Vector3 b)
        {
            return !(a.X == b.X || a.Y == b.Y);
        }
        static bool IsValidMovement(IMap map, Vector3 a, Vector3 b)
        {
            if (!IsWalkable(map, b))
                return false;
            if (IsDiagonal(a, b))
            {
                var z = Math.Max(a.Z, b.Z);// which z to use?
                var d1 = new Vector3(a.X, b.Y, z);
                var d2 = new Vector3(b.X, a.Y, z);
                if (!IsWalkable(map, d1))
                    return false;
                if (!IsWalkable(map, d2))
                    return false;
            }
            return true;
        }

        static bool LineOfSight(IMap map, Vector3 a, Vector3 b)
        {
            return Line.LineOfSight((int)a.X, (int)a.Y, (int)a.Z, (int)b.X, (int)b.Y, (int)b.Z, p => LosFailCondition(map, p)); //map.IsSolid(p
        }

        private static bool LosFailCondition(IMap map, Vector3 p)
        {
            var above = p + Vector3.UnitZ;
            var below = p - Vector3.UnitZ;
            var solidbelow = map.IsSolid(below);
            if (!solidbelow)
                return true;
            var solidabove = map.IsSolid(above);
            var solidcurrent = map.IsSolid(p);
            return solidabove & solidcurrent;
        }

        public static HashSet<Vector3> FloodFill(IMap map, Vector3 global)
        {
            Queue<Vector3> open = new Queue<Vector3>();
            HashSet<Vector3> closed = new HashSet<Vector3>();
            HashSet<Vector3> area = new HashSet<Vector3>();
            open.Enqueue(global);
            while (open.Count > 0)
            {
                var current = open.Dequeue();
                closed.Add(current);

                var neighbors = current.GetNeighborsDiag();
                foreach (var n in neighbors)
                {
                    if (!CanWalkOnBlock(map, n))
                        continue;
                    area.Add(n);
                    if (!closed.Any(nn => n == nn))
                        if (!open.Any(nn => n == nn))
                            open.Enqueue(n);
                }
            }
            return area;
        }

        public static void HandleInaccessibleBlock(IMap map, Vector3 global)
        {
            if (IsInaccessible(map, global))
                return;
            var area = FloodFill(map, global);
            InaccessibleAreas.Add(area, InaccessibleAreasForgetTimer);
        }

        public static bool IsInaccessible(IMap map, Vector3 global)
        {
            foreach (var existingArea in InaccessibleAreas.Keys)
                if (existingArea.Contains(global))
                    return true; // maybe refresh timer?
            return false;
        }

        public static void UpdateInaccessibleAreas()
        {
            var newAreas = new Dictionary<HashSet<Vector3>, int>();
            foreach (var area in InaccessibleAreas)
            {
                var timer = area.Value;
                timer--;
                if (timer > 0)
                    newAreas.Add(area.Key, timer);
            }
            InaccessibleAreas = newAreas;
        }
    }
}
