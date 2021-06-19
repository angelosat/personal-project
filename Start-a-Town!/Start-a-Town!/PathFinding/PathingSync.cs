using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_
{
    public class PathingSync
    {
        public enum States { Stopped, Working, Finished }//, Failed }
        public States State;

        static Dictionary<HashSet<Vector3>, int> InaccessibleAreas = new();
        readonly static int InaccessibleAreasForgetTimer = Engine.TicksPerSecond * 60; //60 seconds

        const float CostStraight = 10, CostDiag = 14;
        public int Ticks = 0;
        

        public class Node : NodeBase
        {
            //public IMap Map;
            public RegionNode RegionNodeGlobal;//, RegionNodeGoal;
            public bool IsQueued;
            public Node(IMap map, Vector3 global, Vector3 goal)
            {
                this.Map = map;
                this.Global = global;
            }
            public override string ToString()
            {
                return this.Global.ToString() + " from " + (this.Parent != null ? this.Parent.Global.ToString() : "null");
            }
        }

        readonly PriorityQueue<float, Node> Open = new();
        readonly List<Node> Closed = new();
        readonly HashSet<Vector3> Handled = new();

        Vector3 Goal;
        readonly Dictionary<Vector3, Node> CachedNodes = new();
        public float Range;

        public Vector3 Start, Finish, FinishPrecise;
        public TargetArgs FinishTarget;
        Path PathInProgress;
        IMap Map;
        Actor Actor;
        static void ConformPathToTerrain(IMap map, Path path)
        {
            return;
            //var newstack = new Stack<Vector3>();
            //foreach(var step in path.Stack.Reverse())
            //{
            //    var blockHeight = Block.GetBlockHeight(map, step - Vector3.UnitZ);
            //    newstack.Push(new Vector3(step.X, step.Y, step.Z + blockHeight - 1));
            //}
            //path.Stack = newstack;
        }

        /// <summary>
        /// Returns the found path and sets the state to stopped
        /// </summary>
        /// <returns></returns>
        public Path GetPath()
        {
            if (this.State != States.Finished)
                throw new Exception();
            this.State = States.Stopped;
            if (this.PathInProgress != null)
            {
                ConformPathToTerrain(this.Map, this.PathInProgress);
                this.PathInProgress.ConformToBlockHeights(this.Map);
            }
            return this.PathInProgress;
        }
        public void Begin(Actor actor, Vector3 start, TargetArgs finish)
        {
            switch(finish.Type)
            {
                case TargetType.Null:
                    this.Begin(actor, start, start);
                    break;

                default:
                    this.Begin(actor, start, finish.Global);
                    break;
            }
        }
        public void Begin(Actor actor, Vector3 start, Vector3 finish, float range = 0)
        {
            var map = actor.Map;
            if (!map.Contains(start) || !map.Contains(finish))
                throw new Exception();
            this.Ticks = 0;
            this.Range = range;
            this.Start = start.Round();
            this.Actor = actor;

            this.FinishTarget = new TargetArgs(map, finish);
            this.FinishPrecise = finish;
            this.Finish = finish.Round();

            if (IsInaccessible(map, this.Finish))
            {
                Fail();
                return;
            }

            this.Goal = this.Finish;
            this.Map = map;

            this.State = PathingSync.States.Working;
            this.PathInProgress = new Path();
            var startNode = new Node(this.Map, this.Start, this.Finish) { RegionNodeGlobal = map.GetNodeAt(this.Start.Below()) };
            var finishNode = new Node(this.Map, this.Finish, this.Finish) { RegionNodeGlobal = map.GetNodeAt(this.Goal.Below()) };
            this.CachedNodes.Clear();
            this.Open.Clear();
            this.Closed.Clear();
            this.Handled.Clear();
            this.CachedNodes.Add(this.Start, startNode);
            this.CachedNodes[this.Finish] = finishNode;
            this.Open.Enqueue(startNode.CostToGoal, startNode);
        }
        public void Begin(Actor actor, Vector3 start, TargetArgs finish, float range = 0)
        {
            var map = actor.Map;
            this.Actor = actor;
            this.Range = range;
            this.Start = start.Round();

            this.FinishTarget = finish;
            this.FinishPrecise = finish.Global;
            this.Finish = this.FinishPrecise.Round();

            if (IsInaccessible(map, this.Finish))
            {
                Fail();
                return;
            }

            this.Goal = this.Finish;
            this.Map = map;

            this.State = PathingSync.States.Working;
            this.PathInProgress = new Path();
            var startNode = new Node(this.Map, this.Start, this.Finish) { RegionNodeGlobal = map.GetNodeAt(this.Start.Below()) };
            var finishNode = new Node(this.Map, this.Finish, this.Finish) { RegionNodeGlobal = map.GetNodeAt(this.Goal.Below()) };
            this.CachedNodes.Clear();
            this.Open.Clear();
            this.Closed.Clear();
            this.CachedNodes.Add(this.Start, startNode);
            this.CachedNodes[this.Finish] = finishNode;
            this.Open.Enqueue(startNode.CostToGoal, startNode);
            startNode.IsQueued = true;
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

            if (this.Open.Count > 0)
            {
                var current = this.Open.Dequeue();

                //if (current.Global == this.Finish)
                //if (Math.Abs(current.Global.X - this.Finish.X) <= this.Range && Math.Abs(current.Global.Y - this.Finish.Y) <= this.Range)
                var l = Vector3.Distance(current.Global, this.Finish); // use manhattan or eucledean distance?
                if(l<=this.Range)
                {
                    this.PathInProgress.Build(current);
                    //this.PathInProgress.Stack.Push(this.Start);
                    if (this.PathInProgress.Stack.Count == 0)
                    {
                        //throw new Exception(); // tried to find path when we already there, should check first if we already there, so throw exception for debug purposes
                        this.PathInProgress.Stack.Push(this.FinishPrecise);
                    }

                    this.State = States.Finished;
                    return;
                }

                this.Closed.Add(current);
                var neighbors = current.Global.GetNeighborsDiag();
                foreach (var n in neighbors) // put visibility check here
                {
                    // maybe check if one of the neighbors is the goal and complete the path here, without checking if the goal is an obstacle?
                    // don't allow diagonals because because of inertia while approaching a goal block diagonally there will be a collision and the agent will jump on top of the block
                    // maybe increase range so as to not collide diagonally?
                    //if(n == this.Finish)// && (n.X == current.Global.X && n.Y == current.Global.Y)) 
                    //    if(n.X == current.Global.X || n.Y == current.Global.Y)
                    if(FinishMode.Touching.IsFinish(this.Finish, current.Global, n))
                    {
                        var goalnode = new Node(this.Map, n, this.Finish) { Parent = current.Parent ?? current }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
                            // TODO: WARNING! if i don't check line of sight with goal, then in the case of a walkable node, the ai will try to walk in a straight line to it and collide with obstacles near the goal
                            // TODO: DONT finish the path without checking for los
                            // TODO: maybe use current node's parent if goal is an obstacle and current node if it's pathable? as a workaround
                        this.PathInProgress.Build(goalnode);
                        this.State = States.Finished;
                        return;
                    }

                    if (this.Closed.Any(nn => nn.Global == n))
                        continue;
                    if (!this.CachedNodes.TryGetValue(n, out Node nnode))
                    {
                        nnode = new Node(this.Map, n, this.Finish);
                        this.CachedNodes[n] = nnode;
                    }

                    if (!this.Open.Any(node => node.Global == n))
                    {
                        nnode.CostFromStart = float.MaxValue;// -1;
                        nnode.Parent = null;
                    }
                    Update(this.Map, current, nnode);
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
        public void WorkMode(FinishMode mode)
        {
            //var before = DateTime.Now;
            do { } while (this.WorkModeStepFaster(mode) && this.Ticks < 100);
            this.Ticks = 0;
            //Console.WriteLine("path found after {0} ms", (DateTime.Now - before).TotalMilliseconds);
        }
        //public bool WorkModeStep(FinishMode mode)
        //{
        //    if (this.State != States.Working)
        //        throw new Exception(); //why is pathfinder called if it's not working?
        //    this.Ticks++;
        //    if (this.Open.Count > 0)
        //    {
        //        var current = this.Open.Dequeue();
        //        //if(FinishMode.Default.IsFinish(this.Goal, current.Global))
        //        if(mode.IsFinish(this.Actor, this.Goal, current.Global))// && this.Map.IsStandable(current.Global)) //GetBlock
        //        {
        //            var goalnode = new Node(this.Map, current.Global, this.Finish) { Parent = current.Parent ?? current }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
        //            this.PathInProgress.Build(goalnode);
        //            Console.WriteLine(string.Format("path found after {0} ticks", this.Ticks));
        //            this.State = States.Finished;
        //            return false;
        //        }

        //        this.Closed.Add(current);
        //        var neighbors = current.Global.GetNeighborsDiag();
        //        foreach (var n in neighbors) // put visibility check here
        //        {
        //            // maybe check if one of the neighbors is the goal and complete the path here, without checking if the goal is an obstacle?
        //            // don't allow diagonals because because of inertia while approaching a goal block diagonally there will be a collision and the agent will jump on top of the block
        //            // maybe increase range so as to not collide diagonally?
        //            //if (FinishMode.Default.IsFinish(this.Finish, current.Global, n))
        //            //{
        //            //    var goalnode = new Node(this.Map, n, this.Finish) { Parent = current.Parent ?? current }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
        //            //    // TODO: WARNING! if i don't check line of sight with goal, then in the case of a walkable node, the ai will try to walk in a straight line to it and collide with obstacles near the goal
        //            //    // TODO: DONT finish the path without checking for los
        //            //    // TODO: maybe use current node's parent if goal is an obstacle and current node if it's pathable? as a workaround
        //            //    this.PathInProgress.Build(goalnode);
        //            //    this.State = States.Finished;
        //            //    return;
        //            //}

        //            if (this.Closed.Any(nn => nn.Global == n))
        //                continue;
        //            Node nnode;
        //            if (!this.CachedNodes.TryGetValue(n, out nnode))
        //            {
        //                nnode = new Node(this.Map, n, this.Finish);
        //                this.CachedNodes[n] = nnode;
        //            }

        //            if (!this.Open.Any(node => node.Global == n))
        //            {
        //                nnode.CostFromStart = float.MaxValue;// -1;
        //                nnode.Parent = null;
        //            }
        //            Update(this.Map, current, nnode);
        //        }
        //    }

        //    else
        //    {
        //        // no path found
        //        HandleInaccessibleBlock(this.Map, this.Finish);
        //        Fail();
        //        return false;
        //    }
        //    return true;
        //}
        //public bool WorkModeStepNodes(FinishMode mode)
        //{
        //    if (this.State != States.Working)
        //        throw new Exception(); //why is pathfinder called if it's not working?
        //    this.Ticks++;
        //    if (this.Open.Count > 0)
        //    {
        //        var current = this.Open.Dequeue();
        //        //if(FinishMode.Default.IsFinish(this.Goal, current.Global))
        //        if (mode.IsFinish(this.Actor, this.Goal, current.Global))
        //        {
        //            var goalnode = new Node(this.Map, current.Global, this.Finish) { Parent = current.Parent ?? current }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
        //            this.PathInProgress.Build(goalnode);
        //            Console.WriteLine(string.Format("path found after {0} ticks", this.Ticks));
        //            this.State = States.Finished;
        //            return false;
        //        }

        //        this.Closed.Add(current);
        //        var currentNode = this.Map.Regions.GetNodeAt(current.Global.Below());
        //        var neighbors = currentNode.GetLinks().Select(n => n.Global.Above());// current.Global.GetNeighborsDiag();
        //        foreach (var n in neighbors) // put visibility check here
        //        {
        //            // maybe check if one of the neighbors is the goal and complete the path here, without checking if the goal is an obstacle?
        //            // don't allow diagonals because because of inertia while approaching a goal block diagonally there will be a collision and the agent will jump on top of the block
        //            // maybe increase range so as to not collide diagonally?
        //            //if (FinishMode.Default.IsFinish(this.Finish, current.Global, n))
        //            //{
        //            //    var goalnode = new Node(this.Map, n, this.Finish) { Parent = current.Parent ?? current }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
        //            //    // TODO: WARNING! if i don't check line of sight with goal, then in the case of a walkable node, the ai will try to walk in a straight line to it and collide with obstacles near the goal
        //            //    // TODO: DONT finish the path without checking for los
        //            //    // TODO: maybe use current node's parent if goal is an obstacle and current node if it's pathable? as a workaround
        //            //    this.PathInProgress.Build(goalnode);
        //            //    this.State = States.Finished;
        //            //    return;
        //            //}

        //            if (this.Closed.Any(nn => nn.Global == n))
        //                continue;
        //            if (!this.CachedNodes.TryGetValue(n, out Node nnode))
        //            {
        //                nnode = new Node(this.Map, n, this.Finish);
        //                this.CachedNodes[n] = nnode;
        //            }

        //            if (!this.Open.Any(node => node.Global == n))
        //            {
        //                nnode.CostFromStart = float.MaxValue;// -1;
        //                nnode.Parent = null;
        //            }
        //            Update(this.Map, current, nnode);
        //        }
        //    }

        //    else
        //    {
        //        // no path found
        //        HandleInaccessibleBlock(this.Map, this.Finish);
        //        Fail();
        //        return false;
        //    }
        //    return true;
        //}
        public bool WorkModeStepFaster(FinishMode mode)
        {
            if (this.State != States.Working)
                throw new Exception(); //why is pathfinder called if it's not working?
            this.Ticks++;
            if (this.Open.Count > 0)
            {
                var current = this.Open.Dequeue();
                current.IsQueued = false;
                //if(FinishMode.Default.IsFinish(this.Goal, current.Global))
                if (mode.IsFinish(this.Actor, this.Goal, current.Global) && this.Map.IsStandableIn(current.Global))
                {
                    var goalnode = new Node(this.Map, current.Global, this.Finish) { Parent = current.Parent ?? current, CellsToTraverse = current.CellsToTraverse }; // assinging the current node's parent seems to work more correctly, but if the parent is null?
                    //this.PathInProgress.Build(goalnode);
                    this.PathInProgress.Build(goalnode, this.Start);

                    //Console.WriteLine(string.Format("path found after {0} ticks", this.Ticks));
                    this.State = States.Finished;
                    return false;
                }

                //this.Closed.Add(current);
                this.Handled.Add(current.Global);
                var currentNode = current.RegionNodeGlobal;// this.Map.Regions.GetNodeAt(current.Global.Below());
                //var neighbors = currentNode.GetLinks();// current.Global.GetNeighborsDiag();
                var neighbors = currentNode.GetLinksLazy();// current.Global.GetNeighborsDiag();
                foreach (var n in neighbors) // put visibility check here
                {
                    var nabove = n.Global.Above();
                    if (this.Handled.Contains(nabove))
                        continue;
                    //if (this.Closed.Any(nn => nn.RegionNodeGlobal == n))
                    //    continue;
                    if (!this.CachedNodes.TryGetValue(nabove, out var nnode))
                    {
                        nnode = new Node(this.Map, nabove, this.Finish) { RegionNodeGlobal = n };
                        this.CachedNodes[n.Global.Above()] = nnode;

                    }

                    //if (!this.Open.Any(node => node.RegionNodeGlobal == n))
                    if (!nnode.IsQueued)
                    {
                        nnode.CostFromStart = float.MaxValue;// -1;
                        nnode.Parent = null;
                    }
                    UpdateNoPathableCheck(this.Map, current, nnode);
                }
            }

            else
            {
                // no path found
                HandleInaccessibleBlock(this.Map, this.Finish);
                Fail();
                return false;
            }
            return true;
        }

        public void WorkOld()
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
                //if (current.Global == new Vector3(21, 27, 64))
                //    "gia na doume".ToConsole();

                // TODO: handle the case where the target block is solid in a better way than this. maybe receive required minimum target range as an argument when starting the pathfinding?
                if (Math.Abs(current.Global.X - this.Finish.X) <= 1 && Math.Abs(current.Global.Y - this.Finish.Y) <= 1)
                //if (current.Global == this.Finish)
                {
                    // TODO: maybe replace vector3 in each path step with a targetArg, so if the final target is an entity, 
                    // we add the entity as a target instead of its global at the time of path calculation

                    // WARNING: we are checking if distance to target is <= 1, this is true at the second to last node, so it get's skipped if we replace its position with the finish position
                    //current.Global = this.FinishPrecise;
                    //this.PathInProgress.Build(current);

                    // FIXED(?) by creating a new node for the finish position with the current node as parent, and build path for it
                    var finishNode = new Node(this.Map, this.Finish, this.Finish) { Parent = current };
                    this.PathInProgress.Build(finishNode);

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
                    //if (n == new Vector3(21, 27, 64))
                    //    "gia na doume".ToConsole();
                    if (this.Closed.Any(nn => nn.Global == n))
                        continue;
                    if (!this.CachedNodes.TryGetValue(n, out Node nnode))
                    {
                        nnode = new Node(this.Map, n, this.Finish);
                        this.CachedNodes[n] = nnode;
                    }

                    if (!this.Open.Any(node => node.Global == n)) 
                    {
                        nnode.CostFromStart = float.MaxValue;// -1;
                        nnode.Parent = null;
                    }
                    Update(this.Map, current, nnode);
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

        

        static float Heuristic(Vector3 start, Vector3 finish)
        {
            var dx = (float)Math.Abs(finish.X - start.X);
            var dy = (float)Math.Abs(finish.Y - start.Y);
            var dz = (float)Math.Abs(finish.Z - start.Z);
            var dmin = Math.Min(dx, dy);
            var dmax = dx + dy - dmin;
            //return dx * CostStraight + dy * CostDiag + dz * 100;
            return (dmax - dmin) * CostStraight + dmin * CostDiag;// +dz * 100; // TODO: find correct cost for climbing

        }
        
        void Update(IMap map, Node current, Node next)
        {
            //if (!IsWalkable(map, current.Global))
            var block = map.GetBlock(next.Global - Vector3.UnitZ);
            //if (!IsValidMovement(map, current.Global, next.Global))
            if (!IsPathableMove(map, current.Global, next.Global))
                return;

            var costOld = next.CostFromStart;
            //ComputeCost(current, next);
            ComputeCostTheta(current, next);

            if (next.CostFromStart < costOld)
            {
                this.Open.Remove(next);
                //this.Open.Enqueue(next.CostFromStart + HeuristicTheta(next.Global, this.Goal), next);
                this.Open.Enqueue(next.CostFromStart + Heuristic(next.Global, this.Goal), next);

                //this.Open.Enqueue(next.CostFromStart + GetCostEuclidean(next.Global, this.Goal), next);
            }
        }
        void UpdateNoPathableCheck(IMap map, Node current, Node next)
        {
            var costOld = next.CostFromStart;
            ComputeCostTheta(current, next);

            if (next.CostFromStart < costOld)
            {
                this.Open.Remove(next);
                this.Open.Enqueue(next.CostFromStart + Heuristic(next.Global, this.Goal), next);
                next.IsQueued = true;
            }
        }

        const float BlockBaseCost = 100;// 10;//0;//1;//2; //5; // TODO: find best cost for making ai prefer pathways
        void ComputeCost(Node current, Node next)
        {
            //var cost = GetCost(current, next);

            var stepcost = GetStepCost(current.Global, next.Global);// GetCost(current, next);
            var blockcost = Block.GetPathingCost(this.Map, next.Global - Vector3.UnitZ) * BlockBaseCost;
            var cost = stepcost + blockcost * BlockBaseCost;
            var costCheck = current.CostFromStart + cost;
            if (costCheck < next.CostFromStart)
            {
                next.Parent = current;
                next.CostFromStart = costCheck;
            }
        }

        void ComputeCostTheta(NodeBase current, NodeBase next)
        {
            var passedcells = new List<Vector3>();
            var los = current.Parent != null && LineOfSight(current.Map, current.Parent.Global, next.Global, out passedcells);
            float cost, costCheck;
            if (los)
            {
                //cost = GetCost(current.Parent, next);
                cost = GetCostTheta(current.Parent, next);

                costCheck = current.Parent.CostFromStart + cost;
                if (costCheck < next.CostFromStart)
                {
                    next.Parent = current.Parent;
                    next.CostFromStart = costCheck;
                    next.CellsToTraverse = passedcells;
                }
            }
            else
            {
                //cost = GetCost(current, next);
                cost = GetCostThetaSingle(current, next);

                costCheck = current.CostFromStart + cost;
                if (costCheck < next.CostFromStart)
                {
                    next.Parent = current;
                    next.CostFromStart = costCheck;
                    next.CellsToTraverse = passedcells;
                }
            }
        }
        float GetCostTheta(NodeBase current, NodeBase next)
        {
            // get an 1-wide line between the two positions and sum the block pathing costs below the line
            if(current.Global.Z != next.Global.Z)
                throw new NotImplementedException();
            var z = next.Global.Z;
            var x1 = (int)(current.Global.X);
            var y1 = (int)(current.Global.Y);
            var x2 = (int)(next.Global.X);
            var y2 = (int)(next.Global.Y);

            var positions = Line.Plot2D(x1, y1, x2, y2);
            positions.RemoveAt(0);
            var positionsVec3 = positions.Select(p => new Vector3(p.X, p.Y, z));
            float cost = 0;
            var a = current.Global;
            foreach (var pos in positionsVec3)
            {
                cost += GetCostThetaSingle(a, pos);
                a = pos;
            }
            return cost;
            //var blocks = from pos in positions select Block.GetPathingCost(this.Map, new Vector3(pos, z - 1));
            //var blocksCost = blocks.Sum()*BlockBaseCost;
            //var distance = Vector3.DistanceSquared(current.Global, next.Global);
            //////cost *= distance;
            ////var cost = distance + blocksCost;
            //var cost = distance * blocksCost;

            //return cost;
        }
        float GetCostThetaSingle(NodeBase current, NodeBase next)
        {
            return this.GetCostThetaSingle(current.Global, next.Global);
        }
        float GetCostThetaSingle(Vector3 current, Vector3 next)
        {
            var blockCost = Block.GetPathingCost(this.Map, next - Vector3.UnitZ);
            //var distanceCost = Vector3.DistanceSquared(current.Global * new Vector3(1, 1, 0), next.Global * new Vector3(1, 1, 0)); //GetCostEuclidean(current, next);
            //var distanceCost = Vector3.DistanceSquared(current.Global, next.Global); //GetCostEuclidean(current, next);
            //var cost = distanceCost + blockCost;
            //var cost = distanceCost * blockCost;
            var stepcost = GetStepCost(current, next);
            var cost = stepcost + blockCost * BlockBaseCost;
            return cost;
        }
        float GetCost(NodeBase current, NodeBase next)
        {
            var blockCost = Block.GetPathingCost(this.Map, next.Global - Vector3.UnitZ) * BlockBaseCost;
            var distanceCost = Vector3.DistanceSquared(current.Global * new Vector3(1, 1, 0), next.Global * new Vector3(1, 1, 0)); //GetCostEuclidean(current, next);
            var cost = distanceCost * blockCost;
            return cost;

            //float cost = 0;
            //if (current.Global.X == next.Global.X || current.Global.Y == next.Global.Y)
            //    cost = CostStraight;
            //else
            //    cost = CostDiag;
            //return cost;
        }
        float GetStepCost(Vector3 a, Vector3 b)
        {
            var cost = (a.X == b.X || a.Y == b.Y) ? CostStraight : CostDiag;
            if (b.Z - a.Z == 1)
            {
                var blockheight = Block.GetBlockHeight(this.Map, b.Below());
                if (blockheight > .5f)
                    cost += CostStraight * 6f;// 4.5f;// *= 20; 
            }
            // the cost of going around a block should be less than the cost of jumping on it. 
            // the cost of going around a block is coststraight * number of blocks the ai should prefer of walking instead of jumping. 
            // for example entering the area around a block and exiting at the opposite side would be coststraight * 4. 
            // if the ai would go completely around it would be coststraight * 6
            // for now we'll let it prefer jumping if it has to walk more than a semicircle around the block so the cost should be coststraight * 4.5 (???)
            return cost;
            //return (a.X == b.X || a.Y == b.Y) ? CostStraight : CostDiag;
        }
        
        static float GetCostEuclidean(NodeBase current, NodeBase next)
        {
            return Vector3.DistanceSquared(current.Global, next.Global);
        }
        static float GetCostEuclidean(Vector3 current, Vector3 next)
        {
            return Vector3.DistanceSquared(current, next);
        }
        static public bool IsWalkable(IMap map, Vector3 global)
        {
            if (!map.IsSolid(global - Vector3.UnitZ))
                return false;

            // TODO: check both at the same time          
            if (map.IsSolid(global))
                return false;
            if (map.IsSolid(global + Vector3.UnitZ)) // replace this with height check?
                return false;

            //var currentSolid = map.IsSolid(global);
            //var aboveSolid = map.IsSolid(global + Vector3.UnitZ);
            //return !(currentSolid && aboveSolid);
            return true;
        }
        static public bool IsPathable(IMap map, Vector3 global)
        {
            return IsPathable1Height(map, global);
            // WARNING! can't check directly below in case the destination is a halbblock with air underneath.
            // so adjust the vector to above().floor() or below().ceiling() or just ceiling() ??
            global = global.CeilingZ();
            if (!map.IsSolid(global.Below())) 
                return false;

            // TODO: check both at the same time          
            if (!map.IsPathable(global))
                return false;
            if (!map.IsPathable(global.Above())) // replace this with height check?
                return false;

            return true;
        }
        static public bool IsPathable1Height(IMap map, Vector3 global)
        {
            // WARNING! can't check directly below in case the destination is a halbblock with air underneath.
            // so adjust the vector to above().floor() or below().ceiling() or just ceiling() ??
            global = global.CeilingZ();
            if (!map.IsSolid(global.Below()))
                return false;

            // TODO: check both at the same time          
            if (!map.IsPathable(global))
                return false;
            //if (!map.IsPathable(global.Above())) // replace this with height check?
            //    return false;

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
        static public bool IsValidMovement(IMap map, Vector3 a, Vector3 b)
        {
            if (!IsWalkable(map, b))
                return false;
            if (IsDiagonal(a, b))
            {
                Vector3 d1, d2;
                if (a.Z == b.Z)
                {
                    d1 = new Vector3(a.X, b.Y, a.Z);
                    d2 = new Vector3(b.X, a.Y, a.Z);
                    if (!IsWalkable(map, d1))
                        return false;
                    if (!IsWalkable(map, d2))
                        return false;
                    return true;
                }
                //else if(a.Z < b.Z)
                //{
                //    d1 = new Vector3(a.X, b.Y, b.Z);
                //    d2 = new Vector3(b.X, a.Y, b.Z);
                //    if (map.IsSolid(d1))
                //        return false;
                //    if (map.IsSolid(d2))
                //        return false;
                //    return true;
                //}
                //else
                //{

                //}
                var z = Math.Max(a.Z, b.Z);// which z to use?
                d1 = new Vector3(a.X, b.Y, z);
                d2 = new Vector3(b.X, a.Y, z);
                // do i need diagonals to be walkable? or just not solid?
                if (map.IsSolid(d1))
                    return false;
                if (map.IsSolid(d2))
                    return false;

                //if (!IsWalkable(map, d1))
                //    return false;
                //if (!IsWalkable(map, d2))
                //    return false;
            }
            return true;
        }
        static public bool IsPathableMove(IMap map, Vector3 a, Vector3 b)
        {
            if (!IsPathable(map, b))
                return false;
            if (IsDiagonal(a, b))
            {
                Vector3 d1, d2;
                if (a.Z == b.Z)
                {
                    d1 = new Vector3(a.X, b.Y, a.Z);
                    d2 = new Vector3(b.X, a.Y, a.Z);
                    if (!IsPathable(map, d1))
                        return false;
                    if (!IsPathable(map, d2))
                        return false;
                    return true;
                }
                //else if(a.Z < b.Z)
                //{
                //    d1 = new Vector3(a.X, b.Y, b.Z);
                //    d2 = new Vector3(b.X, a.Y, b.Z);
                //    if (map.IsSolid(d1))
                //        return false;
                //    if (map.IsSolid(d2))
                //        return false;
                //    return true;
                //}
                //else
                //{

                //}
                var z = Math.Max(a.Z, b.Z);// which z to use?
                d1 = new Vector3(a.X, b.Y, z);
                d2 = new Vector3(b.X, a.Y, z);
                // do i need diagonals to be walkable? or just not solid?
                if (map.IsPathable(d1))
                    return false;
                if (map.IsPathable(d2))
                    return false;

                //if (!IsWalkable(map, d1))
                //    return false;
                //if (!IsWalkable(map, d2))
                //    return false;
            }
            return true;
        }

        static public bool LineOfSight(IMap map, Vector3 a, Vector3 b, out List<Vector3> cells)
        {

            if (a.Z == b.Z)
            {
                return Line.LineOfSight((int)a.X, (int)a.Y, (int)a.Z, (int)b.X, (int)b.Y, (int)b.Z, p => LosPathableFailCondition(map, p), out cells); //map.IsSolid(p
            }                                                                                                                              //return Line.LineOfSightFloat(a.X, a.Y, a.Z, b.X, b.Y, b.Z, p => LosPathableFailCondition(map, p)); //map.IsSolid(p
            cells = new List<Vector3>();
            return false;
        }
        static public bool LineOfSight(IMap map, Vector3 a, Vector3 b)
        {
            
            if (a.Z == b.Z)
                return Line.LineOfSight((int)a.X, (int)a.Y, (int)a.Z, (int)b.X, (int)b.Y, (int)b.Z, p => LosPathableFailCondition(map, p)); //map.IsSolid(p
                //return Line.LineOfSightFloat(a.X, a.Y, a.Z, b.X, b.Y, b.Z, p => LosPathableFailCondition(map, p)); //map.IsSolid(p

            return false;
        }
        public static bool LosPathableFailCondition(IMap map, Vector3 p)
        {
            var below = p - Vector3.UnitZ;
            var solidbelow = map.IsSolid(below);
            if (!solidbelow)
                return true;

            var unpathablecurrent = !map.IsPathable(p);
            return unpathablecurrent; // i'm testing returning here to make the ai prefer going around single height blocks instead of jumping over them 

            var above = p + Vector3.UnitZ;
            var unpathableabove = !map.IsPathable(above);
            return unpathableabove & unpathablecurrent;
        }
        private static bool LosFailCondition(IMap map, Vector3 p)
        {
            var below = p - Vector3.UnitZ;
            var solidbelow = map.IsSolid(below);
            if (!solidbelow)
                return true;

            var above = p + Vector3.UnitZ;
            var solidabove = map.IsSolid(above);
            var solidcurrent = map.IsSolid(p);
            return solidabove & solidcurrent;
        }

        public static HashSet<Vector3> FloodFill(IMap map, Vector3 global)
        {
            var open = new Queue<Vector3>();
            var closed = new HashSet<Vector3>();
            var area = new HashSet<Vector3>();
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

        public abstract class FinishMode
        {
            static public readonly FinishMode Touching = new FinishModeDefault();
            static public readonly FinishMode Exact = new FinishModeOnGoal();
            static public readonly FinishMode Any = new FinishModeOnGoalOrTouching();

            public virtual bool IsFinish(Vector3 goal, Vector3 current, Vector3 neighbor) { throw new Exception(); }
            public abstract bool IsFinish(Actor actor, Vector3 goal, Vector3 current);

            public class FinishModeDefault : FinishMode
            {
                //public override bool IsFinish(Vector3 g, Vector3 c, Vector3 n)
                //{
                //    throw new Exception();
                //    if (n == g)
                //        if (n.X == c.X || n.Y == c.Y)
                //            return true;
                //    return false;
                //}
                public override bool IsFinish(Actor actor, Vector3 goal, Vector3 current)
                {
                    //if ((goal.Z < current.Z - 2) || (goal.Z > current.Z + 2))//4))
                    //    return false;
                    if ((goal.Z < current.Z - 1) || (goal.Z > current.Z + actor.Physics.Reach))//4))
                        return false;
                    var xdist = Math.Abs(goal.X - current.X);
                    var ydist = Math.Abs(goal.Y - current.Y);
                    if ((xdist == 1 && ydist == 0) || (xdist == 0 && ydist == 1))
                        return true;
                    return false;
                }
            }
            class FinishModeOnGoal : FinishMode
            {
                public override bool IsFinish(Actor actor, Vector3 g, Vector3 c)
                {
                    return g == c;
                }
            }
            class FinishModeOnGoalOrTouching : FinishMode
            {
                public override bool IsFinish(Actor actor, Vector3 g, Vector3 c)
                {
                    //throw new NotImplementedException(); // TODO incorporate actor's reach
                    if (g == c)
                        return true;
                    //if ((g.Z < c.Z - 2) || (g.Z > c.Z + 2))//4))
                    //    return false;
                    if ((g.Z < c.Z - 1) || (g.Z > c.Z + actor.Physics.Reach))//4))
                        return false;
                    var xdist = Math.Abs(g.X - c.X);
                    var ydist = Math.Abs(g.Y - c.Y);
                    if ((xdist == 1 && ydist == 0) || (xdist == 0 && ydist == 1))
                        return true;
                    return false;
                }
            }
        }
        
    }
}
