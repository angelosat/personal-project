using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class Region
    {
        public Chunk Chunk;
        public IMap Map { get { return this.Chunk.Map; } }
        public int RoomID
        {
            get
            {
                return this.Room.ID;
            }
            set
            {
                if (value == this.Room.ID)
                    return;// throw new Exception();
                this.Room.Remove(this);
                var newRoom = this.Map.Regions.GetRoomID(value);
                newRoom.Add(this);
                this.Room = newRoom;
            }
        }
        public int RegionID;
        //HashSet<Vector3> Positions = new HashSet<Vector3>();
        //public Dictionary<Vector3, RegionNode> Nodes = new Dictionary<Vector3, RegionNode>();
        public RegionNodeCollection Nodes;

        public Color Color { get { return this.Room.Color; } }
        public int Size = 1;
        public HashSet<Region> Neighbors = new HashSet<Region>();
        public bool Validated;
        public RegionRoom Room;
        public Region(Chunk chunk)
        {
            this.Nodes = new RegionNodeCollection(this);
            this.Chunk = chunk;
            //this.RoomID = chunk.Map.Regions.GetRoomID();
            this.Room = chunk.Map.Regions.CreateRoom(this);
            this.RegionID = chunk.Map.Regions.GetRegionID();
            //this.AssignColor();
        }

        //private void AssignColor()
        //{
        //    var colorand = new Random();
        //    var array = new byte[3];
        //    colorand.NextBytes(array);
        //    this.Color = new Color(array[0], array[1], array[2]);
        //}
        public override string ToString()
        {
            return string.Format("Region: {0} Room: {1} Nodes: {2} Neighbors: {3}", this.RegionID, this.RoomID, this.Nodes.Count, this.Neighbors.Count);
        }
        

        public RegionNode Add(Vector3 local)
        {
            var node = new RegionNode(this, local);
            //this.Nodes.Add(node.Global, node);
            this.Nodes.Add(node);
            return node;
        }
        public void Add(RegionNode node)
        {
            //this.Nodes.Add(node.Global, node);
            //node.Region = this;
            // remove node from node's previous region?
            this.Nodes.Add(node);
        }
        private void Add(IEnumerable<RegionNode> nodes)
        {
            foreach (var n in nodes)
                this.Add(n);
        }
        private void Remove(IEnumerable<RegionNode> nodes)
        {
            foreach (var n in nodes)
                this.Remove(n);
        }
        public void Remove(Vector3 global)
        {
            RegionNode node;
            if(this.Nodes.TryGetValue(global, out node))
                node.ClearLinks();
            this.Nodes.Remove(global);
            //this.Positions.Remove(global);
        }
        internal void Remove(RegionNode n)
        {
            this.Remove(n.Global);
        }
        public IEnumerable<Vector3> GetPositions()
        {
            return this.Nodes.Values.Select(n => n.Global);
            //return this.Positions;
        }
        internal bool Contains(Vector3 global)
        {
            return this.Nodes.ContainsKey(global);
        }
        internal bool Contains(RegionNode newNode)
        {
            return this.Nodes.ContainsKey(newNode.Global);
        }

        public void LinkNodes()
        {
            var toHandle = new Queue<RegionNode>(this.Nodes.Values);
            var handled = new HashSet<Vector3>();
            while (toHandle.Any())
            {
                var node = toHandle.Dequeue();
                handled.Add(node.Local);

                if (node.North == null)
                {
                    var northColumn = node.Global.North();
                    foreach (var north in RegionTracker.GetColumn(northColumn))
                        if (this.Nodes.ContainsKey(north))
                        {
                            var nNode = this.Nodes[north];
                            node.North = nNode;
                            nNode.South = node;
                            if (!handled.Contains(north))
                                toHandle.Enqueue(nNode);
                        }
                }
                if (node.South == null)
                {
                    var southolumn = node.Global.South();
                    foreach (var south in RegionTracker.GetColumn(southolumn))
                        if (this.Nodes.ContainsKey(south))
                        {
                            var sNode = this.Nodes[south];
                            node.South = sNode;
                            sNode.North = node;
                            if (!handled.Contains(south))
                                toHandle.Enqueue(sNode);
                        }
                }
                if (node.West == null)
                {
                    var westColumn = node.Global.West();
                    foreach (var west in RegionTracker.GetColumn(westColumn))
                        if (this.Nodes.ContainsKey(west))
                        {
                            var wNode = this.Nodes[west];
                            node.West = wNode;
                            wNode.East = node;
                            if (!handled.Contains(west))
                                toHandle.Enqueue(wNode);
                        }
                }
                if (node.East == null)
                {
                    var eastColumn = node.Global.East();
                    foreach (var east in RegionTracker.GetColumn(eastColumn))
                        if (this.Nodes.ContainsKey(east))
                        {
                            var eNode = this.Nodes[east];
                            node.East = eNode;
                            eNode.West = node;
                            if (!handled.Contains(east))
                                toHandle.Enqueue(eNode);
                        }
                }
            }
            //}
        }
        public void FindEdges()
        {
            foreach (var node in this.Nodes.Values)//.Where(n=>n.AtEdge()))
            {
                // if one of node's neighbors is null, check all other regions for a neighbor
                if (node.West == null)
                {
                    var adj = node.Global.West();
                    //if (this.Map.Contains(adj))
                    if (this.Map.IsInBounds(adj))
                    {
                        //foreach (var pos in RegionTracker.GetColumn(adj))
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.West = otherNode;
                                otherNode.East = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);
                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
                if (node.East == null)
                {
                    var adj = node.Global.East();
                    //if (this.Map.Contains(adj))
                    if (this.Map.IsInBounds(adj))
                    {
                        //foreach (var pos in RegionTracker.GetColumn(adj))
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.East = otherNode;
                                otherNode.West = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);

                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
                if (node.North == null)
                {
                    var adj = node.Global.North();
                    //if (this.Map.Contains(adj))
                    if (this.Map.IsInBounds(adj))
                    {
                        //foreach (var pos in RegionTracker.GetColumn(adj))
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.North = otherNode;
                                otherNode.South = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);
                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
                if (node.South == null)
                {
                    var adj = node.Global.South();
                    //if (this.Map.Contains(adj))
                    if (this.Map.IsInBounds(adj))
                    {
                        //foreach (var pos in RegionTracker.GetColumn(adj))
                        for (int i = 0; i < VectorHelper.Column3.Length; i++)
                        {
                            var pos = adj + VectorHelper.Column3[i];
                            var otherNode = this.Map.Regions.GetNodeAt(pos);
                            if (otherNode != null
                                && this.Map.IsTraversable(node.Global, pos)
                                )
                            {
                                node.South = otherNode;
                                otherNode.North = node;
                                if (otherNode.Region != this)
                                {
                                    this.Neighbors.Add(otherNode.Region);
                                    otherNode.Region.Neighbors.Add(this);
                                }
                            }
                        }
                    }
                }
            }
        }
        public void FindEdgesOld()
        {
            foreach (var node in this.Nodes.Values)//.Where(n=>n.AtEdge()))
            {
                // if one of node's neighbors is null, check all other regions for a neighbor
                if (node.Local.X == 0)// && node.West == null)
                {
                    var nchunk = this.Chunk.West;
                    if (nchunk != null)
                        foreach (var north in RegionTracker.GetColumn(node.Global.West()))
                        {
                            var nregion = this.Map.GetRegionAt(north);// nchunk.Regions.GetRegionAt(north);
                            if (nregion != null)
                            {
                                RegionNode nnode;
                                nregion.Nodes.TryGetValue(north, out nnode);
                                {
                                    node.West = nnode;
                                    nnode.East = node;
                                    this.Neighbors.Add(nregion);
                                    nregion.Neighbors.Add(this);
                                }
                            }
                        }
                }
                if (node.Local.X == Chunk.Size - 1)//  && node.East == null)
                {
                    var chunk = this.Chunk.East;
                    if (chunk != null)
                        foreach (var pos in RegionTracker.GetColumn(node.Global.East()))
                        {
                            var otherRegion = this.Map.GetRegionAt(pos);//.chunk.Regions.GetRegionAt(pos);
                            if (otherRegion != null)
                            {
                                RegionNode otherNode;
                                otherRegion.Nodes.TryGetValue(pos, out otherNode);
                                {
                                    node.East = otherNode;
                                    otherNode.West = node;
                                    this.Neighbors.Add(otherRegion);
                                    otherRegion.Neighbors.Add(this);
                                }
                            }
                        }
                }
                if (node.Local.Y == 0)//  && node.North == null)
                {
                    var chunk = this.Chunk.North;
                    if (chunk != null)
                        foreach (var pos in RegionTracker.GetColumn(node.Global.North()))
                        {
                            var otherRegion = this.Map.GetRegionAt(pos);//chunk.Regions.GetRegionAt(pos);
                            if (otherRegion != null)
                            {
                                RegionNode otherNode;
                                otherRegion.Nodes.TryGetValue(pos, out otherNode);
                                {
                                    node.North = otherNode;
                                    otherNode.South = node;
                                    this.Neighbors.Add(otherRegion);
                                    otherRegion.Neighbors.Add(this);
                                }
                            }
                        }
                }
                if (node.Local.Y == Chunk.Size - 1)//  && node.South == null)
                {
                    var chunk = this.Chunk.South;
                    if (chunk != null)
                        foreach (var pos in RegionTracker.GetColumn(node.Global.South()))
                        {
                            var otherRegion = this.Map.GetRegionAt(pos);//chunk.Regions.GetRegionAt(pos);
                            if (otherRegion != null)
                            {
                                RegionNode otherNode;
                                otherRegion.Nodes.TryGetValue(pos, out otherNode);
                                {
                                    node.South = otherNode;
                                    otherNode.North = node;
                                    this.Neighbors.Add(otherRegion);
                                    otherRegion.Neighbors.Add(this);
                                }
                            }
                        }
                }
            }
        }

        //internal void SetID(int p)
        //{
        //    foreach (var r in this.GetConnected())
        //    {
        //        r.RoomID = p;
        //    }
        //}
        internal void Paint(Region sourceRegion)
        {
            foreach (var r in this.GetConnected())
            {
                r.RoomID = sourceRegion.RoomID;
                //r.Color = sourceRegion.Color;
            }
        }
        public int CountAllConnectedRegions()
        {
            return this.GetConnected().Count();
        }
        public IEnumerable<Region> GetConnected()
        {
            var queue = new Queue<Region>();
            foreach (var n in this.Neighbors)
                queue.Enqueue(n);
            var counted = new HashSet<Region>() { this };
            while (queue.Any())
            {
                var r = queue.Dequeue();
                counted.Add(r);
                foreach (var n in r.Neighbors)
                {
                    if (!counted.Contains(n))
                        queue.Enqueue(n);
                }
            }
            return counted;
        }
        public int GetSize()
        {
            int n = 0;
            foreach (var region in GetConnected())
                n += region.Nodes.Count;
            return n;
        }
        public void Refresh()
        {
            if (this.Validated)
                return;
            this.Validated = true;
            this.Size = 1;
            //if (this.Nodes.Count == 1 && this.Map.GetBlock(this.Nodes.First().Key.Above()) == Block.Door) // TODO: FIX LOL
                if (this.Nodes.Count == 1 && this.Map.GetBlock(this.Nodes.First().Key.Above()) is BlockDoor) // TODO: FIX LOL
                    return;
            var queue = new Queue<Region>();
            foreach (var n in this.Neighbors)
                queue.Enqueue(n);
            var counted = new HashSet<Region>() { this };
            while (queue.Any())
            {
                var r = queue.Dequeue();
                r.RoomID = this.RoomID;
                //r.Color = this.Color;
                r.Validated = true;
                counted.Add(r);
                this.Size++;
                foreach (var n in r.Neighbors)
                {
                    //if (n.Nodes.Count == 1 && this.Map.GetBlock(n.Nodes.First().Key.Above()) == Block.Door) // TODO: FIX LOL
                        if (n.Nodes.Count == 1 && this.Map.GetBlock(n.Nodes.First().Key.Above()) is BlockDoor) // TODO: FIX LOL
                            continue;
                    if (!counted.Contains(n))
                        queue.Enqueue(n);
                }
            }
            foreach (var r in counted)
                r.Size = this.Size;
        }
        //int CountOld()
        //{
        //    var i = 0;
        //    var queue = new Queue<Region>();
        //    queue.Enqueue(this);
        //    var counted = new HashSet<Region>();
        //    while(queue.Any())
        //    {
        //        var r = queue.Dequeue();
        //        counted.Add(r);
        //        i++;
        //        foreach(var n in r.Neighbors)
        //        {
        //            if (!counted.Contains(n) && !queue.Contains(n))
        //                queue.Enqueue(n);
        //        }
        //    }
        //    return i;
        //}

        internal void DrawNode(Vector3 global, MySpriteBatch sb, Camera cam)
        {
            cam.DrawGridCell(sb, Color.Lime, global.Above());
            var node = this.Nodes[global];//.ToLocal()];
            cam.DrawGridCells(sb, Color.Lime, node.GetLinks().Select(link=> link.Global.Above()));

            //foreach(var link in node.GetLinks())
            //    cam.DrawGridCell(sb, Color.Lime, link.Global.Above());

        }

        internal bool AffectedBy(Vector3 global)
        {
            return this.Contains(global) || this.Contains(global.Below()) || this.Contains(global.Below().Below());
        }

        internal void Handle(Vector3 global)
        {
            if (!this.AffectedBy(global))
                return;
            var isSolid = this.Map.IsSolid(global);
            var below = global.Below();
            var belowbelow = below.Below();
            if (isSolid)
            {
                if (this.Contains(belowbelow))
                {
                    this.Remove(belowbelow);
                    // TODO: check heighbors and seperate regions if necessary
                    // TODO: also must compare height with neighbors and add current to region
                    // here or in the remove method?
                }
                else if (this.Contains(below))
                {
                    var oldNode = this.Nodes[below];
                    var newNode = new RegionNode(this, global);
                    if(newNode.TryReplace(oldNode))
                    {
                        this.Remove(below);
                        this.Add(newNode);
                    }
                    // TODO: check heighbors and seperate regions if necessary
                }
                else if (this.Contains(global))
                {
                    // nothing, the cell was already solid to begin with, if it's part of a region
                }
            }
            //else
            //{
            //    if (this.Contains(global))
            //    {
            //        // block removed that was part of a region. check the two blocks below if they should be added
            //        if (this.Map.IsSolid(below) && !this.Map.IsSolid(global.Above()))
            //        {
            //            var oldNode = this.Nodes[global];
            //            var newNode = new RegionNode(this, below);
            //            if(newNode.TryReplace(oldNode))
            //            {
            //                this.Remove(global);
            //                this.Add(newNode);
            //            }
            //        }
            //    }
            //    else if (this.Map.IsSolid(belowbelow) && !this.Map.IsSolid(below))
            //    {

            //    }
            //}
        }

        internal void Add(Region r)
        {
            foreach(var node in r.Nodes.Values.ToList())
            {
                this.Add(node);
            }
            r.Nodes.Clear();
            foreach (var nRegion in r.Neighbors) 
                // foreach otherregion's neighbors
                // remove otherregion and add this region
                // and add this region
            {
                nRegion.Neighbors.Remove(r);
                nRegion.Neighbors.Add(this);
                this.Neighbors.Add(nRegion);
            }
        }

        internal void Delete()
        {
            this.Room.Remove(this);
            this.Map.Regions.GetRegions(this.Chunk).Regions.Remove(this);
        }
        internal bool IsConnected()
        {
            var n = this.CountLinked();
            return this.Nodes.Count == n;
        }
        internal int CountLinked()
        {
            IEnumerable<RegionNode> temp;
            return this.CountLinked(out temp);
        }
        internal int CountLinked(out IEnumerable<RegionNode> disconnectedNodes)
        {
            var openList = new Queue<RegionNode>();
            var closedList = new HashSet<RegionNode>();
            openList.Enqueue(this.Nodes.First().Value);
            var n = 0;
            while(openList.Any())
            {
                n++;
                var node = openList.Dequeue();
                closedList.Add(node);
                foreach (var l in node.GetLinks().Where(p=>p.Region.Chunk == this.Chunk))
                    openList.Enqueue(l);
            }
            disconnectedNodes = n < this.Nodes.Count / 2 ? closedList : this.Nodes.Values.Except(closedList);
            return n;
        }
        

        internal bool TryGetSubRegionNodesNew(RegionNode startNode, IEnumerable<HashSet<RegionNode>> existing, out HashSet<RegionNode> nodes, out HashSet<Region> neighbors, out int size)
        {
            var toCount = new Queue<RegionNode>();
            var toCountAdded = new HashSet<RegionNode>();
            var counted = new HashSet<RegionNode>();
            toCount.Enqueue(startNode);
            var n = 0;
            HashSet<Region> neighborsFound = new HashSet<Region>();
            while (toCount.Any())
            {
                n++;
                var node = toCount.Dequeue();
                if (existing.Any(e => e.Contains(node)))
                {
                    nodes = null;
                    neighbors = null;
                    size = -1;
                    return false;
                }
                counted.Add(node);
                foreach (var link in node.GetLinks())//.Where(p => p.Region.Chunk == this.Chunk))
                {
                    if (counted.Contains(link) || toCountAdded.Contains(link)) //optimize
                        continue;
                    if (link.Region != this)
                    {
                        //if (!this.Neighbors.Contains(link.Region))
                        //    throw new Exception();
                        if (link.Region.RoomID == this.RoomID)
                            neighborsFound.Add(link.Region);
                        continue;
                    }
                    toCount.Enqueue(link);
                    toCountAdded.Add(link);
                }
            }
            neighbors = neighborsFound;
            size = n;
            nodes = counted;
            return n < this.Nodes.Count;// ? closedList : null;
        }

        internal void TrySplitRegion(Vector3 split)
        {
            var node = this.Nodes.GetValueOrDefault(split);
            if (node == null)
                return;
            this.TrySplitRegion(node);
        }
        internal void TrySplitRegion(RegionNode split) // with door handling
        {
            //var isBelowDoor = this.Map.GetBlock(split.Global.Above()) == Block.Door;
            var isBelowDoor = this.Map.GetBlock(split.Global.Above()) is BlockDoor;

            Region doorRegion = null;
            //RegionNode doorNode;
            var links = split.GetLinks();
            if (isBelowDoor)
            {
                doorRegion = new Region(this.Chunk);
                //doorNode = new RegionNode(doorRegion, split.Local);
                //doorRegion.Add(doorNode);//split.Global);
                //LinkNoPaint(doorRegion, this);
                doorRegion.Add(split);
                this.Map.Regions.Add(doorRegion);
            }
            else
            {
                split.ClearLinks();
            }

            this.Nodes.Remove(split.Global);

            var subRegions = new Dictionary<HashSet<RegionNode>, HashSet<Region>>();
            var maxSize = 0;
            IEnumerable<RegionNode> biggest = null;
            foreach (var link in links.Where(l => l.Chunk == this.Chunk))
            {
                HashSet<Region> neighbors;
                int size;
                HashSet<RegionNode> nodes;
                if (!this.TryGetSubRegionNodesNew(link, subRegions.Keys, out nodes, out neighbors, out size))
                    continue;
                if (size > maxSize)
                {
                    maxSize = size;
                    biggest = nodes;
                }
                subRegions[nodes] = neighbors;
            }
            if (!subRegions.Any())
                return;
            foreach (var nodesPair in subRegions)
            {
                var nodes = nodesPair.Key;
                var neighbors = nodesPair.Value;
                if (nodes == biggest)
                    continue;
                var newRegion = new Region(this.Chunk);
                newRegion.Add(nodes);
                foreach (var n in nodes)
                    this.Nodes.Remove(n.Global);
                foreach (var neigh in neighbors)
                {
                    UnLink(this, neigh);
                    Link(newRegion, neigh);
                }
                this.Map.Regions.Add(newRegion);
                if (isBelowDoor)
                {
                    LinkNoPaint(newRegion, doorRegion);
                }
            }
        }
        private static void LinkNoPaint(Region master, Region slave)
        {
            master.Neighbors.Add(slave);
            slave.Neighbors.Add(master);
        }
        private static void Link(Region master, Region slave)
        {
            slave.Paint(master);
            master.Neighbors.Add(slave);
            slave.Neighbors.Add(master);
        }
        private static void UnLink(Region region1, Region region2)
        {
            region1.Neighbors.Remove(region2);
            region2.Neighbors.Remove(region1);
        }

        public bool IsOutdoors()
        {
            foreach(var node in this.Nodes)
            {
                var heightmapvalue = this.Chunk.GetHeightMapValue(node.Value.Local);
                if (node.Key.Z == heightmapvalue)
                    return true;
                //if (this.Chunk.IsAboveHeightMap(node.Key))
                //    return true;
            }
            return false;
        }

        internal void Write(System.IO.BinaryWriter w)
        {
            this.Nodes.Write(w);
        }
    }
}
