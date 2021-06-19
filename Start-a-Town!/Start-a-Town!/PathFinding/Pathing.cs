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
    public class Pathing
    {
        //enum State { Stopped, Working, Finished }
        //public State State;

        const float CostStraight = 10, CostDiag = 14;
        

        
        PriorityQueue<float, Node> Open = new PriorityQueue<float, Node>();
        //List<Vector3> Closed = new List<Vector3>();
        List<Node> Closed = new List<Node>();

        Vector3 Goal;
        Dictionary<Vector3, Node> Nodes = new Dictionary<Vector3, Node>();

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

        static void ComputeCostTheta(Node current, Node next)
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
            //float cost = 0;
            //if (current.Global.X == next.Global.X || current.Global.Y == next.Global.Y)
            //    cost = CostStraight;
            //else
            //    cost = CostDiag;
            //return cost;
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
            return Line.LineOfSight((int)a.X, (int)a.Y, (int)a.Z, (int)b.X, (int)b.Y, (int)b.Z, p => map.IsSolid(p));
        }
    }
}
